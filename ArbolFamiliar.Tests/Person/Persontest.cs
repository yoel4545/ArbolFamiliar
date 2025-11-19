using ArbolFamiliar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace ArbolFamiliar.Tests
{

    [TestClass]
    public class PersonTests
    {
        [TestMethod]
        public void Edad_Correcta_SiVivo()
        {
            var p = new Person("Ana", "001", new DateTime(2000, 1, 1), "");
            Assert.AreEqual(DateTime.Now.Year - 2000, p.Edad);
        }

        [TestMethod]
        public void Edad_Correcta_SiFallecido()
        {
            var p = new Person("Luis", "002", new DateTime(1950, 1, 1), "", 0, 0, new DateTime(2000, 1, 1));
            Assert.AreEqual(50, p.Edad);
        }

        [TestMethod]
        public void FechasValidas_DeberiaDetectarFechasIncorrectas()
        {
            var p = new Person("Error", "003", DateTime.Now.AddYears(1), "");
            Assert.IsFalse(p.FechasValidas());
        }
    }
}
