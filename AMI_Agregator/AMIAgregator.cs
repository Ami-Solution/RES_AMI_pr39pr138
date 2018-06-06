using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
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
		public static AgregatorDB ADB = new AgregatorDB();

		#region properties

		//device_code, <TipVrednost, lista vrednosti>
		public Dictionary<string, Dictionary<TypeMeasurement, List<double>>> Buffer { get; set; }

		//svaki agregator ima uredjaje, koji imaju svoje datume
		public Dictionary<string, List<DateTime>> Dates { get; set; }

		public string Agregator_code { get; set; }

		private ServiceHost Host { get; set; }

		public State State { get; set; } = State.OFF;

		public IAMI_System_Management Proxy { get; set; }

		public List<string> ListOfDevices { get; set; }

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
			Agregator_code = name;
			Dates = new Dictionary<string, List<DateTime>>();
			ListOfDevices = new List<string>();

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

			if (MainWindow.agregators.ContainsKey(agregator_code))
			{

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

						MainWindow.agregators[agregator_code].ListOfDevices.Add(device_code);
						MainWindow.agregators[agregator_code].Dates.Add(device_code, new List<DateTime>());

					}
				}
				else //ako je iskljucen agregat
				{
					retVal = "OFF";
				}
			}
			else
			{
				retVal = "DELETED";
			}

			return retVal;
		}

		public string RemoveDevice(string agregator_code, string device_code)
		{
			if (MainWindow.agregators.ContainsKey(agregator_code)) //ako obrisemo agregator, ne mozemo obrisati uredjaj iz obrisanog agregatora
			{
				AMIAgregator ag = MainWindow.agregators[agregator_code];
				ag.Proxy.SendDataToSystemDataBase(ag.Agregator_code, ag.Dates, ag.Buffer); //prinudno slanje podataka u bazu, kada se brise uredjaj

				ADB.DeleteDeviceFromLocalDatabase(agregator_code, device_code);
				ag.ListOfDevices.Remove(device_code);

				MainWindow.agregators.Remove(agregator_code);

			}
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
					foreach (var keyValue in values)
					{
						MainWindow.agregators[agregator_code].Buffer[device_code][keyValue.Key].Add(keyValue.Value);
					}

					//dodavanja datuma u listu, tek nakon sto se upisu sva 4 merenja
					MainWindow.agregators[agregator_code].Dates[device_code].Add(dateTime);

					//saljemo podatke u lokalnu bazu podataka
					ADB.SendToLocalDatabase(agregator_code, dateTime, device_code, values);
				}
			}
		}

		public void SendToSystemMenagement(AMIAgregator ag)
		{
			while (MainWindow.agregators.ContainsKey(ag.Agregator_code))
			{
				if (ag.State == State.OFF)
					break;

				int milliseconds = ReadConfig();

				Thread.Sleep(5000); //promeni posle

                if (ag.State == State.OFF) // ako je nakon spavanja ugasen
                    break;

                //ako postoji agregat koji ima uredjaj u sebi, salje podatke, ako postoje vremena postoje podaci
                if (ag.Dates.Count() > 0)
				{
					try
					{
						if (ag.Proxy.SendDataToSystemDataBase(ag.Agregator_code, ag.Dates, ag.Buffer))
						{
							ADB.DeleteAgregatorFromLocalDatabase(ag.Agregator_code);

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
					catch (Exception)
					{

					}

				}
			}
		}

		//sluzi da bi ami_device mogao da pokupi podatke
		public Dictionary<string, List<string>> AgregatorsAndTheirDevices()
		{
			Dictionary<string, List<string>> retList = new Dictionary<string, List<string>>();

			foreach (var ID in MainWindow.agregators)
			{
				retList.Add(ID.Key, MainWindow.agregators[ID.Key].ListOfDevices);
			}

			return retList;
		}

		private int ReadConfig()
		{
			int retVal = 5; //default vreme je 5 minuta, ako je config prazan, ili ako je neki tekst unutra

			using (TextReader tr = new StreamReader("../../ConfigFile.txt"))
			{
				string s = string.Empty;
				if ((s = tr.ReadLine()) != null)
				{
					Int32.TryParse(s, out retVal);
				}

				if (retVal == 0)
					retVal = 5;
			}

			return retVal * 60000; //*60000 -> u milisekunde pretvaramo
		}

		#endregion methods

	}
}
