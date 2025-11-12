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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Mapa nuevoFormulario = new Mapa();
            nuevoFormulario.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            arbolForm nuevoFormulario = new arbolForm();
            nuevoFormulario.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Ayuda nuevoFormulario = new Ayuda();
            nuevoFormulario.Show();
        }
    }
}
