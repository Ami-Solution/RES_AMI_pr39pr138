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
        public string Device_code { get; set; }

        public int CreationTime { get; set; }

        public Dictionary<Storage.TypeMeasurement, double> Measurements { get; set; }

        public string AgregatorID { get; set; }

        public State Status { get => status; set => status = value; }

        public IAMI_Agregator Proxy { get => proxy; set => proxy = value; }  

        public AMICharacteristics()
        {
            Measurements = new Dictionary<Storage.TypeMeasurement, double>();
            Device_code = PasswordGenerator.Generate(length: 10, allowed: Sets.Alphanumerics); //generise random string
            CreationTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; // vreme po unix timestamp formatu (racuna od 1970 do danas sekunde)
        }

        public void Connect(int id)
        {
            var binding = new NetTcpBinding();
            int start = 9000+id;
            string address = $"net.tcp://localhost:{start}/IAMI_Agregator";
            ChannelFactory<IAMI_Agregator> factory = new ChannelFactory<IAMI_Agregator>(binding, new EndpointAddress(address));
            this.proxy = factory.CreateChannel();
        }

		public void SendDataToAgregator(AMICharacteristics ami)
		{
			string retVal = "";
			do
			{
				retVal = ami.Proxy.ReceiveDataFromDevice(ami.AgregatorID, DateTime.Now, ami.Device_code, ami.Measurements);
				Trace.WriteLine(DateTime.Now);
				Thread.Sleep(1000);

			} while (ami.Status == State.ON && retVal == "ON");

			if (retVal == "OFF")
				ami.Status = State.OFF;

			/* Ovde sada treba da se implementira sta se radi ako se obrise agregat, ili ako se agregat ugasi
			 * 
			 */
		}

	}
}
