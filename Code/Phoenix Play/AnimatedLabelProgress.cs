using Phoenix_Play.Classi;
using Phoenix_Play.Download;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Phoenix_Play
{
    public class AnimatedButtonControl : UserControl
    {
        private System.Windows.Forms.Timer animationTimer;
        private int progressPosition = 0;
        private bool isAnimating = false;
        private int _downloadInCorso = 0;
        private int _downloadCompletati = 0;
        private bool downloadsHidden = false;
        private ToolTip progressToolTip;

        public string ButtonText { get; set; }

        public AnimatedButtonControl()
        {
            this.DoubleBuffered = true;
            this.Height = 70;
            this.Cursor = Cursors.Hand;
            this.Click += (s, e) => ToggleDownloadVisibility();
            this.MouseHover += (s, e) => ShowTooltip();
            this.MouseLeave += (s, e) => HideTooltip();

            SetupAnimation();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UIHelper.GetRoundedRect(this.ClientRectangle, 8))
            {
                this.Region = new Region(path);


                using (Brush bgBrush = new SolidBrush(this.BackColor))
                    e.Graphics.FillPath(bgBrush, path);

                if (_downloadInCorso > 0)
                {
                    int barHeight = 6;
                    int barY = this.Height - barHeight - 2;
                    Rectangle barRect = new Rectangle(2, barY, this.Width - 4, barHeight);

                    using (Brush bgBar = new SolidBrush(Color.FromArgb(60, 60, 60)))
                        e.Graphics.FillRectangle(bgBar, barRect);

                    int animBarWidth = 30;
                    int animX = progressPosition % (barRect.Width - animBarWidth);
                    Rectangle animRect = new Rectangle(barRect.X + animX, barRect.Y, animBarWidth, barRect.Height);

                    using (Brush animBrush = new SolidBrush(Color.FromArgb(100, 0, 120, 215)))
                        e.Graphics.FillRectangle(animBrush, animRect);

                    if (_downloadCompletati > 0)
                    {
                        double percent = _downloadCompletati / (double)(_downloadCompletati + _downloadInCorso);
                        int completedWidth = (int)(barRect.Width * percent);
                        Rectangle completedRect = new Rectangle(barRect.X, barRect.Y, completedWidth, barRect.Height);

                        using (Brush completedBrush = new SolidBrush(Color.FromArgb(0, 200, 0)))
                            e.Graphics.FillRectangle(completedBrush, completedRect);
                    }
                }

                Rectangle textRect = new Rectangle(0, 0, this.Width, this.Height - 10);
                TextRenderer.DrawText(e.Graphics, ButtonText, this.Font, textRect, this.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }

        private void SetupAnimation()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 50;
            animationTimer.Tick += (s, e) =>
            {
                progressPosition = (progressPosition + 5) % (this.Width - 25);
                this.Invalidate();
            };
        }

        public void AggiornaDownload(int inCorso, int completati)
        {
            _downloadInCorso = inCorso;
            _downloadCompletati = completati;
            UpdateAnimation();
        }


        private void UpdateAnimation()
        {
            if (_downloadInCorso > 0 && !isAnimating)
            {
                isAnimating = true;
                animationTimer.Start();
            }
            else if (_downloadInCorso == 0 && isAnimating)
            {
                isAnimating = false;
                animationTimer.Stop();
                this.Invalidate();
            }
        }
        private void ToggleDownloadVisibility()
        {
            downloadsHidden = !downloadsHidden;
            this.BackColor = downloadsHidden ? Color.FromArgb(81, 81, 79) : Color.FromArgb(71, 71, 69);

            if (downloadsHidden)
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormDownload downloadForm)
                        downloadForm.Hide();
                }
            }
            else
            {
                int startY = 50; 
                int margin = 10; 

                foreach (Form form in Application.OpenForms)
                {
                    if (form is FormDownload downloadForm)
                    {

                        downloadForm.Location = new Point(downloadForm.Location.X, startY);
                        startY += downloadForm.Height + margin;

                        downloadForm.Show();
                    }
                }
            }
        }



        private void ShowTooltip()
        {
            if (downloadsHidden && _downloadInCorso > 0)
            {
                if (progressToolTip == null)
                {
                    progressToolTip = new ToolTip();
                    progressToolTip.AutoPopDelay = 5000;
                    progressToolTip.InitialDelay = 0;
                    progressToolTip.ReshowDelay = 0;
                }
                progressToolTip.Show(GetDownloadProgressInfo(), this, this.Width / 2, this.Height, 1000);
            }
        }


        private void HideTooltip()
        {
            progressToolTip?.RemoveAll();
        }

        private string GetDownloadProgressInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Download in corso:");
            foreach (Form form in Application.OpenForms)
            {
                if (form is FormDownload downloadForm)
                    sb.AppendLine($"{downloadForm.Text.Replace("Download - ", "")}: {downloadForm.ProgressPercentage}%");
            }
            return sb.ToString();
        }
    }
}
