using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class arbolForm : Form
    {
        private Point panOffset = new Point(0, 0);
        private Point lastMouse;
        private bool dragging = false;
        public static GrafoGenealogico grafo;
        private float zoom = 1.0f;
        private Person selectedPerson = null;

        // Panel lateral y botones
        private Panel sidePanel;
        private Label infoLabel;
        private PictureBox profilePictureBox;
        private Button btnAddChild, btnAddPatner, btnAddParent, btnDelete, btnChangeInfo;
        private Button btnBack;

        public arbolForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.BackColor = Color.White;

            if (grafo == null)
            {
                grafo = new GrafoGenealogico();
            }

            this.Load += ArbolForm_Load;
            this.Paint += ArbolForm_Paint;
            this.MouseDown += ArbolForm_MouseDown;
            this.MouseMove += ArbolForm_MouseMove;
            this.MouseUp += ArbolForm_MouseUp;
            this.MouseWheel += ArbolForm_MouseWheel;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            InitSidePanel();
        }

        private void InitSidePanel()
        {
            sidePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.FromArgb(250, 250, 250)
            };
            this.Controls.Add(sidePanel);

            // --- Botón "Regresar" (estándar, arriba izquierda) ---
            btnBack = new Button
            {
                Text = "← Regresar",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 30),
                Location = new Point(10, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.BackColor = Color.Transparent;
            btnBack.Click += (s, e) => { this.Close(); };
            sidePanel.Controls.Add(btnBack);
            btnBack.BringToFront();

            // --- Título ---
            Label title = new Label
            {
                Text = "Información",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 50),
                Size = new Size(220, 30)
            };
            sidePanel.Controls.Add(title);

            // --- Foto de perfil (primero) ---
            profilePictureBox = new PictureBox
            {
                Size = new Size(140, 140),
                Location = new Point(10, 90),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            sidePanel.Controls.Add(profilePictureBox);
            profilePictureBox.Left = (sidePanel.Width - profilePictureBox.Width) / 2;

            // --- Etiqueta con la información (debajo de la foto) ---
            infoLabel = new Label
            {
                Text = "Ninguna persona seleccionada",
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, 240),
                Size = new Size(sidePanel.Width - 20, 60),
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            sidePanel.Controls.Add(infoLabel);

            // --- Botones inferiores estilo "Google Forms" simplificado ---
            int buttonWidth = sidePanel.Width - 40;
            int buttonHeight = 40;
            int spacing = 10;

            int startY = 356; // justo debajo de btnChangeInfo

            btnAddChild = new Button
            {
                Text = "Agregar Hijo",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, startY + (buttonHeight + spacing) * 0),
                BackColor = Color.FromArgb(66, 133, 244), // azul Google
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAddChild.FlatAppearance.BorderSize = 0;
            sidePanel.Controls.Add(btnAddChild);
            MakeRoundedButton(btnAddChild, 8);

            btnChangeInfo = new Button
            {
                Text = "Cambiar Información",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, startY + (buttonHeight + spacing) * -1),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            }; 
            btnChangeInfo.FlatAppearance.BorderSize = 0;
            sidePanel.Controls.Add(btnChangeInfo);
            MakeRoundedButton(btnChangeInfo, 8);

            btnAddParent = new Button
            {
                Text = "Agregar Padre",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, startY + (buttonHeight + spacing) * 1),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAddParent.FlatAppearance.BorderSize = 0;
            sidePanel.Controls.Add(btnAddParent);
            MakeRoundedButton(btnAddParent, 8);

            btnAddPatner = new Button
            {
                Text = "Agregar Pareja",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, startY + (buttonHeight + spacing) * 2),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnAddPatner.FlatAppearance.BorderSize = 0;
            sidePanel.Controls.Add(btnAddPatner);
            MakeRoundedButton(btnAddPatner, 8);

            btnDelete = new Button
            {
                Text = "Eliminar",
                Size = new Size(buttonWidth, buttonHeight),
                Location = new Point(20, startY + (buttonHeight + spacing) * 3),
                BackColor = Color.FromArgb(220, 75, 75), // rojo suave
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            sidePanel.Controls.Add(btnDelete);
            MakeRoundedButton(btnDelete, 8);

            // Asignar handlers
            btnChangeInfo.Click += BtnChangeInfo_Click;
            btnAddChild.Click += BtnAddChild_Click;
            btnAddParent.Click += BtnAddParent_Click;
            btnAddPatner.Click += BtnAddPatner_Click;
            btnDelete.Click += BtnDelete_Click;

            UpdateInfoPanel();
        }

        // Helper para redondear botones
        private void MakeRoundedButton(Button b, int radius)
        {
            if (b == null) return;
            var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, b.Width, b.Height);
            int r = Math.Min(radius, Math.Min(b.Width / 2, b.Height / 2));
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();
            b.Region = new Region(path);
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
                if (selectedPerson != null)
                {
                    Person p = DetectClickedPerson(e);
                    if (p != null)
                    {
                        selectedPerson = p;
                    }
                }
                else
                {
                    selectedPerson = DetectClickedPerson(e);
                }
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
                profilePictureBox.Image = null;
            }
            else
            {
                infoLabel.Text =
                    $"Nombre: {selectedPerson.GetName}\n" +
                    $"ID: {selectedPerson.GetId}\n" +
                    $"Edad: {selectedPerson.Edad}\n" +
                    $"Fecha de Nacimiento: {selectedPerson.birthdate.ToShortDateString()}\n" +
                    $"Hijos: {selectedPerson.Children.Count}";

                // Cargar imagen de perfil si existe (intenta varias convenciones de nombre)
                string fotoPath = null;
                var t = selectedPerson.GetType();
                var prop = t.GetProperty("FotoPath") ?? t.GetProperty("fotoPath") ?? t.GetProperty("Foto") ?? t.GetProperty("PhotoPath") ?? t.GetProperty("photoPath");
                if (prop != null)
                {
                    fotoPath = prop.GetValue(selectedPerson) as string;
                }

                if (!string.IsNullOrEmpty(fotoPath) && File.Exists(fotoPath))
                {
                    try
                    {
                        using (var fs = new FileStream(fotoPath, FileMode.Open, FileAccess.Read))
                        {
                            profilePictureBox.Image = Image.FromStream(fs);
                        }
                    }
                    catch
                    {
                        profilePictureBox.Image = null;
                    }
                }
                else
                {
                    profilePictureBox.Image = null;
                }
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
                        form.GetLatitud(),
                        form.GetLongitud(),
                        form.GetFechaFallecimiento()
                    );
                }
            }
            return null;
        }
        private void MostrarFormularioPersonaExistente(string titulo)
        {
            if (selectedPerson == null) return;

            using (var form = new PersonForm(selectedPerson, titulo))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Confirmado)
                {
                    // Actualiza los datos en el objeto existente
                    selectedPerson.name = form.txtNombre.Text;
                    selectedPerson.id = form.txtId.Text;
                    selectedPerson.birthdate = form.dateNacimiento.Value;
                    selectedPerson.Latitud = form.GetLatitud();
                    selectedPerson.Longitud = form.GetLongitud();
                    selectedPerson.deathDate = form.GetFechaFallecimiento();
                    selectedPerson.fotoPath = form.FotoPath;

                    grafo.CalculatePositions();
                    Invalidate();
                    UpdateInfoPanel(); // refresca panel lateral
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnChangeInfo_Click(object sender, EventArgs e)
        {
            if (selectedPerson == null)
            {
                MessageBox.Show("Debe seleccionar una persona primero.");
                return;
            }
            Debug.WriteLine("Pressed Change Info");
            MostrarFormularioPersonaExistente("Editar información");
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
            if (!selectedPerson.CanAddParent())
            {
                MessageBox.Show("Error. Máximo de padres alcanzado o no se pueden añadir padres a parejas");
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
            if (selectedPerson.partner != null)
            {
                MessageBox.Show("Error. No se pueden añadir más parejas");
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
