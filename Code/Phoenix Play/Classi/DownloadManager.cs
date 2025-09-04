using Phoenix_Play.Download;

namespace Phoenix_Play.Classi
{
    public class DownloadManager
    {
        public event EventHandler DownloadCompleted;

        private Form1 _mainForm;

        public DownloadManager(Form1 mainForm)
        {
            _mainForm = mainForm;
        }

        public void HandleDownloadClick(Button btn)
        {
            var gameInfo = btn.Tag as GameInfo;

            if (IsGameInstalled(gameInfo.nome_gioco))
            {
                _ = Avvio.AvviaGiocoAsync(gameInfo.nome_gioco);
            }
            else
            {
                StartDownload(gameInfo);
            }
        }

        private bool IsGameInstalled(string gameName)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string saveDirectory = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati");
            string savePath = Path.Combine(saveDirectory, "GamesInstalled.dat");

            if (File.Exists(savePath))
            {
                string? installPath = File.ReadAllLines(savePath)
                                          .FirstOrDefault(l => l.StartsWith(gameName + "|"))
                                          ?.Split('|')[1];

                return !string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath);
            }
            return false;
        }

        private void StartDownload(GameInfo gameInfo)
        {
            _mainForm.downloadInCorso++;
            _mainForm.AggiornaLabelDownload();

            bool downloadTerminato = false;
            var formDownload = new FormDownload(gameInfo)
            {
                Text = $"Download - {gameInfo.nome_gioco}"
            };

            formDownload.DownloadCompleted += (s, args) =>
            {
                downloadTerminato = true;
                _ = _mainForm.Invoke((MethodInvoker)(() =>
                {
                    _mainForm.downloadInCorso--;
                    _mainForm.downloadCompletati++;
                    _mainForm.AggiornaLabelDownload();
                    _mainForm.CaricaLibreria();
                    _mainForm.MostraHome();
                }));
                DownloadCompleted?.Invoke(this, EventArgs.Empty);
            };

            formDownload.FormClosed += (s, args) =>
            {
                if (!downloadTerminato)
                {
                    _ = _mainForm.Invoke((MethodInvoker)(() =>
                    {
                        _mainForm.downloadInCorso--;
                        _mainForm.AggiornaLabelDownload();
                    }));
                }
            };

            formDownload.Show();
        }
    }
}
