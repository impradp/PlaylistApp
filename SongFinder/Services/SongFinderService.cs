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

        public IEnumerable<SongFinderResponse> Find(string query, string resourceName)
        {

            var songFinderResponse = _songFinderHandler.FindSongs(query, resourceName);
            return songFinderResponse ?? new List<SongFinderResponse>();
        }

        public async Task<SongFinderResponse> Download(string query, string resourceName)
        {
            var songFinderResponse = await _songFinderHandler.extractAndSave(query, resourceName);
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
