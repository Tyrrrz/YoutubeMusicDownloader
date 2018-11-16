# YoutubeMusicDownloader

Command line executable that can download and convert YouTube videos or entire playlists into mp3 files.

*This project is obsoleted by [YoutubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) - a GUI application to download YouTube videos.* This repository will be removed soon.

## Download

- [See releases](https://github.com/Tyrrrz/YoutubeMusicDownloader/releases)

## Usage examples

- `YoutubeMusicDownloader.exe "https://www.youtube.com/watch?v=I7RHr4o7t7E"` (video by URL)
- `YoutubeMusicDownloader.exe I7RHr4o7t7E` (video by ID)
- `YoutubeMusicDownloader.exe "https://www.youtube.com/watch?v=0KSOMA3QBU0&list=PLMC9KNkIncKtPzgY-5rmhvj7fax8fdxoj"` (playlist by URL)
- `YoutubeMusicDownloader.exe PLMC9KNkIncKtPzgY-5rmhvj7fax8fdxoj` (playlist by ID)

You can include multiple URLs or IDs simply by separating the arguments with space, like so:

- `YoutubeMusicDownloader.exe I7RHr4o7t7E 05nwADM_X-U ucDCFsiN5Gc` (multiple videos by ID)

Or you can also combine the URLs with IDs:

- `YoutubeMusicDownloader.exe "https://www.youtube.com/watch?v=I7RHr4o7t7E" 05nwADM_X-U ucDCFsiN5Gc` (multiple videos by ID)

Or even combine playlists with videos:

- `YoutubeMusicDownloader.exe I7RHr4o7t7E PLMC9KNkIncKtPzgY-5rmhvj7fax8fdxoj` (video by ID and playlist by ID)

## Libraries used

- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode)
- [YoutubeExplode.Converter](https://github.com/Tyrrrz/YoutubeExplode.Converter)
- [Taglib](https://github.com/mono/taglib-sharp)
- [Tyrrrz.Extensions](https://github.com/Tyrrrz/Extensions)

## Runtimes used

- [FFMPEG](https://ffmpeg.org)
