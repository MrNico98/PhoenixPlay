using Newtonsoft.Json;
using Phoenix_Play.Classi;
using System.Diagnostics;
using System.Globalization;

namespace Phoenix_Play.PanelNavigatore
{
    public partial class UserControl_Impostazioni : UserControl
    {
        private Form1 _mainform1;
        private string updateUrl = "";
        private string latestVersion = "";
        private static readonly HttpClient client = new HttpClient();
        private bool isInitializing = true;
        public UserControl_Impostazioni(Form1 mainform1)
        {
            InitializeComponent();
            _mainform1 = mainform1;
        }

        private void UserControl_Impostazioni_Load(object sender, EventArgs e)
        {
            InizializzaPercorso();
            InizializzaComboBox();
            _ = InizializzaAggiornamento();
        }
        private async Task InizializzaAggiornamento()
        {
            string updateInfoUrl = "";
            string currentVersion = AppConfig.CurrentVersion;
            string lingua = Program.Config.Lingua switch
            {
                "English" => "EN",
                "Italiano" => "IT",
                _ => "IT",
            };

            try
            {
                richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
                richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "checking_for_updates"));
                await Task.Delay(3000);

                var response = await client.GetStringAsync(updateInfoUrl);
                dynamic updateInfo = JsonConvert.DeserializeObject(response);

                latestVersion = (string)updateInfo.version;
                updateUrl = (string)updateInfo.updateUrl;
                var releaseNotes = updateInfo.releaseNotes[lingua];

                richTextBox1.Clear();

