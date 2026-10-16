using System.ComponentModel.DataAnnotations;

namespace Assigment_2.Models
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;

        /// <summary>Duration in seconds (used for sorting and total duration).</summary>
        public int DurationSeconds { get; set; }

        /// <summary>Display format, e.g. "3:32".</summary>
        public string Duration => $"{DurationSeconds / 60}:{DurationSeconds % 60:D2}";

        public string Genre { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public bool IsFavorite { get; set; }

        /// <summary>Position in the playlist (for drag and drop ordering).</summary>
        public int Position { get; set; }
    }

    /// <summary>Request body for creating / updating a song.</summary>
    public class SongRequest
    {
        [Required, StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(200, MinimumLength = 1)]
        public string Artist { get; set; } = string.Empty;

        [StringLength(200)]
        public string Album { get; set; } = string.Empty;

        /// <summary>Duration in "m:ss" format, e.g. "3:32".</summary>
        [Required, RegularExpression(@"^\d{1,3}:[0-5]\d$", ErrorMessage = "Duration must be in m:ss format, e.g. 3:32")]
        public string Duration { get; set; } = "0:00";

        [Required, StringLength(50)]
        public string Genre { get; set; } = string.Empty;

        public string? CoverUrl { get; set; }
        public bool IsFavorite { get; set; }
    }

    /// <summary>Request body for moving a song to a new position.</summary>
    public class MoveRequest
    {
        /// <summary>New position (1-based).</summary>
        [Range(1, int.MaxValue)]
        public int NewPosition { get; set; }
    }

    public class PlaylistSummary
    {
        public int SongCount { get; set; }
        public int TotalSeconds { get; set; }
        public string TotalDuration { get; set; } = string.Empty;
        public int FavoriteCount { get; set; }
        public Dictionary<string, int> Genres { get; set; } = new();
    }
}