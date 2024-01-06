using log4net;
using SongFinder.Models;
using SongFinder.Platforms;

namespace SongFinder.Handler
{
    public class SongFinderHandler
    {
        private readonly ILog _logger;
        private readonly Dictionary<string, IPlatform> platforms = [];


        public SongFinderHandler(string youtubeApiKey, ILog logger)
        {
            _logger = logger;
            platforms.Add("youtube", new Youtube(youtubeApiKey, _logger));
        }

        /// <summary>
        /// Find song from online platforms
        /// </summary>
        /// <param name="query">The substring for the title of song</param>
        /// <param name="resourceName">The platform in which the song is to be searched.</param>
        /// <returns>The list of songs</returns>
        public IEnumerable<SongFinderResponse>? FindSongs(string query, string resourceName)
        {
            ValidatePlatformResource(resourceName);
            return platforms[resourceName.ToLower()].Search(query);
        }

        /// <summary>
        /// Extracts the mp3 and saves locally.
        /// </summary>
        /// <param name="query">The title of the song</param>
        /// <param name="resourceName">The platform in which the song is to be searched.</param>
        /// <returns>The response containing the metadata referencing the attributes of online response</returns>
        public async Task<SongFinderResponse?> ExtractAndSave(string query, string resourceName)
        {
            ValidatePlatformResource(resourceName);
            return await platforms[resourceName.ToLower()].ExtractAndSave(query);
        }

        /// <summary>
        /// Validates the resource registered in system for song finder.
        /// </summary>
        /// <param name="resourceName">The name of the resource platform used to find song.</param>
        /// <exception cref="Exception">Throws if the platform used are not registered for song finder.</exception>
        private void ValidatePlatformResource(string resourceName)
        {
            if (!platforms.ContainsKey(resourceName.ToLower()))
            {
                _logger.Error(string.Format("Platform : {0} not registered for song finder.", resourceName));
                throw new Exception(string.Format("Platform : {0} not registered for song finder.", resourceName));
            }
        }
    }
}
