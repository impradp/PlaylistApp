using SongFinder.Services;
using Microsoft.Azure.Cosmos;
using Playlist_Pro.Services.Song;

namespace Playlist_Pro.DBContext
{
    public class DatabaseClient
    {
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

            var cosmosDbService = new SongService(container);
            return cosmosDbService;
        }

        public static SongFinderService InitilizeKeys(IConfigurationSection configurationSection)
        {
            var apiKey = configurationSection["YoutubeAPIKey"];
            if (apiKey == null)
            {
                throw new ArgumentException("Platform API Keys are missing.");
            }
            var songFinderService = new SongFinderService(apiKey);
            return songFinderService;
        }
    }
}
