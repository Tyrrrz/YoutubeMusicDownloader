using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace YoutubeMusicDownloader
{
    public class Program
    {
        private static readonly YoutubeClient YoutubeClient = new YoutubeClient();
        private static readonly YoutubeConverter YoutubeConverter = new YoutubeConverter(YoutubeClient);

        private static readonly string OutputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Output");

        private static async Task DownloadAndConvertVideoAsync(string id)
        {
            Console.WriteLine($"Working on video [{id}]...");

            // Get video info
            var video = await YoutubeClient.GetVideoAsync(id);
            var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');
            Console.WriteLine($"{video.Title}");

            // Download video as mp3
            Console.WriteLine("Downloading...");
            Directory.CreateDirectory(OutputDirectoryPath);
            var outputFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}.mp3");
            await YoutubeConverter.DownloadVideoAsync(id, outputFilePath);

            // Edit mp3 metadata
            Console.WriteLine("Writing metadata...");
            var idMatch = Regex.Match(video.Title, @"^(?<artist>.*?)-(?<title>.*?)$");
            var artist = idMatch.Groups["artist"].Value.Trim();
            var title = idMatch.Groups["title"].Value.Trim();
            using (var meta = TagLib.File.Create(outputFilePath))
            {
                meta.Tag.Performers = new[] {artist};
                meta.Tag.Title = title;
                meta.Save();
            }

            Console.WriteLine($"Downloaded and converted video [{id}] to [{outputFilePath}]");
        }

        private static async Task DownloadAndConvertPlaylistAsync(string id)
        {
            Console.WriteLine($"Working on playlist [{id}]...");

            // Get playlist info
            var playlist = await YoutubeClient.GetPlaylistAsync(id);
            Console.WriteLine($"{playlist.Title} ({playlist.Videos.Count} videos)");

            // Work on the videos
            Console.WriteLine();
            foreach (var video in playlist.Videos)
            {
                await DownloadAndConvertVideoAsync(video.Id);
                Console.WriteLine();
            }
        }

        private static async Task MainAsync(string[] args)
        {
            foreach (var arg in args)
            {
                // Try to determine the type of the URL/ID that was given

                // Playlist ID
                if (YoutubeClient.ValidatePlaylistId(arg))
                {
                    await DownloadAndConvertPlaylistAsync(arg);
                }

                // Playlist URL
                else if (YoutubeClient.TryParsePlaylistId(arg, out var playlistId))
                {
                    await DownloadAndConvertPlaylistAsync(playlistId);
                }

                // Video ID
                else if (YoutubeClient.ValidateVideoId(arg))
                {
                    await DownloadAndConvertVideoAsync(arg);
                }

                // Video URL
                else if (YoutubeClient.TryParseVideoId(arg, out var videoId))
                {
                    await DownloadAndConvertVideoAsync(videoId);
                }

                // Unknown
                else
                {
                    throw new ArgumentException($"Unrecognized URL or ID: [{arg}]", nameof(arg));
                }

                Console.WriteLine();
            }

            Console.WriteLine("Done");
        }

        public static void Main(string[] args)
        {
            Console.Title = "Youtube Music Downloader";

            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}