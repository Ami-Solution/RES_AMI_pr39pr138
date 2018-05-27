using AMI_Agregator;
using NUnit.Framework;
using Storage;
using System;
using System.Collections.Generic;

namespace AMI_Agregator_Test
{
    [TestFixture]
    public class AMIAgregator_Test
    {
        [Test]
        [TestCase("agregator1")]
        [TestCase("agregator12")]
        [TestCase("agregator123")]
        [TestCase("agregator0")]
        public void AMIAgregator_GoodParametersInConstructor_Instantiated(string name)
        {
            AMIAgregator agr = new AMIAgregator(name);
            MainWindow.agregatorNumber++;

            Assert.AreEqual(agr.Agregator_code, name);
        }

        [Test]
        [TestCase("agregator1","aksjfkelds")]
        public void AMIAgregator_AddDeviceStatusOnNotDuplicate_ReturnsAdded(string agregator_code, string device_code)
        {
            
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code,agr);
            agr.State = State.ON;

            string ret=agr.AddDevice(agr.Agregator_code, device_code);

            Assert.AreEqual("ADDED", ret);
        }

        [Test]
        [TestCase("agregator1","skfjturiii")]
        public void AMIAgregator_AddDeviceStatusOnDuplicate_ReturnsDuplicate(string agregator_code, string device_code)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            agr.State = State.ON;
            agr.AddDevice(agr.Agregator_code, device_code);

            string ret = agr.AddDevice(agr.Agregator_code, device_code);

            Assert.AreEqual("DUPLICATE", ret);
        }

        [Test]
        [TestCase("agregator1", "skfjturiii")]
        public void AMIAgregator_AddDeviceStatusOff_ReturnsOff(string agregator_code, string device_code)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            agr.AddDevice(agr.Agregator_code, device_code);

            string ret = agr.AddDevice(agr.Agregator_code, device_code);

            Assert.AreEqual("OFF", ret);
        }

        static IEnumerable<object[]> returnsOddProperty()
        {
            var measurement = new Dictionary<TypeMeasurement, double>()
                {
                    {TypeMeasurement.ActivePower,222 },
                    {TypeMeasurement.CurrentP,0.1 },
                    {TypeMeasurement.ReactivePower,69.49 },
                    {TypeMeasurement.Voltage,1000 }
                };

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "agregator1", DateTime.Now, "SJkaleKlsk", measurement } };
        }

        [Test]
        [TestCaseSource(nameof(returnsOddProperty))]
        public void AMIAgregator_ReceiveDataFromDeviceAgregatorExistStatusOn_BufferChange(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            agr.State = State.ON;
            MainWindow.agregators[agregator_code].Buffer.Add(
                        device_code,
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            MainWindow.agregators[agregator_code].listOfDevices.Add(device_code);
            MainWindow.agregators[agregator_code].Dates.Add(device_code, new List<DateTime>());
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = MainWindow.agregators[agregator_code].Buffer;

            agr.ReceiveDataFromDevice(agregator_code, dateTime, device_code, values); //kad udje u metodu s bazom, izbaci gresku

            Assert.AreNotEqual(agr.Buffer, oldBuffer);
        }

        [Test]
        [TestCaseSource(nameof(returnsOddProperty))]
        public void AMIAgregator_ReceiveDataFromDeviceAgregatorExistStatusOff_BufferDoesntChange(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            agr.State = State.OFF;
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = MainWindow.agregators[agregator_code].Buffer;

            agr.ReceiveDataFromDevice(agregator_code, dateTime, device_code, values); //kad udje u metodu s bazom, izbaci gresku

            Assert.AreEqual(agr.Buffer, oldBuffer);
        }
    }
}
