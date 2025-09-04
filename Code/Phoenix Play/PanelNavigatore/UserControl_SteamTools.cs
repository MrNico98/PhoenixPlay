using Microsoft.Win32;
using Phoenix_Play.Classi;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace Phoenix_Play.PanelNavigatore
{
    public partial class UserControl_SteamTools : UserControl
    {
        private SteamToolGame? giocoDaScaricare;
        private bool VerificaToolsAttiva = true;
        public UserControl_SteamTools()
        {
            InitializeComponent();
        }

        private void UserControl_SteamTools_Load(object sender, EventArgs e)
        {
            VerificaToolsAttiva = true;
            VerificaSteamTools();
            GestisciPrimoAvvioDownloadFile();
            InizializzaPercorso();
            InizializzaListaCompleta();
        }
        private async void InizializzaListaCompleta()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync("");

                    var catalogo = System.Text.Json.JsonSerializer.Deserialize<SteamToolCatalogo>(json);

                    listboxcompleta.Items.Clear();

                    foreach (var tool in catalogo.SteamTools)
                    {
                        _ = listboxcompleta.Items.Add(tool.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VerificaSteamTools()
        {
            Log(TraduzioniManager.Traduci("UserControl_SteamTools", "verifica_installazione"));

            bool trovato64bit = false;
            bool trovato32bit = false;

            try
            {
                using (RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SteamTools"))
                {
                    trovato64bit = key64 != null;
                }

                using (RegistryKey key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SteamTools"))
                {
                    trovato32bit = key32 != null;
                }
            }
            catch (Exception ex)
            {
                Log("Error:" + ex.Message);
            }

            if (trovato64bit || trovato32bit)
            {
                Log(TraduzioniManager.Traduci("UserControl_SteamTools", "steamtools_installato"));
                btnInstalla.Image = Properties.Resources.pngScaricaSteamFileApp;
                btnInstalla.Text = TraduzioniManager.Traduci("UserControl_SteamTools", "scarica");
                btnInstalla.Enabled = false;
                VerificaToolsAttiva = false;
            }
            else
            {
                Log("❌ SteamTools NON è installato.");
                btnInstalla.Text = TraduzioniManager.Traduci("UserControl_SteamTools", "installa");
                btnInstalla.Enabled = true;
                VerificaToolsAttiva = true;
            }
        }

        public void GestisciPrimoAvvioDownloadFile()
        {
            if (Program.Config.PrimoAvvioDownloadFile)
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = TraduzioniManager.Traduci("UserControl_SteamTools", "seleziona_cartella_download");
                    folderDialog.UseDescriptionForTitle = true;
                    folderDialog.ShowNewFolderButton = true;
                    if (!string.IsNullOrEmpty(Program.Config.PercorsoInstallazioneSteamToolsFile))
                    {
                        folderDialog.SelectedPath = Program.Config.PercorsoInstallazioneSteamToolsFile;
                    }

                    DialogResult result = folderDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string selectedPath = folderDialog.SelectedPath;
                        string phoneixPlaySteamToolsPath = Path.Combine(selectedPath, "PhoneixPlayFileSteamTools");
                        Program.Config.PercorsoInstallazioneSteamToolsFile = phoneixPlaySteamToolsPath;
                        if (!Directory.Exists(phoneixPlaySteamToolsPath))
                        {
                            _ = Directory.CreateDirectory(phoneixPlaySteamToolsPath);
                        }
                        Program.Config.PrimoAvvioDownloadFile = false;
                        Program.Config.Save(Program.ConfigPath);
                    }
                }
            }
            else
            {
            }
        }

        private void InizializzaPercorso()
        {
            string percorso = Program.Config.PercorsoInstallazioneSteamToolsFile;
            textBox_percorsoinstallazionegiochi.Text = percorso;
        }

        private void BtnInstalla_Click(object sender, EventArgs e)
        {
            if (VerificaToolsAttiva)
            {
                string url = "";
                string tempPath = Path.Combine(Path.GetTempPath(), "st-setup-1.8.16.exe");

                Log("📥 " + TraduzioniManager.Traduci("UserControl_SteamTools", "download_steamtools_in_corso"));
                progressBarDownload.Value = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (s, ev) =>
                    {
                        _ = progressBarDownload.Invoke((MethodInvoker)(() =>
                        {
                            progressBarDownload.Value = ev.ProgressPercentage;
                        }));
                    };

                    wc.DownloadFileCompleted += (s, ev) =>
                    {
                        if (ev.Error != null)
                        {
                            Log("❌ Error: " + ev.Error.Message);
                            return;
                        }

                        Log("✅ " + TraduzioniManager.Traduci("UserControl_SteamTools", "download_completato_installazione"));
                        try
                        {
                            _ = Process.Start(tempPath);
                            btnInstalla.Enabled = false;
                        }
                        catch (Exception ex)
                        {
                            Log("❌ Error: " + ex.Message);
                        }
                    };

                    try
                    {
                        wc.DownloadFileAsync(new Uri(url), tempPath);
                    }
                    catch (Exception ex)
                    {
                        Log("❌ Error: " + ex.Message);
                    }
                }
            }
            else if (giocoDaScaricare != null)
            {
                ScaricaGiocoDaCatalogo(giocoDaScaricare);
            }
        }

        private void ScaricaGiocoDaCatalogo(SteamToolGame gioco)
        {
            try
            {
                Log("🌐 " + TraduzioniManager.Traduci("UserControl_SteamTools", "apertura_browser"));

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = gioco.DownloadUrl,
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Log("❌ Error: " + ex.Message);
            }
        }

        private async void textBox_CercaSteam_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string nomeCercato = textBox_CercaSteam.Text.Trim();
                if (string.IsNullOrEmpty(nomeCercato))
                    return;

                Log("🔍 " + string.Format(
                    TraduzioniManager.Traduci("UserControl_SteamTools", "ricerca_gioco"),
                    nomeCercato
                ));
                progressBarDownload.Value = 30;
                await Task.Delay(4000);

                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        string json = await wc.DownloadStringTaskAsync("");

                        progressBarDownload.Value = 70;
                        await Task.Delay(2000);
                        var catalogo = System.Text.Json.JsonSerializer.Deserialize<SteamToolCatalogo>(json);

                        var risultati = catalogo?.SteamTools?
                            .Where(g => g.Name.Contains(nomeCercato, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (risultati != null && risultati.Count > 0)
                        {
                            if (risultati.Count == 1)
                            {
                                giocoDaScaricare = risultati[0];
                                Log("✅ " + string.Format(
                                    TraduzioniManager.Traduci("UserControl_SteamTools", "gioco_trovato"),
                                    giocoDaScaricare.Name
                                ));
                                btnInstalla.Enabled = true;
                                progressBarDownload.Value = 100;
                                await Task.Delay(5000);
                                progressBarDownload.Value = 0;

                            }
                            else
                            {
                                Log("⚠️ " + string.Format(
                                    TraduzioniManager.Traduci("UserControl_SteamTools", "trovati_n_giochi"),
                                    risultati.Count,
                                    nomeCercato
                                ));
                                foreach (var gioco in risultati)
                                {
                                    Log($"- {gioco.Name}");
                                }

                                Log("ℹ️ " + TraduzioniManager.Traduci("UserControl_SteamTools", "specifica_nome_migliore"));

                                giocoDaScaricare = null;
                                btnInstalla.Enabled = false;
                                progressBarDownload.Value = 0;
                            }
                        }
                        else
                        {
                            Log("❌ " + TraduzioniManager.Traduci("UserControl_SteamTools", "nessun_gioco_trovato"));
                            btnInstalla.Enabled = false;
                            progressBarDownload.Value = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("❌ Error: " + ex.Message);
                }
                finally
                {
                    await Task.Delay(3000);
                }

                e.SuppressKeyPress = true;
            }
        }

        private void Log(string message)
        {
            if (richTextBox1.InvokeRequired)
            {
                _ = richTextBox1.Invoke(new Action<string>(Log), message);
                return;
            }

            richTextBox1.SuspendLayout();
            richTextBox1.SelectionColor = Color.Gray;
            richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] ");

            if (message.Contains("❌"))
                richTextBox1.SelectionColor = Color.FromArgb(220, 80, 80);
            else if (message.Contains("✅") || message.Contains("✔"))
                richTextBox1.SelectionColor = Color.FromArgb(80, 200, 120);
            else if (message.Contains("🔍") || message.Contains("📥"))
                richTextBox1.SelectionColor = Color.FromArgb(100, 160, 220);
            else if (message.Contains("⚠️"))
                richTextBox1.SelectionColor = Color.Goldenrod;
            else if (message.Contains("ℹ️"))
                richTextBox1.SelectionColor = Color.LightSkyBlue;
            else if (message.Contains("🌐"))
                richTextBox1.SelectionColor = Color.DeepSkyBlue;
            else
                richTextBox1.SelectionColor = Color.LightGray;

            richTextBox1.AppendText(message + "\n");
            richTextBox1.ScrollToCaret();
            richTextBox1.ResumeLayout();
        }


        private void btnCambia_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = TraduzioniManager.Traduci("UserControl_SteamTools", "seleziona_cartella_download");

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    string phoneixPlaySteamToolsPath = Path.Combine(selectedPath, "PhoneixPlayFileSteamTools");

                    string finalPath;

                    if (Path.GetFileName(selectedPath).Equals("PhoneixPlayFileSteamTools", StringComparison.OrdinalIgnoreCase))
                    {
                        finalPath = selectedPath;
                    }
                    else if (Directory.Exists(phoneixPlaySteamToolsPath))
                    {
                        finalPath = phoneixPlaySteamToolsPath;
                    }
                    else
                    {
                        finalPath = phoneixPlaySteamToolsPath;
                    }
                    string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Phoenix_Play", "userconfig.dat");
                    Program.Config.PercorsoInstallazioneSteamToolsFile = finalPath;
                    Program.Config.Save(configPath);

                    textBox_percorsoinstallazionegiochi.Text = finalPath;
                }
            }
        }
        private async void listboxcompleta_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listboxcompleta.SelectedItem == null)
                return;

            string nomeSelezionato = listboxcompleta.SelectedItem.ToString();
            Log("📥 " + string.Format(
                TraduzioniManager.Traduci("UserControl_SteamTools", "gioco_selezionato"),
                nomeSelezionato
            ));

            try
            {
                using (WebClient wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync("");

                    var catalogo = System.Text.Json.JsonSerializer.Deserialize<SteamToolCatalogo>(json);

                    var gioco = catalogo?.SteamTools?.FirstOrDefault(g => g.Name.Equals(nomeSelezionato, StringComparison.OrdinalIgnoreCase));

                    if (gioco != null)
                    {
                        giocoDaScaricare = gioco;
                        btnInstalla.Enabled = true;

                        Log("✅ " + string.Format(
                            TraduzioniManager.Traduci("UserControl_SteamTools", "gioco_pronto_download"),
                            gioco.Name
                        ));
                    }
                    else
                    {
                        giocoDaScaricare = null;
                        btnInstalla.Enabled = false;

                        Log("❌ " + TraduzioniManager.Traduci("UserControl_SteamTools", "gioco_non_trovato"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log("❌ " + TraduzioniManager.Traduci("UserControl_SteamTools", "errore_generico") + ": " + ex.Message);
                giocoDaScaricare = null;
                btnInstalla.Enabled = false;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string lingua = Program.Config.Lingua ?? "it";
            string link = lingua switch
            {
                "English" => "https://telegra.ph/SteamTools-08-05-2",
                "Italiano" => "https://telegra.ph/SteamTools-08-05",
                _ => "https://telegra.ph/SteamTools-08-05"
            };
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link) { UseShellExecute = true });
        }
    }
}
