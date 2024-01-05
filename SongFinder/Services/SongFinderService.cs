using SongFinder.Models;
using SongFinder.Handler;

namespace SongFinder.Services
{
    public class SongFinderService : ISongFinderService
    {
        private SongFinderHandler _songFinderHandler;
        public SongFinderService(string youtubeAPIKey)
        {
            _songFinderHandler = new SongFinderHandler(youtubeAPIKey);
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
