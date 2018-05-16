using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Common;
using Storage;

namespace AMI_Agregator
{
	public class AMIAgregator : IAMI_Agregator
	{

		#region properties
		//device_code, <TipVrednost, lista vrednosti>
		public Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer { get; set; }

		public string Agregator_code { get; set; }

		private ServiceHost Host { get; set; }

		public State State { get; set; } = State.Off;

		#endregion properties

		#region constructors
		static AMIAgregator()
		{

			MainWindow.agregators = new Dictionary<string, AMIAgregator>()
			{
				{
					"agregator1",
					new AMIAgregator("agregator1")
					{
						buffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>()
						{
							{
								"DEVICE_ID1",
								new Dictionary<TypeMeasurement, List<double>>()
								{
									{ TypeMeasurement.ActivePower, new List<double>() },
									{ TypeMeasurement.ReactivePower, new List<double>() },
									{ TypeMeasurement.Current, new List<double>() },
									{ TypeMeasurement.Voltage, new List<double>() },
								}
							},

						}
					}
				},
			};

		}

		public AMIAgregator()
		{

		}

        public AMIAgregator(string name) //mora parametrizovani konstr jer onako udje u loop petlju u defaultnom jer poziva uvek sam sebe
        {
            this.Agregator_code = name;

			InitialiseBuffer(buffer);
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
			this.buffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>()
			{
				{
					"",
					new Dictionary<TypeMeasurement, List<double>>()
					{
						{ TypeMeasurement.ActivePower, null },
						{ TypeMeasurement.ReactivePower, null },
						{ TypeMeasurement.Current, null },
						{ TypeMeasurement.Voltage, null },
					}
				}
			};
		}

		//provera da li agregator sadrzi uredjaj koji treba da mu se doda
		public bool AddDevice(string agregator_code, string device_code)
		{
			bool retVal = true;

			if (MainWindow.agregators[agregator_code].buffer.ContainsKey(device_code))
			{
				retVal = false;
			}
			else
			{
				MainWindow.agregators[agregator_code].buffer.Add(
					device_code,
					new Dictionary<TypeMeasurement, List<double>>()
					{
						{ TypeMeasurement.ActivePower, new List<double>() },
						{ TypeMeasurement.ReactivePower, new List<double>() },
						{ TypeMeasurement.Current, new List<double>() },
						{ TypeMeasurement.Voltage, new List<double>() },
					});

			}

			return retVal;
		}

		//prihvatimo koji device salje vrednosti. Vrednosti su tipa Struja-100, Napon-200, Snaga-300
		public string ReceiveDataFromDevice(string agregator_code, string device_code, Dictionary<TypeMeasurement, double> values)
		{
			string retVal = "ON";

			AMIAgregator ag = new AMIAgregator();

			if (MainWindow.agregators.TryGetValue(agregator_code, out ag))
			{
				if (ag.State == State.On)
				{
					foreach (var keyValue in values)
					{
						MainWindow.agregators[agregator_code].buffer[device_code][keyValue.Key].Add(keyValue.Value);
					}
				}
				else
				{
					retVal = "OF";
				}
			}
			else
			{
				retVal = "DELETED";
			}
			

			return retVal;
		}

		public void SendToLocalStorage(string device_code, DateTime timestamp)
		{
			//implementirati da salje podatke u bazu svakih 5 minuta (proksi na AMI_System_Managment)
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
