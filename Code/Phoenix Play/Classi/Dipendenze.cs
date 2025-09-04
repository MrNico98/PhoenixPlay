using System.Text.Json.Serialization;

namespace Phoenix_Play.Classi
{
    public class GameInfo
    {
        public string nome_gioco { get; set; }
        public string steamdbid { get; set; }
        public string versione { get; set; }
        public string download { get; set; }
        public string exe { get; set; }
        public List<string> avvii { get; set; }
        public string dipendenze { get; set; }
        public bool online { get; set; }
    }

    public class SteamGameStats
    {
        public string Name { get; set; }
        public string TotalReviews { get; set; }
        public string Metacritic { get; set; }
        public string CurrentPlayers { get; set; }
        public double PositiveReviewPercentage { get; set; }
        public double NegativeReviewPercentage { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public string Size { get; set; } = "?";
    }
    public class RecentGame
    {
        public string GameName { get; set; }
        public string InstallPath { get; set; }
    }
    public class InstalledGame
    {
        public string nome_gioco { get; set; }
        public string percorso { get; set; }
    }
    public static class AppConfig
    {
        public static string CurrentVersion { get; set; } = "0.0.0.2";
    }
    public class SteamToolGame
    {
        [JsonPropertyName("nome_gioco")]
        public string Name { get; set; }

        [JsonPropertyName("download")]
        public string DownloadUrl { get; set; }
    }
    public class SteamToolCatalogo
    {
        [JsonPropertyName("SteamTools")]
        public List<SteamToolGame> SteamTools { get; set; }
    }
}
