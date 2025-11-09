using System;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace ArbolFamiliar
{
    public partial class Mapa : Form
    {
        private GMapControl mapa;

        public Mapa()
        {
            InitializeComponent();
            InicializarMapa();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Mapa
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "Mapa";
            this.Text = "Mapa Familiar";
            this.ResumeLayout(false);
        }

        private void InicializarMapa()
        {
            // Crear mapa
            mapa = new GMapControl();
            mapa.Dock = DockStyle.Fill;
            mapa.MinZoom = 2;
            mapa.MaxZoom = 18;
            mapa.Zoom = 10;
            mapa.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapa.Position = new PointLatLng(9.93, -84.08);
            mapa.CanDragMap = true;
            mapa.DragButton = MouseButtons.Left;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            mapa.ShowCenter = false;
            this.Controls.Add(mapa);

            // Capa para los marcadores
            var overlay = new GMapOverlay("marcadores");

            // Crear personas de prueba
            var persona1 = new Persona("Juan Pérez", "00123456", new DateTime(1980, 1, 1), "", 9.934739, -84.087502);
            var persona2 = new Persona("María López", "00234567", new DateTime(1985, 5, 15), "", 10.016250, -84.216630);
            var persona3 = new Persona("Carlos Rodríguez", "00345678", new DateTime(1975, 8, 20), "", 9.863000, -83.919300);
            var persona4 = new Persona("Ana Martínez", "00456789", new DateTime(1990, 3, 10), "", 9.998200, -84.117300);

            // Crear marcadores personalizados
            var marcador1 = new MarcadorPersonalizado(persona1);
            var marcador2 = new MarcadorPersonalizado(persona2);
            var marcador3 = new MarcadorPersonalizado(persona3);
            var marcador4 = new MarcadorPersonalizado(persona4);

            // Agregar marcadores a la capa
            overlay.Markers.Add(marcador1);
            overlay.Markers.Add(marcador2);
            overlay.Markers.Add(marcador3);
            overlay.Markers.Add(marcador4);

            // Agregar capa al mapa
            mapa.Overlays.Add(overlay);

            // Evento click en marcadores
            mapa.OnMarkerClick += (marker, e) =>
            {
                if (marker is MarcadorPersonalizado marcadorPersonalizado)
                {
                    string mensaje = $"Nombre: {marcadorPersonalizado.persona.GetName}\n" +
                                   $"Cédula: {marcadorPersonalizado.persona.id}\n" +
                                   $"Coordenadas: {marcadorPersonalizado.persona.Latitud:F4}, " +
                                   $"{marcadorPersonalizado.persona.Longitud:F4}";

                    MessageBox.Show(mensaje, "Información de la Persona");
                }
            };
        }
    }
}