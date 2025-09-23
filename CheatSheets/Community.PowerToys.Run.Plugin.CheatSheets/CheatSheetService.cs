using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Core service that queries the remote cheat sheet providers and normalises the results.
/// </summary>
public sealed class CheatSheetService
{
    private static readonly string[] TldrPlatforms = { "common", "linux", "osx", "windows" };

    private static readonly string[] CommonTopics =
    {
        "git", "docker", "kubernetes", "python", "javascript", "typescript", "react", "vue",
        "angular", "node", "npm", "yarn", "bash", "powershell", "sql", "mongodb", "redis",
        "aws", "azure", "gcp", "linux", "vim", "regex", "css", "html", "java", "c#", "go",
        "ssh", "scp", "tmux", "ffmpeg", "curl", "kubectl", "helm", "podman", "nginx", "apache",
        "mysql", "postgresql", "elasticsearch", "webpack", "vscode", "jupyter", "conda", "pip"
    };

    // Enhanced suggestions based on popular commands from all three sources
    private static readonly Dictionary<string, string[]> SmartSuggestions = new()
    {
        ["git"] = new[] { "git reset", "git commit", "git merge", "git rebase", "git stash", "git branch", "git checkout", "git log", "git diff", "git push", "git rm", "git add", "git pull", "git clone" },
        ["docker"] = new[] { "docker build", "docker compose", "docker run", "docker volume", "docker network", "docker ps", "docker exec", "docker logs", "docker pull", "docker push" },
        ["kubernetes"] = new[] { "kubectl get", "kubectl apply", "kubectl describe", "kubectl logs", "kubectl exec", "kubectl delete", "kubectl create", "kubectl port-forward" },
        ["k8s"] = new[] { "kubectl get", "kubectl apply", "kubectl describe", "kubectl logs", "kubectl exec" },
        ["kubectl"] = new[] { "kubectl get pods", "kubectl apply -f", "kubectl describe", "kubectl logs", "kubectl exec -it" },
        ["vim"] = new[] { "vim navigation", "vim search", "vim replace", "vim commands", "vim modes" },
        ["bash"] = new[] { "bash loops", "bash conditionals", "bash variables", "bash functions", "bash arrays" },
        ["powershell"] = new[] { "powershell cmdlets", "powershell variables", "powershell objects", "powershell loops" },
        ["regex"] = new[] { "regex lookahead", "regex groups", "regex anchors", "regex quantifiers", "regex character classes" },
        ["sql"] = new[] { "sql select", "sql join", "sql insert", "sql update", "sql delete", "sql create table" },
        ["python"] = new[] { "python list", "python dict", "python functions", "python classes", "python loops" },
        ["javascript"] = new[] { "javascript array", "javascript object", "javascript promises", "javascript async", "javascript dom" },
        ["js"] = new[] { "javascript array", "javascript object", "javascript promises", "javascript async" },
        ["node"] = new[] { "node modules", "node fs", "node http", "node express", "node package.json" },
        ["npm"] = new[] { "npm install", "npm run", "npm scripts", "npm publish", "npm update" },
        ["yarn"] = new[] { "yarn add", "yarn install", "yarn run", "yarn workspace" },
        ["aws"] = new[] { "aws s3", "aws ec2", "aws lambda", "aws cli", "aws iam" },
        ["linux"] = new[] { "linux commands", "linux permissions", "linux processes", "linux networking", "linux file system" }
    };

    private readonly HttpClient _httpClient;
    private readonly CacheService _cacheService;
    private CheatSheetSourceOptions _options = new();

    public CheatSheetService(CacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(6)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CheatSheetsPlugin", "1.0"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
    }

    public void ConfigureSources(CheatSheetSourceOptions options)
    {
        _options = options ?? new CheatSheetSourceOptions();
    }

    public List<CheatSheetItem> SearchCheatSheets(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<CheatSheetItem>();
        }

        var trimmed = searchTerm.Trim();
        var normalizedKey = trimmed.ToLowerInvariant();
        var cacheKey = $"cheats:v4::{normalizedKey}::{_options.EnableDevHints}_{_options.EnableTldr}_{_options.EnableCheatSh}";

        var cached = _cacheService.Get<List<CheatSheetItem>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var tasks = new List<Task<List<CheatSheetItem>>>();

