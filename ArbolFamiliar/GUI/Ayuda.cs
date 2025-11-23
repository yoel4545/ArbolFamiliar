using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArbolFamiliar
{
    public partial class Ayuda : Form
    {
        private TabControl tabControl;
        private Button btnCerrar;

        public Ayuda()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            CrearInterfazAyuda();
        }

        private void CrearInterfazAyuda()
        {
            this.BackColor = Color.FromArgb(240, 245, 249);

            // Título principal
            Label lblTitulo = new Label()
            {
                Text = "Centro de Ayuda - Árbol Genealógico Familiar",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 20),
                AutoSize = true
            };

            // TabControl para organizar secciones
            tabControl = new TabControl()
            {
                Location = new Point(50, 80),
                Size = new Size(1400, 600),
                Font = new Font("Segoe UI", 10)
            };

            // Crear pestañas
            CrearTabIntroduccion();
            CrearTabArbolGenealogico();
            CrearTabMapaInteractivo();
            CrearTabConsejos();
            CrearTabProblemas();

            // Botón cerrar
            btnCerrar = new Button()
            {
                Text = "Cerrar Ayuda",
                Location = new Point(650, 700),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitulo, tabControl, btnCerrar });
        }

        private void CrearTabIntroduccion()
        {
            TabPage tabIntro = new TabPage("Introducción");

            RichTextBox txtContenido = new RichTextBox()
            {
                Location = new Point(20, 20),
                Size = new Size(1350, 540),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            string contenido = @"BIENVENIDO AL SISTEMA DE ÁRBOL GENEALÓGICO

Esta aplicación te permite crear y visualizar tu árbol genealógico de manera intuitiva, 
con funcionalidades avanzadas de mapeo y análisis familiar.

PRINCIPALES FUNCIONALIDADES:

• Crear y gestionar un árbol genealógico
• Visualizar relaciones familiares de forma gráfica
• Ubicar familiares en mapas interactivos
• Calcular distancias entre familiares
• Analizar estadísticas geográficas

ESTRUCTURA DE LA APLICACIÓN:

1. PANTALLA PRINCIPAL - Navegación entre funcionalidades
2. ÁRBOL GENEALÓGICO - Creación y edición de familiares
3. MAPA INTERACTIVO - Visualización geográfica
4. CENTRO DE AYUDA - Esta pantalla

Siempre comienza desde el nodo 'N/A' que se crea automáticamente 
y ve construyendo tu árbol desde ahí.";

            txtContenido.Text = contenido;
            tabIntro.Controls.Add(txtContenido);
            tabControl.TabPages.Add(tabIntro);
        }

        private void CrearTabArbolGenealogico()
        {
            TabPage tabArbol = new TabPage("Árbol Genealógico");

            RichTextBox txtContenido = new RichTextBox()
            {
                Location = new Point(20, 20),
                Size = new Size(1350, 540),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            string contenido = @"CÓMO USAR EL ÁRBOL GENEALÓGICO

PASO A PASO:

1. INICIAR EL ÁRBOL:
   • Al abrir el árbol genealógico, verás un nodo 'N/A'
   • Este es tu punto de partida

2. SELECCIONAR PERSONAS:
   • Haz CLICK sobre cualquier persona para seleccionarla
   • La persona seleccionada se muestra en el panel lateral

3. AGREGAR FAMILIARES:
   • Con una persona seleccionada, usa los botones del panel lateral:
     - 'Agregar Hijo': Añade un hijo a la persona seleccionada
     - 'Agregar Pareja': Añade una pareja a la persona seleccionada  
     - 'Agregar Padre': Añade un padre a la persona seleccionada

4. COMPLETAR INFORMACIÓN:
   • Al agregar cualquier familiar, se abre un formulario
   • Completa todos los campos obligatorios:
     - Nombre completo
     - Número de cédula
     - Fecha de nacimiento
     - Estado (vivo/fallecido)
     - Coordenadas geográficas (usando PUNTO para decimales)
     - Fotografía (opcional)

5. NAVEGACIÓN EN EL ÁRBOL:
   • ZOOM: Usa la rueda del mouse para acercar/alejar
   • MOVIMIENTO: Arrastra con click izquierdo para moverte
   • SELECCIÓN: Click en cualquier nodo para ver detalles

CONSEJOS IMPORTANTES:

• Las coordenadas usan PUNTO decimal (ej: 9.9347, -84.0875)
• Puedes editar información existente con 'Cambiar Información'";

            txtContenido.Text = contenido;
            tabArbol.Controls.Add(txtContenido);
            tabControl.TabPages.Add(tabArbol);
        }

        private void CrearTabMapaInteractivo()
        {
            TabPage tabMapa = new TabPage("Mapa Interactivo");

            RichTextBox txtContenido = new RichTextBox()
            {
                Location = new Point(20, 20),
                Size = new Size(1350, 540),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            string contenido = @"MAPA INTERACTIVO DE FAMILIARES

FUNCIONALIDADES DEL MAPA:

• VISUALIZACIÓN GEOGRÁFICA:
  - Cada familiar aparece como un marcador en su ubicación
  - Las líneas conectan a los familiares entre sí
  - Las líneas muestran distancias en kilómetros

• INTERACCIÓN CON MARCADORES:
  - Haz CLICK en cualquier foto de familiar en el mapa
  - Se abrirá una ventana con TODAS sus distancias a otros familiares
  - Verás lista con nombres y distancias precisas

• ESTADÍSTICAS GEOGRÁFICAS:
  - Usa el botón 'Ver estadísticas' para obtener:
     Par de familiares más cercano
     Par de familiares más lejano  
     Distancia promedio entre todos

• NAVEGACIÓN EN EL MAPA:
  - ZOOM: Rueda del mouse
  - MOVIMIENTO: Arrastrar con click izquierdo
  - CENTRADO: Automático en tus familiares";

            txtContenido.Text = contenido;
            tabMapa.Controls.Add(txtContenido);
            tabControl.TabPages.Add(tabMapa);
        }

        private void CrearTabConsejos()
        {
            TabPage tabConsejos = new TabPage("Consejos");

            RichTextBox txtContenido = new RichTextBox()
            {
                Location = new Point(20, 20),
                Size = new Size(1350, 540),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            string contenido = @"CONSEJOS Y MEJORES PRÁCTICAS

• FORMATO CORRECTO:
  - Usa siempre PUNTO decimal: 9.9347, -84.0875
  - Latitud: entre -90 y 90 (ej: 9.9347)
  - Longitud: entre -180 y 180 (ej: -84.0875)";

            txtContenido.Text = contenido;
            tabConsejos.Controls.Add(txtContenido);
            tabControl.TabPages.Add(tabConsejos);
        }

        private void CrearTabProblemas()
        {
            TabPage tabProblemas = new TabPage("Problemas Comunes");

            RichTextBox txtContenido = new RichTextBox()
            {
                Location = new Point(20, 20),
                Size = new Size(1350, 540),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            string contenido = @" SOLUCIÓN DE PROBLEMAS COMUNES

ERROR: 'La cadena de entrada no tiene el formato correcto'
• PROBLEMA: Formato incorrecto en coordenadas
• SOLUCIÓN: Usa PUNTO decimal, no coma (9.9347, no 9,9347)

ERROR: 'Latitud/Longitud inválida'
• PROBLEMA: Valores fuera de rango
• SOLUCIÓN: 
  - Latitud debe estar entre -90 y 90
  - Longitud entre -180 y 180

PROBLEMA: No puedo agregar más padres
• SOLUCIÓN: Cada persona puede tener máximo 2 padres
• Verifica que no estés intentando agregar un tercer padre

PROBLEMA: No puedo agregar pareja
• SOLUCIÓN: 
  - La persona ya tiene pareja (solo 1 permitida)

PROBLEMA: El árbol se ve desordenado
• SOLUCIÓN: 
  - Usa la función de zoom y arrastre para reorganizar vista
  - La aplicación ajusta automáticamente las posiciones

PROBLEMA: No veo a todos en el mapa
• SOLUCIÓN:
  - Verifica que todos tengan coordenadas asignadas
  - Coordenadas (0,0) no se muestran en el mapa



La aplicación está diseñada para ser intuitiva, pero si encuentras 
otros problemas, revisa que estés siguiendo los pasos correctamente.";

            txtContenido.Text = contenido;
            tabProblemas.Controls.Add(txtContenido);
            tabControl.TabPages.Add(tabProblemas);
        }


        private void Ayuda_Load(object sender, EventArgs e)
        {

        }
    }
}