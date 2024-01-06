using log4net;
using SongFinder.Models;
using SongFinder.Platforms;

namespace SongFinder.Handler
{
    public class SongFinderHandler
    {
        private readonly Dictionary<string, IPlatform> platforms = new();


        public SongFinderHandler(string youtubeApiKey, ILog logger)
        {
            platforms.Add("youtube", new Youtube(youtubeApiKey, logger));
        }

        /// <summary>
        /// Find song from online platforms
        /// </summary>
        /// <param name="query">The substring for the title of song</param>
        /// <param name="resourceName">The platform in which the song is to be searched.</param>
        /// <returns>The list of songs</returns>
        public IEnumerable<SongFinderResponse>? FindSongs(string query, string resourceName)
        {
            if (platforms.ContainsKey(resourceName.ToLower()))
            {
                return platforms[resourceName.ToLower()].Search(query);
            }

            return null;
        }

        /// <summary>
        /// Extracts the mp3 and saves locally.
        /// </summary>
        /// <param name="query">The title of the song</param>
        /// <param name="resourceName">The platform in which the song is to be searched.</param>
        /// <returns>The response containing the metadata referencing the attributes of online response</returns>
        public async Task<SongFinderResponse?> extractAndSave(string query, string resourceName)
        {
            if (platforms.ContainsKey(resourceName.ToLower()))
            {
                return await platforms[resourceName.ToLower()].ExtractAndSave(query);
            }

            return null;
        }
    }
}
