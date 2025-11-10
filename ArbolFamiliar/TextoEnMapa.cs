using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace ArbolFamiliar
{
    public class TextoEnMapa : GMapMarker
    {
        private string texto;
        private Font fuente;
        private Brush pincel;

        public TextoEnMapa(PointLatLng posicion, string textoKm)
            : base(posicion)
        {
            texto = textoKm;
            fuente = new Font("Arial", 10, FontStyle.Bold);
            pincel = Brushes.Black;
        }

        public override void OnRender(Graphics g)
        {
            SizeF tamaño = g.MeasureString(texto, fuente);
            Point punto = new Point(LocalPosition.X - (int)(tamaño.Width / 2), LocalPosition.Y - (int)(tamaño.Height / 2));
            g.DrawString(texto, fuente, pincel, punto);
        }
    }
}
