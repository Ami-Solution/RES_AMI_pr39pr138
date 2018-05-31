using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Storage;

namespace AMI_Agregator
{

	public class AMIAgregator : IAMI_Agregator
	{

		#region properties
		//device_code, <TipVrednost, lista vrednosti>
		public Dictionary<string, Dictionary<TypeMeasurement, List<double>>> Buffer { get; set; }

		//svaki agregator ima uredjaje, koji imaju svoje datume
		public Dictionary<string, List<DateTime>> Dates { get; set; }

		public string Agregator_code { get; set; }

		private ServiceHost Host { get; set; }

		public State State { get; set; } = State.OFF;

		public IAMI_System_Management Proxy { get; set; }

		public List<string> listOfDevices { get; set; }

		#endregion properties

		#region constructors

		static AMIAgregator()
		{
			MainWindow.agregators = new Dictionary<string, AMIAgregator>();
		}

		public AMIAgregator()
		{

		}

		public AMIAgregator(string name) 
		{
			this.Agregator_code = name;
			Dates = new Dictionary<string, List<DateTime>>();
			listOfDevices = new List<string>();

			var binding = new NetTcpBinding();
			string address = $"net.tcp://localhost:8004/IAMI_System_Management";
			ChannelFactory<IAMI_System_Management> factory = new ChannelFactory<IAMI_System_Management>(binding, new EndpointAddress(address));
			Proxy = factory.CreateChannel();

			InitialiseBuffer(Buffer);
			CreateHost(this.Host);
		}
		#endregion constructors

		#region methods
		private void CreateHost(ServiceHost host)
		{
			host = new ServiceHost(typeof(AMIAgregator));
			NetTcpBinding binding = new NetTcpBinding();
			string address = $"net.tcp://localhost:{9000 + MainWindow.agregatorNumber}/IAMI_Agregator";

			host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
			host.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
			host.AddServiceEndpoint(typeof(IAMI_Agregator), binding, address);
			host.Open();
		}

