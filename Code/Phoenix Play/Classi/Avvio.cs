using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Phoenix_Play.Classi
{
    public static class Avvio
    {
        public static async Task AvviaGiocoAsync(string nomeGioco)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string savePath = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati", "GamesInstalled.dat");

            if (!File.Exists(savePath))
            {
                _ = MessageBox.Show("File dei giochi installati non trovato.");
                return;
            }

            string? installPath = null;
            bool avviiFatti = false;
            var lines = File.ReadAllLines(savePath).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                string[] parts = lines[i].Split('|');
                if (parts.Length >= 2 && parts[0] == nomeGioco)
                {
                    installPath = parts[1];
                    if (parts.Length >= 3 && parts[2] == "avvifatto")
                        avviiFatti = true;
                    break;
                }
            }

            if (installPath == null)
            {
                _ = MessageBox.Show("Percorso di installazione non trovato.");
                return;
            }

            try
            {
                using HttpClient client = new();
                string json = await client.GetStringAsync("");
                var remoteGames = JObject.Parse(json)["Catalogo"].ToObject<List<GameInfo>>();
                var matchedGame = remoteGames.FirstOrDefault(g => g.nome_gioco == nomeGioco);

                if (matchedGame == null)
                {
                    _ = MessageBox.Show("Gioco non trovato nei dati remoti.");
                    return;
                }
                string? TrovaFileRicorsivo(string directory, string fileName)
                {
                    try
                    {
                        string directPath = Path.Combine(directory, fileName);
                        if (File.Exists(directPath))
                            return directPath;
                        var files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
                        return files.FirstOrDefault();
                    }
                    catch
                    {
                        return null;
                    }
                }
                string dependencyExe = matchedGame.dipendenze;
                if (!string.IsNullOrEmpty(dependencyExe))
                {
                    string? dependencyPath = TrovaFileRicorsivo(installPath, dependencyExe);
                    if (dependencyPath == null)
                    {
                        string steamPath = GetSteamInstallPath();
                        if (steamPath == null)
                        {
                            _ = MessageBox.Show("Impossibile trovare Steam nel sistema.");
                            return;
                        }

                        string steamExePath = Path.Combine(steamPath, dependencyExe);
                        if (!File.Exists(steamExePath))
                        {
                            _ = MessageBox.Show($"Impossibile avviare il gioco. Dipendenza mancante: {dependencyExe}");
                            return;
                        }

                        ProcessStartInfo steamProcess = new ProcessStartInfo
                        {
                            FileName = steamExePath,
                            WorkingDirectory = Path.GetDirectoryName(steamExePath),
                            UseShellExecute = true
                        };
                        _ = Process.Start(steamProcess);
                        await Task.Delay(2000);
                    }
                    else
                    {
                        ProcessStartInfo depProcess = new ProcessStartInfo
                        {
                            FileName = dependencyPath,
                            WorkingDirectory = Path.GetDirectoryName(dependencyPath),
                            UseShellExecute = true
                        };
                        _ = Process.Start(depProcess);
                        await Task.Delay(2000);
                    }
                }
                if (matchedGame.avvii != null && matchedGame.avvii.Count > 0 && !avviiFatti)
                {
                    foreach (var avvio in matchedGame.avvii)
                    {
                        string? avvioPath = TrovaFileRicorsivo(installPath, avvio);
                        if (avvioPath != null)
                        {
                            string extension = Path.GetExtension(avvioPath).ToLower();
                            if (extension == ".bat" || extension == ".cmd")
                            {
                                ProcessStartInfo batProcess = new ProcessStartInfo
                                {
                                    FileName = "cmd.exe",
                                    Arguments = $"/C \"{avvioPath}\"",
                                    WorkingDirectory = Path.GetDirectoryName(avvioPath),
                                    CreateNoWindow = false,
                                    UseShellExecute = true
                                };
                                _ = Process.Start(batProcess);
                            }
                            else if (extension == ".exe")
                            {
                                ProcessStartInfo exeProcess = new ProcessStartInfo
                                {
                                    FileName = avvioPath,
                                    WorkingDirectory = Path.GetDirectoryName(avvioPath),
                                    UseShellExecute = true
                                };
                                _ = Process.Start(exeProcess);
                            }
                            else
                            {
                                _ = Process.Start(new ProcessStartInfo
                                {
                                    FileName = avvioPath,
                                    WorkingDirectory = Path.GetDirectoryName(avvioPath),
                                    UseShellExecute = true
                                });
                            }

                            await Task.Delay(2000);
                        }
                        else
                        {
                            _ = MessageBox.Show($"File di avvio '{avvio}' non trovato.");
                        }
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var parts = lines[i].Split('|');
                        if (parts.Length >= 2 && parts[0] == nomeGioco)
                        {
                            lines[i] = $"{parts[0]}|{parts[1]}|avvifatto";
                            break;
                        }
                    }
                    File.WriteAllLines(savePath, lines);
                }
                else if (!string.IsNullOrEmpty(matchedGame.exe))
                {
                    string? exePath = TrovaFileRicorsivo(installPath, matchedGame.exe);
                    if (exePath != null)
                    {
                        ProcessStartInfo exeProcess = new ProcessStartInfo
                        {
                            FileName = exePath,
                            WorkingDirectory = Path.GetDirectoryName(exePath),
                            UseShellExecute = true
                        };

                        using Process process = Process.Start(exeProcess);
                        if (process != null)
                        {
                            DateTime startTime = DateTime.Now;
                            await Task.Run(() => process.WaitForExit());
                            DateTime endTime = DateTime.Now;
                            TimeSpan playedTime = endTime - startTime;
                            AggiornaTempoGiocato(nomeGioco, playedTime);
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show("File exe non trovato.");
                    }
                }
                else
                {
                    _ = MessageBox.Show("Nessun eseguibile o lista di avvii trovati.");
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show("Errore durante l'avvio del gioco: " + ex.Message);
            }
        }

        private static void AggiornaTempoGiocato(string nomeGioco, TimeSpan playedTime)
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string savePath = Path.Combine(localAppDataPath, "Phoenix_Play", "GiochiInstallati", "GamesInstalled.dat");

            if (!File.Exists(savePath))
                return;

            var lines = File.ReadAllLines(savePath).ToList();
            bool trovato = false;

            for (int i = 0; i < lines.Count; i++)
            {
                var parts = lines[i].Split('|');
                if (parts.Length >= 2 && parts[0] == nomeGioco)
                {
                    double totalSeconds = 0;
                    if (parts.Length >= 4)
                        double.TryParse(parts[3], out totalSeconds);

                    totalSeconds += playedTime.TotalSeconds;
                    if (parts.Length >= 4)
                        parts[3] = totalSeconds.ToString();
                    else
                        lines[i] = $"{parts[0]}|{parts[1]}|{(parts.Length >= 3 ? parts[2] : "")}|{totalSeconds}";

                    lines[i] = string.Join("|", parts);
                    trovato = true;
                    break;
                }
            }

            if (!trovato)
            {
                lines.Add($"{nomeGioco}|PercorsoSconosciuto||{playedTime.TotalSeconds}");
            }

            File.WriteAllLines(savePath, lines);
        }

        private static string GetSteamInstallPath()
        {
            try
            {
                string steamPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null);
                if (string.IsNullOrEmpty(steamPath))
                {
                    steamPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);
                }
                return steamPath;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
