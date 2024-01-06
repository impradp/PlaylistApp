using SongFinder.Models;
using SongFinder.Handler;
using log4net;

namespace SongFinder.Services
{
    public class SongFinderService : ISongFinderService
    {
        private readonly SongFinderHandler _songFinderHandler;
        public SongFinderService(string youtubeAPIKey, ILog logger)
        {
            _songFinderHandler = new SongFinderHandler(youtubeAPIKey, logger);
        }

        /// <summary>
        /// Finds song based on provided query from different resources.
        /// </summary>
        /// <param name="query">The query to be searched in platform resource</param>
        /// <param name="resourceName">The name of the platform resource</param>
        /// <returns>The list of all response from platform resource</returns>
        public IEnumerable<SongFinderResponse> Find(string query, string resourceName)
        {

            var songFinderResponse = _songFinderHandler.FindSongs(query, resourceName);
            return songFinderResponse ?? new List<SongFinderResponse>();
        }

        /// <summary>
        /// Downloads the mp3 from the url provided based on the platform resource.
        /// </summary>
        /// <param name="query">The url to be used to download file from platform resource</param>
        /// <param name="resourceName">The name of the platform resource</param>
        /// <returns>The detail of song saved as mp3 in server storage.</returns>
        /// <exception cref="Exception">Throws error if the download and conversion of the provided url outputs empty response</exception>
        public async Task<SongFinderResponse> Download(string query, string resourceName)
        {
            var songFinderResponse = await _songFinderHandler.ExtractAndSave(query, resourceName);
            if (songFinderResponse != null)
            {
                return songFinderResponse;
            }
            else
            {
                throw new Exception("Error extracting and saving song.");
            }
        }
    }
}
