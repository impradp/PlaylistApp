using System;
using System.Collections.Generic;

namespace SongFinder.Models
{
    public class SongFinderResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string Path { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Author { get; set; }

        public SongFinderResponse(string title, string description, string videoUrl, string path, string? thumbnailUrl, string? author)
        {
            Title = title;
            Description = description;
            VideoUrl = videoUrl;
            Path = path;
            ThumbnailUrl = thumbnailUrl;
            Author = author;
        }

    }
}
