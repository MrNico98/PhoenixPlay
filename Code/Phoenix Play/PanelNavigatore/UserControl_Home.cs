using Newtonsoft.Json.Linq;
using Phoenix_Play.Classi;

namespace Phoenix_Play.PanelNavigatore
{
    public partial class UserControl_Home : UserControl
    {
        private Form1 _mainform1;
        private DownloadManager _downloadManager;

        public UserControl_Home(Form1 mainform1)
        {
            InitializeComponent();
            _mainform1 = mainform1;
            _downloadManager = new DownloadManager(_mainform1);
            _ = LoadGamesAsync();
        }
        private async Task LoadGamesAsync()
        {
            try
            {
                string jsonUrl = "";
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(jsonUrl);

                JObject obj = JObject.Parse(json);
                var games = obj["Home"].ToObject<List<GameInfo>>();

                tableLayoutPanelHome.Controls.Clear();
                tableLayoutPanelHome.RowStyles.Clear();
                tableLayoutPanelHome.RowCount = 0;
                tableLayoutPanelHome.ColumnCount = 3;
                tableLayoutPanelHome.ColumnStyles.Clear();
                for (int i = 0; i < 3; i++)
                {
                    _ = tableLayoutPanelHome.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
                }

                int col = 0;
                int row = 0;
                int columns = 3;

                var tasks = games.Select((game, i) => CreateGameCardAsync(game, i == 0)).ToList();
                var gameCards = await Task.WhenAll(tasks);

                tableLayoutPanelHome.SuspendLayout();
                foreach (var gameCard in gameCards)
                {
                    if (col == 0)
                    {
                        tableLayoutPanelHome.RowCount++;
                        _ = tableLayoutPanelHome.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    }

                    tableLayoutPanelHome.Controls.Add(gameCard, col, row);

                    col++;
                    if (col >= columns)
                    {
                        col = 0;
                        row++;
                    }
                }
                tableLayoutPanelHome.ResumeLayout();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<Control> CreateGameCardAsync(GameInfo game, bool isFirst)
        {
            var cardWidth = (tableLayoutPanelHome.Width / 3) - 20;
            var imageTask = LoadSteamGameImageAsync(game.steamdbid);
            var statsTask = GetSteamGameStatsAsync(game.steamdbid);
            await Task.WhenAll(imageTask, statsTask);

            var stats = await statsTask;
            var image = await imageTask;
            RoundedPanel panel = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Height = 240,
                Margin = new Padding(5),
                BackColor = Color.FromArgb(50, 50, 55),
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                CornerRadius = 12
            };

            PictureBox pictureBox = new PictureBox
            {
                Width = panel.Width - 20,
                Height = 95,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = image,
                BackColor = Color.Black,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(pictureBox);
            Label title = new Label
            {
                Text = stats.Name ?? game.nome_gioco,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Width = panel.Width - 20,
                Height = 40,
                Location = new Point(10, pictureBox.Bottom + 3),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.TopLeft
            };
            panel.Controls.Add(title);
            bool isOnline = game.online;
            RichTextBox multiplayer = new RichTextBox
            {
                Location = new Point(10, title.Bottom + 2),
                Width = panel.Width - 20,
                Height = 20,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = panel.BackColor,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ScrollBars = RichTextBoxScrollBars.None
            };
            multiplayer.SelectionColor = Color.White;
            multiplayer.AppendText("Multiplayer:");
            if (isOnline)
            {
                multiplayer.SelectionColor = Color.LightGreen;
                multiplayer.AppendText(" 🟢 Online");
            }
            else
            {
                multiplayer.SelectionColor = Color.IndianRed;
                multiplayer.AppendText(" 🔴 Offline");
            }

            panel.Controls.Add(multiplayer);

            Label version = new Label
            {
                Text = string.IsNullOrEmpty(game.versione)
    ? TraduzioniManager.Traduci("UserControl_Home", "version_not_available")
    : string.Format(TraduzioniManager.Traduci("UserControl_Home", "version"), game.versione),
                Location = new Point(10, title.Bottom + 2),
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(version);
            Label reviews = new Label
            {
                Text = string.Format(TraduzioniManager.Traduci("UserControl_Home", "reviews"), stats.TotalReviews),
                Location = new Point(10, version.Bottom + 1),
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(reviews);
            bool isInstalled = IsGameInstalled(game.nome_gioco);

            string buttonText = isInstalled
                ? TraduzioniManager.Traduci("UserControl_Home", "start_game")
                : TraduzioniManager.Traduci("UserControl_Home", "download_game");

            Size textSize = TextRenderer.MeasureText(buttonText, new Font("Segoe UI", 9, FontStyle.Bold));
            int padding = 10;
            int buttonWidth = textSize.Width + padding;

            Button downloadButton = new Button
            {
                Text = buttonText,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = isInstalled ? Color.FromArgb(40, 167, 69) : Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(buttonWidth, 30),
                Location = new Point(panel.Width - buttonWidth - 10, title.Bottom + 10),
                Anchor = AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Tag = game
            };

            downloadButton.FlatAppearance.BorderSize = 0;
            downloadButton.FlatAppearance.MouseOverBackColor = isInstalled
                ? Color.FromArgb(33, 136, 56)
                : Color.FromArgb(0, 98, 163);
            downloadButton.FlatAppearance.MouseDownBackColor = isInstalled
                ? Color.FromArgb(28, 117, 47)
                : Color.FromArgb(0, 73, 122);
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 10, 10, 180, 90);
            path.AddArc(downloadButton.Width - 10, 0, 10, 10, 270, 90);
            path.AddArc(downloadButton.Width - 10, downloadButton.Height - 10, 10, 10, 0, 90);
            path.AddArc(0, downloadButton.Height - 10, 10, 10, 90, 90);
            downloadButton.Region = new Region(path);
            downloadButton.Click += async (sender, e) =>
            {
                var btn = sender as Button;
                var gameInfo = btn.Tag as GameInfo;
                if (isInstalled)
                {
                    await Avvio.AvviaGiocoAsync(gameInfo.nome_gioco);
                }
                else
                {
                    _downloadManager.HandleDownloadClick(btn);
                }
            };
            panel.Controls.Add(downloadButton);
            Label reviewPercentage = new Label
            {
                Text = string.Format(TraduzioniManager.Traduci("UserControl_Home", "positive_reviews"), stats.PositiveReviewPercentage) +
       " - " +
       string.Format(TraduzioniManager.Traduci("UserControl_Home", "negative_reviews"), stats.NegativeReviewPercentage),
                Location = new Point(10, reviews.Bottom + 1),
                AutoSize = true,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(reviewPercentage);

            Label players = new Label
            {
                Text = string.Format(TraduzioniManager.Traduci("UserControl_Home", "players"), stats.CurrentPlayers),
                AutoSize = true,
                ForeColor = Color.LightBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(downloadButton.Left + -32, downloadButton.Bottom + 25),
                Anchor = AnchorStyles.Right
            };
            panel.Controls.Add(players);

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle,
                    Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid);
                using (var pen = new Pen(Color.FromArgb(60, 60, 60), 1))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, panel.Width - 1, panel.Height - 1));
                }
            };
            panel.Resize += (s, e) =>
            {
                panel.Invalidate();
                downloadButton.Location = new Point(panel.Width - 90, downloadButton.Location.Y);
                players.Location = new Point(panel.Width - 120, players.Location.Y);
                if (downloadButton.Width > 0 && downloadButton.Height > 0)
                {
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddArc(0, 0, 10, 10, 180, 90);
                    path.AddArc(downloadButton.Width - 10, 0, 10, 10, 270, 90);
                    path.AddArc(downloadButton.Width - 10, downloadButton.Height - 10, 10, 10, 0, 90);
                    path.AddArc(0, downloadButton.Height - 10, 10, 10, 90, 90);
                    downloadButton.Region = new Region(path);
                }
            };

