using System;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace ArbolFamiliar
{
    public class MarcadorPersonalizado : GMapMarker
    {
        public Persona persona;
        private Image foto;

        public MarcadorPersonalizado(Persona persona)
            : base(new PointLatLng(persona.Latitud, persona.Longitud))
        {
            this.persona = persona;

            // Cargar la foto
            try
            {
                if (System.IO.File.Exists(persona.fotoPath))
                {
                    foto = Image.FromFile(persona.fotoPath);
                }
                else
                {
                    // Si no hay foto, crear una simple
                    foto = CrearFotoPorDefecto();
                }
            }
            catch
            {
                foto = CrearFotoPorDefecto();
            }

            // Tamaño del marcador
            Size = new Size(50, 60);

            // Tooltip con información
            this.ToolTipText = $"{persona.GetName}\nCédula: {persona.id}";
        }

        private Image CrearFotoPorDefecto()
        {
            Bitmap imagen = new Bitmap(40, 40);
            using (Graphics g = Graphics.FromImage(imagen))
            {
                g.Clear(Color.LightGray);
                g.DrawRectangle(Pens.Black, 0, 0, 39, 39);
            }
            return imagen;
        }

        public override void OnRender(Graphics g)
        {
            // Dibujar fondo blanco
            g.FillRectangle(Brushes.White, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);

            // Dibujar borde negro
            g.DrawRectangle(Pens.Black, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);

            // Dibujar foto
            g.DrawImage(foto, LocalPosition.X + 5, LocalPosition.Y + 5, 40, 40);
        }
    }
}