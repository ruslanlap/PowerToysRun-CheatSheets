// Main.cs - Plugin Entry Point (merged with template style)
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.CheatSheets
{
    /// <summary>
    /// Main class of this plugin that implements all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable, IPluginI18n, ISettingProvider
    {
        /// <summary>
        /// ID of the plugin (kept from the working template).
        /// </summary>
        public static string PluginID => "41BF0604C51A4974A0BAA108826D0A94";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "Cheat Sheets Finder";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Find cheat sheets and command examples instantly";

        private PluginInitContext Context { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        // --- Services from your original implementation ---
        private readonly CheatSheetService _cheatSheetService;
        private readonly CacheService _cacheService;
        private readonly FavoritesService _favoritesService;
        private readonly UsageHistoryService _usageHistoryService;

        // --- Settings cache ---
        private bool _enableDevHints = true;
        private bool _enableTldr = true;
        private bool _enableCheatSh = true;
        private int _cacheDurationHours = 12;

        public Main()
        {
            _cacheService = new CacheService();
            _cheatSheetService = new CheatSheetService(_cacheService);
            _favoritesService = new FavoritesService();
            _usageHistoryService = new UsageHistoryService();
        }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var search = (query?.Search ?? string.Empty).Trim();

            // Default help card with new features info
            if (string.IsNullOrWhiteSpace(search))
            {
                var helpResults = new List<Result>
                {
                    new Result
                    {
                        IcoPath = IconPath,
                        Title = "Cheat Sheets Finder - Enhanced!",
                        SubTitle = "Search commands, try 'cs:popular' for trending, 'cs:git/docker/python' for categories, or 'cs:fav' for favorites",
                        ToolTipData = new ToolTipData("Enhanced Cheat Sheets", "New: Fuzzy search, favorites, categories, offline mode"),
                        Action = _ => true,
                    }
                };

                // Show popular commands from history or default suggestions
                var popularCommands = _usageHistoryService.GetPopularCommands(3);
                if (popularCommands.Any())
                {
                    helpResults.Add(new Result
                    {
                        IcoPath = IconPath,
                        Title = "Your Popular Commands",
                        SubTitle = string.Join(", ", popularCommands.Take(3)),
                        ToolTipData = new ToolTipData("Popular Commands", "Commands you use most often"),
                        Action = _ => true,
                    });
                }
                else
                {
                    // Show some default popular commands to get users started
                    helpResults.Add(new Result
                    {
                        IcoPath = IconPath,
                        Title = "Popular Commands",
                        SubTitle = "git status, docker ps, python -m venv, npm install - try 'cs:popular' for more",
                        ToolTipData = new ToolTipData("Popular Commands", "Common commands to get you started"),
                        Action = _ => 
                        {
                            Context.API.ChangeQuery($"{Context.CurrentPluginMetadata.ActionKeyword} cs:popular");
                            return false;
                        },
                    });
                }

                return helpResults;
            }

            // Handle special commands
            if (search.StartsWith("cs:", StringComparison.OrdinalIgnoreCase))
            {
                return HandleSpecialCommands(search);
            }

            // Configure sources based on settings
            _cheatSheetService.ConfigureSources(new CheatSheetSourceOptions
            {
                EnableDevHints = _enableDevHints,
                EnableTldr = _enableTldr,
                EnableCheatSh = _enableCheatSh,
                CacheDuration = TimeSpan.FromHours(Math.Max(1, _cacheDurationHours)),
            });

            // Get results from multiple sources with enhanced scoring
            var results = new List<Result>();

            // 1. Get personalized suggestions first
            var personalizedSuggestions = _usageHistoryService.GetPersonalizedSuggestions(search, 2);
            foreach (var suggestion in personalizedSuggestions)
            {
                results.Add(new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = $"Suggestion: {suggestion}",
                    SubTitle = "Personal suggestion based on your usage",
                    ToolTipData = new ToolTipData("Personal Suggestion", "Command you've used before with similar searches"),
                    Score = 150, // High priority for personal suggestions
                    Action = _ =>
                    {
                        _usageHistoryService.RecordUsage(suggestion, search);
                        Clipboard.SetDataObject(suggestion);
                        return true;
                    },
                    ContextData = new CheatSheetItem { Command = suggestion, SourceName = "Personal", Title = suggestion }
                });
            }

            // 2. Get favorites that match
            var favoriteMatches = _favoritesService.SearchFavorites(search).Take(2);
            foreach (var fav in favoriteMatches)
            {
                var favResult = CreateResultFromCheatSheet(fav, search);
                favResult.Title = $"Favorite: {fav.Title}";
                favResult.Score += 50; // Boost favorites
                results.Add(favResult);
            }

            // 3. Get offline results for reliability (always show some offline results)
            var offlineResults = OfflineCheatSheets.Search(search).Take(5);
            foreach (var offline in offlineResults)
            {
                var result = CreateResultFromCheatSheet(offline, search);
                result.Score += 10; // Boost offline results slightly for reliability
                results.Add(result);
            }

            // 4. Get online results from all sources
            var onlineItems = _cheatSheetService.SearchCheatSheets(search) ?? Enumerable.Empty<CheatSheetItem>();

            foreach (var sheet in onlineItems.Take(10))
            {
                var result = CreateResultFromCheatSheet(sheet, search);

                // Add usage history boost
                var usageBoost = _usageHistoryService.GetUsageScore(sheet.Command);
                result.Score += usageBoost;

                // Add visual indicators for different sources
                result.Title = GetSourceIcon(sheet.SourceName) + " " + sheet.Title;

                results.Add(result);
            }

            // If nothing matched, show autocomplete suggestions and some default offline results
            if (results.Count == 0)
            {
                var suggestions = _cheatSheetService.GetAutocompleteSuggestions(search) ?? Enumerable.Empty<string>();
                foreach (var s in suggestions.Take(5))
                {
                    results.Add(new Result
                    {
                        QueryTextDisplay = search,
                        IcoPath = IconPath,
                        Title = $"Search for: {s}",
                        SubTitle = "Press Enter to search",
                        Score = 50,
                        Action = _ =>
                        {
                            Context.API.ChangeQuery($"{Context.CurrentPluginMetadata.ActionKeyword} {s}");
                            return false;
                        },
                        ContextData = s,
                    });
                }

                // If still no suggestions, show some popular offline commands
                if (results.Count == 0)
                {
                    var defaultCommands = new[]
                    {
                        ("git status", "Show the working tree status"),
                        ("docker ps", "List running containers"),
                        ("kubectl get pods", "List pods in kubernetes"),
                        ("ls -la", "List files with details"),
                        ("npm install", "Install dependencies")
                    };

                    foreach (var (cmd, desc) in defaultCommands)
                    {
                        results.Add(new Result
                        {
                            QueryTextDisplay = search,
                            IcoPath = IconPath,
                            Title = cmd,
                            SubTitle = desc,
                            Score = 30,
                            Action = _ =>
                            {
                                _usageHistoryService.RecordUsage(cmd, search);
                                Clipboard.SetDataObject(cmd);
                                return true;
                            },
                            ContextData = new CheatSheetItem { Command = cmd, SourceName = "Default", Title = cmd, Description = desc }
                        });
                    }
                }
            }

            // Fallback: simple copy of the raw query (kept from template behavior)
            if (results.Count == 0)
            {
                results.Add(new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "Copy query to clipboard",
                    SubTitle = "No cheat sheets found. Press Enter to copy your text.",
                    ToolTipData = new ToolTipData("Copy", "Copies your input to clipboard"),
                    Action = _ =>
                    {
                        Clipboard.SetDataObject(search);
                        return true;
                    },
                    ContextData = search,
                });
            }

            return results;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            var menus = new List<ContextMenuResult>();

            // Enhanced context menu for CheatSheetItem with new keyboard shortcuts
            if (selectedResult?.ContextData is CheatSheetItem item)
            {
                // Copy command (Enter)
                menus.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy command (Enter)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.Enter,
                    Action = _ =>
                    {
                        _usageHistoryService.RecordUsage(item.Command);
                        Clipboard.SetDataObject(item.Command ?? string.Empty);
                        return true;
                    },
                });

                // Add/Remove from favorites (Alt+Enter)
                var isFavorite = _favoritesService.IsFavorite(item);
                menus.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = isFavorite ? "Remove from favorites (Alt+Enter)" : "Add to favorites (Alt+Enter)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = isFavorite ? "\xE735" : "\xE734", // Unfavorite : Favorite
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Alt,
                    Action = _ =>
                    {
                        _favoritesService.ToggleFavorite(item);
                        return true;
                    },
                });

                // Copy with description (Shift+Enter)
                menus.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy with description (Shift+Enter)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Shift,
                    Action = _ =>
                    {
                        var fullText = $"{item.Command}\n# {item.Description}";
                        _usageHistoryService.RecordUsage(item.Command);
                        Clipboard.SetDataObject(fullText);
                        return true;
                    },
                });

                // Open in browser (Ctrl+Enter)
                if (!string.IsNullOrWhiteSpace(item.Url) && !item.Url.StartsWith("offline://"))
                {
                    menus.Add(new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Open full page (Ctrl+Enter)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE774", // Globe
                        AcceleratorKey = Key.Enter,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            Helper.OpenInBrowser(item.Url);
                            return true;
                        },
                    });
                }
            }

            // Keep template’s generic copy (Ctrl+C) for plain string ContextData
            if (selectedResult?.ContextData is string search)
            {
                menus.Add(new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy to clipboard (Ctrl+C)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        Clipboard.SetDataObject(search);
                        return true;
                    },
                });
            }

            return menus;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that disposes additional objects and events from the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            _cacheService?.Dispose();
            _favoritesService?.Dispose();
            _usageHistoryService?.Dispose();
            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) =>
            IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
                ? "Images/cheatsheets.light.png"
                : "Images/cheatsheets.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

        // --- IPluginI18n ---
        public string GetTranslatedPluginTitle() => "Cheat Sheets Finder";
        public string GetTranslatedPluginDescription() => "Find cheat sheets and command examples instantly";

        // --- ISettingProvider ---
        public Control CreateSettingPanel()
        {
            // Optional: return a real WPF control if you want a custom panel.
            // For now keep it simple; settings are managed via AdditionalOptions.
            return null;
        }

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            if (settings == null || settings.AdditionalOptions == null) return;

            foreach (var option in settings.AdditionalOptions)
            {
                try
                {
                    switch (option.Key)
                    {
                        case "EnableDevHints":
                            _enableDevHints = option.Value;
                            break;
                        case "EnableTldr":
                            _enableTldr = option.Value;
                            break;
                        case "EnableCheatSh":
                            _enableCheatSh = option.Value;
                            break;
                        case "CacheDurationHours":
    _cacheDurationHours = option.Value ? 12 : 2;
                            break;
                    }
                }
                catch
                {
                    // ignore malformed options and continue
                }
            }
        }

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new PluginAdditionalOption
            {
                Key = "EnableDevHints",
                DisplayLabel = "Enable DevHints.io",
                Value = true,
            },
            new PluginAdditionalOption
            {
                Key = "EnableTldr",
                DisplayLabel = "Enable TLDR",
                Value = true,
            },
            new PluginAdditionalOption
            {
                Key = "EnableCheatSh",
                DisplayLabel = "Enable cheat.sh",
                Value = true,
            },
            // NOTE: PowerToys exposes AdditionalOptions as bools by default.
            // If you want a true numeric field, define JSON settings schema and bind it.
            // Here we keep the toggle to "enable long cache" as a simple placeholder.
            new PluginAdditionalOption
            {
                Key = "CacheDurationHours",
                DisplayLabel = "Use extended cache duration",
                Value = true,
            },
        };

        // === Helper Methods ===

        private List<Result> HandleSpecialCommands(string search)
        {
            var command = search.ToLowerInvariant();

            if (command.Equals("cs:fav") || command.Equals("cs:favorites"))
            {
                return GetFavoritesResults();
            }

            if (command.Equals("cs:popular") || command.Equals("cs:trending"))
            {
                return GetPopularCommandsResults();
            }

            if (command.StartsWith("cs:") && command.Length > 3)
            {
                var category = command.Substring(3);
                return GetCategoryResults(category);
            }

            return new List<Result>();
        }

        private List<Result> GetFavoritesResults()
        {
            var favorites = _favoritesService.GetFavorites();
            if (!favorites.Any())
            {
                return new List<Result>
                {
                    new Result
                    {
                        IcoPath = IconPath,
                        Title = "No favorites yet",
                        SubTitle = "Use Alt+Enter on any command to add it to favorites",
                        Action = _ => true
                    }
                };
            }

            return favorites.Take(12).Select(fav =>
            {
                var result = CreateResultFromCheatSheet(fav, "");
                result.Title = $"Favorite: {fav.Title}";
                return result;
            }).ToList();
        }

        private List<Result> GetPopularCommandsResults()
        {
            var popular = _usageHistoryService.GetPopularCommands(8);
            if (!popular.Any())
            {
                // Show some default popular commands from offline cheat sheets
                var defaultPopular = new List<Result>
                {
                    new Result
                    {
                        IcoPath = IconPath,
                        Title = "No usage history yet",
                        SubTitle = "Start using commands to build your personal history",
                        Action = _ => true,
                        Score = 100
                    }
                };

                // Add some popular commands from offline sheets
                var gitCommands = OfflineCheatSheets.GetByCategory("git").Take(2);
                var dockerCommands = OfflineCheatSheets.GetByCategory("docker").Take(2);
                var linuxCommands = OfflineCheatSheets.GetByCategory("linux").Take(2);
                var pythonCommands = OfflineCheatSheets.GetByCategory("python").Take(1);
                var jsCommands = OfflineCheatSheets.GetByCategory("javascript").Take(1);

                foreach (var cmd in gitCommands.Concat(dockerCommands).Concat(linuxCommands).Concat(pythonCommands).Concat(jsCommands))
                {
                    var result = CreateResultFromCheatSheet(cmd, "popular");
                    result.Title = $"Popular: {cmd.Title}";
                    result.SubTitle = $"Popular {cmd.SourceName} command - {cmd.Description}";
                    defaultPopular.Add(result);
                }

                return defaultPopular;
            }

            return popular.Select(cmd => new Result
            {
                IcoPath = IconPath,
                Title = $"Popular: {cmd}",
                SubTitle = "Popular command from your history",
                Action = _ =>
                {
                    _usageHistoryService.RecordUsage(cmd);
                    Clipboard.SetDataObject(cmd);
                    return true;
                },
                ContextData = new CheatSheetItem { Command = cmd, SourceName = "History", Title = cmd }
            }).ToList();
        }

        private List<Result> GetCategoryResults(string category)
        {
            var offlineResults = OfflineCheatSheets.GetByCategory(category);
            if (offlineResults.Any())
            {
                return offlineResults.Take(12).Select(item => CreateResultFromCheatSheet(item, "")).ToList();
            }

            // If no offline results, search online with category prefix
            _cheatSheetService.ConfigureSources(new CheatSheetSourceOptions
            {
                EnableDevHints = _enableDevHints,
                EnableTldr = _enableTldr,
                EnableCheatSh = _enableCheatSh,
                CacheDuration = TimeSpan.FromHours(Math.Max(1, _cacheDurationHours)),
            });

            var onlineResults = _cheatSheetService.SearchCheatSheets(category) ?? Enumerable.Empty<CheatSheetItem>();
            return onlineResults.Take(12).Select(item => CreateResultFromCheatSheet(item, category)).ToList();
        }

        private Result CreateResultFromCheatSheet(CheatSheetItem sheet, string searchTerm)
        {
            // Extract category from SourceName if it exists
            string category = "";
            if (sheet.SourceName != null && sheet.SourceName.Contains(" "))
            {
                var parts = sheet.SourceName.Split(' ', 2);
                if (parts.Length > 1)
                {
                    category = parts[1];
                }
            }
            
            return new Result
            {
                QueryTextDisplay = searchTerm,
                IcoPath = IconPath,
                Title = sheet.Title,
                SubTitle = string.IsNullOrWhiteSpace(sheet.Description) 
                    ? sheet.SourceName 
                    : $"{sheet.Description} [{category}]", // Add category to description
                ToolTipData = new ToolTipData(
                    $"{sheet.Title} ({category})", 
                    $"{sheet.SourceName}\n{sheet.Command}\n\nCategory: {category}"),
                Score = sheet.Score,
                Action = _ =>
                {
                    _usageHistoryService.RecordUsage(sheet.Command, searchTerm);
                    Clipboard.SetDataObject(sheet.Command ?? string.Empty);
                    return true;
                },
                ContextData = sheet,
            };
        }

        private static string GetSourceIcon(string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                return "[Command]";
                
            // Handle case where sourceName already contains category (e.g., "📴 VIM")
            if (sourceName.Contains(" "))
            {
                var parts = sourceName.Split(' ', 2);
                if (parts.Length > 1)
                {
                    // Keep the emoji but format the category nicely
                    return $"{parts[0]} [{parts[1]}]";
                }
            }
            
            // Handle standard sources
            return sourceName.ToLowerInvariant() switch
            {
                "devhints" => "[DevHints]",
                "tldr" => "[TLDR]",
                "cheat.sh" => "[Cheat.sh]",
                "offline" => "[Offline]",
                "personal" => "[Personal]",
                "history" => "[History]",
                _ => $"[{sourceName}]"
            };
        }
    }
}
