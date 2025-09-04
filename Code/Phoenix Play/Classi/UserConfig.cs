namespace Phoenix_Play.Classi
{
    public class UserConfig
    {
        public bool PrimoAvvioDownload { get; set; }
        public string PercorsoInstallazioneGiochi { get; set; }
        public string Lingua { get; set; }
        public bool PrimoAvvioDownloadFile { get; set; }
        public string PercorsoInstallazioneSteamToolsFile { get; set; }

        public static UserConfig Load(string path)
        {
            var config = new UserConfig
            {
                PrimoAvvioDownload = true,
                PrimoAvvioDownloadFile = true
            };

            if (!File.Exists(path)) return config;

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                switch (key)
                {
                    case "PrimoAvvioDownload":
                        config.PrimoAvvioDownload = bool.Parse(value);
                        break;
                    case "PercorsoInstallazioneGiochi":
                        config.PercorsoInstallazioneGiochi = value;
                        break;
                    case "Lingua":
                        config.Lingua = value;
                        break;
                    case "PrimoAvvioDownloadFile":
                        config.PrimoAvvioDownloadFile = bool.Parse(value);
                        break;
                    case "PercorsoInstallazioneSteamToolsFile":
                        config.PercorsoInstallazioneSteamToolsFile = value;
                        break;
                }
            }

            return config;
        }
        public void Save(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "Il percorso non può essere null o vuoto.");
            }
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }
            var lines = new List<string>
    {
        $"PrimoAvvioDownload={PrimoAvvioDownload}",
        $"PercorsoInstallazioneGiochi={PercorsoInstallazioneGiochi}",
        $"Lingua={Lingua}",
        $"PrimoAvvioDownloadFile={PrimoAvvioDownloadFile}",
        $"PercorsoInstallazioneSteamToolsFile={PercorsoInstallazioneSteamToolsFile}"
    };

            try
            {
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Errore durante il salvataggio del file di configurazione: {ex.Message}");
            }
        }
    }
}
