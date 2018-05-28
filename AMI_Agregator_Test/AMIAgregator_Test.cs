using AMI_Agregator;
using Common;
using Moq;
using NUnit.Framework;
using Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

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
        [TestCase("agregator1233","aksjfkelds")]
        public void AMIAgregator_AddDeviceStatusOnNotDuplicate_ReturnsAdded(string agregator_code, string device_code)
        {
            
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code,agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;

            string ret=agr.AddDevice(agr.Agregator_code, device_code);

            Assert.AreEqual("ADDED", ret);
        }

        [Test]
        [TestCase("agregator134","skfjturiii")]
        public void AMIAgregator_AddDeviceStatusOnDuplicate_ReturnsDuplicate(string agregator_code, string device_code)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;
            agr.AddDevice(agr.Agregator_code, device_code);

            string ret = agr.AddDevice(agr.Agregator_code, device_code);

            Assert.AreEqual("DUPLICATE", ret);
        }

        [Test]
        [TestCase("agregator22", "skfjturiii")]
        public void AMIAgregator_AddDeviceStatusOff_ReturnsOff(string agregator_code, string device_code)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
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
            return new[] { new object[] { "agregator144", DateTime.Now, "SJkaleKlsk", measurement } };
        }

        [Test]
        [TestCaseSource(nameof(returnsOddProperty))]
        public void AMIAgregator_ReceiveDataFromDeviceAgregatorExistStatusOn_BufferChange(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
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
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            oldBuffer.Add(
                        device_code,
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            //ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            //// You may want to map to your own exe.config file here.
            //fileMap.ExeConfigFilename = @"C:\Users\Serlok\source\repos\RES_AMI_pr39pr138\AMI_Agregator\App.config";
            //// You can add here LocalUserConfigFilename, MachineConfigFilename and RoamingUserConfigFilename, too
            //System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            agr.ReceiveDataFromDevice(agregator_code, dateTime, device_code, values); 

            Assert.AreNotEqual(agr.Buffer, oldBuffer);

        }

        static IEnumerable<object[]> returnsSameProperty()
        {
            var measurement = new Dictionary<TypeMeasurement, double>()
                {
                    {TypeMeasurement.ActivePower,222 },
                    {TypeMeasurement.CurrentP,0.1 },
                    {TypeMeasurement.ReactivePower,69.49 },
                    {TypeMeasurement.Voltage,1000 }
                };

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "agregator454", DateTime.Now, "SJkaleKlsk", measurement } };
        }

        [Test]
        [TestCaseSource(nameof(returnsSameProperty))]
        public void AMIAgregator_ReceiveDataFromDeviceAgregatorExistStatusOff_BufferDoesntChange(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.OFF;
            MainWindow.agregators[agregator_code].Buffer.Add(
                        device_code,
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            oldBuffer.Add(
                        device_code,
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });

            agr.ReceiveDataFromDevice(agregator_code, dateTime, device_code, values); 

            Assert.AreEqual(agr.Buffer, oldBuffer);
        }

        [Test]
        public void AMIAgregator_SendToSystemMenagementDatesOver0_EmptyAtEnd()
        { //string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer
            AMIAgregator agr = new AMIAgregator("agregator3434");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;
            agr.Dates.Add("skgjtifodls", new List<DateTime>());
            Mock<IAMI_System_Management> moq1 = new Mock<IAMI_System_Management>();
            moq1.Setup(x => x.SendDataToSystemDataBase(It.IsAny<string>(), It.IsAny<Dictionary<string, List<DateTime>>>(),It.IsAny<Dictionary<string, Dictionary<TypeMeasurement, List<double>>>>())).Returns(true);
            IAMI_System_Management sys = moq1.Object;

            MainWindow.agregators["agregator3434"].Buffer.Add(
                        "skgjtifodls",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            oldBuffer.Add(
                        "skgjtifodls",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            MainWindow.agregators["agregator3434"].Buffer["skgjtifodls"][TypeMeasurement.ActivePower].Add(233);
            MainWindow.agregators["agregator3434"].Buffer["skgjtifodls"][TypeMeasurement.CurrentP].Add(123);
            MainWindow.agregators["agregator3434"].Buffer["skgjtifodls"][TypeMeasurement.ReactivePower].Add(56);
            MainWindow.agregators["agregator3434"].Buffer["skgjtifodls"][TypeMeasurement.Voltage].Add(12.12);

            agr.Proxy = sys;

            Task t = Task.Factory.StartNew(() =>
            {
                agr.SendToSystemMenagement(agr);

            });
            Thread.Sleep(8000);
            
            Assert.AreEqual(oldBuffer, agr.Buffer);
        }

        [Test]
        public void AMIAgregator_SendToSystemMenagementDatesNotOver0_SameAsOnStart()
        { //string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer
            AMIAgregator agr = new AMIAgregator("agregator334");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;
           // agr.Dates.Add("skgjtifodls", new List<DateTime>());
            Mock<IAMI_System_Management> moq1 = new Mock<IAMI_System_Management>();
            moq1.Setup(x => x.SendDataToSystemDataBase(It.IsAny<string>(), It.IsAny<Dictionary<string, List<DateTime>>>(), It.IsAny<Dictionary<string, Dictionary<TypeMeasurement, List<double>>>>())).Returns(true);
            IAMI_System_Management sys = moq1.Object;

            MainWindow.agregators["agregator334"].Buffer.Add(
                        "skgjtifodls",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            oldBuffer.Add(
                        "skgjtifodls",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            MainWindow.agregators["agregator334"].Buffer["skgjtifodls"][TypeMeasurement.ActivePower].Add(233);
            MainWindow.agregators["agregator334"].Buffer["skgjtifodls"][TypeMeasurement.CurrentP].Add(123);
            MainWindow.agregators["agregator334"].Buffer["skgjtifodls"][TypeMeasurement.ReactivePower].Add(56);
            MainWindow.agregators["agregator334"].Buffer["skgjtifodls"][TypeMeasurement.Voltage].Add(12.12);

            agr.Proxy = sys;

            Task t = Task.Factory.StartNew(() =>
            {
                agr.SendToSystemMenagement(agr);

            });
            Thread.Sleep(8000);

            Assert.AreNotEqual(oldBuffer, agr.Buffer);
        }

        [Test]
        public void AMIAgregator_SendToSystemMenagementOffState_SameAsOnStart()
        { //string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer
            AMIAgregator agr = new AMIAgregator("agregator88");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.OFF;

            MainWindow.agregators["agregator88"].Buffer.Add(
                        "skgjtifodls",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            oldBuffer.Add(
                        "skgjtifodls",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            MainWindow.agregators["agregator88"].Buffer["skgjtifodls"][TypeMeasurement.ActivePower].Add(233);
            MainWindow.agregators["agregator88"].Buffer["skgjtifodls"][TypeMeasurement.CurrentP].Add(123);
            MainWindow.agregators["agregator88"].Buffer["skgjtifodls"][TypeMeasurement.ReactivePower].Add(56);
            MainWindow.agregators["agregator88"].Buffer["skgjtifodls"][TypeMeasurement.Voltage].Add(12.12);

            Assert.AreNotEqual(oldBuffer, agr.Buffer);
        }

        [Test]
        public void AMIAgregator_AgregatorsAndTheirDevicesPossessAgregatpr_ReturnsFullDict()
        {
            AMIAgregator agr = new AMIAgregator("agregator69");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            Dictionary<string, List<string>> emptyDic = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> fullDic = new Dictionary<string, List<string>>();

            fullDic= agr.AgregatorsAndTheirDevices();

            Assert.AreNotEqual(emptyDic, fullDic);
        }

        [Test]
        public void AMIAgregator_AgregatorsAndTheirDevicesDoesntPossessAgregatpr_ReturnsEmptyDict()
        {
            AMIAgregator agr = new AMIAgregator("agregator69");
            Dictionary<string, List<string>> emptyDic = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> fullDic = new Dictionary<string, List<string>>();

            fullDic = agr.AgregatorsAndTheirDevices();

            Assert.AreEqual(emptyDic, fullDic);
        }
    }
}
