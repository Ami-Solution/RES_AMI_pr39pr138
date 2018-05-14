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

		//predefinisan dictionari koji sadrzi: agregator_id i sam agregator.
		//sam agregator sadrzi svoj id, i buffer
		//buffer je dictionari i cine ga: device_id i novi dictionari: typeMeasurment i lista vrednost
		public static Dictionary<string, AMIAgregator> agregators;

		#region properties
		//device_code, <TipVrednost, lista vrednosti>
		public Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer { get; set; }

		public string Agregator_code { get; set; }

		private ServiceHost host;

		#endregion properties


		#region constructors
		static AMIAgregator()
		{

			agregators = new Dictionary<string, AMIAgregator>()
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
									{ TypeMeasurement.ActivePower, null },
									{ TypeMeasurement.ReactivePower, null },
									{ TypeMeasurement.Current, null },
									{ TypeMeasurement.Voltage, null },
								}
							},

						}
					}
				},
			};

		}

		public AMIAgregator()
		{
			this.Agregator_code = "agregator" + (agregators.Count() + 1);

			InitialiseBuffer(buffer);
			CreateHost(this.host);
		}

        public AMIAgregator(string name) //mora parametrizovani konstr jer onako udje u loop petlju u defaultnom jer poziva uvek sam sebe
        {
            this.Agregator_code = name;

			InitialiseBuffer(buffer);
			CreateHost(this.host);
		}
		#endregion constructors

		#region methods
		private void CreateHost(ServiceHost host)
		{
			host = new ServiceHost(typeof(AMIAgregator));
			NetTcpBinding binding = new NetTcpBinding();
			string address = "";
			if (agregators == null) // zbog prvog statickog agregata
			{
				address = $"net.tcp://localhost:{9001}/IAMI_Agregator"; //+1 zato sto se prvo napravi agregat, pa se onda tek doda u listu agregata (njegova pozicija je broj agregata +1)
			}
			else
			{
				address = $"net.tcp://localhost:{9000 + agregators.Count() + 1}/IAMI_Agregator";
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

			if (agregators[agregator_code].buffer.ContainsKey(device_code))
			{
				retVal = false;
			}
			else
			{
				agregators[agregator_code].buffer.Add(
					device_code,
					new Dictionary<TypeMeasurement, List<double>>()
					{
						{ TypeMeasurement.ActivePower, null },
						{ TypeMeasurement.ReactivePower, null },
						{ TypeMeasurement.Current, null },
						{ TypeMeasurement.Voltage, null },
					});

			}

			return retVal;
		}

		//prihvatimo koji device salje vrednosti. Vrednosti su tipa Struja-100, Napon-200, Snaga-300
		public bool ReceiveDataFromDevice(string agregator_code, string device_code, Dictionary<TypeMeasurement, double> values)
		{
			bool retVal = true;

			foreach (var keyValue in values)
			{
				agregators[agregator_code].buffer[device_code][keyValue.Key].Add(keyValue.Value);
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
            foreach(var ID in agregators)
            {
                retList.Add(ID.Key);
            }

            return retList;
        }
		#endregion methods

	}
}
