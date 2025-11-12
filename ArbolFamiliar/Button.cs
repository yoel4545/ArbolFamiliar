using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbolFamiliar
{
    public class Button
    {
        public Rectangle Bounds { get; private set; }
        public string Text { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; }
        public Font Font { get; set; }
        public Action OnClick { get; set; } 

        public Button(Rectangle bounds, string text, Action onClick = null)
        {
            Bounds = bounds;
            Text = text;
            BackgroundColor = Color.FromArgb(200, 220, 255);
            BorderColor = Color.DarkBlue;
            TextColor = Color.Black;
            Font = new Font("Arial", 10);
            OnClick = onClick;
        }

        public void Draw(Graphics g)
        {
            using (SolidBrush bg = new SolidBrush(BackgroundColor))
                g.FillRectangle(bg, Bounds);

            using (Pen border = new Pen(BorderColor, 2))
                g.DrawRectangle(border, Bounds);

            using (Brush textBrush = new SolidBrush(TextColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(Text, Font, textBrush, Bounds, sf);
            }
        }

        public bool Contains(Point p) => Bounds.Contains(p);

        public void Click()
        {
            OnClick?.Invoke();
        }
    }
}

