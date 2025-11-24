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
        private Form formAbiertoActual = null;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void AbrirFormulario(Form nuevoForm)
        {
            // Cerrar el form anterior si existe
            if (formAbiertoActual != null && !formAbiertoActual.IsDisposed)
            {
                formAbiertoActual.Close();
            }

            // Configurar nuevo form
            nuevoForm.WindowState = FormWindowState.Maximized;
            nuevoForm.FormClosed += (s, args) => {
                this.Show(); // Mostrar Form1 cuando se cierre el form hijo
                formAbiertoActual = null;
            };

            this.Hide(); // Ocultar Form1
            nuevoForm.Show();
            formAbiertoActual = nuevoForm;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (arbolForm.grafo == null || !arbolForm.grafo.GetAllPersons().Any())
            {
                MessageBox.Show("No hay personas en el árbol.");
                return;
            }

            var personas = new List<Person>(arbolForm.grafo.GetAllPersons());
            var personasConCoordenadas = personas.Where(p => p.Latitud != 0 && p.Longitud != 0).ToList();

            if (personasConCoordenadas.Count < 2)
            {
                MessageBox.Show("Debe haber al menos dos personas con coordenadas geográficas válidas para visualizar el mapa.",
                                "Coordenadas insuficientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Mapa nuevoFormulario = new Mapa(personasConCoordenadas, arbolForm.grafo);
            AbrirFormulario(nuevoFormulario);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            arbolForm nuevoFormulario = new arbolForm();
            AbrirFormulario(nuevoFormulario);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Ayuda nuevoFormulario = new Ayuda();
            AbrirFormulario(nuevoFormulario);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
                "¿Estás seguro que quieres salir de la aplicación?",
                "Confirmar salida",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }
    }
}