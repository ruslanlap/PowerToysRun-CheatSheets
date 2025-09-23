using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Service for managing user's favorite cheat sheet commands
/// </summary>
public sealed class FavoritesService : IDisposable
{
    private readonly string _favoritesPath;
    private List<CheatSheetItem> _favorites;
    private bool _disposed;

    public FavoritesService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var pluginDataPath = Path.Combine(appDataPath, "Microsoft", "PowerToys", "PowerToys Run", "Plugins", "CheatSheets");

        if (!Directory.Exists(pluginDataPath))
        {
            Directory.CreateDirectory(pluginDataPath);
        }

        _favoritesPath = Path.Combine(pluginDataPath, "favorites.json");
        _favorites = LoadFavorites();
    }

    public IReadOnlyList<CheatSheetItem> GetFavorites()
    {
        return _favorites.AsReadOnly();
    }

    public bool IsFavorite(CheatSheetItem item)
    {
        return _favorites.Any(f => f.Command == item.Command && f.SourceName == item.SourceName);
    }

    public void AddFavorite(CheatSheetItem item)
    {
        if (IsFavorite(item)) return;

        var favorite = new CheatSheetItem
        {
            Title = item.Title,
            Description = item.Description,
            Command = item.Command,
            Url = item.Url,
            SourceName = item.SourceName,
            Score = item.Score
        };

        _favorites.Insert(0, favorite); // Add to beginning for recent access

        // Limit to 50 favorites to prevent bloat
        if (_favorites.Count > 50)
        {
            _favorites = _favorites.Take(50).ToList();
        }

        SaveFavorites();
    }

    public void RemoveFavorite(CheatSheetItem item)
    {
        _favorites.RemoveAll(f => f.Command == item.Command && f.SourceName == item.SourceName);
        SaveFavorites();
    }

    public void ToggleFavorite(CheatSheetItem item)
    {
        if (IsFavorite(item))
        {
            RemoveFavorite(item);
        }
        else
        {
            AddFavorite(item);
        }
    }

    public List<CheatSheetItem> SearchFavorites(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return _favorites.ToList();
        }

        var term = searchTerm.ToLowerInvariant().Trim();

        return _favorites
            .Where(f =>
                f.Command?.ToLowerInvariant().Contains(term) == true ||
                f.Title?.ToLowerInvariant().Contains(term) == true ||
                f.Description?.ToLowerInvariant().Contains(term) == true ||
                FuzzyMatcher.IsFuzzyMatch(term, f.Command) ||
                FuzzyMatcher.IsFuzzyMatch(term, f.Title))
            .OrderByDescending(f => FuzzyMatcher.CalculateFuzzyScore(term, f.Command))
            .ThenByDescending(f => FuzzyMatcher.CalculateFuzzyScore(term, f.Title))
            .ToList();
    }

    private List<CheatSheetItem> LoadFavorites()
    {
        try
        {
            if (!File.Exists(_favoritesPath))
            {
                return new List<CheatSheetItem>();
            }

            var json = File.ReadAllText(_favoritesPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var favorites = JsonSerializer.Deserialize<List<CheatSheetItem>>(json, options);
            return favorites ?? new List<CheatSheetItem>();
        }
        catch
        {
            // If file is corrupted, start fresh
            return new List<CheatSheetItem>();
        }
    }

    private void SaveFavorites()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_favorites, options);
            File.WriteAllText(_favoritesPath, json);
        }
        catch
        {
            // Fail silently if can't save
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            SaveFavorites();
            _disposed = true;
        }
    }
}