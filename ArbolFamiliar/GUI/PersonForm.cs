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
        public Button btnSeleccionarUbicacion;
        public string titulo;

        // Propiedades de salida
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

        // Configura las propiedades generales de la ventana
        private void ConfigurarFormulario(string titulo)
        {
            this.Text = titulo;
            this.Size = new Size(400, 600);
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

            btnSeleccionarUbicacion = new Button
            {
                Text = "Seleccionar Ubicación",
                Location = new Point(20, y),
                Size = new Size(200, 30)
            };
            btnSeleccionarUbicacion.Click += BtnSeleccionarUbicacion_Click;
            this.Controls.Add(btnSeleccionarUbicacion);
            y += 50;

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

        private void BtnSeleccionarUbicacion_Click(object sender, EventArgs e)
        {
            // Abrir el mapa en modo selección
            using (var mapaSeleccion = new MapaSeleccion())
            {
                // Mostrar el formulario de mapa como modal
                if (mapaSeleccion.ShowDialog() == DialogResult.OK && mapaSeleccion.CoordenadasSeleccionadas)
                {
                    // Actualizar los TextBox con la posición seleccionada
                    txtLatitud.Text = mapaSeleccion.LatitudSeleccionada.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    txtLongitud.Text = mapaSeleccion.LongitudSeleccionada.ToString(System.Globalization.CultureInfo.InvariantCulture);
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

            if (!VerificarFechaNacimiento())
                return false;

            if (!chkViva.Checked && !VerificarFechaFallecimiento())
                return false;

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

        private void PersonForm_Load(object sender, EventArgs e)
        {

        }

        // Metodos auxiliares para crear controles

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