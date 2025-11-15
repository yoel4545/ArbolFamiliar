using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET.WindowsForms.Markers;


namespace ArbolFamiliar
{
    public partial class Mapa : Form
    {
        private GMapControl mapa;
        private List<Person> personas;
        private Button btnEstadisticas;
        private Button button1;
        private GrafoGeografico grafoGeo;

        public Mapa()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            InicializarMapa();
        }

        private void InitializeComponent()
        {
            this.btnEstadisticas = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnEstadisticas
            // 
            this.btnEstadisticas.Location = new System.Drawing.Point(10, 10);
            this.btnEstadisticas.Name = "btnEstadisticas";
            this.btnEstadisticas.Size = new System.Drawing.Size(150, 30);
            this.btnEstadisticas.TabIndex = 0;
            this.btnEstadisticas.Text = "Ver estadísticas";
            this.btnEstadisticas.Click += new System.EventHandler(this.BtnEstadisticas_Click);

            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 47);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 26);
            this.button1.TabIndex = 1;
            this.button1.Text = "Regresar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Mapa
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnEstadisticas);
            this.Name = "Mapa";
            this.Load += new System.EventHandler(this.Mapa_Load);
            this.ResumeLayout(false);

        }

        private void BtnEstadisticas_Click(object sender, EventArgs e)
        {


            if (grafoGeo == null)
            {
                MessageBox.Show("No hay datos geográficos disponibles.", "Estadísticas");
                return;
            }

            var (cercanoA, cercanoB, minDist) = grafoGeo.ObtenerParMasCercano();
            var (lejanoA, lejanoB, maxDist) = grafoGeo.ObtenerParMasLejano();
            double promedio = grafoGeo.CalcularDistanciaPromedio();

            string mensaje = $" Estadísticas geográficas:\n\n" +
                             $" Par más cercano:\n{cercanoA.GetName} ↔ {cercanoB.GetName}: {minDist:F2} km\n\n" +
                             $" Par más lejano:\n{lejanoA.GetName} ↔ {lejanoB.GetName}: {maxDist:F2} km\n\n" +
                             $" Distancia promedio entre familiares: {promedio:F2} km";

            MessageBox.Show(mensaje, "Estadísticas del Mapa");
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

            // RUTA CORREGIDA - sin comillas dobles
            string rutaFoto1 = @"C:\Users\Usuario\Desktop\Arroz\Fondos de pantalla\gon.png";

            // Crear personas de prueba
            var persona1 = new Person("Juan Pérez", "00123456", new DateTime(1980, 1, 1), rutaFoto1, 9.934739, -84.087502);
            var persona2 = new Person("María López", "00234567", new DateTime(1985, 5, 15), "", 10.016250, -84.216630);
            var persona3 = new Person("Carlos Rodríguez", "00345678", new DateTime(1975, 8, 20), "", 9.863000, -83.919300);
            var persona4 = new Person("Ana Martínez", "00456789", new DateTime(1990, 3, 10), "", 9.998200, -84.117300);

            personas = new List<Person> { persona1, persona2, persona3, persona4 };
            grafoGeo = new GrafoGeografico(personas);


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


            var textosKm = new List<GMapMarker>();

            // Crear líneas entre todos los pares
            foreach (var origen in overlay.Markers)
            {
                foreach (var destino in overlay.Markers)
                {
                    if (origen != destino &&
                        origen is MarcadorPersonalizado marcadorA &&
                        destino is MarcadorPersonalizado marcadorB)
                    {
                        var personaA = marcadorA.persona;
                        var personaB = marcadorB.persona;

                        // Verifica que exista la distancia entre A y B
                        if (grafoGeo.GetDistancias().TryGetValue(personaA, out var distanciasDesdeA) &&
                            distanciasDesdeA.TryGetValue(personaB, out double distanciaKm))
                        {
                            // Dibujar línea
                                    var ruta = new GMapRoute(new List<PointLatLng>
                        {
                            origen.Position,
                            destino.Position
                        }, "ruta");

                            ruta.Stroke = new Pen(Color.DarkGray, 1);
                            overlay.Routes.Add(ruta);

                            // Calcular punto medio
                            double latMedio = (origen.Position.Lat + destino.Position.Lat) / 2;
                            double lonMedio = (origen.Position.Lng + destino.Position.Lng) / 2;
                            var puntoMedio = new PointLatLng(latMedio, lonMedio);

                            // Crear marcador de texto
                            var textoKm = new TextoEnMapa(puntoMedio, $"{distanciaKm:F1} km");
                            textosKm.Add(textoKm);

                        }
                    }
                }
            }

            foreach (var texto in textosKm)
            {
                overlay.Markers.Add(texto);
            }

            // Agregar capa al mapa
            mapa.Overlays.Add(overlay);

            // Evento click en marcadores
            mapa.OnMarkerClick += (marker, e) =>
            {
                if (marker is MarcadorPersonalizado marcadorPersonalizado)
                {
                    var persona = marcadorPersonalizado.persona;
                    string mensaje = $"Información de la Persona:\n" +
                                     $"Nombre: {persona.GetName}\n" +
                                     $"Cédula: {persona.id}\n" +
                                     $"Coordenadas: {persona.Latitud:F4}, {persona.Longitud:F4}\n\n";

                    if (grafoGeo != null && grafoGeo.GetDistancias().ContainsKey(persona))
                    {
                        mensaje += $"Distancias desde {persona.GetName}:\n";
                        foreach (var kvp in grafoGeo.GetDistancias()[persona])
                        {
                            mensaje += $"- {kvp.Key.GetName}: {kvp.Value:F2} km\n";
                        }
                    }
                    else
                    {
                        mensaje += "No se encontraron distancias geográficas.";
                    }

                    MessageBox.Show(mensaje, "Detalles del Familiar");
                }
            };

        }


        
        private void Mapa_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}