using Assigment_2.Models;

namespace Assigment_2.Data
{
    /// <summary>
    /// In-memory database. Registered as a singleton; data resets when the app restarts.
    /// </summary>
    public class SongStore
    {
        private readonly List<Song> _songs = new();
        private readonly object _lock = new();
        private int _nextId = 1;

        public SongStore()
        {
            Seed("Blinding Lights", "The Weeknd", "After Hours", 212, "Pop", true);
            Seed("Levitating", "Dua Lipa", "Future Nostalgia", 220, "Pop", false);
            Seed("Watermelon Sugar", "Harry Styles", "Fine Line", 214, "Pop", true);
            Seed("Save Your Tears", "The Weeknd", "After Hours", 246, "R&B", false);
            Seed("Peaches", "Justin Bieber", "Justice", 154, "Pop", true);
        }

        private void Seed(string title, string artist, string album, int seconds, string genre, bool fav)
        {
            _songs.Add(new Song
            {
                Id = _nextId++,
                Title = title,
                Artist = artist,
                Album = album,
                DurationSeconds = seconds,
                Genre = genre,
                IsFavorite = fav,
                Position = _songs.Count + 1
            });
        }

        public List<Song> GetAll()
        {
            lock (_lock) return _songs.OrderBy(s => s.Position).ToList();
        }

        public Song? Get(int id)
        {
            lock (_lock) return _songs.FirstOrDefault(s => s.Id == id);
        }

        public Song Add(Song song)
        {
            lock (_lock)
            {
                song.Id = _nextId++;
                song.Position = _songs.Count + 1;
                _songs.Add(song);
                return song;
            }
        }

        public bool Remove(int id)
        {
            lock (_lock)
            {
                var song = _songs.FirstOrDefault(s => s.Id == id);
                if (song == null) return false;
                _songs.Remove(song);
                Renumber(_songs.OrderBy(s => s.Position).ToList());
                return true;
            }
        }

        public Song? Move(int id, int newPosition)
        {
            lock (_lock)
            {
                var ordered = _songs.OrderBy(s => s.Position).ToList();
                var song = ordered.FirstOrDefault(s => s.Id == id);
                if (song == null) return null;

                ordered.Remove(song);
                var index = Math.Clamp(newPosition - 1, 0, ordered.Count);
                ordered.Insert(index, song);
                Renumber(ordered);
                return song;
            }
        }

        public void Update(Action action)
        {
            lock (_lock) action();
        }

        private static void Renumber(List<Song> ordered)
        {
            for (var i = 0; i < ordered.Count; i++) ordered[i].Position = i + 1;
        }
    }
}