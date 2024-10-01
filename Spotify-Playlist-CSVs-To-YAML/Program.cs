using Microsoft.VisualBasic.FileIO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Spotify_Playlist_CSVs_To_YAML
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string rootDirectory = @"C:\Users\Emanu\Downloads\playlists";

            if (!Directory.Exists(rootDirectory))
            {
                Console.WriteLine($"Error: Directory not found: {rootDirectory}");
                return;
            }

            var playlistData = BuildPlaylistData(rootDirectory);

            if (playlistData.Count == 0)
            {
                Console.WriteLine("No valid playlists found.");
                return;
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(playlistData);

            string outputYamlPath = Path.Combine(Directory.GetCurrentDirectory(), "playlists.yaml");
            File.WriteAllText(outputYamlPath, yaml);

            Console.WriteLine($"YAML file successfully generated at: {outputYamlPath}");
        }

        static Dictionary<string, List<Song>> BuildPlaylistData(string rootDirectory)
        {
            var playlistData = new Dictionary<string, List<Song>>();

            foreach (var filePath in Directory.GetFiles(rootDirectory, "*.csv"))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string playlistName = ExtractPlaylistName(fileName);
                var songs = new List<Song>();

                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.SetDelimiters(new string[] { "," });
                    parser.HasFieldsEnclosedInQuotes = true;

                    parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        if (fields.Length < 4)
                        {
                            Console.WriteLine($"Skipping row due to insufficient data in file: {fileName}.csv");
                            continue;
                        }

                        string title = fields[1].Trim().Trim('"');
                        string artists = fields[3].Trim().Trim('"');

                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artists))
                        {
                            Console.WriteLine($"Skipping row with missing title or artists in file: {fileName}.csv");
                            continue;
                        }

                        if (int.TryParse(title, out _))
                        {
                            title = $"\"{title}\"";
                        }

                        songs.Add(new Song
                        {
                            Artists = artists.Split(',').Select(a => a.Trim()).ToList(),
                            Title = title
                        });
                    }
                }

                if (songs.Any())
                {
                    playlistData[playlistName] = songs;
                }
                else
                {
                    Console.WriteLine($"No valid songs found in file: {fileName}.csv");
                }
            }

            return playlistData;
        }

        static string ExtractPlaylistName(string fileName)
        {
            var parts = fileName.Split(new[] { "kbots_", "_mix_2024" }, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1].Replace('_', ' ').Trim() : "Unknown Playlist";
        }
    }

    class Song
    {
        public List<string> Artists { get; set; }
        public string Title { get; set; }
    }
}
