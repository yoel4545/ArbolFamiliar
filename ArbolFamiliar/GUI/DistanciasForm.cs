using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class DistanciasForm : Form
    {
        public DistanciasForm(Person persona, Dictionary<Person, double> distancias)
        {
            InitializeComponent();
            MostrarDistancias(persona, distancias);
        }

        private void MostrarDistancias(Person persona, Dictionary<Person, double> distancias)
        {
            this.Text = $"Distancias desde {persona.GetName}";
            this.Size = new Size(450, 600); // Un poco más ancho para la información
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPosition = 10;

            // Label información de la persona
            Label lblInfoPersona = new Label()
            {
                Text = $"Información de {persona.GetName}:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPosition),
                AutoSize = true
            };
            yPosition += 25;

            Label lblDetalles = new Label()
            {
                Text = $"Cédula: {persona.id}\n" +
           $"Fecha Nacimiento: {persona.birthdate.ToShortDateString()}\n" +
           $"Coordenadas: {persona.Latitud:F4}, {persona.Longitud:F4}",
                Font = new Font("Arial", 9),
                Location = new Point(10, yPosition),
                AutoSize = true
            };
            yPosition += 55;

            // Separador
            Label lblSeparador = new Label()
            {
                Text = "Distancias a todos los familiares:",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, yPosition),
                AutoSize = true
            };
            yPosition += 25;

            // ListBox para distancias
            ListBox listDistancias = new ListBox()
            {
                Location = new Point(10, yPosition),
                Size = new Size(410, 400),
                Font = new Font("Consolas", 9),
                HorizontalScrollbar = true
            };
            yPosition += 410;

            // Botón cerrar
            Button btnCerrar = new Button()
            {
                Text = "Cerrar",
                Location = new Point(180, yPosition),
                Size = new Size(80, 30)
            };
            btnCerrar.Click += (s, e) => this.Close();

            // Llenar ListBox con las distancias
            foreach (var kvp in distancias)
            {
                if (kvp.Key != persona) // No mostrar distancia a sí mismo
                {
                    string nombre = kvp.Key.GetName.PadRight(30).Substring(0, 30);
                    string distancia = kvp.Value.ToString("F2").PadLeft(10);
                    listDistancias.Items.Add($"{nombre} {distancia} km");
                }
            }

            // Opcional: Ordenar por distancia (más cercanos primero)
            // Podemos agregar esta funcionalidad si es necesaria

            this.Controls.AddRange(new Control[] {
                lblInfoPersona,
                lblDetalles,
                lblSeparador,
                listDistancias,
                btnCerrar
            });
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DistanciasForm
            // 
            this.ClientSize = new System.Drawing.Size(382, 353);
            this.Name = "DistanciasForm";
            this.Load += new System.EventHandler(this.DistanciasForm_Load);
            this.ResumeLayout(false);

        }

        private void DistanciasForm_Load(object sender, EventArgs e)
        {

        }
    }
}