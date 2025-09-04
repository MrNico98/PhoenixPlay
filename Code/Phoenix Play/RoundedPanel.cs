using Phoenix_Play.Classi;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix_Play
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 20;
        public string HeaderText { get; set; } = "";

        public RoundedPanel()
        {
            this.BackColor = Color.LightGray;
            this.Padding = new Padding(10, 30, 10, 10);
            this.Resize += (s, e) =>
            {
                Rectangle rect = this.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                this.Region = new Region(UIHelper.GetRoundedRect(rect, CornerRadius));
                this.Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.BackColor);

            Rectangle rect = this.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using (GraphicsPath path = UIHelper.GetRoundedRect(rect, CornerRadius))
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }
            if (!string.IsNullOrEmpty(HeaderText))
            {
                SizeF textSize = e.Graphics.MeasureString(HeaderText, this.Font);
                e.Graphics.DrawString(HeaderText, this.Font, Brushes.Black, new PointF(10, 0));
            }
        }
    }
}
