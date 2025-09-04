using Newtonsoft.Json.Linq;
using Phoenix_Play.Classi;
using System.Data;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Phoenix_Play.PanelNavigatore
{
    public partial class UserControl_Catalogo : UserControl
    {
        private HashSet<string> generiDisponibili = new();
        private CheckedListBox genreCheckList;
        private Panel filtersPanelReference;
        private DownloadManager _downloadManager;
        private Form1 _mainform1;


        private int currentPage = 1;
        private int itemsPerPage = 20;
        private int totalPages = 1;
        private List<GameInfo> filteredGames = new List<GameInfo>();
        private List<GameInfo> allGames = new List<GameInfo>();
        private FlowLayoutPanel gamesFlowPanel;
        private Panel paginationPanel;
        private TableLayoutPanel mainLayout;

        public UserControl_Catalogo(Form1 mainform1)
        {
            InitializeComponent();
            _mainform1 = mainform1;
            _downloadManager = new DownloadManager(_mainform1);
            CreatePaginationPanel();
            _ = LoadGamesAsync();
        }
        private void CreatePaginationPanel()
        {
            paginationPanel = new Panel
            {
                Name = "paginationPanel",
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(40, 40, 45)
            };

            this.Controls.Add(paginationPanel);
            paginationPanel.BringToFront();
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

                    var genresArray = data["genres"] as JArray;
                    if (genresArray != null)
                    {
                        foreach (var genre in genresArray)
                        {
                            string? genreName = genre["description"]?.ToString();
                            if (!string.IsNullOrEmpty(genreName))
                                stats.Genres.Add(genreName);
                        }
                    }
                    var pcReq = data["pc_requirements"];
                    if (pcReq != null)
                    {
                        string? minHtml = pcReq["minimum"]?.ToString();
                        string? recHtml = pcReq["recommended"]?.ToString();
                    }
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
                stats.Size = "?";
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


        private async Task LoadGamesAsync()
        {
            try
            {
                string jsonUrl = "";
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(jsonUrl);

                JObject obj = JObject.Parse(json);
                allGames = obj["Catalogo"]
                    .ToObject<List<GameInfo>>()
                    .OrderBy(g => g.nome_gioco, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                filteredGames = allGames;
                mainLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    RowStyles =
                {
                    new RowStyle(SizeType.Percent, 100),
                    new RowStyle(SizeType.Absolute, 60)
                }
                };

                TableLayoutPanel contentLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 70),
                    new ColumnStyle(SizeType.Percent, 30)
                }
                };

                gamesFlowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(0, 0, 0, 10)
                };

                Panel filtersPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.FromArgb(40, 40, 45),
                    Padding = new Padding(10)
                };

                filtersPanelReference = filtersPanel;
                contentLayout.Controls.Add(gamesFlowPanel, 0, 0);
                contentLayout.Controls.Add(filtersPanel, 1, 0);
                mainLayout.Controls.Add(contentLayout, 0, 0);
                mainLayout.Controls.Add(paginationPanel, 0, 1);

                tableLayoutPanelHome.Controls.Clear();
                tableLayoutPanelHome.Controls.Add(mainLayout);
                totalPages = (int)Math.Ceiling((double)filteredGames.Count / itemsPerPage);
                if (totalPages == 0) totalPages = 1;
                DisplayPage(currentPage);
                UpdatePaginationControls();

                int gamesProcessed = 0;
                foreach (var game in allGames)
                {
                    _ = Task.Run(async () =>
                    {
                        var stats = await GetCachedStatsAsync(game.steamdbid);

                        lock (generiDisponibili)
                        {
                            foreach (var g in stats.Genres)
                                _ = generiDisponibili.Add(g);
                        }

                        _ = Interlocked.Increment(ref gamesProcessed);
                        if (gamesProcessed == allGames.Count)
                        {
                            filtersPanel.Invoke(() =>
                            {
                                InitializeFilters(filtersPanelReference);
                            });
                        }
                    });
                }
                gamesFlowPanel.Resize += (s, e) =>
                {
                    AggiornaLarghezzaCard(gamesFlowPanel);
                    gamesFlowPanel.PerformLayout();
                };
                gamesFlowPanel.Layout += (s, e) =>
                {
                    foreach (Control card in gamesFlowPanel.Controls)
                    {
                        card.Width = gamesFlowPanel.ClientSize.Width -
                                    (gamesFlowPanel.VerticalScroll.Visible ?
                                     SystemInformation.VerticalScrollBarWidth : 0) - 5;
                    }
                };
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayPage(int pageNumber)
        {
            if (gamesFlowPanel == null) return;

            gamesFlowPanel.SuspendLayout();
            gamesFlowPanel.Controls.Clear();
            int startIndex = (pageNumber - 1) * itemsPerPage;
            int endIndex = Math.Min(startIndex + itemsPerPage, filteredGames.Count);
            for (int i = startIndex; i < endIndex; i++)
            {
                var game = filteredGames[i];
                var skeletonCard = CreateGameCardSkeleton(game);
                skeletonCard.Width = gamesFlowPanel.ClientSize.Width -
                                   (gamesFlowPanel.VerticalScroll.Visible ?
                                    SystemInformation.VerticalScrollBarWidth : 0) - 5;

                gamesFlowPanel.Controls.Add(skeletonCard);
                _ = Task.Run(async () =>
                {
                    var stats = await GetCachedStatsAsync(game.steamdbid);
                    var image = await GetCachedImageAsync(game.steamdbid);

                    skeletonCard.Invoke(() =>
                    {
                        UpdateGameCard(skeletonCard, game, stats, image);
                    });
                });
            }

            gamesFlowPanel.ResumeLayout(true);
            gamesFlowPanel.PerformLayout();
        }

        private void UpdatePaginationControls()
        {
            if (paginationPanel == null) return;

            paginationPanel.Controls.Clear();
            Button prevButton = new Button
            {
                Text = "‹",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(2),
                Enabled = currentPage > 1
            };
            prevButton.FlatAppearance.BorderSize = 0;
            prevButton.Click += (s, e) => NavigateToPage(currentPage - 1);
            GraphicsPath pathPrev = new GraphicsPath();
            pathPrev.AddEllipse(0, 0, prevButton.Width, prevButton.Height);
            prevButton.Region = new Region(pathPrev);

            paginationPanel.Controls.Add(prevButton);
            Label pageLabel = new Label
            {
                Text = $"{currentPage} / {totalPages}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(10, 0, 10, 0)
            };
            paginationPanel.Controls.Add(pageLabel);
            Button nextButton = new Button
            {
                Text = "›",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(2),
                Enabled = currentPage < totalPages
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.Click += (s, e) => NavigateToPage(currentPage + 1);
            GraphicsPath pathNext = new GraphicsPath();
            pathNext.AddEllipse(0, 0, nextButton.Width, nextButton.Height);
            nextButton.Region = new Region(pathNext);

            paginationPanel.Controls.Add(nextButton);
            PositionPaginationControls();
            paginationPanel.Resize += (s, e) => PositionPaginationControls();
        }

        private void PositionPaginationControls()
        {
            if (paginationPanel == null || paginationPanel.Controls.Count < 3) return;

            var prevButton = paginationPanel.Controls[0] as Button;
            var pageLabel = paginationPanel.Controls[1] as Label;
            var nextButton = paginationPanel.Controls[2] as Button;

            if (prevButton == null || pageLabel == null || nextButton == null) return;

            int totalWidth = prevButton.Width + pageLabel.Width + nextButton.Width + 20;
            int startX = (paginationPanel.Width - totalWidth) / 2;
            int centerY = (paginationPanel.Height - prevButton.Height) / 2;

            prevButton.Location = new Point(startX, centerY);
            pageLabel.Location = new Point(startX + prevButton.Width + 10, centerY + 10);
            nextButton.Location = new Point(startX + prevButton.Width + pageLabel.Width + 20, centerY);
        }

        private void NavigateToPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > totalPages) return;

            currentPage = pageNumber;
            DisplayPage(currentPage);
            UpdatePaginationControls();
        }
        private void AggiornaLarghezzaCard(FlowLayoutPanel container)
        {
            int nuovaLarghezza = container.ClientSize.Width - 30;
            foreach (Control card in container.Controls)
            {
                card.Width = nuovaLarghezza;
            }
        }
        private void InitializeFilters(Panel filtersPanel)
        {
            filtersPanel.Controls.Clear();

            Label filtersTitle = new Label
            {
                Text = TraduzioniManager.Traduci("UserControl_Catalogo", "filtri"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            filtersPanel.Controls.Add(filtersTitle);

            Label genreLabel = new Label
            {
                Text = TraduzioniManager.Traduci("UserControl_Catalogo", "genere"),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };
            filtersPanel.Controls.Add(genreLabel);

            genreCheckList = new CheckedListBox
            {
                CheckOnClick = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(40, 40, 45),
                BorderStyle = BorderStyle.None,
                Width = filtersPanel.Width - 25,
                IntegralHeight = false
            };

            var generiOrdinati = generiDisponibili.OrderBy(g => g).ToArray();
            genreCheckList.Items.AddRange(generiOrdinati);
            int itemHeight = genreCheckList.GetItemHeight(0);
            int totalHeight = generiOrdinati.Length * itemHeight + 25;
            int maxReasonableHeight = 500;
            genreCheckList.Height = Math.Min(totalHeight, maxReasonableHeight);

            filtersPanel.Controls.Add(genreCheckList);

            Button applyFiltersButton = new Button
            {
                Text = TraduzioniManager.Traduci("UserControl_Catalogo", "applica_filtri"),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 40,
                Width = filtersPanel.Width - 25,
                Margin = new Padding(0, 20, 0, 0),
                Cursor = Cursors.Hand
            };
            applyFiltersButton.FlatAppearance.BorderSize = 0;
            filtersPanel.Controls.Add(applyFiltersButton);
            applyFiltersButton.Click += ApplyFiltersButton_Click;

            int yPos = 10;
            foreach (Control control in filtersPanel.Controls)
            {
                control.Location = new Point(10, yPos);
                yPos += control.Height + control.Margin.Top + control.Margin.Bottom;
            }
        }

        private Control CreateGameCardSkeleton(GameInfo game)
        {
            RoundedPanel gamePanel = new RoundedPanel
            {
                Height = 152,
                Margin = new Padding(2),
                BackColor = Color.FromArgb(50, 50, 55),
                CornerRadius = 20,
                Tag = "Skeleton",
            };

            gamePanel.Width = tableLayoutPanelHome.Width - 320;

            PictureBox pictureBox = new PictureBox
            {
                Width = 230,
                Height = 126,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black,
                Image = null
            };
            gamePanel.Controls.Add(pictureBox);

            Panel contentPanel = new Panel
            {
                Location = new Point(260, 10),
                Size = new Size(gamePanel.Width - 270, 160),
                Tag = "Content"
            };
            gamePanel.Resize += (s, e) =>
            {
                contentPanel.Width = gamePanel.Width - 270;
            };

            Label title = new Label
            {
                Text = game.nome_gioco,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Width = contentPanel.Width - 20,
                Height = 25,
                Location = new Point(0, 0),
                Name = "Title"
            };
            contentPanel.Controls.Add(title);

            bool isInstalled = IsGameInstalled(game.nome_gioco);
            int textLength = isInstalled
    ? TraduzioniManager.Traduci("UserControl_Home", "start_game").Length
    : TraduzioniManager.Traduci("UserControl_Home", "download_game").Length;

            int buttonWidth = 80 + (textLength * 5);
            buttonWidth = Math.Max(buttonWidth, 80);
            Button downloadButton = new Button
            {
                Text = isInstalled
                    ? TraduzioniManager.Traduci("UserControl_Home", "start_game")
                    : TraduzioniManager.Traduci("UserControl_Home", "download_game"),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = isInstalled ? Color.FromArgb(40, 167, 69) : Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(buttonWidth, 30),
                Location = new Point(contentPanel.Width - buttonWidth - 1, contentPanel.Height - 60),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right,
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

            contentPanel.Controls.Add(downloadButton);
            gamePanel.Controls.Add(contentPanel);
            return gamePanel;
        }

        private void UpdateGameCard(Control gamePanel, GameInfo game, SteamGameStats stats, Image image)
        {
            if (gamePanel.InvokeRequired)
            {
                gamePanel.Invoke(() => UpdateGameCard(gamePanel, game, stats, image));
                return;
            }

            var pictureBox = gamePanel.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pictureBox != null)
            {
                pictureBox.Image = image;
            }

            var contentPanel = gamePanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Tag?.ToString() == "Content");
            if (contentPanel != null)
            {
                var titleLabel = contentPanel.Controls.Find("Title", true).FirstOrDefault() as Label;
                if (titleLabel != null && string.IsNullOrWhiteSpace(titleLabel.Text))
                    titleLabel.Text = stats.Name ?? game.nome_gioco;

                int currentY = titleLabel.Bottom + 5;
                var versionLabel = new Label
                {
                    Text = string.IsNullOrEmpty(game.versione)
                        ? TraduzioniManager.Traduci("UserControl_Home", "version_not_available")
                        : string.Format(TraduzioniManager.Traduci("UserControl_Home", "version"), game.versione),
                    Location = new Point(0, currentY),
                    AutoSize = true,
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9),
                    Anchor = AnchorStyles.Left
                };
                contentPanel.Controls.Add(versionLabel);
                currentY = versionLabel.Bottom + 2;
                bool isOnline = game.online;
                RichTextBox multiplayer = new RichTextBox
                {
                    Location = new Point(4, currentY),
                    Width = contentPanel.Width - 20,
                    Height = 20,
                    BorderStyle = BorderStyle.None,
                    ReadOnly = true,
                    BackColor = contentPanel.BackColor,
                    Font = new Font("Segoe UI", 9),
                    ScrollBars = RichTextBoxScrollBars.None
                };
                multiplayer.SelectionColor = Color.White;
                multiplayer.AppendText("Multiplayer:");
                if (isOnline)
                {
                    multiplayer.SelectionColor = Color.LightGreen;
                    multiplayer.AppendText(" 🟢 Online");
                    currentY = multiplayer.Bottom + 5;
                    var playersLabel = new Label
                    {
                        Text = string.Format(TraduzioniManager.Traduci("UserControl_Home", "players"), stats.CurrentPlayers),
                        Location = new Point(0, currentY),
                        AutoSize = true,
                        ForeColor = Color.LightBlue,
                        Font = new Font("Segoe UI", 9),
                        Anchor = AnchorStyles.Left
                    };
                    contentPanel.Controls.Add(playersLabel);
                    currentY = playersLabel.Bottom + 5;
                }
                else
                {
                    multiplayer.SelectionColor = Color.IndianRed;
                    multiplayer.AppendText(" 🔴 Offline");
                    currentY = multiplayer.Bottom + 5;
                }
                contentPanel.Controls.Add(multiplayer);
                var reviewsLabel = new Label
                {
                    Text = string.Format(
                        TraduzioniManager.Traduci("UserControl_Catalogo", "recensioni"),
                        stats.PositiveReviewPercentage,
                        stats.TotalReviews
                    ),
                    Location = new Point(0, currentY),
                    AutoSize = true,
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9),
                    Anchor = AnchorStyles.Left
                };
                contentPanel.Controls.Add(reviewsLabel);
                currentY = reviewsLabel.Bottom + 10;
                var downloadButton = contentPanel.Controls.OfType<Button>().FirstOrDefault();
                if (downloadButton != null)
                {
                    downloadButton.Location = new Point(
                        contentPanel.Width - downloadButton.Width - 1,
                        currentY
                    );
                }
                int requiredHeight = Math.Max(gamePanel.Height, contentPanel.Bottom + 10);
                gamePanel.Height = requiredHeight;
            }
        }



        private void ApplyFiltersButton_Click(object sender, EventArgs e)
        {
            bool noFilterSelected = genreCheckList == null || genreCheckList.CheckedItems.Count == 0;

            var generiSelezionati = noFilterSelected
                ? null
                : genreCheckList.CheckedItems.Cast<string>().ToHashSet();

            foreach (Control control in tableLayoutPanelHome.Controls)
            {
                if (control is TableLayoutPanel layoutPanel)
                {
                    var gamesPanel = layoutPanel.GetControlFromPosition(0, 0) as FlowLayoutPanel;
                    if (gamesPanel == null) continue;

                    foreach (Panel gameCard in gamesPanel.Controls.OfType<Panel>())
                    {
                        var game = gameCard.Controls.OfType<Panel>()
                            .SelectMany(p => p.Controls.OfType<Button>())
                            .FirstOrDefault()?.Tag as GameInfo;

                        if (game == null || string.IsNullOrEmpty(game.steamdbid)) continue;

                        if (noFilterSelected)
                        {
                            gameCard.Visible = true;
                        }
                        else if (statsCache.TryGetValue(game.steamdbid, out var stats))
                        {
                            bool matchGenres = stats.Genres.Any(g => generiSelezionati.Contains(g));
                            gameCard.Visible = matchGenres;
                        }
                    }
                }
            }
        }

        private Dictionary<string, SteamGameStats> statsCache = new();
        private Dictionary<string, Image> imageCache = new();
        private async Task<SteamGameStats> GetCachedStatsAsync(string id)
        {
            if (!statsCache.TryGetValue(id, out var stats))
            {
                stats = await GetSteamGameStatsAsync(id);
                statsCache[id] = stats;
            }
            return stats;
        }

        private async Task<Image> GetCachedImageAsync(string id)
        {
            if (!imageCache.TryGetValue(id, out var img))
            {
                img = await LoadSteamGameImageAsync(id);
                imageCache[id] = img;
            }
            return img;
        }

        private async Task<Image> LoadSteamGameImageAsync(string appid)
        {
            using HttpClient client = new HttpClient();
            string headerUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg";
            try
            {
                byte[] data = await client.GetByteArrayAsync(headerUrl);
                using MemoryStream ms = new MemoryStream(data);
                return Image.FromStream(ms);
            }
            catch
            {
            }
            string capsuleUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/capsule_231x87.jpg";
            try
            {
                byte[] data = await client.GetByteArrayAsync(capsuleUrl);
                using MemoryStream ms = new MemoryStream(data);
                return Image.FromStream(ms);
            }
            catch
            {

            }
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
                    return Image.FromStream(ms);
                }
            }
            catch
            {

            }
            return SystemIcons.Application.ToBitmap();
        }

        public void SearchGames(string searchQuery)
        {
            _ = LoadGamesRicercati(searchQuery);
        }

        private async Task LoadGamesRicercati(string searchQuery = "")
        {
            try
            {
                string jsonUrl = "";
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(jsonUrl);

                JObject obj = JObject.Parse(json);
                var games = obj["Catalogo"].ToObject<List<GameInfo>>();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    games = games
                        .Where(g => g.nome_gioco.ToLower().Contains(searchQuery))
                        .ToList();
                }
                filteredGames = games.OrderBy(g => g.nome_gioco, StringComparer.OrdinalIgnoreCase).ToList();
                totalPages = (int)Math.Ceiling((double)filteredGames.Count / itemsPerPage);
                if (totalPages == 0) totalPages = 1;
                currentPage = 1;
                DisplayPage(currentPage);
                UpdatePaginationControls();
                _ = Task.Run(async () =>
                {
                    int gamesProcessed = 0;
                    foreach (var game in filteredGames)
                    {
                        var stats = await GetCachedStatsAsync(game.steamdbid);

                        lock (generiDisponibili)
                        {
                            foreach (var g in stats.Genres)
                                generiDisponibili.Add(g);
                        }
                        gamesProcessed++;
                        if (gamesProcessed == filteredGames.Count)
                        {
                            if (filtersPanelReference != null && filtersPanelReference.IsHandleCreated)
                            {
                                filtersPanelReference.Invoke(() =>
                                {
                                    InitializeFilters(filtersPanelReference);
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
    }
}
