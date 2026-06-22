using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace plexrandom;

public class PlexConfig
{
    public string Url { get; set; } = string.Empty;
    public string Port { get; set; } = "32400";
    public string Token { get; set; } = string.Empty;
    public int MinEntries { get; set; } = 10;
    public int DefaultMovieDuration { get; set; } = 90;
    public string Language { get; set; } = "de";
    public List<PlexLibrary> Libraries { get; set; } = new();
    public List<RecentPlaylist> RecentPlaylists { get; set; } = new();
}

public class RecentPlaylist
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PlexLibrary
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    public override string ToString() => Title;
}

public class PlexGenre
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    public string DisplayName => !string.IsNullOrEmpty(Tag) ? Tag : Title;
}

public class PlexMovie
{
    [JsonPropertyName("ratingKey")]
    public string RatingKey { get; set; } = string.Empty;
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("year")]
    public int? Year { get; set; }
    
    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 0; // in milliseconds
    
    [JsonPropertyName("Genre")]
    public List<PlexGenre>? GenreList { get; set; }
    
    [JsonPropertyName("viewCount")]
    public int ViewCount { get; set; } = 0;

    public List<string> Genres => GenreList?.Select(g => g.DisplayName).ToList() ?? new List<string>();
    
    public string GenresDisplay => string.Join(", ", Genres.Take(2));
    
    public string DurationDisplay 
    {
        get
        {
            if (Duration <= 0) return "-";
            var t = TimeSpan.FromMilliseconds(Duration);
            int h = (int)t.TotalHours;
            int m = t.Minutes;
            if (h > 0) return $"{h}h {m}min";
            return $"{m}min";
        }
    }

    // For Playlist Preview
    public int PlaylistPosition { get; set; }
}

public class PlexClient
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("machineIdentifier")]
    public string MachineIdentifier { get; set; } = string.Empty;
}

public class PlexMediaContainer<T>
{
    [JsonPropertyName("MediaContainer")]
    public MediaContainerData<T> MediaContainer { get; set; } = new();
}

public class MediaContainerData<T>
{
    [JsonPropertyName("Directory")]
    public List<T>? Directory { get; set; }
    
    [JsonPropertyName("Metadata")]
    public List<T>? Metadata { get; set; }
}
