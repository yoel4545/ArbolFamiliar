using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class arbolForm : Form
    {
        private GrafoGenealogico grafo;
        public arbolForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            grafo = new GrafoGenealogico();
            this.Load += ArbolForm_Load;
            this.Paint += ArbolForm_Paint;
        }

        private void ArbolForm_Load(object sender, EventArgs e)
        {
            var fundador = new Persona("Juan", "001", new DateTime(1950, 1, 1), "ejemplo", 9.93, -84.08);
            var hijo1 = new Persona("Carlos", "002", new DateTime(1980, 3, 12), "ejemplo", 9.93, -84.09);
            var hija2 = new Persona("Ana", "003", new DateTime(1982, 6, 5), "ejemplo", 9.94, -84.07);
            var padre = new Persona("Pablo", "005", new DateTime(1940, 6, 5), "ejemplo", 9.92, -84.10);
            grafo.AddPersona(fundador);
            grafo.AddChildren(fundador, hijo1);
            grafo.AddChildren(fundador, hija2);
            grafo.AddFather(fundador, padre);
            grafo.CalculatePositions();
            this.Invalidate();
        }

        private void ArbolForm_Paint(object sender, PaintEventArgs e)
        {
            grafo.Draw(e.Graphics);
        }
    }
}
