using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ArbolFamiliar;

namespace ArbolFamiliar.Tests
{
    [TestClass]
    public class MapaTests
    {
        [TestMethod]
        public void Distancia_DeberiaSerCero_SiMismaCoordenada()
        {
            var p1 = new Person("A", "001", DateTime.Now, "", 9.93, -84.08);
            var p2 = new Person("B", "002", DateTime.Now, "", 9.93, -84.08);

            var grafo = new GrafoGeografico(new List<Person> { p1, p2 });

            double distancia = grafo.GetDistancias()[p1][p2];

            Assert.AreEqual(0, distancia, 0.001);
        }

        [TestMethod]
        public void ObtenerParMasLejano_DeberiaRetornarCorrecto()
        {
            var p1 = new Person("A", "001", DateTime.Now, "", 9.93, -84.08);
            var p2 = new Person("B", "002", DateTime.Now, "", 10.0, -84.2);

            var grafo = new GrafoGeografico(new List<Person> { p1, p2 });
            var (a, b, dist) = grafo.ObtenerParMasLejano();

            Assert.AreEqual(p1, a);
            Assert.AreEqual(p2, b);
            Assert.IsTrue(dist > 0);
        }
        [TestMethod]
        public void CalcularDistanciaPromedio_DeberiaSerMayorQueCero()
        {
            var p1 = new Person("A", "013", DateTime.Now, "", 9.93, -84.08);
            var p2 = new Person("B", "014", DateTime.Now, "", 10.0, -84.2);
            var grafo = new GrafoGeografico(new List<Person> { p1, p2 });

            double promedio = grafo.CalcularDistanciaPromedio();

            Assert.IsTrue(promedio > 0);
        }
    }
}
