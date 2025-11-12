using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ArbolFamiliar;

namespace ArbolFamiliar
{
    public partial class arbolForm : Form
    {
        private Point panOffset = new Point(0, 0);
        private Point lastMouse;
        private bool dragging = false;
        private GrafoGenealogico grafo;
        private float zoom = 1.0f;
        private Person selectedPerson = null;
        private List<Button> uiButtons = new List<Button>();

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
            var madre = new Person("Laura", "006", new DateTime(1940, 6, 5), "ejemplo", 0, 0);
            var pareja = new Person("Jean", "006", new DateTime(1940, 6, 5), "ejemplo", 0, 0);
            var hermano = new Person("julio", "006", new DateTime(1940, 6, 5), "ejemplo", 0, 0);

            grafo.AddPerson(fundador);
            grafo.AddChildren(fundador, hijo1);
            grafo.AddChildren(fundador, hija2);
            grafo.AddFather(fundador, padre);
            grafo.AddChildren(hijo1, madre);
            grafo.AddPatner(madre, pareja);
            

            grafo.CalculatePositions();
            this.Invalidate();
        }

        private void ArbolForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(panOffset.X, panOffset.Y);
            e.Graphics.ScaleTransform(zoom, zoom);
            grafo.DrawNodes(e.Graphics);
            e.Graphics.ResetTransform();
            DrawUI(e.Graphics);
        }

        private void ArbolForm_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var btn in uiButtons)
            {
                if (btn.Contains(e.Location))
                {
                    btn.Click();
                    return;
                }
            }
            if (e.Button == MouseButtons.Left)
            {
                selectedPerson = DetectClickedPerson(e);
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

        private void arbolForm_Load_1(object sender, EventArgs e)
        {

        }

        private Person DetectClickedPerson(MouseEventArgs e)
        {
            // Convertir coordenadas de pantalla a coordenadas del grafo (si hay zoom o pan)
            float worldX = (e.X - panOffset.X) / zoom;
            float worldY = (e.Y - panOffset.Y) / zoom;

            // Radio de los nodos (ajusta si usas otro tamaño)
            int radius = grafo.Radius; // o usa una constante si lo tienes fijo

            // Buscar si se hizo clic dentro de algún nodo
            foreach (var p in grafo.GetAllPersons())
            {
                float dx = worldX - (p.x + radius);
                float dy = worldY - (p.y + radius);
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (dist <= radius)
                {
                    Invalidate(); // Redibujar para resaltar el nodo
                    return p;
                }
            }

            // Si no se hizo clic en ningún nodo
            return null;
        }

        public void DrawUI(Graphics g)
        {
            int panelWidth = 250;
            int panelHeight = 400;
            int margin = 10;

            Rectangle panelRect = new Rectangle(margin, margin, panelWidth, panelHeight);
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                g.FillRectangle(bgBrush, panelRect);
            using (Pen borderPen = new Pen(Color.Gray, 2))
                g.DrawRectangle(borderPen, panelRect);

            int closeSize = 25;
            Rectangle closeRect = new Rectangle(margin + panelWidth - closeSize - 5, margin + 5, closeSize, closeSize);

            if (uiButtons.Count == 0)
            {
                uiButtons.Add(new Button(closeRect, "X", () =>
                {
                    selectedPerson = null;
                    Invalidate();
                })
                {
                    BackgroundColor = Color.FromArgb(220, 100, 100),
                    BorderColor = Color.DarkRed,
                    TextColor = Color.White,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                });

                int buttonY = panelRect.Bottom - 150;
                int buttonHeight = 35;
                int buttonSpacing = 15;
                int buttonX = margin + 15;
                int buttonWidth = panelWidth - 40;

                uiButtons.Add(new Button(new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight), "Agregar Hijo", () =>
                {
                    Debug.WriteLine("Agregar hijo presionado");
                }));

                buttonY += buttonHeight + buttonSpacing;
                uiButtons.Add(new Button(new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight), "Agregar Pareja", () =>
                {
                    Debug.WriteLine("Agregar pareja presionado");
                }));

                buttonY += buttonHeight + buttonSpacing;
                uiButtons.Add(new Button(new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight), "Eliminar", () =>
                {
                    Debug.WriteLine("Eliminar presionado");
                }));
            }

            Font titleFont = new Font("Arial", 12, FontStyle.Bold);
            Font normalFont = new Font("Arial", 10);
            Brush textBrush = Brushes.Black;

            int textX = margin + 15;
            int textY = margin + 15;
            g.DrawString("📋 Información", titleFont, textBrush, textX, textY);
            textY += 35;

            if (selectedPerson != null)
            {
                g.DrawString($"Nombre: {selectedPerson.GetName}", normalFont, textBrush, textX, textY);
                textY += 25;
                g.DrawString($"ID: {selectedPerson.GetId}", normalFont, textBrush, textX, textY);
                textY += 25;
                g.DrawString($"Nivel: {selectedPerson.GetLevel}", normalFont, textBrush, textX, textY);
                textY += 25;
                g.DrawString($"Hijos: {selectedPerson.Children.Count}", normalFont, textBrush, textX, textY);
            }
            else
            {
                g.DrawString("Ninguna persona seleccionada", normalFont, textBrush, textX, textY);
            }

            foreach (var btn in uiButtons)
                btn.Draw(g);
        }


    }
}
