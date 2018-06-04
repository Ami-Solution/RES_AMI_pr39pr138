using AMY_System_Management;
using NUnit.Framework;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMI_System_Management_Test
{
    [TestFixture]
    public class SystemDB_Test
    {
        [Test]
        public void GetAllAgregatorsFromDataBase()
        {
            SystemDB db = new SystemDB();

            List<string> list = db.GetAllAgregatorsFromDataBase();

            CollectionAssert.IsNotEmpty(list);
        }

        [Test]
        public void GetAllDevicesFromDataBase()
        {
            SystemDB db = new SystemDB();

            List<string> list = db.GetAllDevicesFromDataBase();

            CollectionAssert.IsNotEmpty(list);
        }

        static IEnumerable<object[]> first()
        {
            return new[] { new object[] { "hWvTBHqiz6", TypeMeasurement.ActivePower.ToString(), DateTime.Now } };
        }

        [Test]
        [TestCaseSource(nameof(first))]
        public void GetDatesAndValuesFromDataBase(string device_code, string typeMeasurment, DateTime selectedDate)
        {
            SystemDB db = new SystemDB();

            Dictionary<DateTime, double> list = db.GetDatesAndValuesFromDataBase(device_code,typeMeasurment,selectedDate);

            CollectionAssert.IsEmpty(list);
        }

        static IEnumerable<object[]> second()
        {
            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "cnnvmmmgky", DateTime.Now } };
        }

        [Test]
        [TestCaseSource(nameof(second))]
        public void GetDatesAndValuesFromDataBase(string device_code, DateTime selectedDate)
        {
            SystemDB db = new SystemDB();

            Dictionary<DateTime, List<double>> retVal = db.GetDatesAndValuesFromDataBase(device_code, selectedDate);

            CollectionAssert.IsEmpty(retVal);
        }

        [Test]
        public void GetEarliesOrLatesttDateFromDatabase()
        {
            SystemDB db = new SystemDB();
            DateTime time = DateTime.Now;

            DateTime newTime = db.GetEarliesOrLatesttDateFromDatabase("EARLIEST");

            Assert.AreNotEqual(time, newTime);
        }

        static IEnumerable<object[]> third()
        {
            // The order of element in the object my be the same expected by your test method
            return new[] { new object[] { "cnnvmmmgky", TypeMeasurement.ActivePower.ToString(), DateTime.Now ,1} };
        }

        [Test]
        [TestCaseSource(nameof(third))]
        public void GetValuesFromDatabase(string code, string typeMeasurment, DateTime selectedDate, out int devicesCount)
        {
            SystemDB db = new SystemDB();

            List<Tuple<DateTime, double>> retVal = db.GetValuesFromDatabase(code, typeMeasurment, selectedDate,out devicesCount);

            CollectionAssert.IsEmpty(retVal);
        }

        
    }
}
