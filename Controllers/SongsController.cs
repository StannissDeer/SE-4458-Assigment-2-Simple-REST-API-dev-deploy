using Assigment_2.Data;
using Assigment_2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Assigment_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SongsController : ControllerBase
    {
        private readonly SongStore _store;

        public SongsController(SongStore store) => _store = store;

        /// <summary>Lists songs with search, genre filter, favorites filter and sorting.</summary>
        /// <param name="search">Searches in title, artist or album.</param>
        /// <param name="genre">Genre filter (e.g. Pop, R&amp;B). Leave empty or use "All" for all genres.</param>
        /// <param name="favoritesOnly">If true, returns only favorite songs.</param>
        /// <param name="sortBy">position (default), title, artist, duration, genre</param>
        /// <param name="order">asc (default) or desc</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Song>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Song>> GetSongs(
            [FromQuery] string? search,
            [FromQuery] string? genre,
            [FromQuery] bool? favoritesOnly,
            [FromQuery] string sortBy = "position",
            [FromQuery] string order = "asc")
        {
            IEnumerable<Song> query = _store.GetAll();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s =>
                    s.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Artist.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Album.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(genre) && !genre.Equals("All", StringComparison.OrdinalIgnoreCase))
                query = query.Where(s => s.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));

            if (favoritesOnly == true)
                query = query.Where(s => s.IsFavorite);

            var desc = order.Equals("desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy.ToLowerInvariant() switch
            {
                "title" => desc ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
                "artist" => desc ? query.OrderByDescending(s => s.Artist) : query.OrderBy(s => s.Artist),
                "duration" => desc ? query.OrderByDescending(s => s.DurationSeconds) : query.OrderBy(s => s.DurationSeconds),
                "genre" => desc ? query.OrderByDescending(s => s.Genre) : query.OrderBy(s => s.Genre),
                _ => desc ? query.OrderByDescending(s => s.Position) : query.OrderBy(s => s.Position)
            };

            return Ok(query.ToList());
        }

        /// <summary>Gets a single song by id.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Song), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Song> GetSongById(int id)
        {
            var song = _store.Get(id);
            return song == null ? NotFound(new { message = $"Song {id} not found" }) : Ok(song);
        }

        /// <summary>Playlist summary: song count and total duration (e.g. "5 songs • 17 min 26 sec").</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(PlaylistSummary), StatusCodes.Status200OK)]
        public ActionResult<PlaylistSummary> GetSummary()
        {
            var songs = _store.GetAll();
            var total = songs.Sum(s => s.DurationSeconds);
            return Ok(new PlaylistSummary
            {
                SongCount = songs.Count,
                TotalSeconds = total,
                TotalDuration = $"{total / 60} min {total % 60} sec",
                FavoriteCount = songs.Count(s => s.IsFavorite),
                Genres = songs.GroupBy(s => s.Genre).ToDictionary(g => g.Key, g => g.Count())
            });
        }

        /// <summary>Distinct genres in the playlist (for the "All Genres" dropdown).</summary>
        [HttpGet("genres")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetGenres() =>
            Ok(_store.GetAll().Select(s => s.Genre).Distinct().OrderBy(g => g).ToList());

        /// <summary>Adds a new song to the end of the playlist.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(Song), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Song> CreateSong([FromBody] SongRequest request)
        {
            var song = _store.Add(new Song
            {
                Title = request.Title.Trim(),
                Artist = request.Artist.Trim(),
                Album = request.Album.Trim(),
                DurationSeconds = ParseDuration(request.Duration),
                Genre = request.Genre.Trim(),
                CoverUrl = request.CoverUrl,
                IsFavorite = request.IsFavorite
            });
            return CreatedAtAction(nameof(GetSongById), new { id = song.Id }, song);
        }

        /// <summary>Updates (edits) a song.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Song), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Song> UpdateSong(int id, [FromBody] SongRequest request)
        {
            var song = _store.Get(id);
            if (song == null) return NotFound(new { message = $"Song {id} not found" });

            _store.Update(() =>
            {
                song.Title = request.Title.Trim();
                song.Artist = request.Artist.Trim();
                song.Album = request.Album.Trim();
                song.DurationSeconds = ParseDuration(request.Duration);
                song.Genre = request.Genre.Trim();
                song.CoverUrl = request.CoverUrl;
                song.IsFavorite = request.IsFavorite;
            });
            return Ok(song);
        }

        /// <summary>Removes a song from the playlist.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteSong(int id) =>
            _store.Remove(id) ? NoContent() : NotFound(new { message = $"Song {id} not found" });

        /// <summary>Toggles the favorite (star) status of a song.</summary>
        [HttpPatch("{id:int}/favorite")]
        [ProducesResponseType(typeof(Song), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Song> ToggleFavorite(int id)
        {
            var song = _store.Get(id);
            if (song == null) return NotFound(new { message = $"Song {id} not found" });

            _store.Update(() => song.IsFavorite = !song.IsFavorite);
            return Ok(song);
        }

        /// <summary>Moves a song to a new position in the playlist (drag and drop).</summary>
        [HttpPatch("{id:int}/move")]
        [ProducesResponseType(typeof(IEnumerable<Song>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Song>> MoveSong(int id, [FromBody] MoveRequest request)
        {
            var song = _store.Move(id, request.NewPosition);
            return song == null ? NotFound(new { message = $"Song {id} not found" }) : Ok(_store.GetAll());
        }

        private static int ParseDuration(string duration)
        {
            var parts = duration.Split(':');
            return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
        }
    }
}