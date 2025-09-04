using Newtonsoft.Json.Linq;
using Phoenix_Play.Classi;
using System.Data;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Phoenix_Play.PanelNavigatore
{
    public partial class UserControl_Libreria : UserControl
    {
        private Form1 _mainform1;
        public UserControl_Libreria(Form1 mainform1)
        {
            InitializeComponent();
            _mainform1 = mainform1;
            _ = LoadGamesAsync();
        }

        private async Task LoadGamesAsync()
        {
            try
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string savePath = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati", "GamesInstalled.dat");

                if (!File.Exists(savePath))
                {
                    return;
                }
                var installedGames = new List<InstalledGame>();
                foreach (var line in File.ReadAllLines(savePath))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        installedGames.Add(new InstalledGame
                        {
                            nome_gioco = parts[0],
                            percorso = parts[1]
                        });
                    }
                }
                string jsonUrl = "";
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(jsonUrl);
                JObject obj = JObject.Parse(json);
                var catalogo = obj["Catalogo"].ToObject<List<GameInfo>>();

                var steamIdMap = catalogo.ToDictionary(g => g.nome_gioco, g => g.steamdbid);
                tableLayoutPanelHome.Controls.Clear();
                tableLayoutPanelHome.RowStyles.Clear();
                tableLayoutPanelHome.RowCount = 0;
                tableLayoutPanelHome.ColumnCount = 2;

                int col = 0, row = 0;

                foreach (var installed in installedGames)
                {
                    if (!steamIdMap.TryGetValue(installed.nome_gioco, out string steamId))
                        continue;

                    Control gameCard = await CreateGameCardAsync(installed.nome_gioco, installed.percorso, steamId);

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

        private async Task<Control> CreateGameCardAsync(string nomeGioco, string percorso, string steamdbid)
        {
            var cardWidth = (tableLayoutPanelHome.Width / 2) - 10;

            RoundedPanel pannelloGioco = new RoundedPanel
            {
                Width = cardWidth,
                Height = 80,
                Margin = new Padding(5),
                BackColor = Color.FromArgb(50, 50, 55),
                BorderStyle = BorderStyle.None
            };

            PictureBox pictureBox = new PictureBox
            {
                Width = 80,
                Height = 60,
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = await LoadSteamGameImageAsync(steamdbid),
                BackColor = Color.Black
            };
            pannelloGioco.Controls.Add(pictureBox);

            Label title = new Label
            {
                Text = nomeGioco,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Width = cardWidth - 150,
                Height = 20,
                Location = new Point(100, 10)
            };
            pannelloGioco.Controls.Add(title);

            Label sizeLabel = new Label
            {
                Text = string.Format(
    TraduzioniManager.Traduci("UserControl_Libreria", "peso"),
    GetGameFolderSize(percorso)
),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Silver,
                AutoSize = false,
                Width = cardWidth - 150,
                Height = 18,
                Location = new Point(100, 32)
            };
            pannelloGioco.Controls.Add(sizeLabel);
            Label tempoGiocatoLabel = new Label
            {
                Text = $"Tempo Giocato: {GetTempoGiocato(nomeGioco)}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Silver,
                AutoSize = false,
                Width = cardWidth - 150,
                Height = 18,
                Location = new Point(100, 50)
            };
            pannelloGioco.Controls.Add(tempoGiocatoLabel);
            Button playButton = new Button
            {
                Text = "▶",
                Width = 30,
                Height = 30,
                Location = new Point(cardWidth - 75, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 200, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            playButton.FlatAppearance.BorderSize = 0;
            playButton.Click += (sender, e) => Avvio.AvviaGiocoAsync(nomeGioco);
            playButton.Paint += (sender, e) =>
            {
                using GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(0, 0, playButton.Width - 1, playButton.Height - 1);
                playButton.Region = new Region(gp);
            };
            pannelloGioco.Controls.Add(playButton);
            Button uninstallButton = new Button
            {
                Text = "🗑",
                Width = 30,
                Height = 30,
                Location = new Point(cardWidth - 40, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            uninstallButton.FlatAppearance.BorderSize = 0;
            uninstallButton.Click += async (sender, e) =>
            {
                Disinstallazione disinstallazione = new(nomeGioco, tableLayoutPanelHome, _mainform1);
                await disinstallazione.DisinstallaDirettamente(pannelloGioco);
            };
            uninstallButton.Paint += (sender, e) =>
            {
                using GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(0, 0, uninstallButton.Width - 1, uninstallButton.Height - 1);
                uninstallButton.Region = new Region(gp);
            };
            pannelloGioco.Controls.Add(uninstallButton);
            pannelloGioco.Controls.SetChildIndex(playButton, 0);
            return pannelloGioco;
        }
        private string GetTempoGiocato(string nomeGioco)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string savePath = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati", "GamesInstalled.dat");

            if (!File.Exists(savePath)) return "0h 0m";

            var lines = File.ReadAllLines(savePath);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 4 && parts[0] == nomeGioco)
                {
                    double seconds;
                    if (double.TryParse(parts[3], out seconds))
                    {
                        int hours = (int)(seconds / 3600);
                        int minutes = (int)((seconds % 3600) / 60);
                        return $"{hours}h {minutes}m";
                    }
                }
            }
            return "0h 0m";
        }

        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect, int nWidthEllipse, int nHeightEllipse);


        private string GetGameFolderSize(string folderPath)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                long totalSize = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                return $"{(totalSize / (1024.0 * 1024 * 1024)):0.00} GB";
            }
            catch
            {
                return "?";
            }
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
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string savePath = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati", "GamesInstalled.dat");

                if (!File.Exists(savePath))
                {
                    return;
                }

                var installedGames = new List<InstalledGame>();
                foreach (var line in File.ReadAllLines(savePath))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2)
                    {
                        installedGames.Add(new InstalledGame
                        {
                            nome_gioco = parts[0],
                            percorso = parts[1]
                        });
                    }
                }
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    installedGames = installedGames
                        .Where(g => g.nome_gioco.ToLower().Contains(searchQuery))
                        .ToList();
                }
                string jsonUrl = "";
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(jsonUrl);
                JObject obj = JObject.Parse(json);
                var catalogo = obj["Catalogo"].ToObject<List<GameInfo>>();

                var steamIdMap = catalogo.ToDictionary(g => g.nome_gioco, g => g.steamdbid);
                tableLayoutPanelHome.Controls.Clear();
                tableLayoutPanelHome.RowStyles.Clear();
                tableLayoutPanelHome.RowCount = 0;
                tableLayoutPanelHome.ColumnCount = 2;

                int col = 0;
                int row = 0;

                foreach (var game in installedGames)
                {
                    if (!steamIdMap.TryGetValue(game.nome_gioco, out string steamdbid))
                        continue;

                    Control gameCard = await CreateGameCardAsync(game.nome_gioco, game.percorso, steamdbid);

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