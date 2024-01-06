using SongFinder.Services;
using Microsoft.Azure.Cosmos;
using Playlist_Pro.Services.Song;
using log4net;

namespace Playlist_Pro.DBContext
{
    public class DatabaseClient
    {
        /// <summary>
        /// Initializes the song container used for song service.
        /// </summary>
        /// <param name="configurationSection">The configuration interface used to configure keys.</param>
        /// <returns>The song service initialized through the container initialization.</returns>
        public static async Task<SongService> InitializeSongContainer(IConfigurationSection configurationSection)
        {
            var databaseName = configurationSection["DatabaseName"];
            var containerName = "Song";
            var account = configurationSection["Account"];
            var key = configurationSection["Key"];

            var client = new CosmosClient(account, key);
            var container = client.GetContainer(databaseName, containerName);
            var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

            var songService = new SongService(container);
            return songService;
        }

        /// <summary>
        /// Initializes keys for the SongFinder platforms used.
        /// </summary>
        /// <param name="configurationSection">The configuration interface used to configure keys.</param>
        /// <returns>The songfinder service for service configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API keys are missing.</exception>
        public static SongFinderService InitializeKeys(IConfigurationSection configurationSection)
        {

            var youTubeApiKey = configurationSection["YoutubeAPIKey"];
            var logger = LogManager.GetLogger(typeof(DatabaseClient));
            if (string.IsNullOrEmpty(youTubeApiKey))
            {
                var errorMessage = "Youtube API Keys are missing.";
                logger.Error(errorMessage);
                throw new ArgumentNullException(nameof(configurationSection), errorMessage);
            }
            var songFinderService = new SongFinderService(youTubeApiKey, logger);

            //Use this section to initlize the container service for other online platforms like soundcloud.
            return songFinderService;
        }
    }

}
