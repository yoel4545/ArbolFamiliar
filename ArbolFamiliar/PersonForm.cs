using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace ArbolFamiliar
{
    public partial class PersonForm : Form
    {
        public TextBox txtNombre;
        public TextBox txtId;
        public TextBox txtLatitud;
        public TextBox txtLongitud;
        public DateTimePicker dateNacimiento;
        public DateTimePicker dateFallecimiento;
        public CheckBox chkViva;
        public PictureBox picFoto;
        public Button btnSeleccionarFoto;

        public string FotoPath { get; private set; } = "";
        public bool Confirmado { get; private set; } = false;

        public PersonForm(string titulo = "Nueva Persona")
        {
            this.Text = titulo;
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CrearControles();
        }

        private void CrearControles()
        {
            int y = 20;

            // Nombre
            Label lblNombre = new Label() { Text = "Nombre:", Location = new Point(20, y), AutoSize = true };
            txtNombre = new TextBox() { Location = new Point(120, y), Width = 200 };
            y += 35;

            // Cédula
            Label lblId = new Label() { Text = "Cédula:", Location = new Point(20, y), AutoSize = true };
            txtId = new TextBox() { Location = new Point(120, y), Width = 200 };
            y += 35;

            // Fecha Nacimiento
            Label lblNacimiento = new Label() { Text = "Nacimiento:", Location = new Point(20, y), AutoSize = true };
            dateNacimiento = new DateTimePicker()
            {
                Location = new Point(120, y),
                Width = 200,
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Now
            };
            y += 35;

            // Estado (vivo/fallecido)
            chkViva = new CheckBox()
            {
                Text = "Persona viva",
                Location = new Point(20, y),
                Checked = true
            };
            chkViva.CheckedChanged += ChkViva_CheckedChanged;
            y += 35;

            // Fecha Fallecimiento
            Label lblFallecimiento = new Label()
            {
                Text = "Fallecimiento:",
                Location = new Point(20, y),
                AutoSize = true
            };
            dateFallecimiento = new DateTimePicker()
            {
                Location = new Point(120, y),
                Width = 200,
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };
            y += 35;

            // Foto
            Label lblFoto = new Label() { Text = "Fotografía:", Location = new Point(20, y), AutoSize = true };
            y += 25;

            picFoto = new PictureBox()
            {
                Location = new Point(20, y),
                Size = new Size(80, 80),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };

            btnSeleccionarFoto = new Button()
            {
                Text = "Seleccionar...",
                Location = new Point(110, y),
                Size = new Size(100, 30)
            };
            btnSeleccionarFoto.Click += BtnSeleccionarFoto_Click;
            y += 90;

            // Coordenadas
            Label lblLatitud = new Label() { Text = "Latitud:", Location = new Point(20, y), AutoSize = true };
            txtLatitud = new TextBox()
            {
                Location = new Point(120, y),
                Width = 200,
                Text = "9"
                
            };
            y += 35;

            Label lblLongitud = new Label() { Text = "Longitud:", Location = new Point(20, y), AutoSize = true };
            txtLongitud = new TextBox()
            {
                Location = new Point(120, y),
                Width = 200,
                Text = "-84"
               
            };
            y += 50;

            // Botones
            Button btnAceptar = new Button()
            {
                Text = "Aceptar",
                Location = new Point(80, y),
                Size = new Size(100, 35)
            };
            btnAceptar.Click += BtnAceptar_Click;

            Button btnCancelar = new Button()
            {
                Text = "Cancelar",
                Location = new Point(200, y),
                Size = new Size(100, 35)
            };
            btnCancelar.Click += BtnCancelar_Click;

            this.Controls.AddRange(new Control[]
            {
                lblNombre, txtNombre,
                lblId, txtId,
                lblNacimiento, dateNacimiento,
                chkViva,
                lblFallecimiento, dateFallecimiento,
                lblFoto, btnSeleccionarFoto, picFoto,
                lblLatitud, txtLatitud,
                lblLongitud, txtLongitud,
                btnAceptar, btnCancelar
            });
        }

        private void ChkViva_CheckedChanged(object sender, EventArgs e)
        {
            dateFallecimiento.Enabled = !chkViva.Checked;
        }

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
                    MessageBox.Show($"Error al cargar imagen: {ex.Message}");
                    FotoPath = "";
                }
            }
        }

        private void BtnAceptar_Click(object sender, EventArgs e)
        {
            if (ValidarDatos())
            {
                Confirmado = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Confirmado = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Error");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("La cédula es obligatoria", "Error");
                return false;
            }

            if (!double.TryParse(txtLatitud.Text, System.Globalization.NumberStyles.Any,
                       System.Globalization.CultureInfo.InvariantCulture, out double lat) || lat < -90 || lat > 90)
            {
                MessageBox.Show("Latitud inválida. Debe ser un número entre -90 y 90\nEj: 9.9347", "Error");
                return false;
            }

            if (!double.TryParse(txtLongitud.Text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lng) || lng < -180 || lng > 180)
            {
                MessageBox.Show("Longitud inválida. Debe ser un número entre -180 y 180\nEj: -84.0875", "Error");
                return false;
            }

            if (dateNacimiento.Value > DateTime.Now)
            {
                MessageBox.Show("La fecha de nacimiento no puede ser futura", "Error");
                return false;
            }

            if (!chkViva.Checked && dateFallecimiento.Value <= dateNacimiento.Value)
            {
                MessageBox.Show("La fecha de fallecimiento debe ser posterior al nacimiento", "Error");
                return false;
            }

            return true;
        }

    
        public DateTime? GetFechaFallecimiento()
        {
            if (chkViva.Checked)
            {
                return null;
            }
            else
            {
                return dateFallecimiento.Value;
            }
        }

        public double GetLatitud() => double.Parse(txtLatitud.Text);
        public double GetLongitud() => double.Parse(txtLongitud.Text);

        private void PersonForm_Load(object sender, EventArgs e)
        {
        }
    }
}