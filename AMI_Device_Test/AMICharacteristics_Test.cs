using System;
using System.Collections.Generic;
using AMI_Device;
using NUnit.Framework;
using Storage;

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
        }
    }
}
