using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class Mapa : Form
    {
        private GMapControl mapa;
        private List<Person> personas;
        private Button btnEstadisticas;
        private Button button1;
        private GrafoGeografico grafoGeo;
        private GrafoGenealogico grafoGenealogico; 

        public double LatitudSeleccionada { get; private set; }
        public double LongitudSeleccionada { get; private set; }
        public bool CoordenadasSeleccionadas { get; private set; }

    
        public Mapa(List<Person> personasDelArbol, GrafoGenealogico grafoGenealogico)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            
            this.personas = personasDelArbol;
            this.grafoGenealogico = grafoGenealogico;

            if (personasDelArbol != null && personasDelArbol.Count > 0)
            {
                this.grafoGeo = new GrafoGeografico(personasDelArbol);
            }

            InicializarMapa();
        }

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

            // btnEstadisticas
            this.btnEstadisticas.Location = new System.Drawing.Point(10, 10);
            this.btnEstadisticas.Name = "btnEstadisticas";
            this.btnEstadisticas.Size = new System.Drawing.Size(150, 30);
            this.btnEstadisticas.TabIndex = 0;
            this.btnEstadisticas.Text = "Ver estadísticas";
            this.btnEstadisticas.Click += new System.EventHandler(this.BtnEstadisticas_Click);

            // button1
            this.button1.Location = new System.Drawing.Point(13, 47);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(147, 26);
            this.button1.TabIndex = 1;
            this.button1.Text = "Regresar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);

            // Mapa
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
            mapa.Position = new PointLatLng(9.93, -84.08); // Costa Rica por defecto
            mapa.CanDragMap = true;
            mapa.DragButton = MouseButtons.Left;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            mapa.ShowCenter = false;
            this.Controls.Add(mapa);

            // Mostrar personas si existen
            if (personas != null && personas.Count > 0)
            {
                MostrarPersonasReales();
            }
            else
            {
                // Solo mostrar el mapa vacío, sin datos de prueba
                var overlay = new GMapOverlay("marcadores");
                mapa.Overlays.Add(overlay);
            }
        }


        private void MostrarPersonasReales()
        {
            mapa.Overlays.Clear();

            var overlayMarcadores = new GMapOverlay("marcadores");
            var overlayRutas = new GMapOverlay("rutas");
            var overlayTextos = new GMapOverlay("textos");

            // Filtrar personas con coordenadas válidas
            var personasConCoordenadas = new List<Person>();
            foreach (var persona in personas)
            {
                if (persona.Latitud != 0 && persona.Longitud != 0)
                {
                    personasConCoordenadas.Add(persona);
                }
            }

            if (personasConCoordenadas.Count == 0)
            {
                MessageBox.Show("Ninguna persona tiene coordenadas geográficas asignadas.", "Mapa Vacío");
                return;
            }

            // Crear marcadores para cada persona con coordenadas
            foreach (var persona in personasConCoordenadas)
            {
                var marcador = new MarcadorPersonalizado(persona);
                overlayMarcadores.Markers.Add(marcador);
            }

            // MODIFICACIÓN: Crear líneas entre TODOS los pares de personas
            for (int i = 0; i < personasConCoordenadas.Count; i++)
            {
                for (int j = i + 1; j < personasConCoordenadas.Count; j++)
                {
                    var personaA = personasConCoordenadas[i];
                    var personaB = personasConCoordenadas[j];

                    if (grafoGeo.GetDistancias().TryGetValue(personaA, out var distanciasDesdeA) &&
                        distanciasDesdeA.TryGetValue(personaB, out double distanciaKm))
                    {
                        // Dibujar línea entre todas las personas
                        var ruta = new GMapRoute(new List<PointLatLng>
                {
                    new PointLatLng(personaA.Latitud, personaA.Longitud),
                    new PointLatLng(personaB.Latitud, personaB.Longitud)
                }, "ruta");

                        // Color diferente según el tipo de relación (si están relacionadas)
                        var color = EstanRelacionadas(personaA, personaB) ?
                                   ObtenerColorRelacion(personaA, personaB) :
                                   Color.LightGray; // Color gris claro para no relacionados

                        ruta.Stroke = new Pen(color, 2);
                        overlayRutas.Routes.Add(ruta);

                        // Texto con distancia (solo para distancias significativas)
                        if (distanciaKm > 1)
                        {
                            double latMedio = (personaA.Latitud + personaB.Latitud) / 2;
                            double lonMedio = (personaA.Longitud + personaB.Longitud) / 2;
                            var textoKm = new TextoEnMapa(new PointLatLng(latMedio, lonMedio), $"{distanciaKm:F1} km");
                            overlayTextos.Markers.Add(textoKm);
                        }
                    }
                }
            }

            // Agregar todas las capas al mapa
            mapa.Overlays.Add(overlayMarcadores);
            mapa.Overlays.Add(overlayRutas);
            mapa.Overlays.Add(overlayTextos);

            // Centrar el mapa en las personas
            CentrarMapaEnPersonas();

            // Evento click en marcadores
            mapa.OnMarkerClick += (marker, e) =>
            {
                if (marker is MarcadorPersonalizado marcadorPersonalizado)
                {
                    var persona = marcadorPersonalizado.persona;
                    MostrarInformacionPersona(persona);
                }
            };
        }
        private void MostrarInformacionPersona(Person persona)
        {
            string mensaje = $"Información de la Persona:\n" +
                             $"Nombre: {persona.GetName}\n" +
                             $"Cédula: {persona.id}\n" +
                             $"Coordenadas: {persona.Latitud:F4}, {persona.Longitud:F4}\n\n";

            if (grafoGeo != null && grafoGeo.GetDistancias().ContainsKey(persona))
            {
                mensaje += $"Distancias a familiares:\n";
                foreach (var kvp in grafoGeo.GetDistancias()[persona])
                {
                    if (EstanRelacionadas(persona, kvp.Key))
                    {
                        string tipoRelacion = ObtenerTipoRelacion(persona, kvp.Key);
                        mensaje += $"- {kvp.Key.GetName} ({tipoRelacion}): {kvp.Value:F2} km\n";
                    }
                }
            }
            else
            {
                mensaje += "No se encontraron distancias geográficas.";
            }

            MessageBox.Show(mensaje, "Detalles del Familiar");
        }


        private bool EstanRelacionadas(Person personaA, Person personaB)
        {
            if (grafoGenealogico == null)
                return false;

            // 1. Son la misma persona
            if (personaA == personaB)
                return false;

            // 2. Son pareja
            if (personaA.Partner == personaB || personaB.Partner == personaA)
                return true;

            // 3. Relación padre-hijo (verificando AMBOS lados de forma más robusta)
            bool aEsPadreDeB = personaB.Parents != null &&
                               personaB.Parents.Contains(personaA);
            bool bEsPadreDeA = personaA.Parents != null &&
                               personaA.Parents.Contains(personaB);

            if (aEsPadreDeB || bEsPadreDeA)
                return true;

            // 4. Son hermanos (comparten al menos un padre)
            if (SonHermanos(personaA, personaB))
                return true;

            return false;
        }

        
        private bool SonHermanos(Person personaA, Person personaB)
        {
            if (personaA.Parents == null || personaB.Parents == null)
                return false;

            // Verificar si comparten al menos un padre
            foreach (var padreA in personaA.Parents)
            {
                if (padreA != null && personaB.Parents.Contains(padreA))
                    return true;
            }
            return false;
        }
        private Color ObtenerColorRelacion(Person personaA, Person personaB)
        {
            if (personaA.Partner == personaB || personaB.Partner == personaA)
                return Color.Black; // Pareja - Rojo

            if ((personaA.Parents != null && Array.Exists(personaA.Parents, p => p == personaB)) ||
                (personaB.Parents != null && Array.Exists(personaB.Parents, p => p == personaA)))
                return Color.Black; // Padre/Hijo - Azul

            if (SonHermanos(personaA, personaB))
                return Color.Black; // Hermanos - Verde

            return Color.Black; // Otro tipo de relación - Gris
        }
        private string ObtenerTipoRelacion(Person personaA, Person personaB)
        {
            if (personaA.Partner == personaB || personaB.Partner == personaA)
                return "Pareja";

            if (personaA.Parents != null && Array.Exists(personaA.Parents, p => p == personaB))
                return "Padre/Madre";

            if (personaB.Parents != null && Array.Exists(personaB.Parents, p => p == personaA))
                return "Hijo/Hija";

            if (SonHermanos(personaA, personaB))
                return "Hermano/Hermana";

            return "Familiar";
        }
        
        private void CentrarMapaEnPersonas()
        {
            if (personas == null || personas.Count == 0) return;

            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLng = double.MaxValue, maxLng = double.MinValue;

            foreach (var persona in personas)
            {
                if (persona.Latitud != 0 && persona.Longitud != 0)
                {
                    minLat = Math.Min(minLat, persona.Latitud);
                    maxLat = Math.Max(maxLat, persona.Latitud);
                    minLng = Math.Min(minLng, persona.Longitud);
                    maxLng = Math.Max(maxLng, persona.Longitud);
                }
            }

            if (minLat != double.MaxValue)
            {
                var centerLat = (minLat + maxLat) / 2;
                var centerLng = (minLng + maxLng) / 2;
                mapa.Position = new PointLatLng(centerLat, centerLng);
            }
        }

       //esto era antes
        private void MostrarDatosDePrueba()
        {
         
            string rutaFoto1 = @"C:\Users\Usuario\Desktop\Arroz\Fondos de pantalla\gon.png";

            // Crear personas de prueba
            var persona1 = new Person("Juan Pérez", "00123456", new DateTime(1980, 1, 1), rutaFoto1, 9.934739, -84.087502);
            var persona2 = new Person("María López", "00234567", new DateTime(1985, 5, 15), "", 10.016250, -84.216630);
            var persona3 = new Person("Carlos Rodríguez", "00345678", new DateTime(1975, 8, 20), "", 9.863000, -83.919300);
            var persona4 = new Person("Ana Martínez", "00456789", new DateTime(1990, 3, 10), "", 9.998200, -84.117300);

            personas = new List<Person> { persona1, persona2, persona3, persona4 };
            grafoGeo = new GrafoGeografico(personas);

            // Para datos de prueba, no tenemos grafo genealógico, así que mostrar todas las conexiones
            MostrarPersonasReales();
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