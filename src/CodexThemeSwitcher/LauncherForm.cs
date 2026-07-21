using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace CodexThemeSwitcher;

internal sealed class LauncherForm : Form
{
    private static readonly Color Canvas = Color.FromArgb(246, 247, 248);
    private static readonly Color Ink = Color.FromArgb(37, 36, 38);
    private static readonly Color Muted = Color.FromArgb(106, 103, 102);
    private static readonly Color Red = Color.FromArgb(158, 47, 46);
    private static readonly Color Line = Color.FromArgb(218, 216, 213);

    private readonly string _appRoot;
    private readonly Label _status = new();
    private readonly ProgressBar _progress = new();
    private readonly Button _retryButton;
    private readonly Button _switcherButton;
    private bool _busy;
    private bool _allowClose;

    public LauncherForm(string appRoot)
    {
        _appRoot = appRoot;
        Text = "Codex 自定义主题启动器";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(560, 250);
        BackColor = Canvas;
        ForeColor = Ink;
        Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        var accent = new Panel
        {
            BackColor = Red,
            Location = new Point(0, 0),
            Size = new Size(6, ClientSize.Height),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
        };
        Controls.Add(accent);

        var title = new Label
        {
            Text = "Codex 自定义主题",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 17f, FontStyle.Bold),
            Location = new Point(34, 26),
        };
        Controls.Add(title);

        var theme = new Label
        {
            Text = $"当前主题：{ReadActiveThemeName()}",
            AutoEllipsis = true,
            ForeColor = Muted,
            Location = new Point(36, 70),
            Size = new Size(490, 24),
        };
        Controls.Add(theme);

        _status.Text = "正在连接 Dream Skin 并启动 Codex…";
        _status.ForeColor = Ink;
        _status.AutoEllipsis = true;
        _status.Location = new Point(36, 111);
        _status.Size = new Size(490, 24);
        Controls.Add(_status);

        _progress.Location = new Point(36, 146);
        _progress.Size = new Size(490, 6);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 22;
        Controls.Add(_progress);

        _switcherButton = CreateButton("打开主题切换器", 178);
        _switcherButton.Location = new Point(188, 184);
        _switcherButton.Click += (_, _) => OpenThemeSwitcher();
        Controls.Add(_switcherButton);

        _retryButton = CreateButton("重试", 94, true);
        _retryButton.Location = new Point(432, 184);
        _retryButton.Click += async (_, _) => await LaunchAsync();
        Controls.Add(_retryButton);

