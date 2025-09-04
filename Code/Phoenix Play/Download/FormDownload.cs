using Microsoft.Playwright;
using Newtonsoft.Json.Linq;
using Phoenix_Play.Classi;
using System.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Phoenix_Play.Download
{
    public partial class FormDownload : Form
    {
        private GameInfo _gameInfo;
        private HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _downloadStopwatch;
        private long _totalBytes;
        public event EventHandler DownloadCompleted;
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;
        public int ProgressPercentage { get; private set; }
        public string FileName { get; private set; }
        public bool IsDownloading { get; private set; } = true;
        public FormDownload(GameInfo gameInfo)
        {
            InitializeComponent();
            FileName = gameInfo.nome_gioco;
            _gameInfo = gameInfo;
            lblTitle.Text = _gameInfo.nome_gioco;
            _ = Task.Run(async () =>
            {
                var image = await LoadSteamGameImageAsync(_gameInfo.steamdbid);
                _ = this.Invoke((MethodInvoker)delegate
                {
                    pictureBoxGame.Image = image;
                });
            });
            _ = Task.Run(async () =>
            {
                var image = await LoadSteamGameImageAsync(_gameInfo.steamdbid);
                _ = this.Invoke((MethodInvoker)delegate
                {
                    pictureBoxGame.Image = image;
                });
            });
            StartDownload();
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

        private async void StartDownload()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _downloadStopwatch = Stopwatch.StartNew();

            var syncContext = SynchronizationContext.Current;

            try
            {
                string downloadUrl = _gameInfo.download;

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "errore_link_mancante");
                    return;
                }
                else
                {
                    var token = _cancellationTokenSource.Token;
                    progressBarMain.Style = ProgressBarStyle.Marquee;
                    progressBarMain.MarqueeAnimationSpeed = 30;
                    downloadUrl = await GetDownloadUrl(this, token, syncContext);
                    await Task.Delay(3000);
                    progressBarMain.Style = ProgressBarStyle.Continuous;
                    progressBarMain.Value = 0;
                    if (string.IsNullOrEmpty(downloadUrl))
                    {
                        lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "errore_link_mancante");
                        return;
                    }
                }

                _gameInfo.download = downloadUrl;
                _httpClient = new HttpClient();
                var response = await _httpClient.GetAsync(_gameInfo.download, HttpCompletionOption.ResponseHeadersRead);

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "errore_500");
                    return;
                }

                _ = response.EnsureSuccessStatusCode();
                lblTitle.Text = _gameInfo.nome_gioco + " - " + TraduzioniManager.Traduci("FormDownload", "download_in_corso");
                _totalBytes = response.Content.Headers.ContentLength ?? -1;
                var receivedBytes = 0L;
                var buffer = new byte[8192];
                var tempPath = Path.GetTempPath();
                _ = Directory.CreateDirectory(tempPath);
                var zipFilePath = Path.Combine(tempPath, $"{_gameInfo.nome_gioco}.zip");

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);

                        receivedBytes += bytesRead;
                        var percentage = (int)((double)receivedBytes / _totalBytes * 100);
                        var speed = receivedBytes / _downloadStopwatch.Elapsed.TotalSeconds;
                        var remainingTime = TimeSpan.FromSeconds((_totalBytes - receivedBytes) / speed);
                        this.Invoke(new Action(() =>
                        {
                            ProgressPercentage = percentage;
                            progressBarMain.Value = percentage;
                            lblPercentage.Text = $"{percentage}% - {FormatBytes(receivedBytes)} / {FormatBytes(_totalBytes)}";
                            lblSpeed.Text = $"{TraduzioniManager.Traduci("FormDownload", "velocita")}: {FormatBytes((long)speed)}/s";
                            lblTimeRemaining.Text = $"{TraduzioniManager.Traduci("FormDownload", "tempo_rimanente")}: {FormatTimeSpan(remainingTime)}";
                        }));
                    }
                }

                _ = Task.Run(() => ExtractGame(zipFilePath));
            }
            catch (OperationCanceledException)
            {
                lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "download_annullato");
            }
            catch (Exception ex)
            {
                lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "errore_durante_download") + ": " + ex.Message;
            }
            finally
            {
                _downloadStopwatch.Stop();
                _httpClient?.Dispose();
            }
        }

        static async Task<string?> GetDownloadUrl(FormDownload formDownload, CancellationToken token, SynchronizationContext syncContext)
        {
            string? downloadUrl = null;
            var allRequests = new List<string>();
            IPage? page = null;
            IBrowser? browser = null;

            try
            {
                syncContext.Post(_ =>
                {
                    formDownload.lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "inizio_recupero_metadati");
                }, null);

                using var playwright = await Playwright.CreateAsync();
                browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                page = await browser.NewPageAsync();

                page.Request += (_, request) =>
                {
                    allRequests.Add(request.Url);

                    if (request.Url.Contains("pixeldrain.com/api/file/") && request.Url.Contains("download"))
                    {
                        downloadUrl = request.Url;
                        syncContext.Post(_ =>
                        {
                            formDownload.lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "url_identificato");
                        }, null);
                    }
                };

                _ = await page.GotoAsync(formDownload._gameInfo.download);

                int maxAttempts = 10;
                int attempt = 0;
                bool progressMessageShown = false;

                while (attempt < maxAttempts && downloadUrl == null)
                {
                    token.ThrowIfCancellationRequested();
                    attempt++;

                    if (!progressMessageShown && attempt > 3)
                    {
                        syncContext.Post(_ =>
                        {
                            formDownload.lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "analisi_in_corso");
                        }, null);
                        progressMessageShown = true;
                    }

                    try
                    {
                        var button = page.Locator("button.button_highlight:has-text('Download')");

                        if (await button.CountAsync() > 0)
                        {
                            await button.First.ClickAsync();
                        }
                        else
                        {
                            var fallback = page.Locator("button.button_highlight").Nth(1);
                            if (await fallback.CountAsync() > 0)
                            {
                                await fallback.ClickAsync();
                            }
                        }
                    }
                    catch
                    {
                        if (attempt == 1)
                        {
                            syncContext.Post(_ =>
                            {
                                formDownload.lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "verifica_elementi");
                            }, null);
                        }
                    }

                    await Task.Delay(5000, token);

                    if (downloadUrl != null)
                        break;

                    await Task.Delay(new Random().Next(1000, 3000), token);
                }

                if (downloadUrl != null)
                {
                    await Task.Delay(2000, token);
                    return downloadUrl;
                }
                else
                {
                    syncContext.Post(_ =>
                    {
                        formDownload.lblPercentage.Text = string.Format(
                            TraduzioniManager.Traduci("FormDownload", "errore_url_non_recuperato"),
                            maxAttempts
                        );
                    }, null);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                syncContext.Post(_ =>
                {
                    formDownload.lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "operazione_annullata");
                }, null);
                return null;
            }
            catch (Exception e)
            {
                syncContext.Post(_ =>
                {
                    formDownload.lblPercentage.Text = TraduzioniManager.Traduci("FormDownload", "errore_generico") + ": " + e.Message;
                }, null);
                return null;
            }
            finally
            {
                if (page != null)
                    await page.CloseAsync();
                if (browser != null)
                    await browser.CloseAsync();
            }
        }


        private void ExtractGame(string zipFilePath)
        {
            try
            {
                string phoenixPlayCommonsPath = Program.Config.PercorsoInstallazioneGiochi;
                string gameInstallPath = Path.Combine(phoenixPlayCommonsPath, _gameInfo.nome_gioco);

                _ = Directory.CreateDirectory(gameInstallPath);

                _ = this.Invoke((MethodInvoker)(() =>
                {
                    lblTitle.Text = $"{_gameInfo.nome_gioco} - {TraduzioniManager.Traduci("FormDownload", "installazione_in_corso")}";
                    progressBarMain.Value = 0;
                    lblPercentage.Text = "0%";
                    lblSpeed.Text = "0 KB/s";
                    lblTimeRemaining.Text = TraduzioniManager.Traduci("FormDownload", "calcolo");
                    progressBarMain.Style = ProgressBarStyle.Continuous;
                }));

                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    long totalBytesToExtract = archive.Entries.Sum(e => e.Length);
                    long extractedBytes = 0;

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    DateTime lastUpdateTime = DateTime.Now;

                    foreach (var entry in archive.Entries)
                    {
                        string destinationPath = Path.Combine(gameInstallPath, entry.FullName);

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            _ = Directory.CreateDirectory(destinationPath);
                        }
                        else
                        {
                            _ = Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                            using (var entryStream = entry.Open())
                            using (var fileStream = File.Create(destinationPath))
                            {
                                byte[] buffer = new byte[81920];
                                int bytesRead;
                                while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                    extractedBytes += bytesRead;

                                    if ((DateTime.Now - lastUpdateTime).TotalMilliseconds >= 100)
                                    {
                                        double progressPercentage = (double)extractedBytes / totalBytesToExtract * 100;
                                        int percentage = (int)progressPercentage;
                                        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                                        double speed = elapsedSeconds > 0 ? extractedBytes / elapsedSeconds : 0;
                                        double remainingSeconds = speed > 0 ? (totalBytesToExtract - extractedBytes) / speed : 0;

                                        _ = this.Invoke((MethodInvoker)delegate
                                        {
                                            progressBarMain.Value = Math.Min(percentage, 100);
                                            lblPercentage.Text = $"{percentage}% - {FormatBytes(extractedBytes)} / {FormatBytes(totalBytesToExtract)}";
                                            lblSpeed.Text = $"{TraduzioniManager.Traduci("FormDownload", "velocita")} {FormatSpeed(speed)}";
                                            lblTimeRemaining.Text = $"{TraduzioniManager.Traduci("FormDownload", "tempo_rimanente")} {FormatTimeRemaining(remainingSeconds)}";
                                        });

                                        lastUpdateTime = DateTime.Now;
                                    }
                                }
                            }
                        }
                    }

                    stopwatch.Stop();
                }

                _ = this.Invoke((MethodInvoker)(() =>
                {
                    lblTitle.Text = $"{_gameInfo.nome_gioco} - {TraduzioniManager.Traduci("FormDownload", "installazione_completata")}";
                    progressBarMain.Value = 100;
                    lblPercentage.Text = "100%";
                    lblSpeed.Text = TraduzioniManager.Traduci("FormDownload", "completato");
                    lblTimeRemaining.Text = "00:00:00";
                    SalvaGioco();
                    Program.Config.Save(Program.ConfigPath);
                    DownloadCompleted?.Invoke(this, EventArgs.Empty);
                }));
            }
            catch (Exception ex)
            {
                _ = this.Invoke((MethodInvoker)(() =>
                {
                    lblPercentage.Text = $"{TraduzioniManager.Traduci("FormDownload", "errore_estrazione")}: {ex.Message}";
                    lblSpeed.Text = TraduzioniManager.Traduci("FormDownload", "errore");
                    lblTimeRemaining.Text = "--:--:--";
                }));
            }
        }

        private string FormatSpeed(double bytesPerSecond)
        {
            return $"{FormatBytes((long)bytesPerSecond)}/s";
        }

        private string FormatTimeRemaining(double seconds)
        {
            if (seconds <= 0)
                return "00:00:00";

            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
        }

        private void SalvaGioco()
        {
            try
            {
                string phoenixPlayCommonsPath = Program.Config.PercorsoInstallazioneGiochi;
                string gameInstallPath = Path.Combine(phoenixPlayCommonsPath, _gameInfo.nome_gioco);
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string saveDirectory = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati");
                _ = Directory.CreateDirectory(saveDirectory);
                string savePath = Path.Combine(saveDirectory, "GamesInstalled.dat");
                List<string> lines = new List<string>();
                if (File.Exists(savePath))
                {
                    lines = File.ReadAllLines(savePath)
                                .Where(line => !string.IsNullOrWhiteSpace(line))
                                .ToList();
                }
                lines.Add($"{_gameInfo.nome_gioco}|{gameInstallPath}");
                File.WriteAllLines(savePath, lines);

                _ = this.Invoke((MethodInvoker)(() =>
                {
                    lblPercentage.Text = $"{TraduzioniManager.Traduci("FormDownload", "informazioni_salvate")}\n";
                }));

                Program.Config.Save(Program.ConfigPath);
            }
            catch (Exception ex)
            {
                _ = this.Invoke((MethodInvoker)(() =>
                {
                    lblPercentage.Text = $"{TraduzioniManager.Traduci("FormDownload", "errore_salvataggio")}: {ex.Message}\n";
                }));
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double formattedSize = bytes;

            while (formattedSize >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                formattedSize /= 1024;
                suffixIndex++;
            }

            return $"{formattedSize:0.##} {suffixes[suffixIndex]}";
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
            else
                return $"{(int)timeSpan.TotalSeconds}s";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void FormDownload_Shown(object sender, EventArgs e)
        {
            EnableDragging(this);
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
    }
}