        if (_options.EnableCheatSh)
        {
            tasks.Add(SearchCheatSh(trimmed));
        }

        if (_options.EnableDevHints)
        {
            tasks.Add(SearchDevHints(trimmed));
        }

        if (_options.EnableTldr)
        {
            tasks.Add(SearchTldr(trimmed));
        }

        if (tasks.Count == 0)
        {
            return new List<CheatSheetItem>();
        }

        try
        {
            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(8));
        }
        catch (AggregateException)
        {
            // Individual tasks will surface their partial results if completed.
        }
        catch (Exception)
        {
            // Ignore unexpected wait issues and continue with whatever finished.
        }

        var combined = new List<CheatSheetItem>();
        foreach (var task in tasks)
        {
            if (task.Status == TaskStatus.RanToCompletion && task.Result is { Count: > 0 })
            {
                combined.AddRange(task.Result);
            }
        }

        var deduped = combined
            .GroupBy(item => $"{item.SourceName}|{item.Command}")
            .Select(group => group.OrderByDescending(x => x.Score).First())
            .OrderByDescending(item => item.Score)
            .ToList();

        if (deduped.Count > 0)
        {
            var cacheDuration = _options.CacheDuration > TimeSpan.Zero
                ? _options.CacheDuration
                : TimeSpan.FromHours(2);

            _cacheService.Set(cacheKey, deduped, cacheDuration);
        }

        return deduped;
    }

    public List<string> GetAutocompleteSuggestions(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<string>();
        }

        var term = searchTerm.ToLowerInvariant().Trim();
        var suggestions = new List<string>();

        // Check for exact smart suggestions first
        foreach (var kvp in SmartSuggestions)
        {
            if (term.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                // If user typed exactly the key, show all suggestions
                if (term.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.AddRange(kvp.Value.Take(6));
                    break;
                }
                // If user typed key + space, filter suggestions
                else if (term.StartsWith($"{kvp.Key} ", StringComparison.OrdinalIgnoreCase))
                {
                    var subterm = term.Substring(kvp.Key.Length + 1);
                    suggestions.AddRange(kvp.Value
                        .Where(s => s.Contains(subterm, StringComparison.OrdinalIgnoreCase))
                        .Take(6));
                    break;
                }
            }
        }

        // If no smart suggestions found, fall back to topic matching
        if (suggestions.Count == 0)
        {
            // Exact matches first
            suggestions.AddRange(CommonTopics
                .Where(topic => topic.Equals(term, StringComparison.OrdinalIgnoreCase))
                .Take(2));

            // Starts with matches
            suggestions.AddRange(CommonTopics
                .Where(topic => topic.StartsWith(term, StringComparison.OrdinalIgnoreCase) &&
                               !topic.Equals(term, StringComparison.OrdinalIgnoreCase))
                .Take(3));

            // Contains matches if we still need more
            if (suggestions.Count < 5)
            {
                suggestions.AddRange(CommonTopics
                    .Where(topic => topic.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                                   !topic.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                    .Take(5 - suggestions.Count));
            }
        }

        return suggestions.Distinct().Take(6).ToList();
    }

    private async Task<List<CheatSheetItem>> SearchCheatSh(string searchTerm)
    {
        var results = new List<CheatSheetItem>();

        try
        {
            var encodedQuery = Uri.EscapeDataString(searchTerm).Replace("%20", "+");
            var requestUrl = $"https://cheat.sh/{encodedQuery}?T";

            using var response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // Handle specific HTTP errors gracefully
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Command not found on cheat.sh - this is normal, just return empty
                    return results;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    // Rate limited - could add to cache with short expiry to avoid hammering
                    return results;
                }
                return results;
            }

            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return results;
            }

            // Check if the response is HTML (error page) and skip processing
            if (payload.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
                payload.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                payload.Contains("<head>", StringComparison.OrdinalIgnoreCase) ||
                payload.Contains("<title>", StringComparison.OrdinalIgnoreCase))
            {
                return results; // Skip HTML error pages
            }

            var lines = payload.Split('\n');
            var currentDescription = string.Empty;

            foreach (var raw in lines)
            {
                if (results.Count >= 15)
                {
                    break;
                }

                var line = raw.TrimEnd();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(">"))
                {
                    currentDescription = line.TrimStart('#', '/', '>', ' ').Trim();
                    continue;
                }

                if (line.Contains("://cheat.sh", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Filter out any remaining HTML tags and HTML content
                if (System.Text.RegularExpressions.Regex.IsMatch(line, @"<[^>]+>") ||
                    line.Contains("<head>", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("<title>", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("</title>", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("</head>", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var cleaned = line.StartsWith("$") ? line.TrimStart('$').TrimStart() : line;

                // Clean up confusing bracket patterns like {{[-A|--all]}} to [-A|--all]
                cleaned = CleanCommandSyntax(cleaned);
                if (string.IsNullOrWhiteSpace(cleaned))
                {
                    continue;
                }

                results.Add(new CheatSheetItem
                {
                    Title = Truncate(cleaned, 80),
                    Description = string.IsNullOrWhiteSpace(currentDescription) ? "From cheat.sh" : currentDescription,
                    Command = cleaned,
                    Url = $"https://cheat.sh/{encodedQuery}",
                    SourceName = "cheat.sh",
                    Score = CalculateScore(searchTerm, cleaned, currentDescription)
                });

                currentDescription = string.Empty;
            }
        }
        catch (HttpRequestException)
        {
            // Network connectivity issues - fail silently
        }
        catch (TaskCanceledException)
        {
            // Request timeout - fail silently
        }
        catch (Exception)
        {
            // Other unexpected errors - fail silently but could log in production
        }

        return results;
    }

    private async Task<List<CheatSheetItem>> SearchDevHints(string searchTerm)
    {
        var results = new List<CheatSheetItem>();

        try
        {
            var slug = Slugify(searchTerm);
            if (string.IsNullOrWhiteSpace(slug))
            {
                return results;
            }

            var rawUrl = $"https://raw.githubusercontent.com/rstacruz/cheatsheets/master/{slug}.md";
            using var response = await _httpClient.GetAsync(rawUrl).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return results;
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
            {
                return results;
            }

            var url = $"https://devhints.io/{slug}";
            var lines = content.Split('\n');
            string title = null;
            var codeLines = new List<string>();
            var descriptionBuilder = new StringBuilder();
            var hasCapturedSection = false;
            var insideCodeBlock = false;

            void CommitSection()
            {
                if (string.IsNullOrWhiteSpace(title) || codeLines.Count == 0)
                {
                    return;
                }

                var commandLine = codeLines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim();
                if (string.IsNullOrWhiteSpace(commandLine))
                {
                    return;
                }

                var description = descriptionBuilder.ToString().Trim();
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = "Snippet from DevHints";
                }

                results.Add(new CheatSheetItem
                {
                    Title = title,
                    Description = description,
                    Command = commandLine,
                    Url = url,
                    SourceName = "DevHints",
                    Score = CalculateScore(searchTerm, commandLine, description)
                });

                hasCapturedSection = true;
            }

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                if (line.StartsWith("---", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith("### ", StringComparison.Ordinal))
                {
                    CommitSection();

                    title = line[4..].Trim();
                    if (title.StartsWith("`", StringComparison.Ordinal) && title.EndsWith("`", StringComparison.Ordinal))
                    {
                        title = title.Trim('`', ' ');
                    }

                    codeLines.Clear();
                    descriptionBuilder.Clear();
                    insideCodeBlock = false;
                    continue;
                }

                if (line.StartsWith("```", StringComparison.Ordinal))
                {
                    insideCodeBlock = !insideCodeBlock;
                    continue;
                }

                if (insideCodeBlock)
                {
                    codeLines.Add(line.Trim());
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("####", StringComparison.Ordinal))
                {
                    continue;
                }

                if (descriptionBuilder.Length == 0)
                {
                    descriptionBuilder.Append(line.Trim());
                }
            }

            CommitSection();

            if (!hasCapturedSection)
            {
                results.Add(new CheatSheetItem
                {
                    Title = $"Open DevHints page for {searchTerm}",
                    Description = "Open the DevHints cheat sheet in your browser.",
                    Command = url,
                    Url = url,
                    SourceName = "DevHints",
                    Score = 40
                });
            }
        }
        catch (HttpRequestException)
        {
            // Network connectivity issues - fail silently
        }
        catch (TaskCanceledException)
        {
            // Request timeout - fail silently
        }
        catch (Exception)
        {
            // Other unexpected errors - fail silently but could log in production
        }

        return results;
    }

    private async Task<List<CheatSheetItem>> SearchTldr(string searchTerm)
    {
        var results = new List<CheatSheetItem>();

        try
        {
            // Try multiple command variations for tldr search
            var commandVariations = GetCommandVariations(searchTerm);

            // Optimize: try common platform first, then OS-specific
            var platformPriority = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? new[] { "common", "windows", "linux", "osx" }
                : new[] { "common", "linux", "osx", "windows" };

            foreach (var commandToken in commandVariations)
            {
                if (string.IsNullOrWhiteSpace(commandToken))
                {
                    continue;
                }

                foreach (var platform in platformPriority)
                {
                    var rawUrl = $"https://raw.githubusercontent.com/tldr-pages/tldr/main/pages/{platform}/{commandToken}.md";
                    using var response = await _httpClient.GetAsync(rawUrl).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        continue;
                    }

                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }

                    string description = null;
                    foreach (var rawLine in content.Split('\n'))
                    {
                        var line = rawLine.Trim();

                        if (line.StartsWith("- ", StringComparison.Ordinal))
                        {
                            description = line[2..].Trim();
                        }
                        else if (line.StartsWith("`", StringComparison.Ordinal) && description != null)
                        {
                            var command = line.Trim('`').Trim();
                            command = CleanCommandSyntax(command); // Apply the same cleaning as cheat.sh

                            if (!string.IsNullOrWhiteSpace(command))
                            {
                                results.Add(new CheatSheetItem
                                {
                                    Title = command,
                                    Description = description,
                                    Command = command,
                                    Url = $"https://tldr.inbrowser.app/pages/{platform}/{commandToken}",
                                    SourceName = $"tldr ({platform})",
                                    Score = CalculateScore(searchTerm, command, description)
                                });
                            }

                            description = null;
                        }
                    }

                    if (results.Count > 0)
                    {
                        break; // Prefer the first platform that matches.
                    }
                }

                if (results.Count > 0)
                {
                    break; // Stop trying variations once we find results
                }
            }
        }
        catch (HttpRequestException)
        {
            // Network connectivity issues - fail silently
        }
        catch (TaskCanceledException)
        {
            // Request timeout - fail silently
        }
        catch (Exception)
        {
            // Other unexpected errors - fail silently but could log in production
        }

        return results;
    }

    private static int CalculateScore(string searchTerm, string command, string description)
    {
        var score = 50;
        var searchLower = searchTerm.ToLowerInvariant().Trim();
        var commandLower = command?.ToLowerInvariant() ?? string.Empty;
        var descriptionLower = description?.ToLowerInvariant() ?? string.Empty;

        // Exact match gets highest score
        if (commandLower.Equals(searchLower, StringComparison.Ordinal))
        {
            score += 100;
        }
        // Command starts with search term
        else if (commandLower.StartsWith(searchLower, StringComparison.Ordinal))
        {
            score += 80;
        }
        // Command contains exact search term
        else if (commandLower.Contains(searchLower, StringComparison.Ordinal))
        {
            score += 50;
        }

        // Bonus for description relevance
        if (descriptionLower.Contains(searchLower, StringComparison.Ordinal))
        {
            score += 25;
        }

        var searchWords = searchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchedWords = 0;

        foreach (var word in searchWords)
        {
            if (word.Length < 2) continue; // Skip very short words

            // Higher score for longer words (more specific)
            var wordBonus = Math.Min(word.Length * 2, 20);

            if (commandLower.StartsWith(word, StringComparison.Ordinal))
            {
                score += wordBonus * 2;
                matchedWords++;
            }
            else if (commandLower.Contains($" {word}", StringComparison.Ordinal) ||
                     commandLower.Contains($"-{word}", StringComparison.Ordinal) ||
                     commandLower.Contains($"_{word}", StringComparison.Ordinal))
            {
                score += wordBonus;
                matchedWords++;
            }
            else if (commandLower.Contains(word, StringComparison.Ordinal))
            {
                score += wordBonus / 2;
                matchedWords++;
            }

            if (descriptionLower.Contains(word, StringComparison.Ordinal))
            {
                score += Math.Min(wordBonus / 3, 5);
            }
        }

        // Bonus for matching multiple words
        if (searchWords.Length > 1 && matchedWords > 1)
        {
            score += (matchedWords * 10);
        }

        // Penalty for very long commands (less likely to be what user wants)
        if (command != null && command.Length > 100)
        {
            score -= 10;
        }

        // Bonus for common/popular commands
        if (IsPopularCommand(commandLower))
        {
            score += 15;
        }

        return Math.Max(score, 1); // Minimum score of 1
    }

    private static bool IsPopularCommand(string command)
    {
        var popularPatterns = new[]
        {
            "git add", "git commit", "git push", "git pull", "git reset", "git log", "git rm", "git mv", "git checkout", "git branch",
            "docker run", "docker build", "docker ps", "docker exec", "docker compose",
            "kubectl get", "kubectl apply", "kubectl describe", "kubectl logs",
            "npm install", "npm run", "yarn add", "yarn install",
            "ls", "cd", "mkdir", "rm", "cp", "mv", "grep", "find", "sed", "awk"
        };

        return popularPatterns.Any(pattern => command.StartsWith(pattern, StringComparison.Ordinal));
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return string.Concat(value.AsSpan(0, maxLength - 1), "…");
    }

    private static string Slugify(string term)
    {
        var builder = new StringBuilder();
        foreach (var c in term.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '.')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        return slug;
    }

    private static string CleanCommandSyntax(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return command;
        }

        var cleaned = command;

        // Remove any HTML content that might have slipped through
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"<[^>]+>.*?</[^>]+>", "");
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"<[^>]+>", "");

        // Fix double curly braces around optional parameters with brackets
        // Example: git add {{[-A|--all]}} -> git add [-A|--all]
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\{\{(\[.*?\])\}\}", "$1");

        // Fix double curly braces around URLs and other content (but preserve single braces for placeholders)
        // Example: git clone {{https://example.com/repo.git}} -> git clone https://example.com/repo.git
        // But keep: git clone {repository-url} (single braces for placeholders)
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\{\{([^{}]+)\}\}", "$1");

        // Clean up cheat.sh specific formatting issues
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"^\s*\$\s*", ""); // Remove leading $ from shell commands
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"^\s*#\s*", ""); // Remove leading # from comments when they're commands

        // Clean up common formatting artifacts
        cleaned = cleaned.Replace("\\n", " "); // Replace literal \n with space
        cleaned = cleaned.Replace("\\t", " "); // Replace literal \t with space
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " "); // Multiple spaces to single space

        // Remove ANSI escape codes that might appear in terminal output
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\x1b\[[0-9;]*m", "");

        // Remove URLs that might appear in commands (keep command, remove URL)
        // But only if the URL is clearly separate from the command structure
        if (cleaned.Contains("http") && !cleaned.StartsWith("git clone", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"https?://[^\s]+", "").Trim();
        }

        // Clean up edge cases where commands might have extra quotes or backticks
        if (cleaned.StartsWith("`") && cleaned.EndsWith("`") && cleaned.Count(c => c == '`') == 2)
        {
            cleaned = cleaned.Trim('`');
        }

        if (cleaned.StartsWith("\"") && cleaned.EndsWith("\"") && cleaned.Count(c => c == '"') == 2)
        {
            cleaned = cleaned.Trim('"');
        }

        return cleaned.Trim();
    }

    private static List<string> GetCommandVariations(string searchTerm)
    {
        var variations = new List<string>();
        var trimmed = searchTerm.Trim().ToLowerInvariant();

        // Add the original term
        variations.Add(trimmed);

        // For multi-word commands, try different approaches
        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length > 1)
        {
            // Try with hyphens (e.g. "git rm" -> "git-rm")
            variations.Add(string.Join("-", words));

            // Try just the command parts (e.g. "git rm file" -> "git-rm")
            if (words.Length >= 2)
            {
                variations.Add($"{words[0]}-{words[1]}");
            }

            // Try just the first word (fallback to original behavior)
            variations.Add(words[0]);
        }

        return variations.Distinct().ToList();
    }
}