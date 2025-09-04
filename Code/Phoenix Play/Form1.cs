using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Phoenix_Play.Classi;
using Phoenix_Play.Download;
using Phoenix_Play.PanelNavigatore;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Phoenix_Play
{
    public partial class Form1 : Form
    {
        private const int borderWidth = 2;
        private int cornerRadius = 20;
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;
        private bool isMaximized = false;
        public int downloadInCorso = 0;
        public int downloadCompletati = 0;
        private static readonly HttpClient client = new HttpClient();
        private List<Control> bottoniNavigazione;
        public Form1()
        {
            string savedLanguage = Program.Config.Lingua ?? "it";

            string cultureCode = savedLanguage switch
            {
                "English" => "en",
                "Italiano" => "it",
                _ => "it",
            };
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            InitializeComponent();
            MostraHome();
            CaricaLibreria();
            AggiornaLabelDownload();
            labelversion.Text = "v." + AppConfig.CurrentVersion;
            this.Padding = new Padding(borderWidth);
        }
        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Color.Black, borderWidth))
            {
                e.Graphics.DrawPath(pen, GetRoundedRectangle(this.ClientRectangle, cornerRadius));
            }
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;
            const int HTCAPTION = 2;

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                Point pos = PointToClient(new Point(m.LParam.ToInt32()));

                if (pos.X <= borderWidth && pos.Y <= borderWidth)
                    m.Result = HTTOPLEFT;
                else if (pos.X >= ClientSize.Width - borderWidth && pos.Y <= borderWidth)
                    m.Result = HTTOPRIGHT;
                else if (pos.X <= borderWidth && pos.Y >= ClientSize.Height - borderWidth)
                    m.Result = HTBOTTOMLEFT;
                else if (pos.X >= ClientSize.Width - borderWidth && pos.Y >= ClientSize.Height - borderWidth)
                    m.Result = HTBOTTOMRIGHT;
                else if (pos.Y <= borderWidth)
                    m.Result = HTTOP;
                else if (pos.Y >= ClientSize.Height - borderWidth)
                    m.Result = HTBOTTOM;
                else if (pos.X <= borderWidth)
                    m.Result = HTLEFT;
                else if (pos.X >= ClientSize.Width - borderWidth)
                    m.Result = HTRIGHT;
                else
                    m.Result = HTCAPTION;
                return;
            }

            base.WndProc(ref m);
        }
        public void EnableDragging(Control control)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _ = ReleaseCapture();
                    _ = SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
        }

        private async Task InizializzaAggiornamento()
        {
            string updateInfoUrl = "";
            string currentVersion = AppConfig.CurrentVersion;

            try
            {
                var response = await client.GetStringAsync(updateInfoUrl);
                dynamic updateInfo = JsonConvert.DeserializeObject(response);
                string latestVersion = (string)updateInfo.version;
                string updateUrl = (string)updateInfo.updateUrl;

                string userLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpper();

                if (latestVersion != currentVersion)
                {
                    string titolo = TraduzioniManager.Traduci("Form1", "update_title");
                    string messaggio = $"{TraduzioniManager.Traduci("Form1", "update_message")}: {latestVersion}.\n{TraduzioniManager.Traduci("Form1", "update_prompt")}";

                    DialogResult dialogResult = MessageBox.Show(
                        messaggio,
                        titolo,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (dialogResult == DialogResult.Yes)
                    {
                        if (panelnavigatore.Controls.Count > 0 && panelnavigatore.Controls[0] is UserControl_Impostazioni)
                            return;
                        labelNavigatore.Text = TraduzioniManager.Traduci("Form1", "settings");
                        panelnavigatore.Controls.Clear();
                        UserControl_Impostazioni impostazioniControl = new UserControl_Impostazioni(this);
                        impostazioniControl.Dock = DockStyle.Fill;
                        panelnavigatore.Controls.Add(impostazioniControl);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private CancellationTokenSource ctsLib = new();
        public async Task CaricaLibreria(string filtro = "")
        {
            ctsLib.Cancel();
            ctsLib = new CancellationTokenSource();
            var token = ctsLib.Token;

            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string savePath = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati", "GamesInstalled.dat");

            if (!File.Exists(savePath)) return;

            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.HorizontalScroll.Enabled = false;

            List<GameInfo> remoteGames = new();

            try
            {
                using HttpClient client = new();
                string json = await client.GetStringAsync("");
                remoteGames = JObject.Parse(json)["Catalogo"].ToObject<List<GameInfo>>();
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var giochiInstallati = File.ReadAllLines(savePath)
                .Select(line => line.Split('|'))
                .Where(parts => parts.Length == 2)
                .Select(parts => new { Nome = parts[0].Trim(), Path = parts[1].Trim() })
                .GroupBy(x => x.Nome, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            HashSet<string> aggiunti = new(StringComparer.OrdinalIgnoreCase);

            foreach (var parts in giochiInstallati)
            {
                if (token.IsCancellationRequested) return;

                string nomeGioco = parts.Nome;
                string installPath = parts.Path;

                if (!string.IsNullOrEmpty(filtro) && !nomeGioco.ToLower().Contains(filtro.ToLower()))
                    continue;

                if (!aggiunti.Add(nomeGioco)) continue;

                GameInfo matchedGame = remoteGames.FirstOrDefault(g => g.nome_gioco.Trim().Equals(nomeGioco, StringComparison.OrdinalIgnoreCase));
                if (matchedGame == null || string.IsNullOrEmpty(matchedGame.exe)) continue;

                string exePath = Path.Combine(installPath, matchedGame.exe);

                Panel gamePanel = new Panel
                {
                    Height = 30,
                    AutoSize = false,
                    Margin = new Padding(5),
                    BackColor = Color.FromArgb(92, 92, 88),
                    Width = flowLayoutPanel1.ClientSize.Width - 25
                };

                int cornerRadius = 10;
                void ApplyRoundedRegion(Control ctrl)
                {
                    ctrl.Region = new Region(UIHelper.GetRoundedRect(ctrl.ClientRectangle, cornerRadius));
                    ctrl.Resize += (s, e2) =>
                    {
                        ctrl.Region = new Region(UIHelper.GetRoundedRect(ctrl.ClientRectangle, cornerRadius));
                    };
                }
                ApplyRoundedRegion(gamePanel);

                gamePanel.Paint += (s, e2) =>
                {
                    e2.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var path = UIHelper.GetRoundedRect(gamePanel.ClientRectangle, cornerRadius))
                    using (SolidBrush brush = new SolidBrush(gamePanel.BackColor))
                    {
                        e2.Graphics.FillPath(brush, path);
                    }
                };

                Label gameLabel = new Label
                {
                    Text = nomeGioco,
                    ForeColor = Color.White,
                    Font = new Font("Tahoma", 9),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0)
                };

                Button playButton = new Button
                {
                    Text = "▶",
                    Dock = DockStyle.Right,
                    Width = 20,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(92, 92, 88),
                    ForeColor = Color.Lime,
                    Font = new Font("Arial", 12),
                    Cursor = Cursors.Hand
                };
                playButton.FlatAppearance.BorderSize = 0;
                playButton.Click += async (sender2, e2) => await Avvio.AvviaGiocoAsync(nomeGioco);

                gamePanel.Controls.Add(playButton);
                gamePanel.Controls.Add(gameLabel);
                AggiungiContextMenuDisinstalla(gamePanel, nomeGioco);

                flowLayoutPanel1.Controls.Add(gamePanel);
            }
        }
        private CancellationTokenSource cts = new();

        private void textcercalibreria_TextChanged(object sender, EventArgs e)
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            Task.Delay(300, token)
                .ContinueWith(t =>
                {
                    if (!t.IsCanceled)
                        Invoke(() => CaricaLibreria(textcercalibreria.Text));
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void GestisciPrimoAvvioDownload()
        {
            if (Program.Config.PrimoAvvioDownload)
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = TraduzioniManager.Traduci("Form1", "select_game_folder_description");
                    folderDialog.UseDescriptionForTitle = true;
                    folderDialog.ShowNewFolderButton = true;

                    if (!string.IsNullOrEmpty(Program.Config.PercorsoInstallazioneGiochi))
                    {
                        folderDialog.SelectedPath = Program.Config.PercorsoInstallazioneGiochi;
                    }

                    DialogResult result = folderDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string selectedPath = folderDialog.SelectedPath;
                        string phoneixPlayCommonsPath = Path.Combine(selectedPath, "PhoneixPlayCommons");

                        Program.Config.PercorsoInstallazioneGiochi = phoneixPlayCommonsPath;

                        if (!Directory.Exists(phoneixPlayCommonsPath))
                        {
                            _ = Directory.CreateDirectory(phoneixPlayCommonsPath);
                        }

                        Program.Config.PrimoAvvioDownload = false;
                        Program.Config.Save(Program.ConfigPath);
                    }
                }
            }
        }

        public void AggiornaLabelDownload()
        {
            if (downloadInCorso == 0)
                labeldownload.ButtonText = TraduzioniManager.Traduci("Form1", "no_downloads_in_progress");
            else
                labeldownload.ButtonText = string.Format(TraduzioniManager.Traduci("Form1", "downloads_in_progress"), downloadInCorso);
            labeldownload.AggiornaDownload(downloadInCorso, 0);
        }



        private void AggiungiContextMenuDisinstalla(Control pannelloGioco, string nomeGioco)
        {
            Disinstallazione disinstalla = new Disinstallazione(nomeGioco, flowLayoutPanel1, this);
            disinstalla.AggiungiContextMenuDisinstalla(pannelloGioco);
        }

        public void MostraHome()
        {
            panelnavigatore.Controls.Clear();
            UserControl_Home homeControl = new UserControl_Home(this);
            labelNavigatore.Text = TraduzioniManager.Traduci("Form1", "new");
            homeControl.Dock = DockStyle.Fill;
            panelnavigatore.Controls.Add(homeControl);
        }
        private void CaricaSezione<T>(string titolo, Func<T> creaControllo, Button? bottoneAttivo) where T : UserControl
        {
            if (panelnavigatore.Controls.Count > 0 && panelnavigatore.Controls[0] is T)
                return;

            labelNavigatore.Text = titolo;
            panelnavigatore.Controls.Clear();

            var controllo = creaControllo();
            controllo.Dock = DockStyle.Fill;
            panelnavigatore.Controls.Add(controllo);

            EvidenziaBottoneAttivo(bottoneAttivo);
        }

        private void EvidenziaBottoneAttivo(Button bottoneAttivo)
        {
            foreach (var btn in bottoniNavigazione)
            {
                btn.BackColor = Color.FromArgb(71, 71, 69);
            }

            if (bottoneAttivo != null)
                bottoneAttivo.BackColor = Color.FromArgb(70, 70, 70);
        }
        public void btn_Home_Click(object sender, EventArgs e)
        {
            CaricaSezione(TraduzioniManager.Traduci("Form1", "new"), () => new UserControl_Home(this), btn_Home);
            textBox_CercaCat.Enabled = true;
        }

        private void btn_Catalogo_Click(object sender, EventArgs e)
        {
            CaricaSezione(TraduzioniManager.Traduci("Form1", "catalogo"), () => new UserControl_Catalogo(this), btn_Catalogo);
            textBox_CercaCat.Enabled = true;
        }

        private void btn_Impostazioni_Click(object sender, EventArgs e)
        {
            CaricaSezione(TraduzioniManager.Traduci("Form1", "impostazioni"), () => new UserControl_Impostazioni(this), btn_Impostazioni);
            textBox_CercaCat.Enabled = false;
        }

        private void btn_SteamTools_Click(object sender, EventArgs e)
        {
            CaricaSezione(TraduzioniManager.Traduci("Form1", "steam_tools"), () => new UserControl_SteamTools(), btn_SteamTools);
            textBox_CercaCat.Enabled = false;
        }

        private void label_libreria_Click(object sender, EventArgs e)
        {
            CaricaSezione(TraduzioniManager.Traduci("Form1", "libreria"), () => new UserControl_Libreria(this), null);
            textBox_CercaCat.Enabled = true;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            bool isFormDownloadOpen = Application.OpenForms.Cast<Form>().Any(f => f is FormDownload);

            if (isFormDownloadOpen)
            {
                var result = MessageBox.Show(
                    TraduzioniManager.Traduci("Form1", "close_download_warning_message"),
                    TraduzioniManager.Traduci("Form1", "close_download_warning_title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes)
                    return;
            }
            var formsToClose = new List<Form>();
            foreach (Form form in Application.OpenForms)
            {
                if (form != this)
                {
                    formsToClose.Add(form);
                }
            }
            foreach (Form form in formsToClose)
            {
                form.Close();
                form.Dispose();
            }
            foreach (Control control in this.Controls)
            {
                if (control is UserControl userControl)
                {
                    userControl.Dispose();
                }
            }

            Environment.Exit(0);
        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            GestisciPrimoAvvioDownload();
            EnableDragging(panelnavigatore);
            EnableDragging(panelBottoni);
            EnableDragging(paneltitolo);
            EnableDragging(panel1);
            labeldownload.Text = TraduzioniManager.Traduci("Form1", "no_downloads_in_progress");
            _ = InizializzaAggiornamento();
        }

        private void textBox_CercaCat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string searchQuery = textBox_CercaCat.Text.Trim().ToLower();

                foreach (Control ctrl in this.panelnavigatore.Controls)
                {
                    if (ctrl is UserControl_Home home)
                    {
                        home.SearchGames(searchQuery);
                        break;
                    }
                    else if (ctrl is UserControl_Catalogo catalogo)
                    {
                        catalogo.SearchGames(searchQuery);
                        break;
                    }
                    else if (ctrl is UserControl_Libreria libreria)
                    {
                        libreria.SearchGames(searchQuery);
                        break;
                    }
                }
            }
        }

        private void btnMaxi_Click(object sender, EventArgs e)
        {
            if (!isMaximized)
            {
                Rectangle workingArea = Screen.FromHandle(this.Handle).WorkingArea;
                this.Location = workingArea.Location;
                this.Size = workingArea.Size;
                btnMaxi.Image = Properties.Resources.pngMassimizzaAppMaxi;
                isMaximized = true;
            }
            else
            {
                this.Size = new Size(1209, 745);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.CenterToScreen();
                btnMaxi.Image = Properties.Resources.pngMassimizzaApp;
                isMaximized = false;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/mrnico98",
                    UseShellExecute = true
                };
                _ = Process.Start(psi);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InizilizzaBottoni();

            int cornerRadius = 15;
            void ApplyRoundedRegion(Control ctrl)
            {
                ctrl.Region = new Region(UIHelper.GetRoundedRect(ctrl.ClientRectangle, cornerRadius));
                ctrl.Resize += (s, e) =>
                {
                    ctrl.Region = new Region(UIHelper.GetRoundedRect(ctrl.ClientRectangle, cornerRadius));
                };
            }
            ApplyRoundedRegion(btn_Home);
            ApplyRoundedRegion(btn_Catalogo);
            ApplyRoundedRegion(btn_SteamTools);
            ApplyRoundedRegion(label_libreria);

            flowLayoutPanel1.Resize += (s, e) =>
            {
                foreach (Control ctrl in flowLayoutPanel1.Controls)
                {
                    if (ctrl is Panel p)
                    {
                        p.Width = flowLayoutPanel1.ClientSize.Width - 25;
                    }
                }
            };
        }

        private void InizilizzaBottoni()
        {
            bottoniNavigazione = new List<Control>
    {
        btn_Home,
        btn_Catalogo,
        btn_SteamTools,
        label_libreria
    };
        }
    }
}
