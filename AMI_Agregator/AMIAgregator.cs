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
		public static Dictionary<string, AMIAgregator> agregators = new Dictionary<string, AMIAgregator>()
		{
			{
				"agregator1",
				new AMIAgregator()
				{
					Agregator_code = "agregator1",
					Device_codes = new List<string>()
					{
						"DEVICE_ID1", "DEVICE_ID2", "DEVICE_ID3", "DEVICE_ID4"
					}
				}

			},

			{
				"agregator2",
				new AMIAgregator()
				{
					Agregator_code = "agregator1",
					Device_codes = new List<string>()
					{
						"DEVICE_ID1", "DEVICE_ID2", "DEVICE_ID5", "DEVICE_ID6"
					}
				}
			}
		};

		public string Agregator_code { get; set; }

		public List<string> Device_codes { get; set; }

		//provera da li odredjeni agregator sadrzi uredjaj koji treba da mu se doda
		public bool AddDevice(string agregator_code, string device_code)
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

		public bool ReceiveDataFromDevice(string device_code, Dictionary<TypeMeasurement, double> values)
		{
			bool retVal = true;



			return retVal;
		}

		public void SendToLocalStorage(string device_code, DateTime timestamp)
		{
			//implementirati da salje podatke u bazu podataka (svake sekunde)
		}
	}
}
