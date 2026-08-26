using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RavageLauncher;

internal static class SteamDetector
{
    private const string ScumRelativePath = @"steamapps\common\SCUM";

    public static string? DetectScumFolder()
    {
        foreach (string library in GetSteamLibraries().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                string candidate = Path.Combine(library, ScumRelativePath);
                string exe = Defaults.ExecutablePath(candidate);
                if (Directory.Exists(candidate) && File.Exists(exe))
                    return Path.GetFullPath(candidate);
            }
            catch
            {
                // Ignore malformed/inaccessible library entries and continue searching.
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSteamLibraries()
    {
        var roots = new List<string>();

        AddIfPresent(roots, ReadRegistryString(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath"));
        AddIfPresent(roots, ReadRegistryString(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath"));
        AddIfPresent(roots, ReadRegistryString(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath"));

        string defaultSteam = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam");
        AddIfPresent(roots, defaultSteam);

        foreach (string steamRoot in roots.ToArray())
        {
            yield return steamRoot;

            string vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdf))
                continue;

            string text;
            try
            {
                text = File.ReadAllText(vdf);
            }
            catch
            {
                continue;
            }

            foreach (Match match in Regex.Matches(text, "\\\"path\\\"\\s+\\\"(?<path>[^\\\"]+)\\\"", RegexOptions.IgnoreCase))
            {
                string path = match.Groups["path"].Value.Replace(@"\\", @"\");
                if (!string.IsNullOrWhiteSpace(path))
                    yield return path;
            }
        }
    }

    private static string? ReadRegistryString(RegistryKey hive, string subKey, string valueName)
    {
        try
        {
            using RegistryKey? key = hive.OpenSubKey(subKey);
            return key?.GetValue(valueName) as string;
        }
        catch
        {
            return null;
        }
    }

    private static void AddIfPresent(List<string> list, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
            list.Add(path.Replace('/', '\\').TrimEnd('\\'));
    }
}
