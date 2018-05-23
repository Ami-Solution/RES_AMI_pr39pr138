using AMI_Agregator;
using NUnit.Framework;
using System;


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
    }
}
