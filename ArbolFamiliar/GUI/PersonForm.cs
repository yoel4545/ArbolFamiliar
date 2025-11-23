using System;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ArbolFamiliar
{
    public partial class PersonForm : Form
    {
        // Campos visuales (mismos nombres para no romper lógica externa)
        public TextBox txtNombre, txtId, txtLatitud, txtLongitud;
        public DateTimePicker dateNacimiento, dateFallecimiento;
        public CheckBox chkViva;
        public PictureBox picFoto;
        public Button btnSeleccionarFoto;
        public Person selectedPerson;
        public Button btnSeleccionarUbicacion;
        public string titulo;

        // Propiedades de salida (igual que antes)
        public string FotoPath { get; private set; } = "";
        public bool Confirmado { get; private set; } = false;

        // Constructor: crear nueva persona
        public PersonForm(Person selectedPerson, string titulo = "Nueva Persona")
        {
            this.titulo = titulo;
            this.selectedPerson = selectedPerson;
            ConfigurarFormulario(titulo);
            CrearControles();
        }

        // Constructor: editar persona existente
        public PersonForm(Person selectedPerson)
        {
            this.titulo = "Editar Persona";
            this.selectedPerson = selectedPerson;
            ConfigurarFormulario(titulo);
            CrearControles();
            AddExistingInfo();
        }

        // Configura propiedades generales y estética
        private void ConfigurarFormulario(string titulo)
        {
            this.Text = titulo;
            this.Size = new Size(480, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Fondo tipo pergamino suave para mantener la estética vintage
            this.BackColor = ColorTranslator.FromHtml("#f5f3eb");

            // Fuente base para controles (mantiene legibilidad)
            this.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        }

        // Crea y posiciona todos los controles del formulario (estética mejorada)
        private void CrearControles()
        {
            int y = 18;

            // Título (Garamond para un toque clásico)
            Label lblTitulo = new Label
            {
                Text = this.titulo.ToUpper(),
                Font = new Font("Garamond", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#3f5030"),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = this.ClientSize.Width,
                Location = new Point(0, y)
            };
            this.Controls.Add(lblTitulo);
            y += 52;

            // Nombre
            CrearLabel("Nombre:", 26, y);
            txtNombre = CrearTextBox(160, y, "");
            y += 40;

            // Cédula
            CrearLabel("Cédula:", 26, y);
            txtId = CrearTextBox(160, y, "");
            y += 40;

            // Fecha de nacimiento
            CrearLabel("Nacimiento:", 26, y);
            dateNacimiento = CrearDatePicker(160, y, DateTime.Now);
            this.Controls.Add(dateNacimiento);
            y += 40;

            // Estado (viva o fallecida)
            chkViva = new CheckBox
            {
                Text = "Persona viva",
                Location = new Point(26, y),
                Checked = true,
                ForeColor = ColorTranslator.FromHtml("#3f5030"),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true
            };
            chkViva.CheckedChanged += ChkViva_CheckedChanged;
            this.Controls.Add(chkViva);
            y += 36;

            // Fecha de fallecimiento
            CrearLabel("Fallecimiento:", 26, y);
            dateFallecimiento = CrearDatePicker(160, y, DateTime.Now);
            dateFallecimiento.Enabled = false;
            this.Controls.Add(dateFallecimiento);
            y += 30;

            // Foto
            CrearLabel("Fotografía:", 26, y);
            y += 40;

            picFoto = new PictureBox
            {
                Location = new Point(26, y),
                Size = new Size(110, 110),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(235, 235, 235)
            };
            this.Controls.Add(picFoto);

            btnSeleccionarFoto = CrearBoton("Seleccionar Foto", 160, y + 36, 160, 38, "#5b79a1");
            btnSeleccionarFoto.Click += BtnSeleccionarFoto_Click;
            this.Controls.Add(btnSeleccionarFoto);
            y += 130;

            // Ubicación
            btnSeleccionarUbicacion = CrearBoton("Seleccionar Ubicación", 26, y, 220, 38, "#5b79a1");
            btnSeleccionarUbicacion.Click += BtnSeleccionarUbicacion_Click;
            this.Controls.Add(btnSeleccionarUbicacion);
            y += 56;

            // Lat / Lng
            CrearLabel("Latitud:", 26, y);
            txtLatitud = CrearTextBox(160, y, "9");
            y += 40;

            CrearLabel("Longitud:", 26, y);
            txtLongitud = CrearTextBox(160, y, "-84");
            y += 54;

            // Botones Aceptar / Cancelar
            Button btnAceptar = CrearBoton("Aceptar", 110, y, 120, 44, "#3f5030");
            btnAceptar.Click += BtnAceptar_Click;
            this.Controls.Add(btnAceptar);

            Button btnCancelar = CrearBoton("Cancelar", 260, y, 120, 44, "#a14f4f");
            btnCancelar.Click += BtnCancelar_Click;
            this.Controls.Add(btnCancelar);
        }

        // Habilita o deshabilita fecha de fallecimiento al cambiar checkbox
        private void ChkViva_CheckedChanged(object sender, EventArgs e)
        {
            dateFallecimiento.Enabled = !chkViva.Checked;
        }

        // Selección de imagen de foto (igual lógica)
        private void BtnSeleccionarFoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Imágenes|*.jpg;*.png;*.jpeg;*.bmp";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FotoPath = dlg.FileName;
                try
                {
                    // mantengo la lógica original de carga
                    picFoto.Image = Image.FromFile(FotoPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la imagen: " + ex.Message);
                    FotoPath = "";
                }
            }
        }

        // Abrir mapa en modo selección (misma lógica)
        private void BtnSeleccionarUbicacion_Click(object sender, EventArgs e)
        {
            using (var mapaSeleccion = new MapaSeleccion())
            {
                if (mapaSeleccion.ShowDialog() == DialogResult.OK && mapaSeleccion.CoordenadasSeleccionadas)
                {
                    txtLatitud.Text = mapaSeleccion.LatitudSeleccionada.ToString(CultureInfo.InvariantCulture);
                    txtLongitud.Text = mapaSeleccion.LongitudSeleccionada.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        // Aceptar: validar y cerrar con OK (misma lógica)
        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (ValidarDatos())
            {
                Confirmado = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // Cancelar: cerrar sin guardar
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Confirmado = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Validaciones (idénticas, no cambié mensajes ni reglas)
        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarError("El nombre es obligatorio.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MostrarError("La cédula es obligatoria.");
                return false;
            }

            double lat, lng;
            if (!double.TryParse(txtLatitud.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lat) || lat < -90 || lat > 90)
            {
                MostrarError("Latitud inválida. Debe ser un número entre -90 y 90 (ejemplo: 9.9347).");
                return false;
            }

            if (!double.TryParse(txtLongitud.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out lng) || lng < -180 || lng > 180)
            {
                MostrarError("Longitud inválida. Debe ser un número entre -180 y 180 (ejemplo: -84.0875).");
                return false;
            }

            if (!VerificarFechaNacimiento())
                return false;

            if (!chkViva.Checked && !VerificarFechaFallecimiento())
                return false;

            return true;
        }

        // Mensaje de error (igual)
        private void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Devuelve la fecha de fallecimiento o null si la persona está viva
        public DateTime? GetFechaFallecimiento()
        {
            return chkViva.Checked ? (DateTime?)null : dateFallecimiento.Value;
        }

        // Devuelve la latitud en formato numérico
        public double GetLatitud()
        {
            return double.Parse(txtLatitud.Text, CultureInfo.InvariantCulture);
        }

        // Devuelve la longitud en formato numérico
        public double GetLongitud()
        {
            return double.Parse(txtLongitud.Text, CultureInfo.InvariantCulture);
        }

        // Carga la info existente (idéntico)
        private void AddExistingInfo()
        {
            if (selectedPerson == null) return;

            txtNombre.Text = selectedPerson.GetName;
            txtId.Text = selectedPerson.GetId;
            dateNacimiento.Value = selectedPerson.birthdate;

            txtLatitud.Text = selectedPerson.Latitud.ToString(CultureInfo.InvariantCulture);
            txtLongitud.Text = selectedPerson.Longitud.ToString(CultureInfo.InvariantCulture);

            if (selectedPerson.deathDate.HasValue)
            {
                chkViva.Checked = false;
                dateFallecimiento.Enabled = true;
                dateFallecimiento.Value = selectedPerson.deathDate.Value;
            }
            else
            {
                chkViva.Checked = true;
                dateFallecimiento.Enabled = false;
            }

            if (!string.IsNullOrEmpty(selectedPerson.fotoPath) && File.Exists(selectedPerson.fotoPath))
            {
                try { picFoto.Image = Image.FromFile(selectedPerson.fotoPath); }
                catch { }
                FotoPath = selectedPerson.fotoPath;
            }
        }

        private void PersonForm_Load(object sender, EventArgs e)
        {
            // Intencionalmente vacío — ninguna inicialización extra necesaria
        }

        // Métodos auxiliares para crear controles con estilo (no cambian lógica externa)

        private Label CrearLabel(string texto, int x, int y)
        {
            Label lbl = new Label
            {
                Text = texto,
                Location = new Point(x, y + 6),
                AutoSize = true,
                ForeColor = ColorTranslator.FromHtml("#3f5030"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CrearTextBox(int x, int y, string valorInicial = "")
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Width = 260,
                Text = valorInicial,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.White
            };
            this.Controls.Add(txt);
            return txt;
        }

        private DateTimePicker CrearDatePicker(int x, int y, DateTime maxDate)
        {
            DateTimePicker picker = new DateTimePicker
            {
                Location = new Point(x, y),
                Width = 260,
                Format = DateTimePickerFormat.Short,
                MaxDate = maxDate,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            return picker;
        }

        // Crear botón estilizado: color de fondo, texto en blanco y bordes redondeados
        private Button CrearBoton(string texto, int x, int y, int w, int h, string colorHex)
        {
            Button btn = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = ColorTranslator.FromHtml(colorHex),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;

            // Para redondear, definimos la región en el evento Paint (compatible con versiones antiguas)
            btn.Paint += delegate (object sender, PaintEventArgs e)
            {
                Button b = (Button)sender;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    int radius = Math.Min(14, Math.Min(b.Width / 4, b.Height / 2));
                    gp.StartFigure();
                    gp.AddArc(0, 0, radius, radius, 180, 90);
                    gp.AddArc(b.Width - radius, 0, radius, radius, 270, 90);
                    gp.AddArc(b.Width - radius, b.Height - radius, radius, radius, 0, 90);
                    gp.AddArc(0, b.Height - radius, radius, radius, 90, 90);
                    gp.CloseAllFigures();
                    b.Region = new Region(gp);
                }
            };

            return btn;
        }

        // Validaciones relacionadas con fechas (idénticas)
        private bool VerificarFechaNacimiento()
        {
            if (dateNacimiento.Value > DateTime.Now)
            {
                MostrarError("La fecha de nacimiento no puede ser en el futuro.");
                return false;
            }
            else if (titulo == "Editar información")
            {
                if (selectedPerson != null && selectedPerson.HasParents())
                {
                    DateTime padresNacimiento = selectedPerson.GetOldestParentBirthdate();
                    if (dateNacimiento.Value <= padresNacimiento.AddYears(12))
                    {
                        MostrarError("La fecha de nacimiento debe ser al menos 12 años posterior a la de los padres.");
                        return false;
                    }
                }
            }
            else if (titulo == "Agregar padre")
            {
                if (selectedPerson != null)
                {
                    DateTime hijoNacimiento = selectedPerson.birthdate;
                    if (dateNacimiento.Value >= hijoNacimiento.AddYears(-12))
                    {
                        MostrarError("La fecha de nacimiento del padre/madre debe ser al menos 12 años anterior a la del hijo/a.");
                        return false;
                    }
                }
            }
            else if (titulo == "Agregar hijo")
            {
                if (selectedPerson != null)
                {
                    DateTime padreNacimiento = selectedPerson.birthdate;
                    if (dateNacimiento.Value <= padreNacimiento.AddYears(12))
                    {
                        MostrarError("La fecha de nacimiento del hijo/a debe ser al menos 12 años posterior a la del padre/madre.");
                        return false;
                    }
                }
            }
            else if (titulo == "Agregar pareja")
            {
                if (!selectedPerson.HasChildren()) return false;
                if (selectedPerson.GetOldestChildBirthdate().AddYears(12) > dateNacimiento.Value)
                {
                    MostrarError("La fecha de nacimiento de la pareja no puede ser más de 12 años posterior a la del hijo/a mayor.");
                    return false;
                }
            }
            return true;
        }

        private bool VerificarFechaFallecimiento()
        {
            if (dateFallecimiento.Value > DateTime.Now)
            {
                MostrarError("La fecha de fallecimiento no puede ser en el futuro.");
                return false;
            }

            if (dateFallecimiento.Value <= dateNacimiento.Value)
            {
                MostrarError("La fecha de fallecimiento debe ser posterior a la de nacimiento.");
                return false;
            }

            if (selectedPerson == null)
                return false;

            if (titulo == "Agregar padre")
            {
                DateTime nacimientoHijo = selectedPerson.birthdate;
                if (dateFallecimiento.Value < nacimientoHijo.AddYears(-1))
                {
                    MostrarError("El padre/madre no puede haber fallecido más de un año antes del nacimiento del hijo/a.");
                    return false;
                }
            }
            else if (titulo == "Agregar hijo")
            {
                DateTime nacimientoPadre = selectedPerson.birthdate;
                if (dateFallecimiento.Value <= nacimientoPadre.AddYears(1))
                {
                    MostrarError("El hijo/a debe fallecer al menos un año después del nacimiento del padre/madre.");
                    return false;
                }
            }
            else if (titulo == "Agregar pareja")
            {
                if (selectedPerson.HasChildren())
                {
                    DateTime hijoMayorNacimiento = selectedPerson.GetOldestChildBirthdate();
                    if (dateFallecimiento.Value <= hijoMayorNacimiento.AddYears(1))
                    {
                        MostrarError("La pareja debe fallecer al menos un año después del nacimiento del hijo/a mayor.");
                        return false;
                    }
                }
            }

            double edad = (dateFallecimiento.Value - dateNacimiento.Value).TotalDays / 365.25;
            if (edad > 120)
            {
                MostrarError("La edad al fallecer no puede superar los 120 años.");
                return false;
            }
            return true;
        }
    }
}
