using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace plexrandom;

public class MainViewModel : BaseViewModel
{
    private readonly PlexService _plexService = new();
    private readonly string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PlexRandom", "config.json");

    private PlexConfig _config = new();
    private ObservableCollection<PlexLibrary> _libraries = new();
    private PlexLibrary? _selectedLibrary;
    private ObservableCollection<string> _genres = new();
    private string? _selectedGenre;
    private bool _onlyUnseen;
    private int _playlistEntryCount = 10;
    private int _maxDuration = 300;
    private string? _yearFrom;
    private string? _yearTo;
    private PlexMovie? _selectedMovie;
    private ObservableCollection<PlexMovie> _playlistPreview = new();
    private ObservableCollection<RecentPlaylist> _recentPlaylists = new();
    private string _refreshButtonText = string.Empty;
    private string _connectionStatus = string.Empty;
    private string _connectionStatusColor = "White";
    private string _libraryStatus = string.Empty;
    private string _libraryStatusColor = "White";
    private bool _isTokenVisible;

    public MainViewModel()
    {
        LoadConfig();
        LocalizationService.SetLanguage(_config.Language);
        UpdateRefreshButtonText();
        _playlistEntryCount = _config.MinEntries;
        _maxDuration = _config.DefaultMovieDuration;
        _yearTo = DateTime.Now.Year.ToString();

        RefreshLibrariesCommand = new RelayCommand(async _ => await RefreshLibrariesAsync());
        RandomizeCommand = new RelayCommand(async _ => await RandomizeAsync(), _ => SelectedLibrary != null);
        SendCommand = new RelayCommand(async _ => await SendToPlexAsync(), _ => PlaylistPreview.Any());
        DeletePlaylistCommand = new RelayCommand(async p => await DeletePlaylistAsync(p as RecentPlaylist));
        PlayMovieCommand = new RelayCommand(async m => await PlayMovieAsync(m as PlexMovie ?? SelectedMovie), m => SelectedMovie != null || (m is PlexMovie));
        CheckConnectionCommand = new RelayCommand(async _ => await CheckConnectionAsync());
        ToggleTokenVisibilityCommand = new RelayCommand(_ => IsTokenVisible = !IsTokenVisible);
        SetLanguageCommand = new RelayCommand(lang =>
        {
            if (lang is string l)
            {
                _config.Language = l;
                LocalizationService.SetLanguage(l);
                UpdateRefreshButtonText();
                if (SelectedLibrary != null) _ = LoadGenresAsync();
                SaveConfig();
            }
        });

        LocalizationService.LanguageChanged += (_, _) => UpdateRefreshButtonText();

        if (!string.IsNullOrEmpty(_config.Url) && !string.IsNullOrEmpty(_config.Token))
        {
            _plexService.UpdateConfig(_config.Url, _config.Port, _config.Token);
            foreach (var lib in _config.Libraries) Libraries.Add(lib);
            foreach (var playlist in _config.RecentPlaylists) RecentPlaylists.Add(playlist);
        }
    }

    private void UpdateRefreshButtonText()
    {
        RefreshButtonText = _config.Libraries.Any()
            ? LocalizationService.Get("RefreshBtn_Refresh")
            : LocalizationService.Get("RefreshBtn_Load");
    }

    #region Properties
    public PlexConfig Config
    {
        get => _config;
        set => SetField(ref _config, value);
    }

    public ObservableCollection<PlexLibrary> Libraries
    {
        get => _libraries;
        set => SetField(ref _libraries, value);
    }

    public PlexLibrary? SelectedLibrary
    {
        get => _selectedLibrary;
        set
        {
            if (SetField(ref _selectedLibrary, value))
            {
                _ = LoadGenresAsync();
            }
        }
    }

    public ObservableCollection<string> Genres
    {
        get => _genres;
        set => SetField(ref _genres, value);
    }

    public string? SelectedGenre
    {
        get => _selectedGenre;
        set => SetField(ref _selectedGenre, value);
    }

    public bool OnlyUnseen
    {
        get => _onlyUnseen;
        set => SetField(ref _onlyUnseen, value);
    }

    public int PlaylistEntryCount
    {
        get => _playlistEntryCount;
        set => SetField(ref _playlistEntryCount, value);
    }

    public int MaxDuration
    {
        get => _maxDuration;
        set => SetField(ref _maxDuration, value);
    }

    public string? YearFrom
    {
        get => _yearFrom;
        set => SetField(ref _yearFrom, value);
    }

