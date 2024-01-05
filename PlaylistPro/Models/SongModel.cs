using Newtonsoft.Json;

namespace Playlist_Pro.Models
{
    public class SongModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "isFavourite")]
        public bool IsFavourite { get; set; } = false;

        [JsonProperty(PropertyName = "platform")]
        public required string Platform { get; set; }

        [JsonProperty(PropertyName = "platformUrl")]
        public required string PlatformUrl { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "thumbnailUrl")]
        public string Thumbnail { get; set; }

    }
}
