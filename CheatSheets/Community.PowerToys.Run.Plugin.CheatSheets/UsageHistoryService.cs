using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Service for tracking command usage and providing personalized suggestions
/// </summary>
public sealed class UsageHistoryService : IDisposable
{
    private readonly string _historyPath;
    private Dictionary<string, CommandUsage> _commandUsage;
    private bool _disposed;

    public UsageHistoryService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var pluginDataPath = Path.Combine(appDataPath, "Microsoft", "PowerToys", "PowerToys Run", "Plugins", "CheatSheets");

        if (!Directory.Exists(pluginDataPath))
        {
            Directory.CreateDirectory(pluginDataPath);
        }

        _historyPath = Path.Combine(pluginDataPath, "usage_history.json");
        _commandUsage = LoadHistory();
    }

    public void RecordUsage(string command, string searchTerm = null)
    {
        if (string.IsNullOrWhiteSpace(command)) return;

        var key = command.ToLowerInvariant().Trim();

        if (!_commandUsage.ContainsKey(key))
        {
            _commandUsage[key] = new CommandUsage
            {
                Command = command,
                Count = 0,
                LastUsed = DateTime.UtcNow,
                SearchTerms = new List<string>()
            };
        }

        var usage = _commandUsage[key];
        usage.Count++;
        usage.LastUsed = DateTime.UtcNow;

        // Track search terms that led to this command
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant().Trim();
            if (!usage.SearchTerms.Contains(term))
            {
                usage.SearchTerms.Add(term);
                // Keep only last 10 search terms per command
                if (usage.SearchTerms.Count > 10)
                {
                    usage.SearchTerms.RemoveAt(0);
                }
            }
        }

        // Clean up old entries - keep only last 100 commands and remove entries older than 90 days
        CleanupHistory();
        SaveHistory();
    }

    public int GetUsageScore(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return 0;

        var key = command.ToLowerInvariant().Trim();
        if (!_commandUsage.ContainsKey(key)) return 0;

        var usage = _commandUsage[key];

        // Calculate score based on usage count and recency
        var daysSinceLastUse = (DateTime.UtcNow - usage.LastUsed).TotalDays;
        var recencyMultiplier = Math.Max(0.1, 1.0 - (daysSinceLastUse / 30.0)); // Decay over 30 days

        return (int)(usage.Count * recencyMultiplier * 10);
    }

    public List<string> GetPopularCommands(int limit = 10)
    {
        return _commandUsage.Values
            .OrderByDescending(u => GetUsageScore(u.Command))
            .Take(limit)
            .Select(u => u.Command)
            .ToList();
    }

    public List<string> GetPersonalizedSuggestions(string searchTerm, int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new List<string>();

        var term = searchTerm.ToLowerInvariant().Trim();

        // Find commands that were accessed via similar search terms
        var suggestions = _commandUsage.Values
            .Where(u => u.SearchTerms.Any(st =>
                st.Contains(term) ||
                term.Contains(st) ||
                FuzzyMatcher.IsFuzzyMatch(term, st, 50)))
            .OrderByDescending(u => GetUsageScore(u.Command))
            .Take(limit)
            .Select(u => u.Command)
            .ToList();

        return suggestions;
    }

    private void CleanupHistory()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90);

        // Remove old entries
        var toRemove = _commandUsage.Values
            .Where(u => u.LastUsed < cutoffDate)
            .Select(u => u.Command.ToLowerInvariant().Trim())
            .ToList();

        foreach (var key in toRemove)
        {
            _commandUsage.Remove(key);
        }

        // Keep only top 100 most used commands if we have more
        if (_commandUsage.Count > 100)
        {
            var topCommands = _commandUsage.Values
                .OrderByDescending(u => GetUsageScore(u.Command))
                .Take(100)
                .ToList();

            _commandUsage.Clear();
            foreach (var usage in topCommands)
            {
                _commandUsage[usage.Command.ToLowerInvariant().Trim()] = usage;
            }
        }
    }

    private Dictionary<string, CommandUsage> LoadHistory()
    {
        try
        {
            if (!File.Exists(_historyPath))
            {
                return new Dictionary<string, CommandUsage>();
            }

            var json = File.ReadAllText(_historyPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var history = JsonSerializer.Deserialize<Dictionary<string, CommandUsage>>(json, options);
            return history ?? new Dictionary<string, CommandUsage>();
        }
        catch
        {
            return new Dictionary<string, CommandUsage>();
        }
    }

    private void SaveHistory()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(_commandUsage, options);
            File.WriteAllText(_historyPath, json);
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
            SaveHistory();
            _disposed = true;
        }
    }
}

public class CommandUsage
{
    public string Command { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime LastUsed { get; set; }
    public List<string> SearchTerms { get; set; } = new();
}