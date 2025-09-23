using System;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Options that control which external sources are queried and how long responses are cached.
/// </summary>
public sealed class CheatSheetSourceOptions
{
    public bool EnableDevHints { get; set; } = true;
    public bool EnableTldr { get; set; } = true;
    public bool EnableCheatSh { get; set; } = true;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(12);
}