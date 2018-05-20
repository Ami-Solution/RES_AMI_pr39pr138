using System;
using System.Collections.Generic;
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

		public List<DateTime> Dates { get; set; }

		public string Agregator_code { get; set; }

		private ServiceHost Host { get; set; }

		public State State { get; set; } = State.OFF;

		public IAMI_System_Management Proxy { get; set; }

		#endregion properties

		#region constructors

		
		static AMIAgregator()
		{
			MainWindow.agregators = new Dictionary<string, AMIAgregator>();
		}
		
		public AMIAgregator()
		{

		}

        public AMIAgregator(string name) //mora parametrizovani konstr jer onako udje u loop petlju u defaultnom jer poziva uvek sam sebe
        {
            this.Agregator_code = name;
			Dates = new List<DateTime>();

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
			string address = "";
			if (MainWindow.agregators == null) // zbog prvog statickog agregata
			{
				address = $"net.tcp://localhost:{9001}/IAMI_Agregator"; 
			}
			else
			{
				address = $"net.tcp://localhost:{9000 + MainWindow.agregatorNumber}/IAMI_Agregator";
			}

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
		public bool AddDevice(string agregator_code, string device_code)
		{
			bool retVal = true;

			if (MainWindow.agregators[agregator_code].Buffer.ContainsKey(device_code))
			{
				retVal = false;
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

			}

			return retVal;
		}

		//prihvatimo koji device salje vrednosti. Vrednosti su tipa Struja-100, Napon-200, Snaga-300
		public string ReceiveDataFromDevice(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
		{
			string retVal = "ON";

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
					MainWindow.agregators[agregator_code].Dates.Add(dateTime);
				}
				else
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

		public void SendToSystemMenagement(AMIAgregator ag)
		{
			while (ag.State == State.ON)
			{
				Thread.Sleep(5000);

				//imamo cetiri merenja, mozda se desi da device upise tek jedno, a on vec posalje u bazu (baza ocekuje 4)
				//datum se dodaje u listu tek kada se upisu sva 4 merenja
				if (ag.Dates.Count() > 1) 
				{
					ag.Proxy.SendDataToDataBase(ag.Agregator_code, ag.Dates, ag.Buffer);
					foreach (var keyValue in ag.Buffer)
					{
						//praznjenje bafera nakon sto se posalje u bazu podataka
						ag.Buffer[keyValue.Key][TypeMeasurement.ActivePower] = new List<double>();
						ag.Buffer[keyValue.Key][TypeMeasurement.ReactivePower] = new List<double>();
						ag.Buffer[keyValue.Key][TypeMeasurement.Voltage] = new List<double>();
						ag.Buffer[keyValue.Key][TypeMeasurement.CurrentP] = new List<double>();
						ag.Dates = new List<DateTime>();
					}
				}
			}
		}

		public List<string> ListOfAgregatorIDs()
        {
            List<string> retList = new List<string>();
            foreach(var ID in MainWindow.agregators)
            {
                retList.Add(ID.Key);
            }

            return retList;
        }

		#endregion methods

	}
}
