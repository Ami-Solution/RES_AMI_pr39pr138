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
        [TestCase("agregator2","aksjfkelds")]
        public void AddDevice_StatusOnNotDuplicate_ReturnsAdded(string agregator_code, string device_code)
        {
            
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code,agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;

            string ret=agr.AddDevice(agr.Agregator_code, device_code);

            Assert.AreEqual("ADDED", ret);
        }

        [Test]
        [TestCase("agregator3","skfjturiii")]
        public void AddDevice_StatusOnDuplicate_ReturnsDuplicate(string agregator_code, string device_code)
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
        [TestCase("agregator4", "skfjturiii")]
        public void AddDevice_StatusOff_ReturnsOff(string agregator_code, string device_code)
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
            return new[] { new object[] { "agregator5", DateTime.Now, "SJkaleKlsk", measurement } };
        }

        [Test]
        [TestCaseSource(nameof(returnsOddProperty))]
        public void ReceiveDataFromDevice_AgregatorExistStatusOn_BufferChange(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
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
            return new[] { new object[] { "agregator6", DateTime.Now, "SJkaleKlsk", measurement } };
        }

        [Test]
        [TestCaseSource(nameof(returnsSameProperty))]
        public void ReceiveDataFromDevice_AgregatorExistStatusOff_BufferDoesntChange(string agregator_code, DateTime dateTime, string device_code, Dictionary<TypeMeasurement, double> values)
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
        public void SendToSystem_MenagementDatesOver0_EmptyAtEnd()
        { //string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer
            AMIAgregator agr = new AMIAgregator("agregator7");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;
            agr.Dates.Add("aassffggaewa", new List<DateTime>());
            Mock<IAMI_System_Management> moq1 = new Mock<IAMI_System_Management>();
            moq1.Setup(x => x.SendDataToSystemDataBase(It.IsAny<string>(), It.IsAny<Dictionary<string, List<DateTime>>>(),It.IsAny<Dictionary<string, Dictionary<TypeMeasurement, List<double>>>>())).Returns(true);
            IAMI_System_Management sys = moq1.Object;

            MainWindow.agregators["agregator7"].Buffer.Add(
                        "aassffggaewa",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });

            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> oldBuffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            oldBuffer.Add(
                        "aassffggaewa",
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });
            MainWindow.agregators["agregator7"].Buffer["aassffggaewa"][TypeMeasurement.ActivePower].Add(233);
            MainWindow.agregators["agregator7"].Buffer["aassffggaewa"][TypeMeasurement.CurrentP].Add(123);
            MainWindow.agregators["agregator7"].Buffer["aassffggaewa"][TypeMeasurement.ReactivePower].Add(56);
            MainWindow.agregators["agregator7"].Buffer["aassffggaewa"][TypeMeasurement.Voltage].Add(12.12);

            agr.Proxy = sys;

            Task t = Task.Factory.StartNew(() =>
            {
                agr.SendToSystemMenagement(agr);

            });
            Thread.Sleep(8000);
            
            Assert.AreEqual(oldBuffer, agr.Buffer);
        }

        [Test]
        public void SendToSystemMenagement_DatesNotOver0_SameAsOnStart()
        { //string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer
            AMIAgregator agr = new AMIAgregator("agregator8");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.ON;
           // agr.Dates.Add("skgjtifodls", new List<DateTime>());
            Mock<IAMI_System_Management> moq1 = new Mock<IAMI_System_Management>();
            moq1.Setup(x => x.SendDataToSystemDataBase(It.IsAny<string>(), It.IsAny<Dictionary<string, List<DateTime>>>(), It.IsAny<Dictionary<string, Dictionary<TypeMeasurement, List<double>>>>())).Returns(true);
            IAMI_System_Management sys = moq1.Object;

            MainWindow.agregators["agregator8"].Buffer.Add(
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
            MainWindow.agregators["agregator8"].Buffer["skgjtifodls"][TypeMeasurement.ActivePower].Add(233);
            MainWindow.agregators["agregator8"].Buffer["skgjtifodls"][TypeMeasurement.CurrentP].Add(123);
            MainWindow.agregators["agregator8"].Buffer["skgjtifodls"][TypeMeasurement.ReactivePower].Add(56);
            MainWindow.agregators["agregator8"].Buffer["skgjtifodls"][TypeMeasurement.Voltage].Add(12.12);

            agr.Proxy = sys;

            Task t = Task.Factory.StartNew(() =>
            {
                agr.SendToSystemMenagement(agr);

            });
            Thread.Sleep(8000);

            Assert.AreNotEqual(oldBuffer, agr.Buffer);
        }

        [Test]
        public void SendToSystemMenagement_OffState_SameAsOnStart()
        { //string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer
            AMIAgregator agr = new AMIAgregator("agregator9");
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            agr.State = State.OFF;

            MainWindow.agregators["agregator9"].Buffer.Add(
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
            MainWindow.agregators["agregator9"].Buffer["skgjtifodls"][TypeMeasurement.ActivePower].Add(233);
            MainWindow.agregators["agregator9"].Buffer["skgjtifodls"][TypeMeasurement.CurrentP].Add(123);
            MainWindow.agregators["agregator9"].Buffer["skgjtifodls"][TypeMeasurement.ReactivePower].Add(56);
            MainWindow.agregators["agregator9"].Buffer["skgjtifodls"][TypeMeasurement.Voltage].Add(12.12);

            Assert.AreNotEqual(oldBuffer, agr.Buffer);
        }

        [Test]
        public void AgregatorsAndTheirDevices_PossessAgregator_ReturnsFullDict()
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
        public void AgregatorsAndTheirDevices_DoesntPossessAgregator_ReturnsEmptyDict()
        {
            MainWindow.agregators.Clear();
            AMIAgregator agr = new AMIAgregator("agregator79");
            MainWindow.agregatorNumber++;
            Dictionary<string, List<string>> emptyDic = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> fullDic = new Dictionary<string, List<string>>();

            fullDic = agr.AgregatorsAndTheirDevices();

            Assert.AreEqual(emptyDic, fullDic);
        }

        [Test]
        [TestCase("agregator10","lgktpyofkk")]
        public void RemoveDevice(string agregator_code, string device_code)
        {
            AMIAgregator agr = new AMIAgregator(agregator_code);
            MainWindow.agregators.Add(agr.Agregator_code, agr);
            MainWindow.agregatorNumber++;
            Mock<IAMI_System_Management> moq1 = new Mock<IAMI_System_Management>();
            moq1.Setup(x => x.SendDataToSystemDataBase(It.IsAny<string>(), It.IsAny<Dictionary<string, List<DateTime>>>(), It.IsAny<Dictionary<string, Dictionary<TypeMeasurement, List<double>>>>())).Returns(true);
            IAMI_System_Management sys = moq1.Object;
            MainWindow.agregators[agregator_code].Buffer.Add(
                        device_code,
                        new Dictionary<TypeMeasurement, List<double>>()
                        {
                        { TypeMeasurement.ActivePower, new List<double>() },
                        { TypeMeasurement.ReactivePower, new List<double>() },
                        { TypeMeasurement.CurrentP, new List<double>() },
                        { TypeMeasurement.Voltage, new List<double>() },
                        });

            string ret= agr.RemoveDevice(agregator_code, device_code);

            Assert.AreEqual("DELETED", ret);
        }
    }
}
