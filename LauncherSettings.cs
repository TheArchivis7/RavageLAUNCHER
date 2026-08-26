using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

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
            var serializer = new DataContractJsonSerializer(typeof(StoredSettings));

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var stored = serializer.ReadObject(stream) as StoredSettings;
            if (stored is null)
                return new LauncherSettings();

            var settings = new LauncherSettings();

            if (!string.IsNullOrWhiteSpace(stored.ScumRoot))
                settings.ScumRoot = stored.ScumRoot;
            if (!string.IsNullOrWhiteSpace(stored.ModsFolder))
                settings.ModsFolder = stored.ModsFolder;
            if (!string.IsNullOrWhiteSpace(stored.ExecutablePath))
                settings.ExecutablePath = stored.ExecutablePath;
            if (!string.IsNullOrWhiteSpace(stored.GraphicsApi))
                settings.GraphicsApi = stored.GraphicsApi;

            settings.AdditionalArguments = stored.AdditionalArguments ?? string.Empty;
            return settings;
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);

        var stored = new StoredSettings
        {
            ScumRoot = ScumRoot,
            ModsFolder = ModsFolder,
            ExecutablePath = ExecutablePath,
            GraphicsApi = GraphicsApi,
            AdditionalArguments = AdditionalArguments
        };

        var serializer = new DataContractJsonSerializer(typeof(StoredSettings));
        using var stream = new MemoryStream();
        serializer.WriteObject(stream, stored);
        File.WriteAllText(SettingsPath, Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RavageLauncher",
        "settings.json");

    [DataContract]
    private sealed class StoredSettings
    {
        [DataMember]
        public string? ScumRoot { get; set; }

        [DataMember]
        public string? ModsFolder { get; set; }

        [DataMember]
        public string? ExecutablePath { get; set; }

        [DataMember]
        public string? GraphicsApi { get; set; }

        [DataMember]
        public string? AdditionalArguments { get; set; }
    }
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
