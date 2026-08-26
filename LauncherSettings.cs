using System.Text.Json;

namespace RavageLauncher;

internal sealed class LauncherSettings
{
    public string ScumRoot { get; set; } = Defaults.ScumRoot;
    public string ModsFolder { get; set; } = Defaults.ModsFolder(Defaults.ScumRoot);
    public string ExecutablePath { get; set; } = Defaults.ExecutablePath(Defaults.ScumRoot);
    public string GraphicsApi { get; set; } = "DX12";
    public string AdditionalArguments { get; set; } = string.Empty;

    public static LauncherSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new LauncherSettings();

            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<LauncherSettings>(json) ?? new LauncherSettings();
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RavageLauncher",
        "settings.json");
}

internal static class Defaults
{
    public static string ScumRoot
    {
        get
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrWhiteSpace(basePath))
                basePath = @"C:\Program Files (x86)";

            return Path.Combine(basePath, "Steam", "steamapps", "common", "SCUM");
        }
    }

    public static string ModsFolder(string scumRoot) =>
        Path.Combine(scumRoot, "SCUM", "Content", "Paks", "~mods");

    public static string ExecutablePath(string scumRoot) =>
        Path.Combine(scumRoot, "SCUM", "Binaries", "Win64", "SCUM.exe");
}
