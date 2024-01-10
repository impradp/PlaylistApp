using Playlist_Pro.Models;
using SongFinder.Models;

namespace Playlist_Pro.Services.Song
{
    public interface ISongService
    {
        Task<IEnumerable<SongModel>> GetMultipleAsync();
        Task<SongModel> GetAsync(string id);
        Task AddAsync(SongFinderResponse songFinderResponse, SongModel song);
        Task UpdateAsync(string id, SongModel song);
        Task DeleteAsync(string id);
        Task<IEnumerable<SongModel>> GetByNameAsync(string songName);
    }
}
