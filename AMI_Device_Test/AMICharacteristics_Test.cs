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

        [Test]
        [TestCase("aksk4k5jDD", "agregator1")]
        public void AMICharacteristics_ParameterizedConstructorHaveValues_Instantiated(string device_code, string agregator_code)
        {
            AMICharacteristics ami = new AMICharacteristics(device_code, agregator_code);

            Assert.AreEqual(ami.Device_code, device_code);
            Assert.AreEqual(ami.AgregatorID, agregator_code);
        }

        [Test]
        [TestCase("aksk4k5jD", "agregator1")]
        [TestCase("", "agregator1")]
        [TestCase("aksk4k5jDD", "agregator")]
        [TestCase("aksk4k5jDD", "")]
        public void AMICharacteristics_ParameterizedConstructorHaveValues_NotInstantiated(string device_code, string agregator_code)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                AMICharacteristics ami = new AMICharacteristics(device_code,agregator_code);
            });
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
                return new[] { new object[] { "agregator1", "SJkaleKlsk", measurement },
                               new object[] { "agregator2", "trl4k556s3", measurement }};

            }
        }


        [Test]
        [TestCaseSource("MySourceProperty")]
        public void SendDataToAgregator_ProxyReturnsAdd_DifferentMeasurements(string agrID,string devID, Dictionary<TypeMeasurement, double> measurement)
        {
            AMICharacteristics ami = new AMICharacteristics();
            Mock<IAMI_Agregator> moq1 = new Mock<IAMI_Agregator>();
            //Mock<IAMI_Agregator> moq2 = new Mock<IAMI_Agregator>();
            //mock.Setup(x => x.DoSomething(It.IsAny<string>(), It.IsAny<int>())).Returns((string x, int y) => x);
            moq1.Setup(x => x.AddDevice(It.IsAny<string>(), It.IsAny<string>())).Returns("ADDED");
            moq1.Setup(x => x.ReceiveDataFromDevice(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Dictionary<TypeMeasurement, double>>()));
            IAMI_Agregator agr1 = moq1.Object;
           // IAMI_Agregator agr2 = moq2.Object;
           //samo da nes commit
            MainWindow.AvailableAMIDevices.Add(devID, ami);
            ami.Proxy = agr1;

            ami.SendDataToAgregator(agrID, devID, measurement);

            Assert.AreNotEqual(ami.Measurements, measurement);
        }

        static IEnumerable<object[]> DuplicateMethod()
        {
            var measurement = new Dictionary<TypeMeasurement, double>()
                {
                    {TypeMeasurement.ActivePower,222 },
                    {TypeMeasurement.CurrentP,0.1 },
                    {TypeMeasurement.ReactivePower,69.49 },
                    {TypeMeasurement.Voltage,1000 }
                };

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "agregator2", "askdwskdls", measurement }, };
        }

        [Test]
        [TestCaseSource(nameof(DuplicateMethod))]
        public void SendDataToAgregator_ProxyReturnsDuplicate_DifferentDeviceCode(string agrID, string devID, Dictionary<TypeMeasurement, double> measurement)
        {
            AMICharacteristics ami = new AMICharacteristics();
            Mock<IAMI_Agregator> moq1 = new Mock<IAMI_Agregator>();
            //Mock<IAMI_Agregator> moq2 = new Mock<IAMI_Agregator>();
            //mock.Setup(x => x.DoSomething(It.IsAny<string>(), It.IsAny<int>())).Returns((string x, int y) => x);
            moq1.Setup(x => x.AddDevice(It.IsAny<string>(), It.IsAny<string>())).Returns("DUPLICATE");
            moq1.Setup(x => x.ReceiveDataFromDevice(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Dictionary<TypeMeasurement, double>>()));
            IAMI_Agregator agr1 = moq1.Object;
            // IAMI_Agregator agr2 = moq2.Object;

            MainWindow.AvailableAMIDevices.Add(devID, ami);
            ami.Proxy = agr1;

            ami.SendDataToAgregator(agrID, devID, measurement);

            Assert.AreNotEqual(ami.Device_code, devID);
        }

        [Test]
        [TestCaseSource(nameof(DuplicateMethod))]
        public void SendDataToAgregator_DeviceAdded_DifferentDeviceCode(string agrID, string devID, Dictionary<TypeMeasurement, double> measurement)
        {
            AMICharacteristics ami = new AMICharacteristics();
            Mock<IAMI_Agregator> moq1 = new Mock<IAMI_Agregator>();
            //Mock<IAMI_Agregator> moq2 = new Mock<IAMI_Agregator>();
            //mock.Setup(x => x.DoSomething(It.IsAny<string>(), It.IsAny<int>())).Returns((string x, int y) => x);
            //moq1.Setup(x => x.AddDevice(It.IsAny<string>(), It.IsAny<string>())).Returns("DUPLICATE");
            moq1.Setup(x => x.ReceiveDataFromDevice(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<Dictionary<TypeMeasurement, double>>()));
            IAMI_Agregator agr1 = moq1.Object;
            // IAMI_Agregator agr2 = moq2.Object;

            MainWindow.AvailableAMIDevices.Add(devID, ami);
            ami.Added = true;
            ami.Proxy = agr1;

            ami.SendDataToAgregator(agrID, devID, measurement);

            Assert.AreNotEqual(ami.Measurements, measurement);
        }
    }
}
