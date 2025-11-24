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
        // =====================================================
        // DECLARACIÓN DE COMPONENTES Y VARIABLES PRIVADAS
        // =====================================================

        // Componente principal del mapa (control GMap)
        protected GMapControl mapa;

        // Lista de personas que se mostrarán en el mapa
        private List<Person> personas;

        // Botones de la interfaz
        protected Button btnEstadisticas;
        protected Button btnRegresar;

        // Grafos para gestión de datos geográficos y genealógicos
        private GrafoGeografico grafoGeo;
        private GrafoGenealogico grafoGenealogico;

        // Propiedades para almacenar coordenadas seleccionadas
        public double LatitudSeleccionada { get; protected set; }
        public double LongitudSeleccionada { get; protected set; }
        public bool CoordenadasSeleccionadas { get; protected set; }

        // =====================================================
        // CONSTRUCTORES
        // =====================================================

        
        // Constructor principal que recibe datos del árbol genealógico
        
       
        public Mapa(List<Person> personasDelArbol, GrafoGenealogico grafoGenealogico)
        {
            // Configuración inicial de estilo y apariencia
            InicializarEstiloBase();

            // Asignación de parámetros a variables de clase
            this.personas = personasDelArbol;
            this.grafoGenealogico = grafoGenealogico;

            // Creación del grafo geográfico si hay personas disponibles
            if (personasDelArbol != null && personasDelArbol.Count > 0)
                this.grafoGeo = new GrafoGeografico(personasDelArbol);

            // Inicialización del componente del mapa
            InicializarMapa();
        }

        public Mapa()
        {
            InicializarEstiloBase();
            InicializarMapa();
        }

        // MÉTODOS DE CONFIGURACIÓN DE ESTILO
      
        // Configura el estilo visual base del formulario
  
        private void InicializarEstiloBase()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;           // Eliminar bordes estándar
            this.WindowState = FormWindowState.Maximized;          // Pantalla completa
            this.BackColor = ColorTranslator.FromHtml("#f5f3eb");  // Color de fondo estilo pergamino
            this.Font = new Font("Segoe UI", 10);                  // Fuente estándar
        }


        //Inicializa y configura los componentes visuales del formulario
        
        private void InitializeComponent()
        {
            // Creación de botones
            this.btnEstadisticas = new Button();
            this.btnRegresar = new Button();
            this.SuspendLayout();

            // --- CONFIGURACIÓN DEL BOTÓN DE ESTADÍSTICAS ---
            this.btnEstadisticas.Text = "Ver estadísticas";
            this.btnEstadisticas.Size = new Size(180, 40);
            this.btnEstadisticas.Location = new Point(20, 20);
            AplicarEstiloBoton(this.btnEstadisticas, "#5b79a1");  // Color azul
            this.btnEstadisticas.Click += new EventHandler(this.BtnEstadisticas_Click);

            // --- CONFIGURACIÓN DEL BOTÓN REGRESAR ---
            this.btnRegresar.Text = "← Regresar";
            this.btnRegresar.Size = new Size(120, 36);
            this.btnRegresar.Location = new Point(20, 70);
            AplicarEstiloBoton(this.btnRegresar, "#a14f4f");      // Color rojo
            this.btnRegresar.Click += new EventHandler(this.BtnRegresar_Click);

            // --- CONFIGURACIÓN FINAL DEL FORMULARIO ---
            this.ClientSize = new Size(1000, 700);
            this.Controls.Add(this.btnEstadisticas);
            this.Controls.Add(this.btnRegresar);
            this.Name = "Mapa";
            this.ResumeLayout(false);
        }

 
        private void AplicarEstiloBoton(Button boton, string colorHex)
        {
            // Configuración básica de colores y estilo
            boton.BackColor = ColorTranslator.FromHtml(colorHex);
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Evento personalizado para dibujar bordes redondeados
            boton.Paint += (s, e) =>
            {
                var b = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;  // Suavizado de bordes

                // Creación de región con esquinas redondeadas
                using (GraphicsPath gp = new GraphicsPath())
                {
                    int r = 12;  // Radio de las esquinas
                    gp.AddArc(0, 0, r, r, 180, 90);
                    gp.AddArc(b.Width - r, 0, r, r, 270, 90);
                    gp.AddArc(b.Width - r, b.Height - r, r, r, 0, 90);
                    gp.AddArc(0, b.Height - r, r, r, 90, 90);
                    gp.CloseAllFigures();
                    b.Region = new Region(gp);  // Aplicar región al botón
                }
            };
        }

        
        // INICIALIZACIÓN Y CONFIGURACIÓN DEL MAPA
       
       // Configura e inicializa el componente del mapa
        
        private void InicializarMapa()
        {
            // Creación del control del mapa
            mapa = new GMapControl();
            mapa.Dock = DockStyle.Fill;                            // Ocupar todo el espacio disponible
            mapa.MinZoom = 2;                                      // Zoom mínimo permitido
            mapa.MaxZoom = 18;                                     // Zoom máximo permitido
            mapa.Zoom = 10;                                        // Zoom inicial
            mapa.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;  // Proveedor de mapas
            mapa.Position = new PointLatLng(9.93, -84.08);         // Posición inicial (Costa Rica)
            mapa.CanDragMap = true;                                // Permitir arrastrar el mapa
            mapa.DragButton = MouseButtons.Left;                   // Botón para arrastrar
            mapa.ShowCenter = false;                               // Ocultar marcador central
            mapa.BringToFront();                                   // Traer al frente

            // Configuración del modo de acceso a mapas
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            this.Controls.Add(mapa);

            // Configurar botones como hijos del mapa
            this.btnEstadisticas.Parent = mapa;
            this.btnRegresar.Parent = mapa;
            this.btnEstadisticas.BringToFront();
            this.btnRegresar.BringToFront();

            // Mostrar personas si existen datos, sino crear overlay vacío
            if (personas != null && personas.Count > 0)
                MostrarPersonasReales();
            else
                mapa.Overlays.Add(new GMapOverlay("marcadores"));
        }

        // =====================================================
        // MANEJADORES DE EVENTOS
        // =====================================================


        // Maneja el clic en el botón de estadísticas
       
        private void BtnEstadisticas_Click(object sender, EventArgs e)
        {
            // Verificar disponibilidad de datos geográficos
            if (grafoGeo == null)
            {
                MessageBox.Show("No hay datos geográficos disponibles.", "Estadísticas");
                return;
            }

            // Obtener estadísticas del grafo geográfico
            var (cercanoA, cercanoB, minDist) = grafoGeo.ObtenerParMasCercano();
            var (lejanoA, lejanoB, maxDist) = grafoGeo.ObtenerParMasLejano();
            double promedio = grafoGeo.CalcularDistanciaPromedio();

            // Construir mensaje con las estadísticas
            string mensaje =
                " Estadísticas geográficas\n\n" +
                $" Par más cercano:\n{cercanoA.GetName} ↔ {cercanoB.GetName}\n   Distancia: {minDist:F2} km\n\n" +
                $" Par más lejano:\n{lejanoA.GetName} ↔ {lejanoB.GetName}\n   Distancia: {maxDist:F2} km\n\n" +
                $" Distancia promedio entre familiares: {promedio:F2} km";

            MessageBox.Show(mensaje, "Estadísticas del mapa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

      
        // Maneja el clic en el botón regresar
       
        private void BtnRegresar_Click(object sender, EventArgs e)
        {
            this.Close();  // Cerrar el formulario actual
        }

        
        // MÉTODOS DE VISUALIZACIÓN DE DATOS EN EL MAPA
       

        
        // Muestra todas las personas y sus relaciones en el mapa
        
        private void MostrarPersonasReales()
        {
            // Limpiar overlays existentes
            mapa.Overlays.Clear();

            // Crear overlays para diferentes tipos de elementos
            var overlayMarcadores = new GMapOverlay("marcadores");
            var overlayRutas = new GMapOverlay("rutas");
            var overlayTextos = new GMapOverlay("textos");

            // Filtrar personas con coordenadas válidas
            var personasConCoordenadas = personas
                .Where(p => p.Latitud != 0 && p.Longitud != 0)
                .ToList();

            // Verificar si hay personas para mostrar
            if (personasConCoordenadas.Count == 0)
            {
                MessageBox.Show("Ninguna persona tiene coordenadas geográficas asignadas.", "Mapa Vacío");
                return;
            }

            // Agregar marcadores para cada persona
            foreach (var persona in personasConCoordenadas)
            {
                var marcador = new MarcadorPersonalizado(persona);
                overlayMarcadores.Markers.Add(marcador);
            }

            // Dibujar rutas y distancias entre personas
            for (int i = 0; i < personasConCoordenadas.Count; i++)
            {
                for (int j = i + 1; j < personasConCoordenadas.Count; j++)
                {
                    var personaA = personasConCoordenadas[i];
                    var personaB = personasConCoordenadas[j];

                    // Verificar si existe distancia calculada entre las personas
                    if (grafoGeo.GetDistancias().TryGetValue(personaA, out var distanciasDesdeA) &&
                        distanciasDesdeA.TryGetValue(personaB, out double distanciaKm))
                    {
                        // Crear ruta entre las dos personas
                        var ruta = new GMapRoute(new List<PointLatLng>
                        {
                            new PointLatLng(personaA.Latitud, personaA.Longitud),
                            new PointLatLng(personaB.Latitud, personaB.Longitud)
                        }, "ruta");

                        // Asignar color según si están relacionadas
                        var color = EstanRelacionadas(personaA, personaB)
                            ? ColorTranslator.FromHtml("#5b79a1")  // Azul para familiares
                            : Color.FromArgb(80, 90, 90, 90);      // Gris tenue para no familiares

                        ruta.Stroke = new Pen(color, 2);
                        overlayRutas.Routes.Add(ruta);

                        // Agregar texto con la distancia si es significativa
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

            // Agregar todos los overlays al mapa
            mapa.Overlays.Add(overlayMarcadores);
            mapa.Overlays.Add(overlayRutas);
            mapa.Overlays.Add(overlayTextos);

            // Centrar el mapa en las personas
            CentrarMapaEnPersonas();

            // Configurar evento de clic en marcadores
            mapa.OnMarkerClick += (marker, e) =>
            {
                if (marker is MarcadorPersonalizado marcadorPersonalizado)
                    MostrarInformacionPersona(marcadorPersonalizado.persona);
            };
        }

 
        // Muestra información detallada de una persona seleccionada
   
      
        private void MostrarInformacionPersona(Person persona)
        {
            // Verificar si existen distancias para esta persona
            if (grafoGeo != null && grafoGeo.GetDistancias().ContainsKey(persona))
            {
                // Mostrar formulario de distancias
                using (var formDistancias = new DistanciasForm(persona, grafoGeo.GetDistancias()[persona]))
                    formDistancias.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se encontraron distancias para esta persona.", "Información");
            }
        }

        // METODOS DE LÓGICA DE RELACIONES FAMILIARES
        

       
        // Determina si dos personas están relacionadas familiarmente
        
        private bool EstanRelacionadas(Person personaA, Person personaB)
        {
            if (grafoGenealogico == null) return false;
            if (personaA == personaB) return false;  // Una persona no está relacionada consigo misma

            // Verificar diferentes tipos de relaciones
            if (personaA.Partner == personaB || personaB.Partner == personaA) return true;  // Son pareja
            if (personaB.Parents?.Contains(personaA) == true || personaA.Parents?.Contains(personaB) == true) return true;  // Relación padre-hijo

            return SonHermanos(personaA, personaB);  // Verificar si son hermanos
        }

      
        // Determina si dos personas son hermanos (comparten al menos un padre)
      
        private bool SonHermanos(Person a, Person b)
        {
            if (a.Parents == null || b.Parents == null) return false;
            return a.Parents.Any(p => p != null && b.Parents.Contains(p));
        }

        // =====================================================
        // MÉTODOS DE UTILIDAD Y CONFIGURACIÓN DEL MAPA
        // =====================================================

        
        // Centra el mapa en el área que contiene todas las personas
        
        private void CentrarMapaEnPersonas()
        {
            if (personas == null || personas.Count == 0) return;

            // Obtener coordenadas válidas
            var latitudes = personas.Where(p => p.Latitud != 0).Select(p => p.Latitud).ToList();
            var longitudes = personas.Where(p => p.Longitud != 0).Select(p => p.Longitud).ToList();

            if (!latitudes.Any() || !longitudes.Any()) return;

            // Calcular límites geográficos
            double minLat = latitudes.Min();
            double maxLat = latitudes.Max();
            double minLng = longitudes.Min();
            double maxLng = longitudes.Max();

            // Calcular punto central
            double centerLat = (minLat + maxLat) / 2;
            double centerLng = (minLng + maxLng) / 2;

            // Posicionar el mapa en el centro calculado
            mapa.Position = new PointLatLng(centerLat, centerLng);
        }


        
        // Evento de carga del formulario (pendiente de implementación)
        
        private void Mapa_Load(object sender, EventArgs e)
        {
            // TODO: Implementar lógica de inicialización adicional si es necesaria
        }


        // Maneja el clic en un botón genérico (pendiente de especificar)
        
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();  // Cerrar el formulario
        }
    }
}