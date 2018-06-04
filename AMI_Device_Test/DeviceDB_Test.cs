using AMI_Device;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMI_Device_Test
{
    [TestFixture]
    public class DeviceDB_Test
    {
        [Test]
        public void LoadNotAddedDevices_DoesntContainKey_DeviceAdded()
        {
            int cnt = MainWindow.AvailableAMIDevices.Count;
            DeviceDB db = new DeviceDB();
            AMICharacteristics ami = new AMICharacteristics();
            MainWindow.AvailableAMIDevices.Add(ami.Device_code, ami);

            db.LoadNotAddedDevices();

            Assert.AreNotEqual(cnt, MainWindow.AvailableAMIDevices.Count);
        }

        [Test]
        [TestCase("kkglhllhkt")]
        public void RemoveDeviceFromDataBase_Removed(string device_code)
        {
            DeviceDB db = new DeviceDB();
            db.SaveDeviceToDataBase("agregator1", device_code);

            db.RemoveDeviceFromDataBase(device_code);

            Assert.IsNotNull(db);
        }

        [Test]
        [TestCase("agregator1","kkglhllhkt")]
        public void SaveDeviceToDataBase_Saved(string agregator_code, string device_code)
        {
            DeviceDB db = new DeviceDB();

            db.SaveDeviceToDataBase("agregator1", device_code);

            Assert.IsNotNull(db);
        }
    }
}
