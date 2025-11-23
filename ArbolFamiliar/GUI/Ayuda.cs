using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            ConfigurarEstiloVentana();
            CrearInterfazAyuda();
        }

        private void ConfigurarEstiloVentana()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = ColorTranslator.FromHtml("#f5f3eb"); // tono pergamino
            this.Font = new Font("Segoe UI", 10);
        }

        private void CrearInterfazAyuda()
        {
            // Fondo base y estilo general
            this.BackColor = ColorTranslator.FromHtml("#f5f3eb");

            // --- Título principal ---
            Label lblTitulo = new Label()
            {
                Text = "Centro de Ayuda - Árbol Genealógico Familiar",
                Font = new Font("Garamond", 22, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#3f5030"),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };
            this.Controls.Add(lblTitulo);

            // --- TabControl central ---
            tabControl = new TabControl()
            {
                Location = new Point(60, 100),
                Size = new Size(1400, 620),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Appearance = TabAppearance.Normal
            };

            // Colores suaves para las pestañas
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += (s, e) =>
            {
                TabPage tab = tabControl.TabPages[e.Index];
                Rectangle rect = e.Bounds;

                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color backColor = selected ? ColorTranslator.FromHtml("#5b79a1") : Color.FromArgb(230, 230, 230);
                Color foreColor = selected ? Color.White : ColorTranslator.FromHtml("#3f5030");

                using (SolidBrush br = new SolidBrush(backColor))
                    e.Graphics.FillRectangle(br, rect);

                using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(tab.Text, tabControl.Font, new SolidBrush(foreColor), rect, sf);
            };

            CrearTabIntroduccion();
            CrearTabArbolGenealogico();
            CrearTabMapaInteractivo();
            CrearTabConsejos();
            CrearTabProblemas();
            this.Controls.Add(tabControl);

            // --- Botón cerrar ---
            btnCerrar = CrearBotonEstilizado("← Regresar", "#a14f4f", 650, 740, 160, 44);
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);
        }

        private Button CrearBotonEstilizado(string texto, string colorHex, int x, int y, int w, int h)
        {
            Button b = new Button()
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = ColorTranslator.FromHtml(colorHex),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            b.FlatAppearance.BorderSize = 0;

            b.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    int r = 14;
                    gp.AddArc(0, 0, r, r, 180, 90);
                    gp.AddArc(btn.Width - r, 0, r, r, 270, 90);
                    gp.AddArc(btn.Width - r, btn.Height - r, r, r, 0, 90);
                    gp.AddArc(0, btn.Height - r, r, r, 90, 90);
                    gp.CloseAllFigures();
                    btn.Region = new Region(gp);
                }
            };
            return b;
        }

        // --- Secciones del TabControl ---

        private void CrearTabIntroduccion()
        {
            AgregarPestaña("Introducción", @"BIENVENIDO AL SISTEMA DE ÁRBOL GENEALÓGICO

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
y ve construyendo tu árbol desde ahí.");
        }

        private void CrearTabArbolGenealogico()
        {
            AgregarPestaña("Árbol Genealógico", @"CÓMO USAR EL ÁRBOL GENEALÓGICO

PASO A PASO:
1. INICIAR EL ÁRBOL:
   • Al abrir el árbol, verás un nodo 'N/A' como punto de partida.
2. SELECCIONAR PERSONAS:
   • Haz CLICK sobre una persona para seleccionarla.
3. AGREGAR FAMILIARES:
   • Usa los botones del panel lateral:
     - 'Agregar Hijo', 'Agregar Pareja', 'Agregar Padre'.
4. COMPLETAR INFORMACIÓN:
   • Completa nombre, cédula, fechas y coordenadas.
5. NAVEGACIÓN:
   • ZOOM con la rueda del mouse.
   • ARRÁSTRA el árbol con clic izquierdo.
   • SELECCIONA nodos para ver detalles.");
        }

        private void CrearTabMapaInteractivo()
        {
            AgregarPestaña("Mapa Interactivo", @"MAPA INTERACTIVO DE FAMILIARES

FUNCIONALIDADES:
• Cada familiar aparece como un marcador.
• Las líneas conectan familiares y muestran distancias.
• Click en un marcador → ver distancias con otros familiares.

ESTADÍSTICAS:
• Botón 'Ver estadísticas':
  - Par más cercano y más lejano.
  - Distancia promedio entre familiares.");
        }

        private void CrearTabConsejos()
        {
            AgregarPestaña("Consejos", @"CONSEJOS Y BUENAS PRÁCTICAS

• Usa siempre punto decimal en coordenadas (9.9347, -84.0875)
• Latitud: -90 a 90
• Longitud: -180 a 180
• Guarda los datos antes de cerrar la app.");
        }

        private void CrearTabProblemas()
        {
            AgregarPestaña("Problemas Comunes", @"SOLUCIÓN DE PROBLEMAS

• 'Formato incorrecto': usa punto decimal.
• 'Latitud inválida': revisa rango de coordenadas.
• No puedo agregar padre: máximo dos padres por persona.
• No aparece en el mapa: debe tener coordenadas asignadas.");
        }

        private void AgregarPestaña(string titulo, string contenido)
        {
            TabPage tab = new TabPage(titulo);
            RichTextBox texto = new RichTextBox()
            {
                Text = contenido,
                Font = new Font("Segoe UI", 11),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            tab.Controls.Add(texto);
            tabControl.TabPages.Add(tab);
        }
    }
}
