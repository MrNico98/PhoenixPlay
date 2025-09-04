using Newtonsoft.Json;

namespace Phoenix_Play.Classi
{
    public static class TraduzioniManager
    {
        private static Dictionary<string, Dictionary<string, string>>? _traduzioni;
        private static string? _lingua;

        public static void CaricaTraduzioni(string lingua)
        {
            _lingua = lingua;
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Phoenix_Play",
                "Traduttore",
                $"{lingua}.json"
            );

            if (!File.Exists(filePath))
            {
                _traduzioni = new Dictionary<string, Dictionary<string, string>>();
                return;
            }

            string json = File.ReadAllText(filePath);
            _traduzioni = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        }

        public static string Traduci(string sezione, string chiave)
        {
            if (_traduzioni == null) return chiave;

            if (_traduzioni.TryGetValue(sezione, out var sez))
            {
                if (sez.TryGetValue(chiave, out var testo))
                    return testo;
            }

            return chiave;
        }
        public static string Traduci(string sezione, string chiave, params object[] args)
        {
            string testo = Traduci(sezione, chiave);
            return string.Format(testo, args);
        }

    }
}
