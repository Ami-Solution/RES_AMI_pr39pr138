using System;
using System.Collections.Generic;
using AMI_Device;
using NUnit.Framework;
using Storage;
using Moq;
using Common;

namespace AMI_Device_Test
{
    [TestFixture]
    public class AMICharacteristics_Test
    {
        
        [Test]
        public void AMICharacteristics_GoodParameters_Instantiated()
        {
            AMICharacteristics ami = new AMICharacteristics();

            Assert.IsInstanceOf(typeof(DateTime), ami.CreationTime);
            Assert.IsInstanceOf(typeof(string), ami.Device_code);
            Assert.IsInstanceOf(typeof(Dictionary<TypeMeasurement, double>), ami.Measurements);
        }
        [Test]
        public void AMICharacteristics_NotNull_Instantiated()
        {
            AMICharacteristics ami = new AMICharacteristics();

            Assert.NotNull(ami.CreationTime);
            Assert.NotNull(ami.Device_code);
            Assert.NotNull(ami.Measurements);
        }

        [Test]
        public void AMICharacteristics_HaveSomeValues_Instantiated()
        {
            AMICharacteristics ami = new AMICharacteristics();

            Assert.IsNotEmpty(ami.Device_code);
            //CollectionAssert.IsEmpty(ami.Measurements);
        }

        [Test]
        public void AMICharacteristics_DoesntHaveSomeValues_Instantiated()
        {
            AMICharacteristics ami = new AMICharacteristics();

            //Assert.IsNotEmpty(ami.Device_code);
            CollectionAssert.IsEmpty(ami.Measurements);
            //Assert.IsInstanceOf(typeof(AMICharacteristics), ami);
        }

        static object[] MySourceProperty
        {
            get
            {
                var measurement = new Dictionary<TypeMeasurement, double>()
                {
                    {TypeMeasurement.ActivePower,120 },
                    {TypeMeasurement.CurrentP,220.23 },
                    {TypeMeasurement.ReactivePower,69 },
                    {TypeMeasurement.Voltage,10.4 }
                };
                // Do what you want with your dictionary

                // The order of element in the object must be the same expected by your test method
                //yield return new object[] { "agr1", "askdwskdls", measurement }; ne moze yield iz nekog razloga
                return new[] { new object[] { "agr1", "askdwskdls", measurement },
                               new object[] { "agr2", "trl4k556s3", measurement }};

            }
        }


        [Test]
        [TestCaseSource("MySourceProperty")]
        public void SendDataToAgregator_GoodVariables_ReturnsOff(string agrID,string devID, Dictionary<TypeMeasurement, double> measurement)
        {
            AMICharacteristics ami = new AMICharacteristics();
            ami.Status = State.ON;
            Mock<IAMI_Agregator> moq = new Mock<IAMI_Agregator>();
            //mock.Setup(x => x.DoSomething(It.IsAny<string>(), It.IsAny<int>())).Returns((string x, int y) => x);
            //moq.Setup(x => x.ReceiveDataFromDevice(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Dictionary<TypeMeasurement, double>>())).Returns("OFF");
            IAMI_Agregator agr1 = moq.Object;
            MainWindow.AvailableAMIDevices.Add(devID, ami);
            ami.Proxy = agr1;

            ami.SendDataToAgregator(agrID, devID, measurement);

            Assert.AreEqual(State.OFF, ami.Status);
        }
    }
}
