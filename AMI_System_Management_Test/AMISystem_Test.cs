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
    //ukoliko ima/nema vrednosti
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
            bool result = false ;
            //Mock<> moq1 = new Mock<>();
            //moq1.Setup(x => x.SendDataToSystemDataBase(It.IsAny<string>(), It.IsAny<Dictionary<string, List<DateTime>>>(), It.IsAny<Dictionary<string, Dictionary<TypeMeasurement, List<double>>>>())).Returns(true);
            //IAMI_System_Management sys = moq1.Object;

            result = sys.SendDataToSystemDataBase(agregator_code, dateTimeList, buffer);

            Assert.AreEqual(true, result);
        }
    }
}
