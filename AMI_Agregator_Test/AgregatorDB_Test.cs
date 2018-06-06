using AMI_Agregator;
using NUnit.Framework;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMI_Agregator_Test
{
    [TestFixture]
    public class AgregatorDB_Test
    {
        [Test]
        [TestCase("agregator21")]
        public void DeleteAgregatorFromLocalDatabase_Deleted(string agregator_code)
        {
            AgregatorDB db = new AgregatorDB();
            MainWindow.agregatorNumber++;

            db.DeleteAgregatorFromLocalDatabase(agregator_code);

            Assert.NotNull(db);
        }

        [Test]
        [TestCase("agregator22","ggghhlyslk")]
        public void DeleteDeviceFromLocalDatabase(string agregator_code, string device_code)
        {
            AgregatorDB db = new AgregatorDB();
            MainWindow.agregatorNumber++;

            db.DeleteDeviceFromLocalDatabase(agregator_code,device_code);

            Assert.NotNull(db);
        }

        static IEnumerable<object[]> first()
        {
            var measurement = new Dictionary<TypeMeasurement, double>()
                {
                    {TypeMeasurement.ActivePower,222 },
                    {TypeMeasurement.CurrentP,0.1 },
                    {TypeMeasurement.ReactivePower,69.49 },
                    {TypeMeasurement.Voltage,1000 }
                };

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "agregator23", DateTime.Now, "SJkaleKlsk", measurement } };
        }

        [Test]
        [TestCaseSource(nameof(first))]
        public void SendToLocalDatabase(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
        {
            AgregatorDB db = new AgregatorDB();
            double first = values[TypeMeasurement.ActivePower];
            MainWindow.agregatorNumber++;

            db.SendToLocalDatabase(agregator_code,dateTime,device_code,values);

            Assert.AreEqual(first, values[TypeMeasurement.ActivePower]);
        }

        [Test]
        public void LoadAllDataFromLocalDataBase()
        {
            AgregatorDB db = new AgregatorDB();
            AMIAgregator ami = new AMIAgregator("agregator244");
            MainWindow.agregators.Add(ami.Agregator_code, ami);
            MainWindow.agregatorNumber++;

            db.LoadAllAgregators();

            Assert.NotNull(db);
        }

        [Test]
        [TestCase("agregator25")]
        public void RemoveAgregatorFromDataBase(string agregator_code)
        {
            AgregatorDB db = new AgregatorDB();
            MainWindow.agregatorNumber++;

            db.RemoveAgregatorFromDataBase(agregator_code);

            Assert.NotNull(db);
        }

        [Test]
        [TestCase("agregator26")]
        public void SaveAgragatorToDataBase(string agregator_code)
        {
            AgregatorDB db = new AgregatorDB();
            MainWindow.agregatorNumber++;

            db.SaveAgragatorToDataBase(agregator_code);

            Assert.NotNull(db);
        }
    }
}
