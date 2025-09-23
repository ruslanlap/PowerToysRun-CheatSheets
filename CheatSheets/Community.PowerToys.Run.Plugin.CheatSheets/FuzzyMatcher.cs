using System;
using System.Linq;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Fuzzy string matching utility for better command discovery
/// </summary>
public static class FuzzyMatcher
{
    /// <summary>
    /// Calculate fuzzy match score between search term and target string
    /// Returns 0-100 where higher is better match
    /// </summary>
    public static int CalculateFuzzyScore(string searchTerm, string target)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(target))
            return 0;

        var search = searchTerm.ToLowerInvariant().Trim();
        var text = target.ToLowerInvariant();

        // Exact match gets highest score
        if (text.Equals(search))
            return 100;

        // Starts with search term
        if (text.StartsWith(search))
            return 90;

        // Contains exact search term
        if (text.Contains(search))
            return 80;

        // Calculate character-level fuzzy matching
        var fuzzyScore = CalculateLevenshteinSimilarity(search, text);

        // Bonus for word boundary matches
        var wordBoundaryScore = CalculateWordBoundaryScore(search, text);

        // Character sequence matching (for typos)
        var sequenceScore = CalculateSequenceScore(search, text);

        return Math.Max(Math.Max(fuzzyScore, wordBoundaryScore), sequenceScore);
    }

    /// <summary>
    /// Check if search term fuzzy matches target with minimum threshold
    /// </summary>
    public static bool IsFuzzyMatch(string searchTerm, string target, int minScore = 30)
    {
        return CalculateFuzzyScore(searchTerm, target) >= minScore;
    }

    private static int CalculateLevenshteinSimilarity(string search, string text)
    {
        if (search.Length == 0) return 0;
        if (text.Length == 0) return 0;

        var distance = CalculateLevenshteinDistance(search, text);
        var maxLength = Math.Max(search.Length, text.Length);
        var similarity = (1.0 - (double)distance / maxLength) * 100;

        return (int)Math.Max(0, similarity);
    }

    private static int CalculateLevenshteinDistance(string s1, string s2)
    {
        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;

        for (var j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= s1.Length; i++)
        {
            for (var j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }

    private static int CalculateWordBoundaryScore(string search, string text)
    {
        var words = text.Split(new char[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
        var maxScore = 0;

        foreach (var word in words)
        {
            if (word.StartsWith(search))
                maxScore = Math.Max(maxScore, 75);
            else if (word.Contains(search))
                maxScore = Math.Max(maxScore, 60);
        }

        return maxScore;
    }

    private static int CalculateSequenceScore(string search, string text)
    {
        if (search.Length < 2) return 0;

        var searchChars = search.ToCharArray();
        var textIndex = 0;
        var matchedChars = 0;

        foreach (var ch in searchChars)
        {
            var found = false;
            for (var i = textIndex; i < text.Length; i++)
            {
                if (text[i] == ch)
                {
                    matchedChars++;
                    textIndex = i + 1;
                    found = true;
                    break;
                }
            }
            if (!found) break;
        }

        var sequenceScore = (matchedChars * 100) / search.Length;

        // Bonus for consecutive character matches
        if (matchedChars == search.Length)
            sequenceScore += 20;

        return Math.Min(sequenceScore, 75); // Cap at 75 to prioritize exact matches
    }
}