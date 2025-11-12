using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class PersonForm : Form
    {
        public TextBox txtNombre;
        public TextBox txtId;
        public DateTimePicker dateNacimiento;
        public string FotoPath => "foto"; // por ahora fijo
        public bool Confirmado { get; private set; } = false;

        public PersonForm(string titulo = "Nueva Persona")
        {
            this.Text = titulo;
            this.Size = new Size(300, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblNombre = new Label() { Text = "Nombre:", Location = new Point(20, 20), AutoSize = true };
            txtNombre = new TextBox() { Location = new Point(100, 18), Width = 150 };

            Label lblId = new Label() { Text = "ID:", Location = new Point(20, 60), AutoSize = true };
            txtId = new TextBox() { Location = new Point(100, 58), Width = 150 };

            Label lblFecha = new Label() { Text = "Nacimiento:", Location = new Point(20, 100), AutoSize = true };
            dateNacimiento = new DateTimePicker() { Location = new Point(100, 98), Width = 150 };

            Button btnAceptar = new Button()
            {
                Text = "Aceptar",
                Location = new Point(40, 150),
                DialogResult = DialogResult.OK
            };
            btnAceptar.Click += (s, e) => { Confirmado = true; this.Close(); };

            Button btnCancelar = new Button()
            {
                Text = "Cancelar",
                Location = new Point(160, 150),
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.Click += (s, e) => { Confirmado = false; this.Close(); };

            this.Controls.AddRange(new Control[] { lblNombre, txtNombre, lblId, txtId, lblFecha, dateNacimiento, btnAceptar, btnCancelar });
        }

        private void PersonForm_Load(object sender, EventArgs e)
        {

        }
    }
}
