using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class arbolForm : Form
    {
        private Point panOffset = new Point(0, 0);
        private Point lastMouse;
        private bool dragging = false;
        private float zoom = 1.0f;
        private Person selectedPerson = null;
        public static GrafoGenealogico grafo;
        private Panel sidePanel;
        private Label infoLabel;
        private PictureBox profilePictureBox;
        private Button btnAddChild, btnAddPatner, btnAddParent, btnDelete, btnChangeInfo, btnBack, btnCenter;
        private Panel marcoContainer;
        private Image backgroundImage;
        private static Image marcoGlobal = null;

        public arbolForm()
        {
            InitializeComponent();
            ConfigurarVentana();

            // Cargar imagen de fondo
            string fondoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "fondo_arbol.jpg");
            if (File.Exists(fondoPath))
            {
                backgroundImage = Image.FromFile(fondoPath);
            }

            // Inicializar grafo y nodo inicial
            if (grafo == null)
            {
                grafo = new GrafoGenealogico();
                grafo.CrearNodoInicial(); // Asegura que haya al menos un nodo
            }

            AsignarEventos();
            CrearPanelLateral();
        }

        private void ConfigurarVentana()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }

        private void AsignarEventos()
        {
            Load += ArbolForm_Load;
            Paint += ArbolForm_Paint;
            MouseDown += ArbolForm_MouseDown;
            MouseMove += ArbolForm_MouseMove;
            MouseUp += ArbolForm_MouseUp;
            MouseWheel += ArbolForm_MouseWheel;
        }

        private void CrearPanelLateral()
        {
            sidePanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = ColorTranslator.FromHtml("#3f5030")
            };
            Controls.Add(sidePanel);

            CrearBotonRegresar();
            CrearTitulo();
            CrearFotoPerfil();
            CrearInfoLabel();
            CrearBotonesInferiores();
            UpdateInfoPanel();
        }

        private void CrearBotonRegresar()
        {
            btnBack = CrearBoton("← Regresar", 10, 10, 90, 30, Color.Transparent, Color.Black);
            btnBack.Click += (s, e) => Close();
            sidePanel.Controls.Add(btnBack);
        }

        private void CrearTitulo()
        {
            Label title = new Label
            {
                Text = "Información",
                Font = new Font("Garamond", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 50),
                Size = new Size(220, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            sidePanel.Controls.Add(title);
        }

        private void CrearFotoPerfil()
        {
            marcoContainer = new Panel
            {
                Size = new Size(190, 190),
                Location = new Point((sidePanel.Width - 190) / 2, 100),
                BackColor = Color.Transparent
            };
            marcoContainer.Paint += MarcoContainer_Paint;

            profilePictureBox = new PictureBox
            {
                Size = new Size(160, 160),
                Location = new Point((marcoContainer.Width - 160) / 2, (marcoContainer.Height - 160) / 2),
                SizeMode = PictureBoxSizeMode.Normal,
                BackColor = Color.FromArgb(235, 235, 235),
                BorderStyle = BorderStyle.None
            };
            profilePictureBox.Paint += ProfilePictureBox_Paint;

            marcoContainer.Controls.Add(profilePictureBox);
            sidePanel.Controls.Add(marcoContainer);
        }

        private void CrearInfoLabel()
        {
            infoLabel = new Label
            {
                Text = "Ninguna persona seleccionada",
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                ForeColor = Color.WhiteSmoke,
                Location = new Point(20, 300),
                Size = new Size(sidePanel.Width - 40, 180),
                AutoSize = false,
                TextAlign = ContentAlignment.TopLeft
            };
            sidePanel.Controls.Add(infoLabel);
        }

        private void CrearBotonesInferiores()
        {
            int startY = 500;
            int buttonWidth = sidePanel.Width - 60;
            int buttonHeight = 42;
            int spacing = 12;

            Color azulSuave = ColorTranslator.FromHtml("#5b79a1");
            Color rojoSuave = ColorTranslator.FromHtml("#a14f4f");

            btnChangeInfo = CrearBoton("Cambiar Información", 30, startY, buttonWidth, buttonHeight, azulSuave);
            btnCenter = CrearBoton("Centrar Vista", 30, startY + 1 * (buttonHeight + spacing), buttonWidth, buttonHeight, azulSuave);
            btnAddChild = CrearBoton("Agregar Hijo", 30, startY + 2 * (buttonHeight + spacing), buttonWidth, buttonHeight, azulSuave);
            btnAddParent = CrearBoton("Agregar Padre", 30, startY + 3 * (buttonHeight + spacing), buttonWidth, buttonHeight, azulSuave);
            btnAddPatner = CrearBoton("Agregar Pareja", 30, startY + 4 * (buttonHeight + spacing), buttonWidth, buttonHeight, azulSuave);
            btnDelete = CrearBoton("Eliminar", 30, startY + 5 * (buttonHeight + spacing), buttonWidth, buttonHeight, rojoSuave);

            sidePanel.Controls.AddRange(new Control[] { btnChangeInfo, btnAddChild, btnAddParent, btnAddPatner, btnDelete, btnCenter });

            btnChangeInfo.Click += BtnChangeInfo_Click;
            btnAddChild.Click += BtnAddChild_Click;
            btnAddParent.Click += BtnAddParent_Click;
            btnAddPatner.Click += BtnAddPatner_Click;
            btnDelete.Click += BtnDelete_Click;
            btnCenter.Click += BtnCenter_Click;
        }

        private Button CrearBoton(string texto, int x, int y, int w, int h, Color bg, Color? fg = null)
        {
            Button b = new Button
            {
                Text = texto,
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = bg,
                ForeColor = fg ?? Color.White,
                FlatStyle = FlatStyle.Flat
            };
            b.FlatAppearance.BorderSize = 0;
            RedondearBoton(b, 8);
            return b;
        }

        private void RedondearBoton(Button b, int radio)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, b.Width, b.Height);
            int r = Math.Min(radio, Math.Min(b.Width / 2, b.Height / 2));
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
            if (backgroundImage != null)
            {
                e.Graphics.DrawImage(backgroundImage, new Rectangle(sidePanel.Width, 0, Width - sidePanel.Width, Height));
            }

            e.Graphics.TranslateTransform(panOffset.X + sidePanel.Width, panOffset.Y);
            e.Graphics.ScaleTransform(zoom, zoom);
            grafo.DrawNodes(e.Graphics);
        }

        private void ArbolForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.X > sidePanel.Width)
            {
                Person res = DetectClickedPerson(e);
                if (res != null) selectedPerson = res;
                UpdateInfoPanel();
                dragging = true;
                lastMouse = e.Location;
            }
        }

        private void ArbolForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;
            panOffset.X += e.X - lastMouse.X;
            panOffset.Y += e.Y - lastMouse.Y;
            lastMouse = e.Location;
            Invalidate();
        }

        private void ArbolForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) dragging = false;
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
                if (Math.Sqrt(dx * dx + dy * dy) <= radius) return p;
            }

            return null;
        }

        private void UpdateInfoPanel()
        {
            if (selectedPerson == null)
            {
                infoLabel.Text = "Ninguna persona seleccionada";
                profilePictureBox.Image = null;
                return;
            }

            infoLabel.Text = $"Nombre: {selectedPerson.GetName}\n" +
                             $"Cédula: {selectedPerson.GetId}\n" +
                             $"Edad: {selectedPerson.Edad}\n" +
                             $"Nacimiento: {selectedPerson.birthdate.ToShortDateString()}\n" +
                             (selectedPerson.deathDate.HasValue ? $"Fallecimiento: {selectedPerson.deathDate.Value.ToShortDateString()}\n" : "") +
                             $"Hijos: {selectedPerson.Children.Count}\n" +
                             $"Latitud: {selectedPerson.Latitud:F5}\n" +
                             $"Longitud: {selectedPerson.Longitud:F5}";

            profilePictureBox.Image = CargarImagen(selectedPerson);
            profilePictureBox.Invalidate();
        }

        private Image CargarImagen(Person persona)
        {
            string fotoPath = ObtenerFotoPath(persona);
            if (string.IsNullOrEmpty(fotoPath) || !File.Exists(fotoPath)) return null;
            try
            {
                using (var fs = new FileStream(fotoPath, FileMode.Open, FileAccess.Read))
                {
                    return Image.FromStream(fs);
                }
            }
            catch { return null; }
        }

        private string ObtenerFotoPath(Person persona)
        {
            var tipo = persona.GetType();
            var prop = tipo.GetProperty("FotoPath") ?? tipo.GetProperty("fotoPath") ?? tipo.GetProperty("Foto") ?? tipo.GetProperty("PhotoPath") ?? tipo.GetProperty("photoPath");
            return prop != null ? prop.GetValue(persona) as string : null;
        }

        private Person MostrarFormularioNuevaPersona(string titulo)
        {
            using (var form = new PersonForm(selectedPerson, titulo))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Confirmado)
                {
                    Debug.WriteLine("Creando nueva persona desde el formulario.");
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
                Debug.WriteLine("No se creó nueva persona desde el formulario.");
            }
            return null;
        }

        private void MostrarFormularioPersonaExistente(string titulo)
        {
            if (selectedPerson == null) return;

            using (var form = new PersonForm(selectedPerson))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Confirmado)
                {
                    selectedPerson.name = form.txtNombre.Text;
                    selectedPerson.id = form.txtId.Text;
                    selectedPerson.birthdate = form.dateNacimiento.Value;
                    selectedPerson.Latitud = form.GetLatitud();
                    selectedPerson.Longitud = form.GetLongitud();
                    selectedPerson.deathDate = form.GetFechaFallecimiento();
                    selectedPerson.fotoPath = form.FotoPath;

                    grafo.CalculatePositions();
                    Invalidate();
                    UpdateInfoPanel();
                }
            }
        }

        private void BtnChangeInfo_Click(object sender, EventArgs e)
        {
            if (!VerificarSeleccion()) return;
            MostrarFormularioPersonaExistente("Editar información");
        }

        private void BtnAddChild_Click(object sender, EventArgs e)
        {
            if (!VerificarSeleccion()) return;

            var nuevo = MostrarFormularioNuevaPersona("Agregar hijo");
            if (nuevo != null)
            {
                grafo.AddChildren(selectedPerson, nuevo);
                ActualizarGrafo();
            }
        }

        private void BtnAddParent_Click(object sender, EventArgs e)
        {
            if (!VerificarSeleccion()) return;
            if (!selectedPerson.CanAddParent())
            {
                MessageBox.Show("Esta persona ya tiene el máximo de padres permitidos.");
                return;
            }

            var nuevoPadre = MostrarFormularioNuevaPersona("Agregar padre");
            if (nuevoPadre != null)
            {
                grafo.AddParent(selectedPerson, nuevoPadre);
                ActualizarGrafo();
            }
        }

        private void BtnAddPatner_Click(object sender, EventArgs e)
        {
            if (!VerificarSeleccion()) return;
            if (selectedPerson.partner != null)
            {
                MessageBox.Show("Esta persona ya tiene pareja.");
                return;
            }

            var nuevaPareja = MostrarFormularioNuevaPersona("Agregar pareja");
            Debug.WriteLine(nuevaPareja == null ? "No se creó nueva pareja." : "Nueva pareja creada.");
            if (nuevaPareja != null)
            {
                Debug.WriteLine("Agregando pareja al grafo.");
                grafo.AddPatner(selectedPerson, nuevaPareja);
                ActualizarGrafo();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!VerificarSeleccion()) return;

            if (grafo.GetAllPersons().Count() <= 1)
            {
                MessageBox.Show("No se puede eliminar el último nodo del árbol.");
                return;
            }

            if (MessageBox.Show("¿Eliminar a " + selectedPerson.GetName + "?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                grafo.DeletePerson(selectedPerson);
                selectedPerson = null;
                ActualizarGrafo();
            }
        }

        private void BtnCenter_Click(object sender, EventArgs e)
        {
            panOffset = new Point(0, 0);
            zoom = 1.0f;
            Invalidate();
        }

        private void ActualizarGrafo()
        {
            grafo.CalculatePositions();
            Invalidate();
            UpdateInfoPanel();
        }

        private bool VerificarSeleccion()
        {
            if (selectedPerson == null)
            {
                MessageBox.Show("Debe seleccionar una persona primero.");
                return false;
            }
            return true;
        }

        private void ProfilePictureBox_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.Clear(pb.BackColor);

            if (pb.Image != null)
            {
                Rectangle destRect = GetBestFitRectangle(pb.Image, pb.ClientRectangle);
                e.Graphics.DrawImage(pb.Image, destRect);
            }
        }

        private Rectangle GetBestFitRectangle(Image img, Rectangle container)
        {
            float ratioImg = (float)img.Width / img.Height;
            float ratioContainer = (float)container.Width / container.Height;

            int width, height;
            if (ratioImg > ratioContainer)
            {
                width = container.Width;
                height = (int)(container.Width / ratioImg);
            }
            else
            {
                height = container.Height;
                width = (int)(container.Height * ratioImg);
            }

            int x = container.X + (container.Width - width) / 2;
            int y = container.Y + (container.Height - height) / 2;
            return new Rectangle(x, y, width, height);
        }

        private void MarcoContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            string marcoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "marco.png");
            if (File.Exists(marcoPath))
            {
                using (Image marco = Image.FromFile(marcoPath))
                {
                    e.Graphics.DrawImage(marco, new Rectangle(0, 0, ((Panel)sender).Width, ((Panel)sender).Height));
                }
            }
        }
    }
}