                if (latestVersion != currentVersion)
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
                    richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "new_version_available") + "\n\n");

                    richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                    richTextBox1.AppendText(string.Format(TraduzioniManager.Traduci("UserControl_Impostazioni", "current_version"), currentVersion) + "\n");
                    richTextBox1.AppendText(string.Format(TraduzioniManager.Traduci("UserControl_Impostazioni", "new_version"), latestVersion) + "\n\n");
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                    richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "changes_applied") + ":\n");
                    foreach (var note in releaseNotes)
                    {
                        richTextBox1.AppendText("• " + (string)note + "\n");
                    }

                    richTextBox1.AppendText("\n");
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
                    richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "click_to_update"));

                    btnUpdate.Enabled = true;
                    btnUpdate.Cursor = Cursors.Hand;
                }
                else
                {
                    richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
                    richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "already_up_to_date") + "\n\n");
                    richTextBox1.AppendText(string.Format(TraduzioniManager.Traduci("UserControl_Impostazioni", "current_version"), currentVersion) + "\n\n");
                    richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "thank_you_for_using_latest_version"));

                    btnUpdate.Enabled = false;
                    btnUpdate.Cursor = Cursors.No;
                }
            }
            catch (Exception ex)
            {
                richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
                richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "error_during_check") + "\n\n");
                richTextBox1.AppendText(string.Format(TraduzioniManager.Traduci("UserControl_Impostazioni", "error_message"), ex.Message) + "\n\n");
                richTextBox1.AppendText(TraduzioniManager.Traduci("UserControl_Impostazioni", "retry_later"));

                btnUpdate.Enabled = false;
                btnUpdate.Cursor = Cursors.No;
                btnUpdate.BackColor = SystemColors.Control;
            }

            richTextBox1.BorderStyle = BorderStyle.FixedSingle;
            richTextBox1.BackColor = Color.FromArgb(45, 45, 55);
        }


        private async Task DownloadAndUpdate(string updateUrl, string version)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Phoenix_Play", "Traduttore");
            if (Directory.Exists(folderPath))
            {
                try
                {
                    Directory.Delete(folderPath, true);
                    Console.WriteLine("La cartella 'Traduttore' è stata eliminata.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante l'eliminazione della cartella 'Traduttore': {ex.Message}");
                }
            }
            string updateFilePath = Path.Combine(Path.GetTempPath(), $"Phoenix Play{version}.exe");
            using (var progressForm = new ProgressForm())
            {
                progressForm.Show();
                progressForm.SetMarquee();
                try
                {
                    await DownloadFileWithProgress(updateUrl, updateFilePath, progressForm);
                    string currentExecutablePath = Application.ExecutablePath;
                    File.Move(currentExecutablePath, Path.ChangeExtension(currentExecutablePath, ".old"), true);
                    File.Move(updateFilePath, currentExecutablePath);
                    _ = Process.Start(currentExecutablePath);
                    Application.Exit();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    progressForm.CompleteOperation();
                }
            }
        }

        private async Task DownloadFileWithProgress(string url, string filePath, ProgressForm progressForm)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            _ = response.EnsureSuccessStatusCode();
            var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var buffer = new byte[8192];
            long bytesRead = 0;
            int read;
            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read);
                progressForm.Invoke(new Action(() =>
                progressForm.SetStatus("Download...", (int)((bytesRead * 100) / totalBytes))));
            }
        }

        private void InizializzaComboBox()
        {
            comboBox1.Items.Clear();
            _ = comboBox1.Items.Add("Italiano");
            _ = comboBox1.Items.Add("English");
            string lingua = Program.Config.Lingua ?? "it";
            string cultureCode = lingua switch
            {
                "English" => "en",
                "Italiano" => "it",
                _ => "it",
            };
            isInitializing = true;
            if (comboBox1.Items.Contains(lingua))
            {
                comboBox1.SelectedItem = lingua;
            }
            else
            {
                comboBox1.SelectedItem = "English";
            }
            isInitializing = false;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isInitializing)
                return;
            Program.Config.Lingua = comboBox1.SelectedItem.ToString();
            Program.Config.Save(Program.ConfigPath);
            string cultureCode = Program.Config.Lingua switch
            {
                "English" => "en",
                "Italiano" => "it",
                _ => "it",
            };
            TraduzioniManager.CaricaTraduzioni(Program.Config.Lingua);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            Application.Restart();
        }



        private void InizializzaPercorso()
        {
            string percorso = Program.Config.PercorsoInstallazioneGiochi;
            textBox_percorsoinstallazionegiochi.Text = percorso;
        }

        private void btnCambia_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = TraduzioniManager.Traduci("UserControl_Impostazioni", "select_installation_folder");

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    string phoneixPlayCommonsPath = Path.Combine(selectedPath, "PhoneixPlayCommons");

                    string finalPath;

                    if (Path.GetFileName(selectedPath).Equals("PhoneixPlayCommons", StringComparison.OrdinalIgnoreCase))
                    {
                        finalPath = selectedPath;
                    }
                    else if (Directory.Exists(phoneixPlayCommonsPath))
                    {
                        finalPath = phoneixPlayCommonsPath;
                    }
                    else
                    {
                        finalPath = phoneixPlayCommonsPath;
                    }

                    Program.Config.PercorsoInstallazioneGiochi = finalPath;
                    Program.Config.Save(Program.ConfigPath);
                    textBox_percorsoinstallazionegiochi.Text = finalPath;
                }
            }
        }

        private async void btnInstallaRequisiti_Click(object sender, EventArgs e)
        {
            string tempPath = Path.GetTempPath();

            string dxUrl = "";
            string vcredistUrl = "";

            string dxPath = Path.Combine(tempPath, "dxwebsetup.exe");
            string vcredistPath = Path.Combine(tempPath, "VisualCppRedist_AIO.exe");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    btnInstallaRequisiti.Enabled = false;
                    btnInstallaRequisiti.Text = TraduzioniManager.Traduci("UserControl_Impostazioni", "downloading");

                    using (var dxStream = await client.GetStreamAsync(dxUrl))
                    using (var fileStream = File.Create(dxPath))
                        await dxStream.CopyToAsync(fileStream);
                    using (var vcredistStream = await client.GetStreamAsync(vcredistUrl))
                    using (var fileStream = File.Create(vcredistPath))
                        await vcredistStream.CopyToAsync(fileStream);

                    btnInstallaRequisiti.Text = TraduzioniManager.Traduci("UserControl_Impostazioni", "installing");
                    _ = Process.Start(dxPath);
                    _ = Process.Start(vcredistPath);

                    btnInstallaRequisiti.Text = TraduzioniManager.Traduci("UserControl_Impostazioni", "installed");
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Error: {ex.Message}", "Phoenix Play", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnInstallaRequisiti.Text = TraduzioniManager.Traduci("UserControl_Impostazioni", "error");
                }
                finally
                {
                    btnInstallaRequisiti.Enabled = true;
                }
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;
            richTextBox1.AppendText($"\n\n{TraduzioniManager.Traduci("UserControl_Impostazioni", "updating_started")}");

            try
            {
                await DownloadAndUpdate(updateUrl, latestVersion);
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"\n❌ {TraduzioniManager.Traduci("UserControl_Impostazioni", "error")} {ex.Message}");
                btnUpdate.Enabled = true;
            }
        }
    }
}
