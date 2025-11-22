using System;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class PersonForm : Form
    {
        // Campos visuales
        public TextBox txtNombre, txtId, txtLatitud, txtLongitud;
        public DateTimePicker dateNacimiento, dateFallecimiento;
        public CheckBox chkViva;
        public PictureBox picFoto;
        public Button btnSeleccionarFoto;
        public Person selectedPerson;

        // Propiedades de salida
        public string FotoPath { get; private set; } = "";
        public bool Confirmado { get; private set; } = false;

        // Constructor: crear nueva persona
        public PersonForm(string titulo = "Nueva Persona")
        {
            ConfigurarFormulario(titulo);
            CrearControles();
        }

        // Constructor: editar persona existente
        public PersonForm(Person selectedPerson, string titulo = "Editar Persona")
        {
            this.selectedPerson = selectedPerson;
            ConfigurarFormulario(titulo);
            CrearControles();
            AddExistingInfo();
        }

        // Configura las propiedades generales de la ventana
        private void ConfigurarFormulario(string titulo)
        {
            this.Text = titulo;
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        // Crea y posiciona todos los controles del formulario
        private void CrearControles()
        {
            int y = 20;

            // Nombre
            CrearLabel("Nombre:", 20, y);
            txtNombre = CrearTextBox(120, y);
            y += 35;

            // Cédula
            CrearLabel("Cédula:", 20, y);
            txtId = CrearTextBox(120, y);
            y += 35;

            // Fecha de nacimiento
            CrearLabel("Nacimiento:", 20, y);
            dateNacimiento = CrearDatePicker(120, y, DateTime.Now);
            this.Controls.Add(dateNacimiento);
            y += 35;

            // Estado (viva o fallecida)
            chkViva = new CheckBox
            {
                Text = "Persona viva",
                Location = new Point(20, y),
                Checked = true
            };
            chkViva.CheckedChanged += ChkViva_CheckedChanged;
            this.Controls.Add(chkViva);
            y += 35;

            // Fecha de fallecimiento
            CrearLabel("Fallecimiento:", 20, y);
            dateFallecimiento = CrearDatePicker(120, y, DateTime.Now);
            dateFallecimiento.Enabled = false;
            this.Controls.Add(dateFallecimiento);
            y += 40;

            // Foto
            CrearLabel("Fotografía:", 20, y);
            y += 25;

            picFoto = new PictureBox
            {
                Location = new Point(20, y),
                Size = new Size(80, 80),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            this.Controls.Add(picFoto);

            btnSeleccionarFoto = new Button
            {
                Text = "Seleccionar...",
                Location = new Point(110, y),
                Size = new Size(100, 30)
            };
            btnSeleccionarFoto.Click += BtnSeleccionarFoto_Click;
            this.Controls.Add(btnSeleccionarFoto);
            y += 90;

            // Coordenadas
            CrearLabel("Latitud:", 20, y);
            txtLatitud = CrearTextBox(120, y, "9");
            y += 35;

            CrearLabel("Longitud:", 20, y);
            txtLongitud = CrearTextBox(120, y, "-84");
            y += 50;

            // Botones de acción
            Button btnAceptar = CrearBoton("Aceptar", 80, y);
            btnAceptar.Click += BtnAceptar_Click;

            Button btnCancelar = CrearBoton("Cancelar", 200, y);
            btnCancelar.Click += BtnCancelar_Click;

            this.Controls.AddRange(new Control[] { btnAceptar, btnCancelar });
        }

        // Evento: habilita o deshabilita la fecha de fallecimiento según el estado
        private void ChkViva_CheckedChanged(object sender, EventArgs e)
        {
            dateFallecimiento.Enabled = !chkViva.Checked;
        }

        // Evento: selección de imagen de foto
        private void BtnSeleccionarFoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Imágenes|*.jpg;*.png;*.jpeg;*.bmp";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FotoPath = dlg.FileName;
                try
                {
                    picFoto.Image = Image.FromFile(FotoPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la imagen: " + ex.Message);
                    FotoPath = "";
                }
            }
        }

        // Evento: aceptar formulario
        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (ValidarDatos())
            {
                Confirmado = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        // Evento: cancelar formulario
        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Confirmado = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Valida que los datos ingresados sean correctos antes de guardar
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

            if (dateNacimiento.Value > DateTime.Now)
            {
                MostrarError("La fecha de nacimiento no puede ser futura.");
                return false;
            }

            if (!chkViva.Checked && dateFallecimiento.Value <= dateNacimiento.Value)
            {
                MostrarError("La fecha de fallecimiento debe ser posterior al nacimiento.");
                return false;
            }

            return true;
        }

        // Muestra un mensaje de error genérico
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

        // Carga los datos de una persona existente al editar
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

        // Metodos auxiliares

        private Label CrearLabel(string texto, int x, int y)
        {
            Label lbl = new Label
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true
            };
            this.Controls.Add(lbl);
            return lbl;
        }

        private TextBox CrearTextBox(int x, int y, string valorInicial = "")
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Width = 200,
                Text = valorInicial
            };
            this.Controls.Add(txt);
            return txt;
        }

        private DateTimePicker CrearDatePicker(int x, int y, DateTime maxDate)
        {
            DateTimePicker picker = new DateTimePicker
            {
                Location = new Point(x, y),
                Width = 200,
                Format = DateTimePickerFormat.Short,
                MaxDate = maxDate
            };
            return picker;
        }

        private Button CrearBoton(string texto, int x, int y)
        {
            Button btn = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(100, 35)
            };
            return btn;
        }
    }
}
