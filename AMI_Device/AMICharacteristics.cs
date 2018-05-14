using Common;
using MlkPwgen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AMI_Device
{
    public class AMICharacteristics
    {
        private string status = "OFF";
        private IAMI_Agregator proxy;
        public string Name { get; set; }

        public int CreationTime { get; set; }

        public Dictionary<Enum, double> Measurements { get; set; }

        public string AgregatorID { get; set; }

        public string Status { get => status; set => status = value; }

        public IAMI_Agregator Proxy { get => proxy; set => proxy = value; }  

        public AMICharacteristics()
        {
            Measurements = new Dictionary<Enum, double>();
            Name = PasswordGenerator.Generate(length: 10, allowed: Sets.Alphanumerics); //generise random string
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
    }
}
