using SongFinder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SongFinder.Services
{
    public interface ISongFinderService
    {
        IEnumerable<SongFinderResponse> find(string query, string resourceName);

        Task<SongFinderResponse> download(string query, string resourceName);
    }
}
