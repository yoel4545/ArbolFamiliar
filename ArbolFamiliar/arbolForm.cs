using System;
using System.Diagnostics;
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
        private Person selectedPerson = null;

        // Panel lateral y botones
        private Panel sidePanel;
        private Label infoLabel;
        private Button btnAddChild, btnAddPatner, btnAddParent, btnDelete, btnClose;

        public arbolForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.White;

            grafo = new GrafoGenealogico();

            this.Load += ArbolForm_Load;
            this.Paint += ArbolForm_Paint;
            this.MouseDown += ArbolForm_MouseDown;
            this.MouseMove += ArbolForm_MouseMove;
            this.MouseUp += ArbolForm_MouseUp;
            this.MouseWheel += ArbolForm_MouseWheel;

            InitSidePanel();
        }

        private void InitSidePanel()
        {
            sidePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            this.Controls.Add(sidePanel);

            // --- Título ---
            Label title = new Label
            {
                Text = "📋 Información",
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 10),
                Size = new Size(180, 30)
            };
            sidePanel.Controls.Add(title);

            // --- Botón de cerrar (arriba derecha) ---
            btnClose = new Button
            {
                Text = "✖",
                ForeColor = Color.White,
                BackColor = Color.IndianRed,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Size = new Size(30, 30),
                Location = new Point(sidePanel.Width - 40, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => { selectedPerson = null; UpdateInfoPanel(); Invalidate(); };
            sidePanel.Controls.Add(btnClose);

            // --- Etiqueta con la información ---
            infoLabel = new Label
            {
                Text = "Ninguna persona seleccionada",
                Font = new Font("Arial", 9),
                Location = new Point(10, 50),
                Size = new Size(sidePanel.Width - 20, 140),
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            sidePanel.Controls.Add(infoLabel);

            // --- Botones inferiores ---
            int buttonWidth = sidePanel.Width - 40;
            int buttonHeight = 40;
            int bottomY = sidePanel.Height - (buttonHeight + 10) * 4 - 20;

            btnAddChild = new Button
            {
                Text = "Agregar Hijo",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, bottomY),
                BackColor = Color.LightBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            sidePanel.Controls.Add(btnAddChild);

            btnAddParent = new Button
            {
                Text = "Agregar Padre",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, bottomY + buttonHeight + 10),
                BackColor = Color.LightBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            sidePanel.Controls.Add(btnAddParent);
            btnAddPatner = new Button
            {
                Text = "Agregar Pareja",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, bottomY + (buttonHeight + 10) * 2),
                BackColor = Color.LightBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            sidePanel.Controls.Add(btnAddPatner);

            btnDelete = new Button
            {
                Text = "Eliminar",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, bottomY + (buttonHeight + 10) * 3),
                BackColor = Color.LightCoral,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            sidePanel.Controls.Add(btnDelete);
            btnAddChild.Click += BtnAddChild_Click;
            btnAddParent.Click += BtnAddParent_Click;
            btnAddPatner.Click += BtnAddPatner_Click;
            btnDelete.Click += BtnDelete_Click;

            UpdateInfoPanel();
        }


        private void ArbolForm_Load(object sender, EventArgs e)
        {

            grafo.CalculatePositions();
            Invalidate();
        }

        private void ArbolForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(panOffset.X + sidePanel.Width, panOffset.Y);
            e.Graphics.ScaleTransform(zoom, zoom);
            grafo.DrawNodes(e.Graphics);
        }

        private void ArbolForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.X > sidePanel.Width)
            {
                selectedPerson = DetectClickedPerson(e);
                UpdateInfoPanel();
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
            zoom = e.Delta > 0 ? zoom * 1.1f : zoom / 1.1f;
            zoom = Math.Max(0.1f, Math.Min(zoom, 5.0f));

            panOffset.X = (int)(e.X - (e.X - panOffset.X) * (zoom / oldZoom));
            panOffset.Y = (int)(e.Y - (e.Y - panOffset.Y) * (zoom / oldZoom));

            Invalidate();
        }

        private Person DetectClickedPerson(MouseEventArgs e)
        {
            float worldX = (e.X - panOffset.X - sidePanel.Width) / zoom;
            float worldY = (e.Y - panOffset.Y) / zoom;

            int radius = grafo.Radius;
            foreach (var p in grafo.GetAllPersons())
            {
                float dx = worldX - (p.x + radius);
                float dy = worldY - (p.y + radius);
                if (Math.Sqrt(dx * dx + dy * dy) <= radius)
                    return p;
            }
            return null;
        }

        private void UpdateInfoPanel()
        {
            if (selectedPerson == null)
            {
                infoLabel.Text = "Ninguna persona seleccionada";
            }
            else
            {
                infoLabel.Text =
                    $"Nombre: {selectedPerson.GetName}\n" +
                    $"ID: {selectedPerson.GetId}\n" +
                    $"Nivel: {selectedPerson.GetLevel}\n" +
                    $"Hijos: {selectedPerson.Children.Count}";
            }
        }

        private Person MostrarFormularioNuevaPersona(string titulo)
        {
            using (var form = new PersonForm(titulo))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Confirmado)
                {
                    return new Person(
                        form.txtNombre.Text,
                        form.txtId.Text,
                        form.dateNacimiento.Value,
                        form.FotoPath,
                        0, 0
                    );
                }
            }
            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnAddChild_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Pressed");
            if (selectedPerson == null)
            {
                MessageBox.Show("Debe seleccionar una persona primero.");
                return;
            }

            var nuevo = MostrarFormularioNuevaPersona("Agregar hijo");
            if (nuevo != null)
            {
                grafo.AddChildren(selectedPerson, nuevo);
                grafo.CalculatePositions();
                Invalidate();
            }
        }
        private void BtnAddParent_Click(object sender, EventArgs e)
        {
            if (selectedPerson == null)
            {
                MessageBox.Show("Debe seleccionar una persona primero.");
                return;
            }
            var nuevoPadre = MostrarFormularioNuevaPersona("Agregar padre");
            if (nuevoPadre != null)
            {
                grafo.AddParent(selectedPerson, nuevoPadre);
                grafo.CalculatePositions();
                Invalidate();
            }
        }

        private void BtnAddPatner_Click(object sender, EventArgs e)
        {
            if (selectedPerson == null)
            {
                MessageBox.Show("Debe seleccionar una persona primero.");
                return;
            }

            var nuevaPareja = MostrarFormularioNuevaPersona("Agregar pareja");
            if (nuevaPareja != null)
            {
                grafo.AddPatner(selectedPerson, nuevaPareja);
                grafo.CalculatePositions();
                Invalidate();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedPerson == null)
            {
                MessageBox.Show("Debe seleccionar una persona primero.");
                return;
            }

            if (MessageBox.Show($"¿Eliminar a {selectedPerson.GetName}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                grafo.DeletePerson(selectedPerson);
                selectedPerson = null;
                grafo.CalculatePositions();
                Invalidate();
            }
        }

    }
}
