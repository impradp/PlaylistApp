using Microsoft.AspNetCore.Mvc;
using Playlist_Pro.Models;
using Playlist_Pro.Services.Song;
using SongFinder.Services;

namespace Playlist_Pro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;
        private readonly ISongFinderService _songFinderService;

        public SongsController(ISongService songService, ISongFinderService songFinderService)
        {
            _songService = songService;
            _songFinderService = songFinderService;
        }

        /// <summary>
        /// Fetches all the available songs.
        /// </summary>
        /// <returns>The list of all songs</returns>
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return Ok(await _songService.GetMultipleAsync());
        }

        /// <summary>
        /// Fetches the song by id
        /// </summary>
        /// <param name="id">The unique identifier of song.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return Ok(await _songService.GetAsync(id));
        }

        /// <summary>
        /// Generates the mp3 file locally for any platform available songs.
        /// </summary>
        /// <param name="song">The details of the song to be converted from platform specific</param>
        /// <returns>The details of newly saved song</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SongModel song)
        {
            // Find Song from Platforms: Youtube,Soundcloud
            var onlineResponse = await _songFinderService.Download(song.PlatformUrl, song.Platform);

            song.Id = Guid.NewGuid().ToString();
            song.Path = string.IsNullOrEmpty(song.Path) ? onlineResponse.Path : song.Path;
            song.Name = string.IsNullOrEmpty(song.Name) ? onlineResponse.Title : song.Name;
            song.Description = string.IsNullOrEmpty(song.Description) ? onlineResponse.Description : song.Description;
            song.Thumbnail = string.IsNullOrEmpty(song.Thumbnail) ? onlineResponse.ThumbnailUrl : song.Thumbnail;
            song.Author = string.IsNullOrEmpty(song.Author) ? onlineResponse.Author : song.Author;
            await _songService.AddAsync(song);

            return CreatedAtAction(nameof(Get), new { id = song.Id }, song);
        }

        /// <summary>
        /// Updates the details of song.
        /// </summary>
        /// <param name="song">The updated values for a particular song</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] SongModel song)
        {
            await _songService.UpdateAsync(song.Id, song);
            return NoContent();

        }

        /// <summary>
        /// Deletes the song by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of song</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _songService.DeleteAsync(id);
            return NoContent();
        }

        ///<summary>
        /// Searches the songs locally and from platforms like youtube and soundcloud
        /// </summary>
        /// <param name="name">The initials of song</param>
        /// <returns>The list of all songs available in local device and from online platforms</returns>
        [HttpGet("searchByName/{name}")]
        public async Task<IActionResult> SearchSongsByName(string name)
        {
            var onlineResponse = _songFinderService.Find(name, "Youtube");
            return Ok(new { local = await _songService.GetByNameAsync(name), platforms = onlineResponse });
        }
    }
}
