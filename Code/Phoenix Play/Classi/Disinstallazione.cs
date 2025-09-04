namespace Phoenix_Play.Classi
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal class Disinstallazione
    {
        private string nomeGioco;
        private string savePath;
        private Control pannelloContenitore;
        private Form1 _mainForm;
        public Disinstallazione(string nomeGioco, Control pannelloContenitore, Form1 mainForm)
        {
            this.nomeGioco = nomeGioco;
            _mainForm = mainForm;
            this.pannelloContenitore = pannelloContenitore;
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string saveDirectory = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati");
            savePath = Path.Combine(saveDirectory, "GamesInstalled.dat");
        }

        public void AggiungiContextMenuDisinstalla(Control pannelloGioco)
        {
            ContextMenuStrip contextMenu = new();
            ToolStripMenuItem disinstallaItem = new(TraduzioniManager.Traduci("Form1", "disinstalla"));

            disinstallaItem.Click += async (sender, e) => await DisinstallaGioco(pannelloGioco);

            _ = contextMenu.Items.Add(disinstallaItem);
            pannelloGioco.ContextMenuStrip = contextMenu;
        }
        public async Task DisinstallaDirettamente(Control pannelloGioco)
        {
            await DisinstallaGioco(pannelloGioco);
        }
        private async Task DisinstallaGioco(Control pannelloGioco)
        {
            var result = MessageBox.Show(
                TraduzioniManager.Traduci("Form1", "conferma_disinstallazione_testo", nomeGioco),
                TraduzioniManager.Traduci("Form1", "conferma_disinstallazione_titolo"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var disinstallazioneComando = await ControllaDisinstallazioneNelCatalogo();
                    if (!string.IsNullOrEmpty(disinstallazioneComando))
                    {
                        AvviaComandoDisinstallazione(disinstallazioneComando);
                    }

                    string[] allLines = File.ReadAllLines(savePath);
                    string gameLine = allLines.FirstOrDefault(l => l.StartsWith(nomeGioco + "|"));

                    if (gameLine != null)
                    {
                        string[] parts = gameLine.Split('|');
                        if (parts.Length == 2)
                        {
                            string installPath = parts[1];
                            if (Directory.Exists(installPath))
                            {
                                try
                                {
                                    Directory.Delete(installPath, true);
                                }
                                catch (Exception ex)
                                {
                                    _ = MessageBox.Show(
                                        TraduzioniManager.Traduci("Form1", "errore_eliminazione_cartella", ex.Message),
                                        TraduzioniManager.Traduci("Form1", "errore"),
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                    return;
                                }
                            }
                        }

                        List<string> updatedLines = allLines.Where(l => !l.StartsWith(nomeGioco + "|")).ToList();
                        File.WriteAllLines(savePath, updatedLines);
                    }

                    pannelloContenitore.Controls.Remove(pannelloGioco);
                    _mainForm.MostraHome();
                    _mainForm.CaricaLibreria();
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show(
                        TraduzioniManager.Traduci("Form1", "errore_disinstallazione", ex.Message),
                        TraduzioniManager.Traduci("Form1", "errore"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }


        private async Task<string?> ControllaDisinstallazioneNelCatalogo()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = "";
                    string jsonContent = await client.GetStringAsync(url);

                    JObject catalogoJson = JObject.Parse(jsonContent);
                    var gioco = catalogoJson["Catalogo"]
                        .FirstOrDefault(j => j["nome_gioco"].ToString() == nomeGioco);

                    if (gioco != null)
                    {
                        return gioco["disinstallazione"]?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Errore durante il recupero del catalogo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return null;
        }

        private void AvviaComandoDisinstallazione(string comando)
        {
            if (File.Exists(comando))
            {
                try
                {
                    _ = System.Diagnostics.Process.Start(comando);
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Errore nell'avvio del comando di disinstallazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _ = MessageBox.Show("Il comando di disinstallazione non è stato trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
