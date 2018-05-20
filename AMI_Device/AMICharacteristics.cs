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

        public void Connect(int id)
        {
            var binding = new NetTcpBinding();
            int start = 9000+id;
            string address = $"net.tcp://localhost:{start}/IAMI_Agregator";
            ChannelFactory<IAMI_Agregator> factory = new ChannelFactory<IAMI_Agregator>(binding, new EndpointAddress(address));
            this.proxy = factory.CreateChannel();
        }

		public void SendDataToAgregator(string agrID,string devID,Dictionary<TypeMeasurement,double> measurement) //ako stoji AMIChar ne mogu da dodam u interfejs, a ako ne dodam u interfejs, ne mozemo UNIT test
		{
			string retVal = "";
            AMICharacteristics ami = MainWindow.AvailableAMIDevices[devID];
            do
			{
                retVal = ami.Proxy.ReceiveDataFromDevice(agrID,DateTime.Now,devID,measurement);
				Trace.WriteLine(DateTime.Now);
				Thread.Sleep(1000);

                double V = rand.Next(300);
                double I = rand.Next(300);
                double P = rand.Next(300);
                double S = rand.Next(300);

                ami.Measurements.Clear(); //posto hocemo da imamo samo 4 vrednosti

                ami.Measurements.Add(TypeMeasurement.Voltage, V);
                ami.Measurements.Add(TypeMeasurement.CurrentP, I);
                ami.Measurements.Add(TypeMeasurement.ActivePower, P);
                ami.Measurements.Add(TypeMeasurement.ReactivePower, S);

            } while (ami.Status == State.ON && retVal == "ON");

			if (retVal == "OFF")
				ami.Status = State.OFF;

			/* Ovde sada treba da se implementira sta se radi ako se obrise agregat, ili ako se agregat ugasi
			 * 
			 */
		}

	}
}
