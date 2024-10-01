using Microsoft.VisualBasic.FileIO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Spotify_Playlist_CSVs_To_YAML
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string[] paths =
            {
                @"C:\Users\Emanu\Downloads\liked",
                @"C:\Users\Emanu\Downloads\playlists"
            };

            foreach (string path in paths)
            {
                CreateYAML(path);
            }
        }

        private static void CreateYAML(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
                Console.WriteLine($"Error: Directory not found: {rootDirectory}");
                return;
            }

            Dictionary<string, List<Song>> playlistData = BuildPlaylistData(rootDirectory);

            if (playlistData.Count == 0)
            {
                Console.WriteLine("No valid playlists found.");
                return;
            }

            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(playlistData);

            string yamlFileName = Path.GetFileName(rootDirectory);

            string outputYamlPath = Path.Combine(Directory.GetCurrentDirectory(), $"{yamlFileName}.yaml");
            File.WriteAllText(outputYamlPath, yaml);

            Console.WriteLine($"YAML file successfully generated at: {outputYamlPath}");
        }

        private static Dictionary<string, List<Song>> BuildPlaylistData(string rootDirectory)
        {
            Dictionary<string, List<Song>> playlistData = [];

            foreach (string filePath in Directory.GetFiles(rootDirectory, "*.csv"))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string playlistName = ExtractPlaylistName(fileName);
                List<Song> songs = [];

                using (TextFieldParser parser = new(filePath))
                {
                    parser.SetDelimiters([","]);
                    parser.HasFieldsEnclosedInQuotes = true;

                    parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        string[]? fields = parser.ReadFields();

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
                            title = $"'{title}'";
                        }

                        songs.Add(new Song
                        {
                            Artists = artists.Split(',').Select(a => a.Trim()).ToList(),
                            Title = title
                        });
                    }
                }

                if (songs.Count != 0)
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

        private static string ExtractPlaylistName(string fileName)
        {
            string[] parts = fileName.Split(new[] { "kbots_", "_mix_2024" }, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1].Replace('_', ' ').Trim() : fileName;
        }
    }

    internal class Song
    {
        public List<string> Artists { get; set; }
        public string Title { get; set; }
    }
}
