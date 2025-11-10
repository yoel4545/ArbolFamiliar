using ArbolFamiliar;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class arbolForm : Form
    {
        private Point panOffset = new Point(0, 0);
        private Point lastMouse;
        private bool dragging = false;
        private GrafoGenealogico grafo;
        private float zoom = 1.0f;

        public arbolForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.White;

            grafo = new GrafoGenealogico();

            this.Load += ArbolForm_Load;
            this.MouseDown += ArbolForm_MouseDown;
            this.MouseMove += ArbolForm_MouseMove;
            this.MouseUp += ArbolForm_MouseUp;
            this.MouseWheel += ArbolForm_MouseWheel;
            this.Paint += ArbolForm_Paint;
        }

        private void ArbolForm_Load(object sender, EventArgs e)
        {
            var fundador = new Person("Juan", "001", new DateTime(1950, 1, 1), "ejemplo", 0, 0);
            var hijo1 = new Person("Carlos", "002", new DateTime(1980, 3, 12), "ejemplo", 0, 0);
            var hija2 = new Person("Ana", "003", new DateTime(1982, 6, 5), "ejemplo", 0, 0);
            var padre = new Person("Pablo", "005", new DateTime(1940, 6, 5), "ejemplo", 0, 0);

            grafo.AddPerson(fundador);
            grafo.AddChildren(fundador, hijo1);
            grafo.AddChildren(fundador, hija2);
            grafo.AddFather(fundador, padre);

            grafo.CalculatePositions();
            this.Invalidate();
        }

        private void ArbolForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(panOffset.X, panOffset.Y);
            e.Graphics.ScaleTransform(zoom, zoom);
            grafo.Draw(e.Graphics);
        }

        private void ArbolForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                lastMouse = e.Location;
            }
        }

        private void ArbolForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                panOffset.X += e.X - lastMouse.X;
                panOffset.Y += e.Y - lastMouse.Y;
                lastMouse = e.Location;
                Invalidate();
            }
        }

        private void ArbolForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                dragging = false;
        }

        private void ArbolForm_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoom;

            if (e.Delta > 0)
                zoom *= 1.1f;
            else
                zoom /= 1.1f;

            zoom = Math.Max(0.1f, Math.Min(zoom, 5.0f));


            panOffset.X = (int)(e.X - (e.X - panOffset.X) * (zoom / oldZoom));
            panOffset.Y = (int)(e.Y - (e.Y - panOffset.Y) * (zoom / oldZoom));

            Invalidate();
        }
    }
}
