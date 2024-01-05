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
            var logger = Log4NetConfig.GetLogger();
            try
            {
                // Find Song from Platforms: Youtube,Soundcloud
                #region Extract And Save locally
                logger.Info(string.Format("Fetching songs from youtube"));

                var onlineResponse = await _songFinderService.download(song.PlatformUrl, song.Platform);

                logger.Info(string.Format("Saved youtube song locally with title:{0}", onlineResponse.Title));
                #endregion

                song.Id = Guid.NewGuid().ToString();
                song.Path = onlineResponse.Path;
                song.Name = onlineResponse.Title;
                song.Description = onlineResponse.Description;
                song.Thumbnail = onlineResponse.ThumbnailUrl;
                await _songService.AddAsync(song);

                return CreatedAtAction(nameof(Get), new { id = song.Id }, song);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Exception occured with error message :{0}", ex.Message));
                return BadRequest("Error extracting and saving song.");

            }


        }

        /// <summary>
        /// Updates the details of song.
        /// </summary>
        /// <param name="song">The updated values for a particular song</param>
        /// <returns></returns>
        [HttpPut("{id}")]
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
            var logger = Log4NetConfig.GetLogger();
            try
            {
                logger.Info(string.Format("Fetching songs from youtube"));

                #region Fetch from online platform
                var onlineResponse = _songFinderService.find(name, "Youtube");
                #endregion

                logger.Info(string.Format("Youtube API Fetch Completed with {0} results", onlineResponse.Count()));

                return Ok(new { local = await _songService.GetByNameAsync(name), platforms = onlineResponse });
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Exception occured with error message :{0}", ex.Message));
                return BadRequest("Error searching songs.");
            }
        }
    }
}
