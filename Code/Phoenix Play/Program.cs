using Phoenix_Play.Classi;
using System.Globalization;

namespace Phoenix_Play
{
    internal static class Program
    {
        public static UserConfig Config;
        public static string ConfigPath { get; private set; }

        [STAThread]
        static void Main()
        {
            string configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Phoenix_Play",
                "ConfigurazioneUtente",
                "userconfig.dat"
            );

            if (string.IsNullOrEmpty(configPath))
            {
                _ = MessageBox.Show("Il percorso di configurazione non è valido.");
                return;
            }

            _ = Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            ConfigPath = configPath;
            Config = UserConfig.Load(configPath);
            if (string.IsNullOrEmpty(Config.Lingua))
            {
                var culture = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
                Config.Lingua = culture == "it" ? "Italiano" : "English";
                Config.Save(configPath);
            }
            CopiaFileTraduzione("Italiano");
            CopiaFileTraduzione("English");
            ApplicationConfiguration.Initialize();
            TraduzioniManager.CaricaTraduzioni(Config.Lingua);
            Application.Run(new Form1());
        }

        private static void CopiaFileTraduzione(string lingua)
        {
            string traduzioneDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Phoenix_Play",
                "Traduttore"
            );
            if (!Directory.Exists(traduzioneDirectoryPath))
            {
                Directory.CreateDirectory(traduzioneDirectoryPath);
            }

            string traduzioneFilePath = Path.Combine(traduzioneDirectoryPath, $"{lingua}.json");

            if (!File.Exists(traduzioneFilePath))
            {
                if (lingua == "Italiano")
                {
                    string contenuto = System.Text.Encoding.UTF8.GetString(Properties.Resources.Italiano);
                    File.WriteAllText(traduzioneFilePath, contenuto);
                }
                else if (lingua == "English")
                {
                    string contenuto = System.Text.Encoding.UTF8.GetString(Properties.Resources.English);
                    File.WriteAllText(traduzioneFilePath, contenuto);
                }
            }
        }
    }
}