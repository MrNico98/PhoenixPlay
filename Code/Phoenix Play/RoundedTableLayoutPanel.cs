using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix_Play
{
    public class RoundedTableLayoutPanel : TableLayoutPanel
    {
        public int CornerRadius { get; set; } = 40;
        public Color RowBorderColor { get; set; } = Color.Gray;
        public int RowBorderWidth { get; set; } = 1;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                path.AddArc(Width - CornerRadius - 1, 0, CornerRadius, CornerRadius, 270, 90);
                path.AddArc(Width - CornerRadius - 1, Height - CornerRadius - 1, CornerRadius, CornerRadius, 0, 90);
                path.AddArc(0, Height - CornerRadius - 1, CornerRadius, CornerRadius, 90, 90);
                path.CloseAllFigures();
                this.Region = new Region(path);
            }
            if (RowCount > 1)
            {
                using (Pen pen = new Pen(RowBorderColor, RowBorderWidth))
                {
                    int y = 0;
                    int[] rowHeights = GetRowHeights();
                    for (int i = 0; i < RowCount - 1; i++)
                    {
                        y += rowHeights[i];
                        e.Graphics.DrawLine(pen, 0, y, Width, y);
                    }
                }
            }
        }

    }
}
