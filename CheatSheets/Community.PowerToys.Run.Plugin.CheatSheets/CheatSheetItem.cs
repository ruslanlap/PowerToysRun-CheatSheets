namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Represents a single cheat sheet entry rendered in PowerToys Run.
/// </summary>
public class CheatSheetItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Command { get; set; }
    public string Url { get; set; }
    public string SourceName { get; set; }
    public int Score { get; set; }
}