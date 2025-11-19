using ArbolFamiliar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ArbolFamiliar.Tests
{
    [TestClass]
    public class GrafoGenealogicoTests
    {
        [TestMethod]
        public void AddChildren_DeberiaAgregarHijoYPadre()
        {
            var padre = new Person("Padre", "004", new DateTime(1980, 1, 1), "");
            var hijo = new Person("Hijo", "005", new DateTime(2010, 1, 1), "");
            var grafo = new GrafoGenealogico();

            grafo.AddChildren(padre, hijo);

            Assert.IsTrue(padre.Children.Contains(hijo));
            Assert.IsTrue(hijo.Parents.Contains(padre));
        }

        [TestMethod]
        public void AddPartner_DeberiaEstablecerRelacionBidireccional()
        {
            var a = new Person("A", "006", new DateTime(1980, 1, 1), "");
            var b = new Person("B", "007", new DateTime(1982, 1, 1), "");
            var grafo = new GrafoGenealogico();

            grafo.AddPatner(a, b);

            Assert.AreEqual(b, a.Partner);
            Assert.AreEqual(a, b.Partner);
        }

        [TestMethod]
        public void DeletePerson_SinPadresNiPareja_EliminaComponente()
        {
            var p = new Person("Solo", "008", new DateTime(1990, 1, 1), "");
            var grafo = new GrafoGenealogico();
            grafo.AddPerson(p);

            grafo.DeletePerson(p);

            Assert.IsFalse(grafo.GetAllPersons().Contains(p));
        }
    }
}
