using System;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace ArbolFamiliar
{
    public class MapaSeleccion : Mapa
    {
        private Button btnConfirmar;
        private GMapOverlay overlaySeleccion;
        private PointLatLng? puntoSeleccionado;

        public MapaSeleccion() : base()
        {
            // Ocultar botones heredados
            btnEstadisticas.Visible = false;


            ConfigurarSeleccion();
        }

        private void ConfigurarSeleccion()
        {
            Text = "Seleccionar ubicación";
            CoordenadasSeleccionadas = false;

            // Limpiar overlays y agregar uno nuevo para la selección
            overlaySeleccion = new GMapOverlay("seleccion");
            mapa.Overlays.Clear();
            mapa.Overlays.Add(overlaySeleccion);

            // Crear botón clásico de Windows Forms
            btnConfirmar = new Button
            {
                Text = "Confirmar ubicación",
                Size = new Size(150, 30),
                Location = new Point(15, 15)
            };
            btnConfirmar.Click += BtnConfirmar_Click;

            // Agregar el botón directamente sobre el mapa
            mapa.Controls.Add(btnConfirmar);
            btnConfirmar.BringToFront();

            // Habilitar clic en el mapa
            mapa.MouseClick += Mapa_MouseClick;
        }


        private void Mapa_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            var punto = mapa.FromLocalToLatLng(e.X, e.Y);
            puntoSeleccionado = punto;

            overlaySeleccion.Markers.Clear();
            overlaySeleccion.Markers.Add(new GMarkerGoogle(punto, GMarkerGoogleType.red_dot));

            LatitudSeleccionada = punto.Lat;
            LongitudSeleccionada = punto.Lng;
            CoordenadasSeleccionadas = true;
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            if (!puntoSeleccionado.HasValue)
            {
                MessageBox.Show("Seleccione un punto en el mapa antes de confirmar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.btnEstadisticas.Location = new System.Drawing.Point(113, 100);
            this.btnEstadisticas.Text = "";

            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "MapaSeleccion";
            this.ResumeLayout(false);

        }
    }
}