    public string? YearTo
    {
        get => _yearTo;
        set => SetField(ref _yearTo, value);
    }

    public PlexMovie? SelectedMovie
    {
        get => _selectedMovie;
        set => SetField(ref _selectedMovie, value);
    }

    public ObservableCollection<PlexMovie> PlaylistPreview
    {
        get => _playlistPreview;
        set => SetField(ref _playlistPreview, value);
    }

    public ObservableCollection<RecentPlaylist> RecentPlaylists
    {
        get => _recentPlaylists;
        set => SetField(ref _recentPlaylists, value);
    }

    public string RefreshButtonText
    {
        get => _refreshButtonText;
        set => SetField(ref _refreshButtonText, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetField(ref _connectionStatus, value);
    }

    public string ConnectionStatusColor
    {
        get => _connectionStatusColor;
        set => SetField(ref _connectionStatusColor, value);
    }

    public string LibraryStatus
    {
        get => _libraryStatus;
        set => SetField(ref _libraryStatus, value);
    }

    public string LibraryStatusColor
    {
        get => _libraryStatusColor;
        set => SetField(ref _libraryStatusColor, value);
    }

    public bool IsTokenVisible
    {
        get => _isTokenVisible;
        set => SetField(ref _isTokenVisible, value);
    }

    public string PlexToken
    {
        get => _config.Token;
        set
        {
            if (_config.Token != value)
            {
                _config.Token = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    #region Commands
    public ICommand RefreshLibrariesCommand { get; }
    public ICommand RandomizeCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
    public ICommand PlayMovieCommand { get; }
    public ICommand CheckConnectionCommand { get; }
    public ICommand ToggleTokenVisibilityCommand { get; }
    public ICommand SetLanguageCommand { get; private set; } = null!;
    #endregion

    private void LoadConfig()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<PlexConfig>(json) ?? new PlexConfig();
            }
            catch { _config = new PlexConfig(); }
        }
    }

    private void SaveConfig()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(LocalizationService.Format("Msg_SaveError", ex.Message));
        }
    }

    private async Task RefreshLibrariesAsync()
    {
        LibraryStatus = LocalizationService.Get("Status_Loading");
        LibraryStatusColor = "White";

        _plexService.UpdateConfig(_config.Url, _config.Port, _config.Token);
        var libs = await _plexService.GetLibrariesAsync();

        if (libs.Any())
        {
            Libraries.Clear();
            _config.Libraries.Clear();
            foreach (var lib in libs.Where(l => l.Type == "movie"))
            {
                Libraries.Add(lib);
                _config.Libraries.Add(lib);
            }

            UpdateRefreshButtonText();
            LibraryStatus = "OK";
            LibraryStatusColor = "LightGreen";
            SaveConfig();
        }
        else
        {
            LibraryStatus = "FAILED";
            LibraryStatusColor = "Red";
        }
    }

    private async Task CheckConnectionAsync()
    {
        ConnectionStatus = LocalizationService.Get("Status_Checking");
        ConnectionStatusColor = "White";

        _plexService.UpdateConfig(_config.Url, _config.Port, _config.Token);
        var success = await _plexService.CheckConnectionAsync();

        if (success)
        {
            ConnectionStatus = "OK";
            ConnectionStatusColor = "LightGreen";
        }
        else
        {
            ConnectionStatus = "FAILED";
            ConnectionStatusColor = "Red";
        }
    }

    private async Task LoadGenresAsync()
    {
        if (SelectedLibrary == null) return;
        var genres = await _plexService.GetGenresAsync(SelectedLibrary.Key);
        var allGenres = LocalizationService.Get("Status_AllGenres");
        Genres.Clear();
        Genres.Add(allGenres);
        foreach (var g in genres.OrderBy(x => x)) Genres.Add(g);
        SelectedGenre = allGenres;
    }

    private async Task RandomizeAsync()
    {
        if (SelectedLibrary == null) return;
        
        var allMovies = await _plexService.GetAllMoviesAsync(SelectedLibrary.Key);
        
        // Filter
        var filtered = allMovies.AsEnumerable();
        if (OnlyUnseen) filtered = filtered.Where(m => m.ViewCount == 0);
        if (SelectedGenre != null && SelectedGenre != "Alle Genres")
        {
            filtered = filtered.Where(m => m.Genres.Contains(SelectedGenre));
        }

        const int maxDurationSliderMax = 300; // slider maximum = "no limit"
        if (MaxDuration < maxDurationSliderMax)
        {
            filtered = filtered.Where(m => m.Duration <= MaxDuration * 60000);
        }

        if (int.TryParse(YearFrom, out int yearFrom))
        {
            filtered = filtered.Where(m => m.Year >= yearFrom);
        }

        if (int.TryParse(YearTo, out int yearTo))
        {
            filtered = filtered.Where(m => m.Year <= yearTo);
        }

        var list = filtered.ToList();
        if (!list.Any())
        {
            MessageBox.Show(LocalizationService.Get("Msg_NoMoviesFound"));
            return;
        }

        // Pick movies by sampling a random year first, then a random movie from that year
        // This avoids over-representing years with many movies
        var selectedMovies = new List<PlexMovie>();
        var moviesByYear = list.Where(m => m.Year.HasValue)
                               .GroupBy(m => m.Year!.Value)
                               .ToDictionary(g => g.Key, g => g.ToList());
        
        // Add movies without year as a separate "year" if any
        var moviesNoYear = list.Where(m => !m.Year.HasValue).ToList();
        if (moviesNoYear.Any()) moviesByYear[-1] = moviesNoYear;

        var random = new Random();
        int countToSelect = Math.Min(PlaylistEntryCount, list.Count);

        while (selectedMovies.Count < countToSelect && moviesByYear.Any())
        {
            // Pick a random year
            var years = moviesByYear.Keys.ToList();
            var randomYear = years[random.Next(years.Count)];
            
            // Pick a random movie from that year
            var moviesInYear = moviesByYear[randomYear];
            var randomMovie = moviesInYear[random.Next(moviesInYear.Count)];
            
            selectedMovies.Add(randomMovie);
            
            // Remove movie from pool
            moviesInYear.Remove(randomMovie);
            if (!moviesInYear.Any())
            {
                moviesByYear.Remove(randomYear);
            }
        }

        PlaylistPreview.Clear();
        for (int i = 0; i < selectedMovies.Count; i++)
        {
            selectedMovies[i].PlaylistPosition = i + 1;
            PlaylistPreview.Add(selectedMovies[i]);
        }
    }

    private async Task SendToPlexAsync()
    {
        if (!PlaylistPreview.Any()) return;
        
        var title = $"Random Playlist {DateTime.Now:yyyy-MM-dd HH:mm}";
        var playlistId = await _plexService.CreatePlaylistAsync(title, PlaylistPreview.ToList());
        
        if (!string.IsNullOrEmpty(playlistId))
        {
            var newPlaylist = new RecentPlaylist 
            { 
                Id = playlistId, 
                Title = title, 
                CreatedAt = DateTime.Now 
            };
            
            _config.RecentPlaylists.Insert(0, newPlaylist);
            RecentPlaylists.Insert(0, newPlaylist);
            SaveConfig();
            
            MessageBox.Show(LocalizationService.Get("Msg_PlaylistSent"));
        }
        else
        {
            MessageBox.Show(LocalizationService.Get("Msg_PlaylistSendError"));
        }
    }

    private async Task DeletePlaylistAsync(RecentPlaylist? playlist)
    {
        if (playlist == null) return;

        var result = MessageBox.Show(
            LocalizationService.Format("Msg_DeleteConfirm", playlist.Title),
            LocalizationService.Get("Msg_DeleteTitle"),
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        // Prüfen ob Playlist noch existiert
        bool exists = await _plexService.PlaylistExistsAsync(playlist.Id);
        
        if (exists)
        {
            bool success = await _plexService.DeletePlaylistAsync(playlist.Id);
            if (!success)
            {
                MessageBox.Show(LocalizationService.Get("Msg_DeleteError"));
                return;
            }
        }
        else
        {
            MessageBox.Show(LocalizationService.Get("Msg_PlaylistGone"));
        }

        // Aus lokaler Liste entfernen (auch wenn sie in Plex schon weg war)
        _config.RecentPlaylists.Remove(playlist);
        RecentPlaylists.Remove(playlist);
        SaveConfig();
    }

    private async Task PlayMovieAsync(PlexMovie? movie)
    {
        if (movie == null || SelectedLibrary == null) return;

        var clients = await _plexService.GetClientsAsync();
        var client = clients.FirstOrDefault();

        if (client == null)
        {
            MessageBox.Show(LocalizationService.Get("Msg_NoClient"));
            return;
        }

        var success = await _plexService.PlayMediaAsync(movie.RatingKey, client.MachineIdentifier, SelectedLibrary.Key);
        if (!success)
        {
            MessageBox.Show(LocalizationService.Format("Msg_PlayError", client.Name));
        }
    }
}
