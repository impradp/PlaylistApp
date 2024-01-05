using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Playlist_Pro;
using SongFinder.Models;
using YoutubeExplode;

namespace SongFinder.Platforms
{
    public class Youtube : IPlatform
    {
        private readonly string _apiKey;
        private readonly YoutubeClient _youtubeClient;

        public Youtube(string apiKey)
        {
            _apiKey = apiKey;
            _youtubeClient = new YoutubeClient();
        }

        /// <summary>
        /// Fetches the list of songs from youtube based on keyword passed 
        /// </summary>
        /// <param name="query">The keyword of song</param>
        /// <returns>The list of all available songs in youtube based on keyword.</returns>
        public IEnumerable<SongFinderResponse> Search(string query)
        {
            var logger = Log4NetConfig.GetLogger();
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = _apiKey,
                    ApplicationName = "PlaylistPro"
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = query;
                searchListRequest.MaxResults = 10;

                logger.Info(string.Format("Search list request initiated through Youtube for query:{0}", query));

                var searchListResponse = searchListRequest.Execute();

                logger.Info(string.Format("Search list request completed through Youtube for query:{0} with {1} results", query, searchListResponse.Items.Count));

                List<SongFinderResponse> response = new List<SongFinderResponse>();
                foreach (var searchResult in searchListResponse.Items)
                {
                    if (searchResult.Id.Kind == "youtube#video")
                    {
                        var videoRequest = youtubeService.Videos.List("snippet,contentDetails");
                        videoRequest.Id = searchResult.Id.VideoId;
                        var videoResponse = videoRequest.Execute();

                        var video = videoResponse.Items[0];
                        // Build the video URL based on the ID
                        string videoUrl = $"https://www.youtube.com/watch?v={video.Id}";

                        // Create a SongFinderResponse object with metadata and URL
                        response.Add(new SongFinderResponse(video.Snippet.Title, video.Snippet.Description, videoUrl, "", ""));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Exception occured with message:{0} while searching song list through Youtube for query:{1}", ex.Message, query));
                throw new Exception(ex.Message);
            }

        }

        /// <summary>
        /// Extracts and saves the mp3 from youtube server to local library
        /// </summary>
        /// <param name="youtubeUrl">The youtube url of the song to be downloaded and extracted.</param>
        /// <returns>The response containing the online metadata of the song</returns>
        public async Task<SongFinderResponse> ExtractAndSave(string youtubeUrl)
        {
            var logger = Log4NetConfig.GetLogger();
            try
            {
                logger.Info(string.Format("Extracting video id from youtube url:{0}", youtubeUrl));
                var videoId = ExtractVideoIdFromUrl(youtubeUrl);

                logger.Info(string.Format("Fetch video details from youtube API for videoId:{0}", videoId));
                var videoTask = _youtubeClient.Videos.GetAsync(videoId);
                var streamManifestTask = _youtubeClient.Videos.Streams.GetManifestAsync(videoId);

                var video = await videoTask.ConfigureAwait(false); // ConfigureAwait to avoid deadlocks
                var streamManifest = await streamManifestTask.ConfigureAwait(false);

                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (audioStreamInfo == null)
                {
                    logger.Error("Audio stream not found");
                    throw new Exception("Audio stream not found.");
                }

                var audioStream = await _youtubeClient.Videos.Streams.GetAsync(audioStreamInfo).ConfigureAwait(false);

                if (audioStream == null)
                {
                    logger.Error("Audio stream could not be retrieved.");
                    throw new Exception("Audio stream could not be retrieved.");
                }

                var validPattern = "[^a-zA-Z0-9_]+";
                var songTitle = Regex.Replace(video.Title, validPattern, "_");

                var outputPath = Path.Combine(@"D:\Files", $"{songTitle}.mp3");

                await _youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, outputPath).ConfigureAwait(false);
                logger.Info(string.Format("Audio successfully downloaded in output path:{0}", outputPath));

                var thumbnailUrl = video.Thumbnails.FirstOrDefault()?.Url;

                return new SongFinderResponse(video.Title, video.Description, video.Url, $"{songTitle}.mp3", thumbnailUrl ?? "");
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Exception occured with messsage:{0} while extracting and converting song from youtube.", ex.Message));
                throw;
            }
        }

        private string ExtractVideoIdFromUrl(string url)
        {
            string pattern = @"(?:\?|&)v=([a-zA-Z0-9_-]{11})";

            Regex regex = new Regex(pattern);

            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception("Video ID not found");
            }
        }
    }
}
