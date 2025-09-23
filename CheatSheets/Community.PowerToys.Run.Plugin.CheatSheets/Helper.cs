using System;
using System.Diagnostics;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Common helper utilities for the cheat sheet plugin.
/// </summary>
public static class Helper
{
    public static void OpenInBrowser(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Swallow exceptions - PowerToys should not crash because opening a browser failed.
        }
    }
}