using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ArbolFamiliar;

namespace ArbolFamiliar.Tests
{
    [TestClass]
    public class MapaFiltroTests
    {
        [TestMethod]
        public void PersonasConCoordenadas_DeberiaFiltrarCorrectamente()
        {
            var p1 = new Person("Con coordenadas", "001", DateTime.Now, "", 9.93, -84.08);
            var p2 = new Person("Sin coordenadas", "002", DateTime.Now, "", 0, 0);
            var p3 = new Person("Solo latitud", "003", DateTime.Now, "", 10.0, 0);
            var p4 = new Person("Solo longitud", "004", DateTime.Now, "", 0, -84.0);

            var personas = new List<Person> { p1, p2, p3, p4 };

            var conCoordenadas = personas.Where(p => p.Latitud != 0 && p.Longitud != 0).ToList();

            Assert.AreEqual(1, conCoordenadas.Count);
            Assert.AreEqual("Con coordenadas", conCoordenadas[0].GetName);
        }
    }
}