		private void InitialiseBuffer(Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
		{
			this.Buffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
		}

		//provera da li agregator sadrzi uredjaj koji treba da mu se doda
		public string AddDevice(string agregator_code, string device_code)
		{
			string retVal = "ADDED";

			if (MainWindow.agregators[agregator_code].State == State.ON)
			{
				if (MainWindow.agregators[agregator_code].Buffer.ContainsKey(device_code))
				{
					retVal = "DUPLICATE"; //ako postoji vec devajs sa istim kodom u agregatoru
				}
				else
				{
					MainWindow.agregators[agregator_code].Buffer.Add(
						device_code,
						new Dictionary<TypeMeasurement, List<double>>()
						{
						{ TypeMeasurement.ActivePower, new List<double>() },
						{ TypeMeasurement.ReactivePower, new List<double>() },
						{ TypeMeasurement.CurrentP, new List<double>() },
						{ TypeMeasurement.Voltage, new List<double>() },
						});

					MainWindow.agregators[agregator_code].listOfDevices.Add(device_code);
					MainWindow.agregators[agregator_code].Dates.Add(device_code, new List<DateTime>());

				}
			}
			else //ako je iskljucen agregat
			{
				retVal = "OFF";
			}

			return retVal;
		}

		public string RemoveDevice(string agregator_code, string device_code)
		{
			AMIAgregator ag = MainWindow.agregators[agregator_code];
			ag.Proxy.SendDataToSystemDataBase(ag.Agregator_code, ag.Dates, ag.Buffer); //prinudno slanje podataka u bazu, kada se brise uredjaj

			DeleteFromLocalDatabase(agregator_code, device_code);
			ag.Buffer.Remove(device_code);
			ag.listOfDevices.Remove(device_code);
			ag.Dates.Remove(device_code);

			return "DELETED";
		}

		//prihvatimo koji device salje vrednosti. Vrednosti su tipa Struja-100, Napon-200, Snaga-300
		public void ReceiveDataFromDevice(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
		{
			AMIAgregator ag = new AMIAgregator();

			if (MainWindow.agregators.TryGetValue(agregator_code, out ag))
			{
				if (ag.State == State.ON)
				{
					//Trace.WriteLine("Prihvatam podatke");
					foreach (var keyValue in values)
					{
						MainWindow.agregators[agregator_code].Buffer[device_code][keyValue.Key].Add(keyValue.Value);
					}

					//dodavanja datuma u listu, tek nakon sto se upisu sva 4 merenja
					MainWindow.agregators[agregator_code].Dates[device_code].Add(dateTime);

					//saljemo podatke u lokalnu bazu podataka
					SendToLocalDatabase(agregator_code, dateTime, device_code, values);
				}
			}
		}

		public void SendToSystemMenagement(AMIAgregator ag)
		{
			while (ag.State == State.ON)
			{
				Thread.Sleep(5000);

				//ako postoji agregat koji ima uredjaj u sebi, salje podatke
				if (ag.Dates.Count() > 0)
				{
					if (ag.Proxy.SendDataToSystemDataBase(ag.Agregator_code, ag.Dates, ag.Buffer))
					{
						DeleteFromLocalDatabase(ag.Agregator_code);

						foreach (var keyValue in ag.Buffer)
						{
							//praznjenje bafera nakon sto se posalje u bazu podataka
							ag.Buffer[keyValue.Key][TypeMeasurement.ActivePower] = new List<double>();
							ag.Buffer[keyValue.Key][TypeMeasurement.ReactivePower] = new List<double>();
							ag.Buffer[keyValue.Key][TypeMeasurement.Voltage] = new List<double>();
							ag.Buffer[keyValue.Key][TypeMeasurement.CurrentP] = new List<double>();
							ag.Dates[keyValue.Key] = new List<DateTime>();
						}
					}


				}
			}
		}

		public void DeleteFromLocalDatabase(string agregator_code)
		{
			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM AMI_LocalData_Table WHERE Agregator_Code like '{agregator_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		private void DeleteFromLocalDatabase(string agregator_code, string device_code)
		{
			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"DELETE FROM AMI_LocalData_Table WHERE Agregator_Code like '{agregator_code}' and Device_Code like '{device_code}'";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteReader();

			}
		}

		private void SendToLocalDatabase(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
		{
			double Voltage = 0;
			double CurrentP = 0;
			double ActivePower = 0;
			double ReactivePower = 0;

			//vadimo rednosti iz recninka
			foreach (var keyValue in values)
			{

				if (keyValue.Key == TypeMeasurement.CurrentP)
					CurrentP = keyValue.Value;

				if (keyValue.Key == TypeMeasurement.ActivePower)
					ActivePower = keyValue.Value;

				if (keyValue.Key == TypeMeasurement.Voltage)
					Voltage = keyValue.Value;

				if (keyValue.Key == TypeMeasurement.ReactivePower)
					ReactivePower = keyValue.Value;

			}

			string CS = ConfigurationManager.ConnectionStrings["DBCS_AMI_Agregator"].ConnectionString;
			using (SqlConnection con = new SqlConnection(CS))
			{
				con.Open();
				SqlCommand cmd;

				string query = $"INSERT INTO AMI_LocalData_Table(Agregator_Code, Device_Code, Voltage, CurrentP, ActivePower, ReactivePower, DateAndTime) " +
				$"VALUES('{agregator_code}', '{device_code}', {Voltage}, {CurrentP}, {ActivePower}, {ReactivePower}, '{dateTime}')";

				cmd = new SqlCommand(query, con);
				cmd.ExecuteNonQuery();

			}
		}

		public Dictionary<string, List<string>> AgregatorsAndTheirDevices()
		{
			Dictionary<string, List<string>> retList = new Dictionary<string, List<string>>();

			foreach (var ID in MainWindow.agregators)
			{
				retList.Add(ID.Key, MainWindow.agregators[ID.Key].listOfDevices);
			}

			return retList;
		}
		#endregion methods

	}
}
