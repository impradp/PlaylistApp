using Playlist_Pro.Models;

namespace Playlist_Pro.Services.Song
{
    public interface ISongService
    {
        Task<IEnumerable<SongModel>> GetMultipleAsync();
        Task<SongModel> GetAsync(string id);
        Task AddAsync(SongModel item);
        Task UpdateAsync(string id, SongModel item);
        Task DeleteAsync(string id);
        Task<IEnumerable<SongModel>> GetByNameAsync(string songName);
    }
}
