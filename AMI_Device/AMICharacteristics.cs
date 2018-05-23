using Common;
using MlkPwgen;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMI_Device
{

	public class AMICharacteristics : IAMI_Device
	{
		private State status = State.OFF;
		private IAMI_Agregator proxy;
		private static System.Random rand = new System.Random();

		public bool Added { get; set; } = false;

		public string Device_code { get; set; }

		public DateTime CreationTime { get; set; }

		public Dictionary<Storage.TypeMeasurement, double> Measurements { get; set; }

		public string AgregatorID { get; set; }

		public State Status { get => status; set => status = value; }

		public IAMI_Agregator Proxy { get => proxy; set => proxy = value; }

		public AMICharacteristics()
		{
			Measurements = new Dictionary<Storage.TypeMeasurement, double>();
			Device_code = PasswordGenerator.Generate(length: 10, allowed: Sets.Alphanumerics); //generise random string
			CreationTime = DateTime.Now;
		}

		public AMICharacteristics(string device_code, string agregator_code)
		{
			this.Added = true;
			this.Device_code = device_code;
			this.AgregatorID = agregator_code;
			string id = agregator_code.Substring(9);
			Connect(Convert.ToInt32(id));

			Measurements = new Dictionary<Storage.TypeMeasurement, double>();
		}

		public void Connect(int id)
		{
			var binding = new NetTcpBinding();
			int start = 9000 + id;
			string address = $"net.tcp://localhost:{start}/IAMI_Agregator";
			ChannelFactory<IAMI_Agregator> factory = new ChannelFactory<IAMI_Agregator>(binding, new EndpointAddress(address));
			this.proxy = factory.CreateChannel();
		}

		public void SendDataToAgregator(string agrID, string devID, Dictionary<TypeMeasurement, double> measurement) //ako stoji AMIChar ne mogu da dodam u interfejs, a ako ne dodam u interfejs, ne mozemo UNIT test
		{
			string retVal = "";
			AMICharacteristics ami = MainWindow.AvailableAMIDevices[devID];

			//ako uredjaj nije prikljucen na agregat
			if (!ami.Added)
			{
				retVal = ami.Proxy.AddDevice(agrID, devID);

				if (retVal == "ADDED") //ako je uspesno dodat
				{
					ami.Added = true;
					ami.Proxy.ReceiveDataFromDevice(agrID, DateTime.Now, devID, measurement);
					ami.GenerateRandomValues();
				}
				else if (retVal == "DUPLICATE") // ako je duplikat, dodaje se uredjaju nova vrednost imena
				{
					ami.Device_code = PasswordGenerator.Generate(length: 10, allowed: Sets.Alphanumerics);
				}
			}
			else
			{
				ami.Proxy.ReceiveDataFromDevice(agrID, DateTime.Now, devID, measurement);
				ami.GenerateRandomValues();
			}

		}

		//dodata metoda za generisanje random vrednosti, zbog ponavljanja koda
		public void GenerateRandomValues()
		{
			double V = rand.Next(300);
			double I = rand.Next(300);
			double P = rand.Next(300);
			double S = rand.Next(300);

			Measurements.Clear(); //posto hocemo da imamo samo 4 vrednosti

			Measurements.Add(TypeMeasurement.Voltage, V);
			Measurements.Add(TypeMeasurement.CurrentP, I);
			Measurements.Add(TypeMeasurement.ActivePower, P);
			Measurements.Add(TypeMeasurement.ReactivePower, S);
		}

	}
}
