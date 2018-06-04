using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using AMI_System_Management;
using NUnit.Framework;
using Storage;

namespace AMI_System_Management_Test
{

    //bufercnt >0 || =0
    [TestFixture]
    public class AMISystem_Test
    {
        static IEnumerable<object[]> first()
        {
            List<DateTime> dates = new List<DateTime>()
            {
                DateTime.Now
            };
            Dictionary<string, List<DateTime>> dateTimeList = new Dictionary<string, List<DateTime>>()
            {
                { "sjflslakge",dates}
            };
            Dictionary<TypeMeasurement, List<double>> temp = new Dictionary<TypeMeasurement, List<double>>()
            {
                {TypeMeasurement.ActivePower,new List<double>(){123} },
                {TypeMeasurement.CurrentP,new List<double>(){22} },
                {TypeMeasurement.ReactivePower,new List<double>(){342} },
                {TypeMeasurement.Voltage,new List<double>(){11.2} },
            };
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>()
            {
                {"sjflslakge", temp}
            };

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "agregator1", dateTimeList, buffer } };
        }

        [Test]
        [TestCaseSource(nameof(first))]
        public void SendDataToSystemDataBase_BufferNotEmpty_ReturnsTrue(string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
        {
            AMISystem_Management sys = new AMISystem_Management();
            bool result = false;

            result = sys.SendDataToSystemDataBase(agregator_code, dateTimeList, buffer);

            Assert.AreEqual(true, result);
        }

        static IEnumerable<object[]> second()
        {
            List<DateTime> dates = new List<DateTime>()
            {
                DateTime.Now
            };
            Dictionary<string, List<DateTime>> dateTimeList = new Dictionary<string, List<DateTime>>()
            {
                { "lhkyopdftg",dates}
            };
            
            Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer = new Dictionary<string, Dictionary<TypeMeasurement, List<double>>>();
            

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "agregator2", dateTimeList, buffer } };
        }

        [Test]
        [TestCaseSource(nameof(second))]
        public void SendDataToSystemDataBase_BufferEmpty_ReturnsFalse(string agregator_code, Dictionary<string, List<DateTime>> dateTimeList, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer)
        {
            AMISystem_Management sys = new AMISystem_Management();
            bool result = false;

            result = sys.SendDataToSystemDataBase(agregator_code, dateTimeList, buffer);

            Assert.AreEqual(false, result);
        }
        // pun/prazan
        [Test]
        public void GetAllAgregatorsFromDataBase_ThereIsAgregatorsInBase_ReturnsList()
        {
            AMISystem_Management sys = new AMISystem_Management();

            List<string> list = AMISystem_Management.SDB.GetAllDevicesFromDataBase();

            CollectionAssert.IsNotEmpty(list);
        }

        [Test]
        public void GetEarliesOrLatesttDateFromDatabase_ArgumentEarliest_ReturnsTime()
        {
            AMISystem_Management sys = new AMISystem_Management();
            DateTime time = DateTime.Now;

            DateTime newTime= AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase("EARLIEST");

            Assert.AreNotEqual(time, newTime);
        }

        [Test]
        public void GetEarliesOrLatesttDateFromDatabase_ArgumentLatest_ReturnsTime()
        {
            AMISystem_Management sys = new AMISystem_Management();
            DateTime time = DateTime.Now;

            DateTime newTime = AMISystem_Management.SDB.GetEarliesOrLatesttDateFromDatabase("LATEST");

            Assert.AreNotEqual(time, newTime);
        }

        static IEnumerable<object[]> Third()
        {

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "hWvTBHqiz6", TypeMeasurement.ActivePower.ToString(), DateTime.Now } };
        }

        [Test]
        [TestCaseSource(nameof(Third))] //treba da ide notEmpty
        public void GetDatesAndValuesFromDataBase(string device_code, string typeMeasurment, DateTime selectedDate)
        {
            //Dictionary<DateTime, double> empty = new Dictionary<DateTime, double>();

            Dictionary<DateTime, double> retVal = AMISystem_Management.SDB.GetDatesAndValuesFromDataBase(device_code, typeMeasurment, selectedDate);

            CollectionAssert.IsEmpty(retVal);
        }

        static IEnumerable<object[]> Fourth()
        {

            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "ffkkggttyu", TypeMeasurement.ActivePower.ToString(), DateTime.Now,1 } };
        }

        [Test]
        [TestCaseSource(nameof(Fourth))] //treba da ide notEmpty
        public void GetValuesFromDatabase(string code, string typeMeasurment, DateTime selectedDate, out int devicesCount)
        {
            //List<Tuple<DateTime, double>> empty = new List<Tuple<DateTime, double>>();
            devicesCount = 1;

            List<Tuple<DateTime, double>> retValue = AMISystem_Management.SDB.GetValuesFromDatabase(code, typeMeasurment, selectedDate, out devicesCount);

            CollectionAssert.IsEmpty(retValue);
        }

        static IEnumerable<object[]> Fifth()
        {
            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "cnnvmmmgky", DateTime.Now } };
        }

        [Test]
        [TestCaseSource(nameof(Fifth))] //treba da ide notEmpty
        public void GetDatesAndValuesFromDataBase(string device_code, DateTime selectedDate)
        {
            Dictionary<DateTime, List<double>> retVal = AMISystem_Management.SDB.GetDatesAndValuesFromDataBase(device_code, selectedDate);

            CollectionAssert.IsEmpty(retVal);
        }
    }
}
