using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RavageLauncher;

internal sealed class MainForm : Form
{
    private const string ModArchiveUrl = "https://github.com/TheArchivis7/RavageMODS/releases/latest/download/mods.zip";

    private readonly TextBox _scumRoot = new();
    private readonly TextBox _modsFolder = new();
    private readonly TextBox _exePath = new();
    private readonly ComboBox _renderer = new();
    private readonly TextBox _arguments = new();
    private readonly Button _detectButton = new();
    private readonly Button _browseRootButton = new();
    private readonly Button _browseModsButton = new();
    private readonly Button _browseExeButton = new();
    private readonly Button _playButton = new();
    private readonly ProgressBar _progress = new();
    private readonly Label _status = new();
    private readonly RichTextBox _log = new();

    private LauncherSettings _settings = new();
    private string _lastRoot = string.Empty;
    private Process? _gameProcess;
    private bool _busy;

    private static readonly HttpClient Http = CreateHttpClient();

    public MainForm()
    {
        Text = "Ravage Launcher v0.3";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(860, 750);
        Size = new Size(920, 810);
        BackColor = Color.FromArgb(22, 24, 28);
        ForeColor = Color.Gainsboro;
        Font = new Font("Segoe UI", 9.5f);

        BuildUi();
        Load += MainForm_Load;
        Shown += MainForm_Shown;
        FormClosing += MainForm_FormClosing;
    }

    private void BuildUi()
    {
        var title = new Label
        {
            Text = "RAVAGE PVE /* ™ LAUNCHER",
            Font = new Font("Segoe UI Semibold", 23f, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.White,
            Location = new Point(28, 22)
        };

        var subtitle = new Label
        {
            Text = "No noise, no BS.",
            AutoSize = true,
            ForeColor = Color.Silver,
            Location = new Point(31, 66)
        };

        Controls.Add(title);
        Controls.Add(subtitle);

        int top = 110;
        AddPathRow("SCUM installation", _scumRoot, _browseRootButton, "Browse", top);
        top += 76;
        AddPathRow("Mods folder", _modsFolder, _browseModsButton, "Browse", top);
        top += 76;
        AddPathRow("SCUM executable", _exePath, _browseExeButton, "Browse", top);
        top += 76;

        var rendererLabel = MakeLabel("Graphics API", 30, top);
        Controls.Add(rendererLabel);

        _renderer.SetBounds(30, top + 25, 250, 31);
        _renderer.DropDownStyle = ComboBoxStyle.DropDownList;
        _renderer.BackColor = Color.FromArgb(32, 35, 40);
        _renderer.ForeColor = Color.WhiteSmoke;
        _renderer.Items.Add("DirectX 12 (-dx12)");
        _renderer.Items.Add("DirectX 11 (-dx11)");
        Controls.Add(_renderer);

        var requiredArgs = new Label
        {
            Text = "Always enabled:  -nobattleye  -fileopenlog",
            AutoSize = true,
            ForeColor = Color.Silver,
            Location = new Point(310, top + 31)
        };
        Controls.Add(requiredArgs);

        top += 72;

        var argsLabel = MakeLabel("Additional launch arguments (optional)", 30, top);
        Controls.Add(argsLabel);
        _arguments.SetBounds(30, top + 25, 535, 31);
        StyleTextBox(_arguments);
        Controls.Add(_arguments);

        _detectButton.Text = "DETECT SCUM FOLDER";
        _detectButton.SetBounds(710, 134, 160, 33);
        StyleSecondaryButton(_detectButton);
        _detectButton.Click += DetectButton_Click;
        Controls.Add(_detectButton);

        _playButton.Text = "PLAY";
        _playButton.Font = new Font("Segoe UI Semibold", 15f, FontStyle.Bold);
        _playButton.SetBounds(585, top + 19, 285, 50);
        _playButton.BackColor = Color.FromArgb(47, 105, 166);
        _playButton.ForeColor = Color.White;
        _playButton.FlatStyle = FlatStyle.Flat;
        _playButton.FlatAppearance.BorderSize = 0;
        _playButton.Cursor = Cursors.Hand;
        _playButton.Click += PlayButton_Click;
        Controls.Add(_playButton);

        top += 87;
        _progress.SetBounds(30, top, 840, 18);
        Controls.Add(_progress);

        _status.Text = "Ready";
        _status.AutoSize = false;
        _status.SetBounds(30, top + 27, 840, 24);
        _status.ForeColor = Color.LightGray;
        Controls.Add(_status);

        _log.SetBounds(30, top + 57, 840, 115);
        _log.ReadOnly = true;
        _log.BackColor = Color.FromArgb(15, 17, 20);
        _log.ForeColor = Color.FromArgb(205, 210, 216);
        _log.BorderStyle = BorderStyle.FixedSingle;
        _log.Font = new Font("Consolas", 9f);
        Controls.Add(_log);

        _browseRootButton.Click += BrowseRootButton_Click;
        _browseModsButton.Click += BrowseModsButton_Click;
        _browseExeButton.Click += BrowseExeButton_Click;

        _scumRoot.Leave += ScumRoot_Leave;
        _modsFolder.Leave += (_, _) => SaveSettingsFromUi();
        _exePath.Leave += (_, _) => SaveSettingsFromUi();
        _renderer.SelectedIndexChanged += (_, _) => SaveSettingsFromUi();
        _arguments.Leave += (_, _) => SaveSettingsFromUi();
    }

    private void AddPathRow(string caption, TextBox box, Button browse, string browseText, int top)
    {
        Controls.Add(MakeLabel(caption, 30, top));
        box.SetBounds(30, top + 25, 535, 31);
        StyleTextBox(box);
        Controls.Add(box);

        browse.Text = browseText;
        browse.SetBounds(585, top + 24, 110, 33);
        StyleSecondaryButton(browse);
        Controls.Add(browse);
    }

    private static Label MakeLabel(string text, int left, int top) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Color.Gainsboro,
        Location = new Point(left, top)
    };

