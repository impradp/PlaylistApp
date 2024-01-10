using log4net;
using Microsoft.Azure.Cosmos;
using Playlist_Pro.Models;
using PlaylistPro.Exceptions;
using SongFinder.Models;

namespace Playlist_Pro.Services.Song
{
    public class SongService : ISongService
    {
        private readonly Container _container;
        private readonly ILog _logger;

        public SongService(
            Container container)
        {
            _container = container;
            _logger = LogManager.GetLogger(typeof(SongService));
        }

        /// <summary>
        /// Adds song into the library
        /// </summary>
        /// <param name="song">The song to be saved.</param>
        /// <returns>The song saved in local library.</returns>
        public async Task AddAsync(SongFinderResponse songFinderResponse, SongModel song)
        {
            try
            {
                var mappedResponse = PrepareWithOnlineResponse(songFinderResponse, song);

                song.Id = Guid.NewGuid().ToString();
                await _container.CreateItemAsync(mappedResponse, new PartitionKey(mappedResponse.Id));
                _logger.Info("Song saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception occured while saving song with message:{0}", ex.Message));
                throw new CustomException(string.Format("Exception occured while saving song with message:{0}", ex.Message));
            }

        }

        /// <summary>
        /// Deletes the definite song from the library.
        /// </summary>
        /// <param name="id">The unique identifier of the song to be deleted</param>
        /// <returns></returns>
        public async Task DeleteAsync(string id)
        {
            try
            {
                await _container.DeleteItemAsync<SongModel>(id, new PartitionKey(id));
                _logger.Info("Song deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception occured while deleting song with message:{0}", ex.Message));
                throw new CustomException(string.Format("Exception occured while deleting song with message:{0}", ex.Message));
            }

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
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception occured while fetching song with message:{0}", ex.Message));
                throw new CustomException(string.Format("Exception occured while fetching song with message:{0}", ex.Message));
            }
        }

        /// <summary>
        /// Fetches the list of all songs from local library
        /// </summary>
        /// <returns>The list of all songs.</returns>
        public async Task<IEnumerable<SongModel>> GetMultipleAsync()
        {
            try
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
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception occured while fetching all songs with message:{0}", ex.Message));
                throw new CustomException(string.Format("Exception occured while fetching all songs with message:{0}", ex.Message));
            }
        }

        /// <summary>
        /// Updates the definite song.
        /// </summary>
        /// <param name="id">The id of the song to be updated.</param>
        /// <param name="song">The attribute of the song to be updated.</param>
        /// <returns>The updated object of the definite song</returns>
        public async Task UpdateAsync(string id, SongModel song)
        {
            try
            {
                await _container.UpsertItemAsync(song, new PartitionKey(id));
            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception occured while updating song with message:{0}", ex.Message));
                throw new CustomException(string.Format("Exception occured while updating song with message:{0}", ex.Message));
            }

        }

        /// <summary>
        /// Fetches the song by its keyword
        /// </summary>
        /// <param name="songName">The name or keyword of song.</param>
        /// <returns>The list of all available song based on keyword both from local library and online platforms.</returns>
        public async Task<IEnumerable<SongModel>> GetByNameAsync(string songName)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Info(string.Format("Exception occured while fetching song from title with message:{0}", ex.Message));
                throw new CustomException(string.Format("Exception occured while fetching song from title with message:{0}", ex.Message));
            }

        }

        /// <summary>
        /// Map Online Response with SongModel entity
        /// </summary>
        /// <param name="songFinderResponse">The online response for selected platform url</param>
        /// <param name="song">The song model containing initially submitted values.</param>
        /// <returns>The song model with online values</returns>
        private SongModel PrepareWithOnlineResponse(SongFinderResponse songFinderResponse, SongModel song)
        {
            song.Path = songFinderResponse.Path;
            song.Name = string.IsNullOrEmpty(song.Name) ? songFinderResponse.Title : song.Name;
            song.Description = songFinderResponse.Description;
            song.Thumbnail = songFinderResponse.ThumbnailUrl;
            song.Author = songFinderResponse.Author;
            return song;
        }
    }
}
