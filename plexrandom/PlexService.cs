using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace plexrandom;

public class PlexService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl = string.Empty;
    private string _token = string.Empty;

    public PlexService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public void UpdateConfig(string url, string port, string token)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _baseUrl = string.Empty;
        }
        else
        {
            _baseUrl = url.TrimEnd('/');
            // Add protocol if missing
            if (!_baseUrl.StartsWith("http://") && !_baseUrl.StartsWith("https://"))
            {
                _baseUrl = "http://" + _baseUrl;
            }
            
            // Add port if not already present in the URL
            if (!string.IsNullOrWhiteSpace(port) && !_baseUrl.Split('/').Last().Contains(":"))
            {
                _baseUrl = $"{_baseUrl}:{port}";
            }
        }

        _token = token;
        _httpClient.DefaultRequestHeaders.Remove("X-Plex-Token");
        _httpClient.DefaultRequestHeaders.Add("X-Plex-Token", _token);
    }

    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/library/sections");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<PlexLibrary>> GetLibrariesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PlexMediaContainer<PlexLibrary>>($"{_baseUrl}/library/sections");
            return response?.MediaContainer?.Directory ?? new List<PlexLibrary>();
        }
        catch (Exception)
        {
            return new List<PlexLibrary>();
        }
    }

    public async Task<List<string>> GetGenresAsync(string libraryKey)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PlexMediaContainer<PlexGenre>>($"{_baseUrl}/library/sections/{libraryKey}/genre");
            return response?.MediaContainer?.Directory?.Select(g => g.DisplayName).ToList() ?? new List<string>();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    public async Task<List<PlexMovie>> GetAllMoviesAsync(string libraryKey)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PlexMediaContainer<PlexMovie>>($"{_baseUrl}/library/sections/{libraryKey}/all");
            return response?.MediaContainer?.Metadata ?? new List<PlexMovie>();
        }
        catch (Exception)
        {
            return new List<PlexMovie>();
        }
    }

    public async Task<string> GetMachineIdentifierAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PlexMediaContainer<object>>($"{_baseUrl}/");
            // The machine identifier is at the root MediaContainer
            // We need a custom way to get it if it's not in a list
            var json = await _httpClient.GetStringAsync($"{_baseUrl}/");
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("MediaContainer").GetProperty("machineIdentifier").GetString() ?? "";
        }
        catch (Exception)
        {
            return "";
        }
    }

    public async Task<string?> CreatePlaylistAsync(string title, List<PlexMovie> movies)
    {
        try
        {
            var machineId = await GetMachineIdentifierAsync();
            if (string.IsNullOrEmpty(machineId)) return null;

            var ratingKeys = string.Join(",", movies.Select(m => m.RatingKey));
            var playlistUri = $"server://{machineId}/com.plexapp.plugins.library/library/metadata/{ratingKeys}";

            var url = $"{_baseUrl}/playlists?type=video&title={Uri.EscapeDataString(title)}&smart=0&uri={Uri.EscapeDataString(playlistUri)}";
            var response = await _httpClient.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                // Plex returns the created playlist in a MediaContainer/Metadata list
                var metadata = doc.RootElement.GetProperty("MediaContainer").GetProperty("Metadata");
                if (metadata.GetArrayLength() > 0)
                {
                    return metadata[0].GetProperty("ratingKey").GetString();
                }
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> DeletePlaylistAsync(string playlistId)
    {
        try
        {
            var url = $"{_baseUrl}/playlists/{playlistId}";
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> PlaylistExistsAsync(string playlistId)
    {
        try
        {
            var url = $"{_baseUrl}/playlists/{playlistId}";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<PlexClient>> GetClientsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PlexMediaContainer<PlexClient>>($"{_baseUrl}/clients");
            return response?.MediaContainer?.Directory ?? new List<PlexClient>();
        }
        catch (Exception)
        {
            return new List<PlexClient>();
        }
    }

    private int _commandId = 1;
    public async Task<bool> PlayMediaAsync(string ratingKey, string clientIdentifier, string libraryKey)
    {
        try
        {
            var machineId = await GetMachineIdentifierAsync();
            var url = $"{_baseUrl}/player/proxy/playback/playMedia?key={Uri.EscapeDataString("/library/metadata/" + ratingKey)}&machineIdentifier={machineId}&containerKey={Uri.EscapeDataString("/library/sections/" + libraryKey + "/all")}&commandID={_commandId++}&X-Plex-Token={_token}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Plex-Target-Client-Identifier", clientIdentifier);
            request.Headers.Add("X-Plex-Client-Identifier", "PlexRandomizerWPF");
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