    private static void StyleTextBox(TextBox box)
    {
        box.BackColor = Color.FromArgb(32, 35, 40);
        box.ForeColor = Color.WhiteSmoke;
        box.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void StyleSecondaryButton(Button button)
    {
        button.BackColor = Color.FromArgb(45, 49, 56);
        button.ForeColor = Color.WhiteSmoke;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.FromArgb(73, 79, 88);
        button.Cursor = Cursors.Hand;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        _settings = LauncherSettings.Load();
        _scumRoot.Text = _settings.ScumRoot;
        _modsFolder.Text = _settings.ModsFolder;
        _exePath.Text = _settings.ExecutablePath;
        _arguments.Text = _settings.AdditionalArguments;
        _renderer.SelectedIndex = string.Equals(_settings.GraphicsApi, "DX11", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        _lastRoot = _scumRoot.Text.Trim();

        Log("Launcher started.");
        Log("The Reverend's sick mind.");
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (IsScumRunning())
        {
            SetStatus("SCUM is already running. Close it before starting Ravage.");
            Log("SCUM process detected. Startup cleanup skipped.");
            return;
        }

        if (Directory.Exists(_modsFolder.Text.Trim()))
        {
            SetStatus("Cleaning stale ~mods folder...");
            Log("Stale ~mods folder found. Removing it before this session.");
            bool cleaned = await DeleteDirectoryWithRetriesAsync(_modsFolder.Text.Trim());
            SetStatus(cleaned ? "Ready" : "Could not remove stale ~mods folder. Check permissions/files in use.");
        }
    }

    private void ScumRoot_Leave(object? sender, EventArgs e)
    {
        string root = _scumRoot.Text.Trim();
        if (!string.Equals(root, _lastRoot, StringComparison.OrdinalIgnoreCase))
        {
            _modsFolder.Text = Defaults.ModsFolder(root);
            _exePath.Text = Defaults.ExecutablePath(root);
            _lastRoot = root;
            Log("SCUM root changed; derived paths updated.");
        }
        SaveSettingsFromUi();
    }

    private void DetectButton_Click(object? sender, EventArgs e)
    {
        SetStatus("Searching Steam libraries...");
        Application.DoEvents();

        string? detected = SteamDetector.DetectScumFolder();
        if (detected is null)
        {
            SetStatus("SCUM installation not found automatically.");
            Log("Detection failed. Use Browse or edit the path manually.");
            MessageBox.Show(
                this,
                "SCUM was not found in the Steam libraries I could detect.\n\nSelect the SCUM folder manually with Browse.",
                "SCUM not found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplyRoot(detected);
        SaveSettingsFromUi();
        SetStatus("SCUM detected.");
        Log($"SCUM detected: {detected}");
    }

    private void BrowseRootButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the SCUM installation folder",
            SelectedPath = Directory.Exists(_scumRoot.Text) ? _scumRoot.Text : string.Empty,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            ApplyRoot(dialog.SelectedPath);
            SaveSettingsFromUi();
        }
    }

    private void BrowseModsButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the ~mods folder location",
            SelectedPath = Directory.Exists(_modsFolder.Text) ? _modsFolder.Text : string.Empty,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _modsFolder.Text = dialog.SelectedPath;
            SaveSettingsFromUi();
        }
    }

