using System.Text.RegularExpressions;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using log4net;
using PlaylistPro.Exceptions;
using SongFinder.Models;
using YoutubeExplode;

namespace SongFinder.Platforms
{
    public partial class Youtube : IPlatform
    {

        [GeneratedRegex("[^a-zA-Z0-9_]+")]
        private static partial Regex RegexPatternExcludingSpecialCharacters();

        private readonly ILog _logger;
        private readonly string _apiKey;
        private readonly YoutubeClient _youtubeClient;
        private readonly YouTubeService _youTubeService;

        public Youtube(string apiKey, ILog logger)
        {
            _apiKey = apiKey;
            _youtubeClient = new YoutubeClient();
            _logger = logger;
            _youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "PlaylistPro"
            });
        }

        /// <summary>
        /// Fetches the list of songs from youtube based on keyword passed 
        /// </summary>
        /// <param name="query">The keyword of song</param>
        /// <returns>The list of all available songs in youtube based on keyword.</returns>
        public IEnumerable<SongFinderResponse> Search(string query)
        {
            try
            {
                _logger.Info(string.Format("Search list request initiated through Youtube for query:{0}", query));

                var searchListRequest = _youTubeService.Search.List("snippet");
                searchListRequest.Q = query;
                searchListRequest.MaxResults = 10;

                var searchListResponse = searchListRequest.Execute();

                _logger.Info(string.Format("Search list request completed through Youtube for query:{0} with {1} results", query, searchListResponse.Items.Count));

                var response = new List<SongFinderResponse>();

                foreach (var searchResult in searchListResponse.Items.Where(item => item.Id.Kind == "youtube#video"))
                {
                    var video = GetVideoDetails(searchResult.Id.VideoId);

                    if (video != null)
                    {
                        string videoUrl = $"https://www.youtube.com/watch?v={video.Id}";
                        response.Add(new SongFinderResponse(video.Snippet.Title, video.Snippet.Description, videoUrl, "", "", ""));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                LogAndThrowException("search", ex, query);
                throw;
            }
        }

        /// <summary>
        /// Generic method to log and throw exception for catch block.
        /// </summary>
        /// <param name="operation">The operation used when the exception encountered.</param>
        /// <param name="ex">The exception message through stack trace.</param>
        /// <param name="query">The query used when the exception encountered.</param>
        /// <exception cref="Exception">Throws exception based on diffrent operations.</exception>
        private void LogAndThrowException(string operation, Exception ex, string? query)
        {
            _logger.Error($"Exception occurred during {operation} operation: {ex.Message} for query: {query ?? "N/A"}");
            throw new CustomException($"Exception occurred during {operation} operation: {ex.Message} for query: {query ?? "N/A"}");
        }

        /// <summary>
        /// The video details from youtube API.
        /// </summary>
        /// <param name="videoId">The video id of youtube.</param>
        /// <returns>The video response from youtube api.</returns>
        private Video? GetVideoDetails(string videoId)
        {
            var videoRequest = _youTubeService.Videos.List("snippet,contentDetails");
            videoRequest.Id = videoId;
            var videoResponse = videoRequest.Execute();

            return videoResponse.Items.FirstOrDefault();
        }

        /// <summary>
        /// Extracts and saves the mp3 from youtube server to local library
        /// </summary>
        /// <param name="youtubeUrl">The youtube url of the song to be downloaded and extracted.</param>
        /// <returns>The response containing the online metadata of the song</returns>
        public async Task<SongFinderResponse> ExtractAndSave(string url)
        {
            try
            {
                #region Validating and Fetching Audio Stream from YouTube
                _logger.Info(string.Format("Extracting video id from youtube url:{0}", url));
                var videoId = ExtractVideoIdFromUrl(url);

                _logger.Info(string.Format("Fetch video details from youtube API for videoId:{0}", videoId));
                var videoTask = _youtubeClient.Videos.GetAsync(videoId);
                var streamManifestTask = _youtubeClient.Videos.Streams.GetManifestAsync(videoId);

                var video = await videoTask.ConfigureAwait(false); // ConfigureAwait to avoid deadlocks
                var streamManifest = await streamManifestTask.ConfigureAwait(false);

                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (audioStreamInfo == null)
                {
                    _logger.Error("Audio stream not found");
                    throw new CustomException("Audio stream not found.");
                }
                #endregion

                #region Converting audio stream and downloading into system storage
                var audioStream = await _youtubeClient.Videos.Streams.GetAsync(audioStreamInfo).ConfigureAwait(false);

                if (audioStream == null)
                {
                    _logger.Error("Audio stream could not be retrieved.");
                    throw new CustomException("Audio stream could not be retrieved.");
                }

                var songTitle = GetValidSongTitle(video.Title);

                //TODO: Use bucket to store these extracted mp3.
                var outputPath = Path.Combine(@"D:\Private\audio-player-tutorial\src\assets", $"{songTitle}.mp3");

                await _youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, outputPath).ConfigureAwait(false);
                _logger.Info(string.Format("Audio successfully downloaded in output path:{0}", outputPath));
                #endregion

                var thumbnailUrl = video.Thumbnails?[0].Url;
                var author = video.Author?.ChannelTitle;

                return new SongFinderResponse(video.Title, video.Description, video.Url, $"{songTitle}.mp3", thumbnailUrl, author);
            }
            catch (Exception ex)
            {
                LogAndThrowException("extract and save", ex, url);
                throw;
            }
        }

        /// <summary>
        /// Generates valid song title for the mp3 track created.
        /// </summary>
        /// <param name="title">The title from video selected from youtube.</param>
        /// <returns>The valid title for mp3 track generated.</returns>
        private string GetValidSongTitle(string title)
        {
            return RegexPatternExcludingSpecialCharacters().Replace(title, "_");
        }

        /// <summary>
        /// Extract youtube video id using regex
        /// </summary>
        /// <param name="url">The youtube url</param>
        /// <returns>The extracted video ID if found.</returns>
        /// <exception cref="Exception">Throws video id not found exception</exception>
        private string ExtractVideoIdFromUrl(string url)
        {
            string pattern = @"(?:\?|&)v=([a-zA-Z0-9_-]{11})";

            Regex regex = new(pattern);

            Match match = regex.Match(url);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                _logger.Error("Video ID not found in the provided YouTube URL: " + url);
                throw new ArgumentException("Invalid or unrecognized YouTube URL", nameof(url));
            }
        }

    }
}