        Shown += async (_, _) => await LaunchAsync();
        FormClosing += (_, eventArgs) =>
        {
            if (_busy && !_allowClose) eventArgs.Cancel = true;
        };
    }

    private async Task LaunchAsync()
    {
        if (_busy) return;
        var closeAfterLaunch = false;
        _busy = true;
        _allowClose = false;
        ControlBox = false;
        _retryButton.Enabled = false;
        _switcherButton.Enabled = false;
        _progress.Visible = true;
        _status.ForeColor = Ink;
        _status.Text = "正在连接 Dream Skin 并启动 Codex…";

        try
        {
            if (!await IsThemeSessionReadyAsync())
            {
                var script = Path.Combine(_appRoot, "engine", "scripts", "start-dream-skin.ps1");
                var result = await RunPowerShellAsync(script, "-Port", "9335", "-PromptRestart");
                if (result.ExitCode != 0 && IsDelayedInjectorExit(result.Error, result.Output))
                {
                    _status.Text = "旧热切换服务正在退出，即将自动重试…";
                    await Task.Delay(2500);
                    result = await RunPowerShellAsync(script, "-Port", "9335", "-PromptRestart");
                }
                if (result.ExitCode != 0)
                    throw new InvalidOperationException(LastUsefulLine(result.Error, result.Output));
                if (result.Output.Contains("cancelled", StringComparison.OrdinalIgnoreCase))
                    throw new OperationCanceledException("启动已取消，Codex 未被更改。");
            }
            if (!IsThemeServiceRunning())
                throw new InvalidOperationException("Codex 已启动，但热切换服务未能保持连接。");

            ActivateCodexWindow();
            _progress.Visible = false;
            _status.ForeColor = Color.FromArgb(31, 122, 74);
            _status.Text = "启动完成，Codex 已连接当前自定义主题。";
            await Task.Delay(900);
            closeAfterLaunch = true;
        }
        catch (Exception error)
        {
            _progress.Visible = false;
            _status.ForeColor = Red;
            _status.Text = error.Message;
            MessageBox.Show(this, error.Message, "无法启动 Codex 自定义主题", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _busy = false;
            ControlBox = true;
            _retryButton.Enabled = true;
            _switcherButton.Enabled = File.Exists(Path.Combine(_appRoot, "CodexThemeSwitcher.exe"));
        }

        if (closeAfterLaunch)
        {
            _allowClose = true;
            Close();
        }
    }

    private async Task<(int ExitCode, string Output, string Error)> RunPowerShellAsync(string script, params string[] arguments)
    {
        if (!File.Exists(script)) throw new FileNotFoundException("缺少 Dream Skin 启动脚本。", script);
        var systemPowerShell = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell", "v1.0", "powershell.exe");
        var powerShell = File.Exists(systemPowerShell) ? systemPowerShell : "powershell.exe";
        var start = new ProcessStartInfo(powerShell)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = _appRoot,
        };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-ExecutionPolicy");
        start.ArgumentList.Add("Bypass");
        start.ArgumentList.Add("-File");
        start.ArgumentList.Add(script);
        foreach (var argument in arguments) start.ArgumentList.Add(argument);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("无法启动 PowerShell。");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var exitCode = process.ExitCode;
        var streamTasks = Task.WhenAll(outputTask, errorTask);
        if (await Task.WhenAny(streamTasks, Task.Delay(750)) == streamTasks)
            return (exitCode, await outputTask, await errorTask);

        _ = streamTasks.ContinueWith(task => _ = task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        return (exitCode, string.Empty, string.Empty);
    }

    private string ReadActiveThemeName()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CodexDreamSkin", "active-theme", "theme.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.GetProperty("name").GetString() ?? "自定义主题";
        }
        catch
        {
            return "Dream Skin 默认主题";
        }
    }

    private static bool IsThemeServiceRunning()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CodexDreamSkin", "state.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var processId = document.RootElement.GetProperty("injectorPid").GetInt32();
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> IsThemeSessionReadyAsync()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CodexDreamSkin", "state.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var state = document.RootElement;
            var processId = state.GetProperty("injectorPid").GetInt32();
            var port = state.GetProperty("port").GetInt32();
            var browserId = state.GetProperty("browserId").GetString();
            if (port is < 1 or > 65535 || !Guid.TryParse(browserId, out _)) return false;

            using var process = Process.GetProcessById(processId);
            if (process.HasExited) return false;

            using var handler = new HttpClientHandler { UseProxy = false };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetStringAsync($"http://127.0.0.1:{port}/json/version");
            using var version = JsonDocument.Parse(response);
            var socket = version.RootElement.GetProperty("webSocketDebuggerUrl").GetString();
            if (!Uri.TryCreate(socket, UriKind.Absolute, out var socketUri)) return false;
            return socketUri.Host is "127.0.0.1" or "localhost" &&
                socketUri.AbsolutePath.EndsWith($"/browser/{browserId}", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static void ActivateCodexWindow()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CodexDreamSkin", "state.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var executable = document.RootElement.GetProperty("codexExe").GetString();
            var processName = Path.GetFileNameWithoutExtension(executable);
            if (string.IsNullOrWhiteSpace(processName)) return;

            foreach (var process in Process.GetProcessesByName(processName))
            {
                using (process)
                {
                    if (process.MainWindowHandle == IntPtr.Zero) continue;
                    ShowWindowAsync(process.MainWindowHandle, 9);
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }
        catch { }
    }

    private void OpenThemeSwitcher()
    {
        var path = Path.Combine(_appRoot, "CodexThemeSwitcher.exe");
        if (!File.Exists(path)) return;
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, WorkingDirectory = _appRoot });
    }

    private static string LastUsefulLine(params string[] values) => values
        .SelectMany(value => value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        .Select(line => line.Trim())
        .LastOrDefault(line => line.Length > 0) ?? "未知错误";

    private static bool IsDelayedInjectorExit(params string[] values) => values.Any(value =>
        value.Contains("recorded Dream Skin injector did not stop", StringComparison.OrdinalIgnoreCase));

    private static Button CreateButton(string text, int width, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Red : Color.White,
            ForeColor = primary ? Color.White : Ink,
            Cursor = Cursors.Hand,
        };
        button.FlatAppearance.BorderColor = primary ? Red : Line;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(111, 31, 33) : Color.FromArgb(247, 246, 245);
        return button;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindowAsync(IntPtr window, int command);
}
