using MlkPwgen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMI_Device
{
    public class AMICharacteristics
    {
        public string Name { get; set; }

        public int CreationTime { get; set; }

        public Dictionary<Enum, double> Measurements { get; set; }

        public string AgregatorID { get; set; }
        public string Status { get => status; set => status = value; }

        private string status = "OFF";

        public AMICharacteristics()
        {
            Measurements = new Dictionary<Enum, double>();
            Name = PasswordGenerator.Generate(length: 10, allowed: Sets.Alphanumerics); //generise random string
            CreationTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; // vreme po unix timestamp formatu (racuna od 1970 do danas sekunde)
        }
    }
}
