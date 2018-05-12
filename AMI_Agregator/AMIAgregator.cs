using System;
using System.Collections.Generic;
using System.Linq;
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
		//															   struja,napon,akt snaga, reakt snaga, i svaka sadrzi svoju listu vrednosti

		public Dictionary<string, AMIAgregator> agregators { get; set; }

		public List<string> agregator_ids { get; set; }

		public AMIAgregator()
		{
			agregators = new Dictionary<string, AMIAgregator>()
			{
				{
					"agregator1",
					new AMIAgregator()
					{
						Agregator_code = "agregator1",
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

							{
								"DEVICE_ID2",
								new Dictionary<TypeMeasurement, List<double>>()
								{
									{ TypeMeasurement.ActivePower, null },
									{ TypeMeasurement.ReactivePower, null },
									{ TypeMeasurement.Current, null },
									{ TypeMeasurement.Voltage, null },
								}
							}
						}
					}
				},

			};

			agregator_ids = agregators.Keys.ToList();
		}

		public string Agregator_code { get; set; }

		//device_code, <TipVrednost, lista vrednosti>
		public Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer { get; set; }

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
				agregators[agregator_code].buffer.ContainsKey(device_code);
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

	}
}
