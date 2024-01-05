using SongFinder.Models;

namespace SongFinder.Services
{
    public interface ISongFinderService
    {
        IEnumerable<SongFinderResponse> Find(string query, string resourceName);

        Task<SongFinderResponse> Download(string query, string resourceName);
    }
}
