using Microsoft.Azure.Cosmos;
using Playlist_Pro.Models;

namespace Playlist_Pro.Services.Song
{
    public class SongService : ISongService
    {
        private Container _container;

        public SongService(
            Container container)
        {
            _container = container;
        }

        /// <summary>
        /// Adds song into the library
        /// </summary>
        /// <param name="song">The song to be saved.</param>
        /// <returns>The song saved in local library.</returns>
        public async Task AddAsync(SongModel song)
        {
            await _container.CreateItemAsync(song, new PartitionKey(song.Id));
        }

        /// <summary>
        /// Deletes the definite song from the library.
        /// </summary>
        /// <param name="id">The unique identifier of the song to be deleted</param>
        /// <returns></returns>
        public async Task DeleteAsync(string id)
        {
            await _container.DeleteItemAsync<SongModel>(id, new PartitionKey(id));
        }

        /// <summary>
        /// Fetches the song details based on its unique id.
        /// </summary>
        /// <param name="id">The unique identifier of song.</param>
        /// <returns>The song based on the id.</returns>
        public async Task<SongModel> GetAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<SongModel>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch //For handling item not found and other exceptions
            {
                throw;
            }
        }

        /// <summary>
        /// Fetches the list of all songs from local library
        /// </summary>
        /// <returns>The list of all songs.</returns>
        public async Task<IEnumerable<SongModel>> GetMultipleAsync()
        {
            var query = _container.GetItemQueryIterator<SongModel>(new QueryDefinition("SELECT * FROM c"));

            var results = new List<SongModel>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        /// <summary>
        /// Updates the definite song.
        /// </summary>
        /// <param name="id">The id of the song to be updated.</param>
        /// <param name="song">The attribute of the song to be updated.</param>
        /// <returns>The updated object of the definite song</returns>
        public async Task UpdateAsync(string id, SongModel song)
        {
            await _container.UpsertItemAsync(song, new PartitionKey(id));
        }

        /// <summary>
        /// Fetches the song by its keyword
        /// </summary>
        /// <param name="songName">The name or keyword of song.</param>
        /// <returns>The list of all available song based on keyword both from local library and online platforms.</returns>
        public async Task<IEnumerable<SongModel>> GetByNameAsync(string songName)
        {

            var query = new QueryDefinition("SELECT * FROM c WHERE CONTAINS(LOWER(c.name), @name)")
            .WithParameter("@name", songName.ToLower());

            var queryResponse = _container.GetItemQueryIterator<SongModel>(query);

            var results = new List<SongModel>();
            while (queryResponse.HasMoreResults)
            {
                var response = await queryResponse.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }
    }
}
