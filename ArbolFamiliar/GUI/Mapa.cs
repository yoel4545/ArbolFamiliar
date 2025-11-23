using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class Mapa : Form
    {
        protected GMapControl mapa;
        private List<Person> personas;
        protected Button btnEstadisticas;
        protected Button btnRegresar;
        private GrafoGeografico grafoGeo;
        private GrafoGenealogico grafoGenealogico;

        public double LatitudSeleccionada { get; protected set; }
        public double LongitudSeleccionada { get; protected set; }
        public bool CoordenadasSeleccionadas { get; protected set; }

        public Mapa(List<Person> personasDelArbol, GrafoGenealogico grafoGenealogico)
        {
            InicializarEstiloBase();
            this.personas = personasDelArbol;
            this.grafoGenealogico = grafoGenealogico;

            if (personasDelArbol != null && personasDelArbol.Count > 0)
                this.grafoGeo = new GrafoGeografico(personasDelArbol);

            InicializarMapa();
        }

        public Mapa()
        {
            InicializarEstiloBase();
            InicializarMapa();
        }

        // --- ESTILO BASE GENERAL ---
        private void InicializarEstiloBase()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorTranslator.FromHtml("#f5f3eb"); // tono pergamino
            this.Font = new Font("Segoe UI", 10);
        }

        private void InitializeComponent()
        {
            this.btnEstadisticas = new Button();
            this.btnRegresar = new Button();
            this.SuspendLayout();

            // --- BOTÓN DE ESTADÍSTICAS ---
            this.btnEstadisticas.Text = "Ver estadísticas";
            this.btnEstadisticas.Size = new Size(180, 40);
            this.btnEstadisticas.Location = new Point(20, 20);
            AplicarEstiloBoton(this.btnEstadisticas, "#5b79a1");
            this.btnEstadisticas.Click += new EventHandler(this.BtnEstadisticas_Click);

            // --- BOTÓN REGRESAR ---
            this.btnRegresar.Text = "← Regresar";
            this.btnRegresar.Size = new Size(120, 36);
            this.btnRegresar.Location = new Point(20, 70);
            AplicarEstiloBoton(this.btnRegresar, "#a14f4f");
            this.btnRegresar.Click += new EventHandler(this.BtnRegresar_Click);

            // --- FORM ---
            this.ClientSize = new Size(1000, 700);
            this.Controls.Add(this.btnEstadisticas);
            this.Controls.Add(this.btnRegresar);
            this.Name = "Mapa";
            this.ResumeLayout(false);
        }

        private void AplicarEstiloBoton(Button boton, string colorHex)
        {
            boton.BackColor = ColorTranslator.FromHtml(colorHex);
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            boton.Paint += (s, e) =>
            {
                var b = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    int r = 12;
                    gp.AddArc(0, 0, r, r, 180, 90);
                    gp.AddArc(b.Width - r, 0, r, r, 270, 90);
                    gp.AddArc(b.Width - r, b.Height - r, r, r, 0, 90);
                    gp.AddArc(0, b.Height - r, r, r, 90, 90);
                    gp.CloseAllFigures();
                    b.Region = new Region(gp);
                }
            };
        }

        // --- INICIALIZACIÓN DEL MAPA ---
        private void InicializarMapa()
        {
            mapa = new GMapControl();
            mapa.Dock = DockStyle.Fill;
            mapa.MinZoom = 2;
            mapa.MaxZoom = 18;
            mapa.Zoom = 10;
            mapa.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            mapa.Position = new PointLatLng(9.93, -84.08); // Costa Rica
            mapa.CanDragMap = true;
            mapa.DragButton = MouseButtons.Left;
            mapa.ShowCenter = false;
            mapa.BringToFront();

            GMaps.Instance.Mode = AccessMode.ServerOnly;
            this.Controls.Add(mapa);

            // Mover botones encima del mapa
            this.btnEstadisticas.Parent = mapa;
            this.btnRegresar.Parent = mapa;
            this.btnEstadisticas.BringToFront();
            this.btnRegresar.BringToFront();

            if (personas != null && personas.Count > 0)
                MostrarPersonasReales();
            else
                mapa.Overlays.Add(new GMapOverlay("marcadores"));
        }

        // --- EVENTOS ---
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

            string mensaje =
                "📍 Estadísticas geográficas\n\n" +
                $"🔸 Par más cercano:\n{cercanoA.GetName} ↔ {cercanoB.GetName}\n   Distancia: {minDist:F2} km\n\n" +
                $"🔹 Par más lejano:\n{lejanoA.GetName} ↔ {lejanoB.GetName}\n   Distancia: {maxDist:F2} km\n\n" +
                $"🌍 Distancia promedio entre familiares: {promedio:F2} km";

            MessageBox.Show(mensaje, "Estadísticas del mapa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRegresar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // --- DIBUJAR PERSONAS Y RELACIONES ---
        private void MostrarPersonasReales()
        {
            mapa.Overlays.Clear();
            var overlayMarcadores = new GMapOverlay("marcadores");
            var overlayRutas = new GMapOverlay("rutas");
            var overlayTextos = new GMapOverlay("textos");

            var personasConCoordenadas = personas
                .Where(p => p.Latitud != 0 && p.Longitud != 0)
                .ToList();

            if (personasConCoordenadas.Count == 0)
            {
                MessageBox.Show("Ninguna persona tiene coordenadas geográficas asignadas.", "Mapa Vacío");
                return;
            }

            foreach (var persona in personasConCoordenadas)
            {
                var marcador = new MarcadorPersonalizado(persona);
                overlayMarcadores.Markers.Add(marcador);
            }

            for (int i = 0; i < personasConCoordenadas.Count; i++)
            {
                for (int j = i + 1; j < personasConCoordenadas.Count; j++)
                {
                    var personaA = personasConCoordenadas[i];
                    var personaB = personasConCoordenadas[j];

                    if (grafoGeo.GetDistancias().TryGetValue(personaA, out var distanciasDesdeA) &&
                        distanciasDesdeA.TryGetValue(personaB, out double distanciaKm))
                    {
                        var ruta = new GMapRoute(new List<PointLatLng>
                        {
                            new PointLatLng(personaA.Latitud, personaA.Longitud),
                            new PointLatLng(personaB.Latitud, personaB.Longitud)
                        }, "ruta");

                        var color = EstanRelacionadas(personaA, personaB)
                            ? ColorTranslator.FromHtml("#5b79a1")
                            : Color.FromArgb(80, 90, 90, 90);

                        ruta.Stroke = new Pen(color, 2);
                        overlayRutas.Routes.Add(ruta);

                        if (distanciaKm > 1)
                        {
                            double latMedio = (personaA.Latitud + personaB.Latitud) / 2;
                            double lonMedio = (personaA.Longitud + personaB.Longitud) / 2;
                            overlayTextos.Markers.Add(new TextoEnMapa(new PointLatLng(latMedio, lonMedio),
                                $"{distanciaKm:F1} km"));
                        }
                    }
                }
            }

            mapa.Overlays.Add(overlayMarcadores);
            mapa.Overlays.Add(overlayRutas);
            mapa.Overlays.Add(overlayTextos);

            CentrarMapaEnPersonas();

            mapa.OnMarkerClick += (marker, e) =>
            {
                if (marker is MarcadorPersonalizado marcadorPersonalizado)
                    MostrarInformacionPersona(marcadorPersonalizado.persona);
            };
        }

        private void MostrarInformacionPersona(Person persona)
        {
            if (grafoGeo != null && grafoGeo.GetDistancias().ContainsKey(persona))
            {
                using (var formDistancias = new DistanciasForm(persona, grafoGeo.GetDistancias()[persona]))
                    formDistancias.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se encontraron distancias para esta persona.", "Información");
            }
        }

        private bool EstanRelacionadas(Person personaA, Person personaB)
        {
            if (grafoGenealogico == null) return false;
            if (personaA == personaB) return false;
            if (personaA.Partner == personaB || personaB.Partner == personaA) return true;
            if (personaB.Parents?.Contains(personaA) == true || personaA.Parents?.Contains(personaB) == true) return true;
            return SonHermanos(personaA, personaB);
        }

        private bool SonHermanos(Person a, Person b)
        {
            if (a.Parents == null || b.Parents == null) return false;
            return a.Parents.Any(p => p != null && b.Parents.Contains(p));
        }

        private void CentrarMapaEnPersonas()
        {
            if (personas == null || personas.Count == 0) return;

            var latitudes = personas.Where(p => p.Latitud != 0).Select(p => p.Latitud);
            var longitudes = personas.Where(p => p.Longitud != 0).Select(p => p.Longitud);

            if (!latitudes.Any() || !longitudes.Any()) return;

            double centerLat = (latitudes.Min() + latitudes.Max()) / 2;
            double centerLng = (longitudes.Min() + longitudes.Max()) / 2;
            mapa.Position = new PointLatLng(centerLat, centerLng);
        }
    }
}