            return panel;
        }

        private async Task<SteamGameStats> GetSteamGameStatsAsync(string appid)
        {
            SteamGameStats stats = new SteamGameStats();

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            try
            {
                string urlStore = $"https://store.steampowered.com/api/appdetails?appids={appid}&cc=us&l=en";
                string jsonStore = await client.GetStringAsync(urlStore);
                JObject storeObj = JObject.Parse(jsonStore);

                var data = storeObj[appid]["data"];
                if (data != null && data.HasValues)
                {
                    stats.Name = data["name"]?.ToString();
                }
                string urlReviews = $"https://store.steampowered.com/appreviews/{appid}?json=1&language=all&purchase_type=all&num_per_page=0";
                string jsonReviews = await client.GetStringAsync(urlReviews);
                JObject reviewsObj = JObject.Parse(jsonReviews);

                if (reviewsObj["success"]?.ToString() == "1")
                {
                    var querySummary = reviewsObj["query_summary"];
                    if (querySummary != null)
                    {
                        stats.TotalReviews = querySummary["total_reviews"]?.ToString() ?? "0";
                        int total = int.Parse(stats.TotalReviews);
                        int positive = int.Parse(querySummary["total_positive"]?.ToString() ?? "0");
                        int negative = int.Parse(querySummary["total_negative"]?.ToString() ?? "0");

                        stats.PositiveReviewPercentage = total > 0 ? (positive * 100) / total : 0;
                        stats.NegativeReviewPercentage = total > 0 ? (negative * 100) / total : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
                stats.TotalReviews = "?";
                stats.PositiveReviewPercentage = 0;
                stats.NegativeReviewPercentage = 0;
            }
            try
            {
                string urlPlayers = $"https://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid={appid}";
                string jsonPlayers = await client.GetStringAsync(urlPlayers);
                JObject playerObj = JObject.Parse(jsonPlayers);
                stats.CurrentPlayers = playerObj["response"]?["player_count"]?.ToString() ?? "0";
            }
            catch
            {
                stats.CurrentPlayers = "?";
            }

            return stats;
        }

        private bool IsGameInstalled(string nomeGioco)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string saveDirectory = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati");
            string savePath = Path.Combine(saveDirectory, "GamesInstalled.dat");

            if (!File.Exists(savePath))
                return false;

            var lines = File.ReadAllLines(savePath);
            return lines.Any(line =>
                line.Split('|')[0].Trim().Equals(nomeGioco.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static readonly Dictionary<string, Image> _steamImageCache = new();

        private async Task<Image> LoadSteamGameImageAsync(string appid)
        {
            if (_steamImageCache.TryGetValue(appid, out var cachedImage))
                return cachedImage;

            using HttpClient client = new HttpClient();
            string headerUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg";

            try
            {
                byte[] data = await client.GetByteArrayAsync(headerUrl);
                using MemoryStream ms = new MemoryStream(data);
                var image = Image.FromStream(ms);
                _steamImageCache[appid] = image;
                return image;
            }
            catch { }

            string capsuleUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/capsule_231x87.jpg";
            try
            {
                byte[] data = await client.GetByteArrayAsync(capsuleUrl);
                using MemoryStream ms = new MemoryStream(data);
                var image = Image.FromStream(ms);
                _steamImageCache[appid] = image;
                return image;
            }
            catch { }

            try
            {
                string urlStore = $"https://store.steampowered.com/api/appdetails?appids={appid}&cc=us&l=en";
                string json = await client.GetStringAsync(urlStore);
                JObject obj = JObject.Parse(json);

                var headerImageUrl = obj[appid]?["data"]?["header_image"]?.ToString();
                if (!string.IsNullOrEmpty(headerImageUrl))
                {
                    byte[] data = await client.GetByteArrayAsync(headerImageUrl);
                    using MemoryStream ms = new MemoryStream(data);
                    var image = Image.FromStream(ms);
                    _steamImageCache[appid] = image;
                    return image;
                }
            }
            catch { }

            var fallback = SystemIcons.Application.ToBitmap();
            _steamImageCache[appid] = fallback;
            return fallback;
        }


        public void SearchGames(string searchQuery)
        {
            _ = LoadGamesRicercati(searchQuery);
        }

        private async Task LoadGamesRicercati(string searchQuery = "")
        {
            try
            {
                string jsonUrl = "https://raw.githubusercontent.com/MrNico98//refs/heads/main/Navigatore/Home.json";
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(jsonUrl);

                JObject obj = JObject.Parse(json);
                var games = obj["Home"].ToObject<List<GameInfo>>();
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    games = games.Where(g => g.nome_gioco.ToLower().Contains(searchQuery)).ToList();

                }

                tableLayoutPanelHome.Controls.Clear();
                tableLayoutPanelHome.RowStyles.Clear();
                tableLayoutPanelHome.RowCount = 0;
                tableLayoutPanelHome.ColumnCount = 2;

                int col = 0;
                int row = 0;

                for (int i = 0; i < games.Count; i++)
                {
                    var game = games[i];
                    Control gameCard = await CreateGameCardAsync(game, i == 0);

                    if (col == 0)
                    {
                        tableLayoutPanelHome.RowCount++;
                        _ = tableLayoutPanelHome.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    }

                    tableLayoutPanelHome.Controls.Add(gameCard, col, row);

                    col++;
                    if (col > 1)
                    {
                        col = 0;
                        row++;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
