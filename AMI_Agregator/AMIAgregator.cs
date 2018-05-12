using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage;

namespace AMI_Agregator
{
	public class AMIAgregator
	{
		//predefinisan dictionari koji sadrzi: agregator_id i sam agregator.
		//sam agregator sadrzi svoj id, i buffer
		//buffer je dictionari i cine ga: device_id i novi dictionari: typeMeasurment i lista vrednost
		//															   struja,napon,akt snaga, reakt snaga, i svaka sadrzi svoju listu vrednosti
		public static Dictionary<string, AMIAgregator> agregators = new Dictionary<string, AMIAgregator>()
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

		public string Agregator_code { get; set; }

		public List<string> Device_codes { get; set; }

		//device_code, <TipVrednost, lista vrednosti>
		public Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer { get; set; }

		//provera da li agregator sadrzi uredjaj koji treba da mu se doda
		private bool AddDevice(string agregator_code, string device_code)
		{
			bool retVal = true;
			
			if (agregators[agregator_code].Device_codes.Contains(device_code))
			{
				retVal = false;
			}
			else
			{
				agregators[agregator_code].Device_codes.Add(device_code);
			}

			return retVal;
		}

		//prihvatimo koji device salje vrednosti. Vrednosti su tipa Stura-100, Napon-200, Snaga-300
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
			//implementirati da salje podatke u bazu podataka (svake sekunde)
		}
	}
}