    private void BrowseExeButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select SCUM.exe",
            Filter = "SCUM executable (SCUM.exe)|SCUM.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true,
            FileName = "SCUM.exe"
        };

        string currentDir = Path.GetDirectoryName(_exePath.Text.Trim()) ?? string.Empty;
        if (Directory.Exists(currentDir))
            dialog.InitialDirectory = currentDir;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _exePath.Text = dialog.FileName;
            SaveSettingsFromUi();
        }
    }

    private void ApplyRoot(string root)
    {
        root = Path.GetFullPath(root.Trim());
        _scumRoot.Text = root;
        _modsFolder.Text = Defaults.ModsFolder(root);
        _exePath.Text = Defaults.ExecutablePath(root);
        _lastRoot = root;
    }

    private async void PlayButton_Click(object? sender, EventArgs e)
    {
        if (_busy)
            return;

        SaveSettingsFromUi();

        string root = _scumRoot.Text.Trim();
        string mods = _modsFolder.Text.Trim();
        string exe = _exePath.Text.Trim();
        string args = BuildLaunchArguments();

        string? validationError = ValidatePaths(root, mods, exe);
        if (validationError is not null)
        {
            MessageBox.Show(this, validationError, "Cannot start Ravage", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SetStatus(validationError);
            return;
        }

        if (IsScumRunning())
        {
            MessageBox.Show(this, "SCUM is already running. Close it before starting Ravage.", "SCUM already running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetBusy(true);
        string tempDir = Path.Combine(Path.GetTempPath(), "RavageLauncher");
        string zipPath = Path.Combine(tempDir, "mods.zip");

        try
        {
            Log("Preparing a clean Ravage session.");
            SetStatus("Removing existing ~mods...");
            if (!await DeleteDirectoryWithRetriesAsync(mods))
                throw new IOException($"Could not remove existing mods folder: {mods}");

            Directory.CreateDirectory(mods);
            Log($"Created: {mods}");

            Directory.CreateDirectory(tempDir);
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            SetStatus("Downloading latest Ravage modpack...");
            _progress.Style = ProgressBarStyle.Continuous;
            _progress.Value = 0;
            await DownloadFileAsync(ModArchiveUrl, zipPath, new Progress<int>(p => _progress.Value = ClampProgress(p)));
            Log("Mod archive downloaded.");

            SetStatus("Extracting mods...");
            _progress.Style = ProgressBarStyle.Marquee;
            ExtractZipSafely(zipPath, mods);
            NormalizeNestedModsFolder(mods);
            if (!Directory.EnumerateFiles(mods, "*.pak", SearchOption.AllDirectories).Any())
                throw new InvalidDataException("No .pak files were found after extracting mods.zip.");
            Log("Mod archive extracted.");

            try { File.Delete(zipPath); } catch { /* temp cleanup is non-critical */ }

            SetStatus("Launching SCUM...");
            _progress.Style = ProgressBarStyle.Continuous;
            _progress.Value = 100;

            var startInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(exe)!,
                UseShellExecute = false
            };

            _gameProcess = Process.Start(startInfo) ?? throw new InvalidOperationException("Windows did not return a SCUM process.");
            Log($"Launch arguments: {args}");
            Log($"SCUM started (PID {_gameProcess.Id}).");
            SetStatus("SCUM is running. ~mods will be deleted when the game closes.");

            await Task.Run(() => _gameProcess.WaitForExit());
            await WaitForAllScumProcessesToExitAsync();
            Log("SCUM exited.");
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Ravage Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _gameProcess?.Dispose();
            _gameProcess = null;

            SetStatus("Cleaning ~mods...");
            bool cleaned = await DeleteDirectoryWithRetriesAsync(mods);
            if (cleaned)
            {
                Log("SCUM installation returned to clean state.");
                SetStatus("Ready.");
            }
            else
            {
                Log("WARNING: ~mods could not be fully removed.");
                SetStatus("WARNING: Could not delete ~mods. Close any program using those files and restart the launcher.");
            }

            try { if (File.Exists(zipPath)) File.Delete(zipPath); } catch { }
            _progress.Style = ProgressBarStyle.Continuous;
            _progress.Value = 0;
            SetBusy(false);
        }
    }

    private static string? ValidatePaths(string root, string mods, string exe)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return "The SCUM installation folder does not exist.";

        if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
            return "SCUM.exe was not found at the configured executable path.";

        string? modsParent = Path.GetDirectoryName(mods);
        if (string.IsNullOrWhiteSpace(modsParent) || !Directory.Exists(modsParent))
            return "The parent folder for ~mods does not exist. Check the Mods Folder path.";

        return null;
    }

    private static async Task DownloadFileAsync(string url, string destination, IProgress<int> progress)
    {
        using HttpResponseMessage response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long? total = response.Content.Headers.ContentLength;
        using Stream input = await response.Content.ReadAsStreamAsync();
        using FileStream output = new(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

        byte[] buffer = new byte[81920];
        long received = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await output.WriteAsync(buffer, 0, read);
            received += read;
            if (total is > 0)
                progress.Report((int)(received * 100L / total.Value));
        }

        if (new FileInfo(destination).Length == 0)
            throw new InvalidDataException("GitHub returned an empty mod archive.");

        progress.Report(100);
    }

    private static void ExtractZipSafely(string zipPath, string destinationRoot)
    {
        string root = Path.GetFullPath(destinationRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        using ZipArchive archive = ZipFile.OpenRead(zipPath);
        if (archive.Entries.Count == 0)
            throw new InvalidDataException("The downloaded mods.zip is empty.");

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string target = Path.GetFullPath(Path.Combine(root, entry.FullName));
            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Unsafe path found in mod archive: {entry.FullName}");

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(target);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            if (File.Exists(target))
                File.Delete(target);
            entry.ExtractToFile(target);
        }
    }


    private static void NormalizeNestedModsFolder(string modsRoot)
    {
        // Be forgiving if mods.zip was created by zipping the ~mods folder itself.
        string nested = Path.Combine(modsRoot, "~mods");
        if (!Directory.Exists(nested))
            return;

        foreach (string directory in Directory.GetDirectories(nested))
        {
            string destination = Path.Combine(modsRoot, Path.GetFileName(directory));
            if (Directory.Exists(destination))
                Directory.Delete(destination, recursive: true);
            Directory.Move(directory, destination);
        }

        foreach (string file in Directory.GetFiles(nested))
        {
            string destination = Path.Combine(modsRoot, Path.GetFileName(file));
            if (File.Exists(destination))
                File.Delete(destination);
            File.Move(file, destination);
        }

        Directory.Delete(nested, recursive: true);
    }

    private static async Task<bool> DeleteDirectoryWithRetriesAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return true;

        for (int attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return true;
            }
            catch (IOException) when (attempt < 12)
            {
                await Task.Delay(500);
            }
            catch (UnauthorizedAccessException) when (attempt < 12)
            {
                TryClearReadOnlyAttributes(path);
                await Task.Delay(500);
            }
            catch
            {
                return false;
            }
        }

        return !Directory.Exists(path);
    }

    private static void TryClearReadOnlyAttributes(string path)
    {
        try
        {
            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                FileAttributes attrs = File.GetAttributes(file);
                if ((attrs & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
            }
        }
        catch
        {
            // Best-effort only. The next delete attempt will decide whether cleanup succeeds.
        }
    }

    private static bool IsScumRunning()
    {
        try
        {
            Process[] processes = Process.GetProcessesByName("SCUM");
            bool running = processes.Length > 0;
            foreach (Process process in processes)
                process.Dispose();
            return running;
        }
        catch
        {
            return false;
        }
    }

    private static async Task WaitForAllScumProcessesToExitAsync()
    {
        // SCUM.exe normally remains the game process. This extra wait prevents cleanup
        // from firing too early if the executable hands off to another SCUM process.
        for (;;)
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName("SCUM");
            }
            catch
            {
                return;
            }

            if (processes.Length == 0)
                return;

            foreach (Process process in processes)
                process.Dispose();

            await Task.Delay(1000);
        }
    }

    private string BuildLaunchArguments()
    {
        string graphicsArg = _renderer.SelectedIndex == 1 ? "-dx11" : "-dx12";
        string required = $"-nobattleye -fileopenlog {graphicsArg}";
        string additional = _arguments.Text.Trim();

        return string.IsNullOrWhiteSpace(additional)
            ? required
            : $"{required} {additional}";
    }

    private static int ClampProgress(int value)
    {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }

    private void SaveSettingsFromUi()
    {
        _settings.ScumRoot = _scumRoot.Text.Trim();
        _settings.ModsFolder = _modsFolder.Text.Trim();
        _settings.ExecutablePath = _exePath.Text.Trim();
        _settings.GraphicsApi = _renderer.SelectedIndex == 1 ? "DX11" : "DX12";
        _settings.AdditionalArguments = _arguments.Text.Trim();

        try
        {
            _settings.Save();
        }
        catch (Exception ex)
        {
            Log($"Could not save settings: {ex.Message}");
        }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        _playButton.Enabled = !busy;
        _detectButton.Enabled = !busy;
        _browseRootButton.Enabled = !busy;
        _browseModsButton.Enabled = !busy;
        _browseExeButton.Enabled = !busy;
        _scumRoot.ReadOnly = busy;
        _modsFolder.ReadOnly = busy;
        _exePath.ReadOnly = busy;
        _renderer.Enabled = !busy;
        _arguments.ReadOnly = busy;
    }

    private void SetStatus(string text)
    {
        _status.Text = text;
        _status.Refresh();
    }

    private void Log(string text)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] {text}";
        _log.AppendText(line + Environment.NewLine);
        _log.SelectionStart = _log.TextLength;
        _log.ScrollToCaret();
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        SaveSettingsFromUi();

        if (e.CloseReason == CloseReason.UserClosing && _gameProcess is { HasExited: false })
        {
            e.Cancel = true;
            MessageBox.Show(
                this,
                "SCUM is still running. Keep Ravage Launcher open so it can delete ~mods when the game closes.\n\nClose SCUM first, then the launcher will clean up automatically.",
                "SCUM is running",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            WindowState = FormWindowState.Minimized;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RavageLauncher", "0.2"));
        client.Timeout = TimeSpan.FromMinutes(15);
        return client;
    }
}
