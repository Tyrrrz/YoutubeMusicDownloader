using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using YoutubeExplode;
using YoutubeExplode.Models;
using Tyrrrz.Extensions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeMusicDownloader
{
    public class Program
    {
        private static readonly YoutubeClient YoutubeClient = new YoutubeClient();
        private static readonly Cli FfmpegCli = new Cli("ffmpeg.exe");

        private static readonly string TempDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
        private static readonly string OutputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Output");

        private static MediaStreamInfo GetBestAudioStreamInfo(VideoInfo videoInfo)
        {
            if (videoInfo.AudioStreams.Any())
                return videoInfo.AudioStreams.OrderBy(s => s.Bitrate).Last();
            if (videoInfo.MixedStreams.Any())
                return videoInfo.MixedStreams.OrderBy(s => s.VideoQuality).Last();
            throw new Exception("No applicable media streams found for this video");
        }

        private static async Task DownloadAndConvertVideoAsync(string id)
        {
            Console.WriteLine($"Working on video [{id}]...");

            // Get video info
            var videoInfo = await YoutubeClient.GetVideoInfoAsync(id);
            string cleanTitle = videoInfo.Title.Except(Path.GetInvalidFileNameChars());
            Console.WriteLine($"{videoInfo.Title}");

            // Get highest bitrate audio-only or highest quality mixed stream
            var streamInfo = GetBestAudioStreamInfo(videoInfo);

            // Download to temp file
            Console.WriteLine("Downloading...");
            Directory.CreateDirectory(TempDirectoryPath);
            string streamFileExt = streamInfo.Container.GetFileExtension();
            string streamFilePath = Path.Combine(TempDirectoryPath, $"{Guid.NewGuid()}.{streamFileExt}");
            await YoutubeClient.DownloadMediaStreamAsync(streamInfo, streamFilePath);

            // Convert to mp3
            Console.WriteLine("Converting...");
            Directory.CreateDirectory(OutputDirectoryPath);
            string outFilePath = Path.Combine(OutputDirectoryPath, $"{cleanTitle}.mp3");
            await FfmpegCli.ExecuteAsync($"-i \"{streamFilePath}\" -q:a 0 -map a \"{outFilePath}\" -y");

            // Delete temp file
            Console.WriteLine("Deleting temp file...");
            File.Delete(streamFilePath);

            // Edit mp3 metadata
            Console.WriteLine("Writing metadata...");
            var idMatch = Regex.Match(videoInfo.Title, @"^(?<artist>.*?)-(?<title>.*?)$");
            string artist = idMatch.Groups["artist"].Value.Trim();
            string title = idMatch.Groups["title"].Value.Trim();
            using (var meta = TagLib.File.Create(outFilePath))
            {
                meta.Tag.Performers = new[] {artist};
                meta.Tag.Title = title;
                meta.Save();
            }

            Console.WriteLine($"Downloaded and converted video [{id}] to [{outFilePath}]");
        }

        private static async Task DownloadAndConvertPlaylistAsync(string id)
        {
            Console.WriteLine($"Working on playlist [{id}]...");

            // Get playlist info
            var playlistInfo = await YoutubeClient.GetPlaylistInfoAsync(id);
            Console.WriteLine($"{playlistInfo.Title} ({playlistInfo.Videos.Count} videos)");

            // Work on the videos
            Console.WriteLine();
            foreach (var video in playlistInfo.Videos)
            {
                await DownloadAndConvertVideoAsync(video.Id);
                Console.WriteLine();
            }
        }

        private static async Task MainAsync(string[] args)
        {
            foreach (string arg in args)
            {
                // Try to determine the type of the URL/ID that was given

                // Playlist ID
                if (YoutubeClient.ValidatePlaylistId(arg))
                {
                    await DownloadAndConvertPlaylistAsync(arg);
                }

                // Playlist URL
                else if (YoutubeClient.TryParsePlaylistId(arg, out string playlistId))
                {
                    await DownloadAndConvertPlaylistAsync(playlistId);
                }

                // Video ID
                else if (YoutubeClient.ValidateVideoId(arg))
                {
                    await DownloadAndConvertVideoAsync(arg);
                }

                // Video URL
                else if (YoutubeClient.TryParseVideoId(arg, out string videoId))
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