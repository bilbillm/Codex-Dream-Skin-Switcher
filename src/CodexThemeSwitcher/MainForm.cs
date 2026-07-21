using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace CodexThemeSwitcher;

internal sealed class MainForm : Form
{
    private static readonly Color Canvas = Color.FromArgb(242, 244, 245);
    private static readonly Color Surface = Color.White;
    private static readonly Color Ink = Color.FromArgb(37, 36, 38);
    private static readonly Color Muted = Color.FromArgb(106, 103, 102);
    private static readonly Color Line = Color.FromArgb(218, 216, 213);
    private static readonly Color Red = Color.FromArgb(158, 47, 46);
    private static readonly Color RedSoft = Color.FromArgb(246, 234, 233);
    private static readonly Color Success = Color.FromArgb(31, 122, 74);
    private static readonly Color SuccessSoft = Color.FromArgb(229, 244, 235);

    private readonly string _appRoot;
    private readonly string _themesRoot;
    private readonly ListBox _themeList = new();
    private readonly PictureBox _preview = new();
    private readonly Label _themeName = new();
    private readonly Label _themeMeta = new();
    private readonly Label _themeStatus = new();
    private readonly Label _connection = new();
    private readonly Label _footerStatus = new();
    private readonly Button _homePreviewButton;
    private readonly Button _taskPreviewButton;
    private readonly Button _applyButton;
    private readonly Button _serviceButton;
    private readonly ProgressBar _progress = new();
    private IReadOnlyList<ThemeInfo> _themes = Array.Empty<ThemeInfo>();
    private bool _showTaskPreview;

    public MainForm(string appRoot)
    {
        _appRoot = appRoot;
        _themesRoot = Path.Combine(_appRoot, "themes");
        Text = "Codex 自定义主题切换器";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 650);
        Size = new Size(1180, 760);
        BackColor = Canvas;
        ForeColor = Ink;
        Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        _homePreviewButton = CreateSegmentButton("首页预览", true);
        _taskPreviewButton = CreateSegmentButton("任务页预览", false);
        _homePreviewButton.Click += (_, _) => SetPreviewMode(false);
        _taskPreviewButton.Click += (_, _) => SetPreviewMode(true);

        _applyButton = CreateButton("应用主题", true, 124);
        _applyButton.Click += async (_, _) => await ApplySelectedThemeAsync();
        _serviceButton = CreateButton("启动热切换服务", false, 168);
        _serviceButton.Click += async (_, _) => await StartServiceAsync();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Canvas,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        Controls.Add(root);

        var header = BuildHeader();
        root.Controls.Add(header, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(20, 14, 20, 18),
            BackColor = Canvas,
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.Controls.Add(content, 0, 1);

        content.Controls.Add(BuildCatalogPanel(), 0, 0);
        content.Controls.Add(BuildPreviewPanel(), 1, 0);

        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(20, 0, 20, 0) };
        footer.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Line), 0, 0, footer.Width, 0);
        _footerStatus.Text = "就绪";
        _footerStatus.ForeColor = Muted;
        _footerStatus.AutoSize = true;
        _footerStatus.Location = new Point(20, 12);
        footer.Controls.Add(_footerStatus);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 24;
        _progress.Size = new Size(150, 4);
        _progress.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        _progress.Visible = false;
        footer.Controls.Add(_progress);
        footer.Resize += (_, _) => _progress.Location = new Point(footer.Width - 170, 19);
        root.Controls.Add(footer, 0, 2);

        Shown += (_, _) => RefreshCatalog(true);
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(20, 0, 20, 0) };
        header.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Line), 0, header.Height - 1, header.Width, header.Height - 1);

        var title = new Label
        {
            Text = "Codex 自定义主题",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 16f, FontStyle.Bold),
            ForeColor = Ink,
            Location = new Point(20, 14),
        };
        var subtitle = new Label
        {
            Text = "预览并热切换 Dream Skin 主题",
            AutoSize = true,
            ForeColor = Muted,
            Location = new Point(22, 45),
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        _connection.AutoSize = true;
        _connection.Padding = new Padding(10, 5, 10, 5);
        _connection.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        header.Controls.Add(_connection);
        header.Resize += (_, _) =>
        {
            _connection.Location = new Point(header.Width - _connection.Width - 20, 22);
        };
        return header;
    }

    private Control BuildCatalogPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0, 0, 14, 0), Padding = new Padding(14) };
        panel.Paint += DrawPanelBorder;

        var title = new Label { Text = "主题库", AutoSize = true, Font = new Font(Font.FontFamily, 11f, FontStyle.Bold), Location = new Point(14, 15) };
        panel.Controls.Add(title);

        var toolRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 88,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 0),
        };
        var import = CreateButton("导入主题", false, 112);
        import.Click += (_, _) => ImportTheme();
        var refresh = CreateButton("刷新", false, 72);
        refresh.Click += (_, _) => RefreshCatalog(false);
        var open = CreateButton("打开目录", false, 112);
        open.Click += (_, _) => OpenThemesDirectory();
        toolRow.Controls.Add(import);
        toolRow.Controls.Add(refresh);
        toolRow.Controls.Add(open);
        panel.Controls.Add(toolRow);

        _themeList.BorderStyle = BorderStyle.None;
        _themeList.DrawMode = DrawMode.OwnerDrawFixed;
        _themeList.ItemHeight = 58;
        _themeList.IntegralHeight = false;
        _themeList.BackColor = Surface;
        _themeList.ForeColor = Ink;
        _themeList.Location = new Point(8, 50);
        _themeList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _themeList.Size = new Size(panel.ClientSize.Width - 16, panel.ClientSize.Height - 142);
        _themeList.DrawItem += DrawThemeItem;
        _themeList.SelectedIndexChanged += (_, _) => UpdateSelectedTheme();
        _themeList.DoubleClick += async (_, _) => await ApplySelectedThemeAsync();
        panel.Controls.Add(_themeList);
        panel.Resize += (_, _) => _themeList.Size = new Size(panel.ClientSize.Width - 16, panel.ClientSize.Height - 142);
        return panel;
    }

    private Control BuildPreviewPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(18), Margin = new Padding(0) };
        panel.Paint += DrawPanelBorder;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Surface,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 142));
        panel.Controls.Add(layout);

        var previewTabs = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0),
            Margin = new Padding(0),
        };
        layout.Controls.Add(previewTabs, 0, 0);

        _preview.Dock = DockStyle.Fill;
        _preview.BackColor = Color.FromArgb(28, 31, 34);
        _preview.SizeMode = PictureBoxSizeMode.Zoom;
        _preview.Margin = new Padding(0);
        layout.Controls.Add(_preview, 0, 1);

        var bottom = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(0, 12, 0, 0), Margin = new Padding(0) };
        layout.Controls.Add(bottom, 0, 2);

        _themeName.AutoSize = true;
        _themeName.Font = new Font(Font.FontFamily, 14f, FontStyle.Bold);
        _themeName.Location = new Point(0, 14);
        bottom.Controls.Add(_themeName);
        _themeMeta.AutoSize = true;
        _themeMeta.ForeColor = Muted;
        _themeMeta.Location = new Point(2, 47);
        bottom.Controls.Add(_themeMeta);
        _themeStatus.AutoSize = true;
        _themeStatus.ForeColor = Muted;
        _themeStatus.Location = new Point(2, 72);
        bottom.Controls.Add(_themeStatus);

        var actions = new FlowLayoutPanel
        {
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };
        actions.Controls.Add(_serviceButton);
        actions.Controls.Add(_applyButton);
        bottom.Controls.Add(actions);
        bottom.Resize += (_, _) => actions.Location = new Point(bottom.Width - actions.Width, bottom.Height - actions.Height - 4);

        previewTabs.Controls.Add(_homePreviewButton);
        previewTabs.Controls.Add(_taskPreviewButton);
        return panel;
    }

    private void RefreshCatalog(bool selectActive)
    {
        var selectedId = (_themeList.SelectedItem as ThemeInfo)?.Id;
        _themes = ThemeCatalog.Load(_themesRoot);
        _themeList.BeginUpdate();
        _themeList.Items.Clear();
        foreach (var theme in _themes) _themeList.Items.Add(theme);
        _themeList.EndUpdate();

        if (selectActive) selectedId = ReadActiveThemeId() ?? selectedId;
        var index = selectedId is null ? -1 : _themes.ToList().FindIndex(theme => theme.Id == selectedId);
        _themeList.SelectedIndex = index >= 0 ? index : (_themes.Count > 0 ? 0 : -1);
        RefreshConnectionStatus();
        SetFooter($"已加载 {_themes.Count} 个主题");
    }

    private void UpdateSelectedTheme()
    {
        if (_themeList.SelectedItem is not ThemeInfo theme)
        {
            _themeName.Text = "没有可用主题";
            _themeMeta.Text = "把含 theme.json 的主题文件夹放入 themes 目录。";
            _themeStatus.Text = string.Empty;
            ReplacePreviewImage(null);
            _applyButton.Enabled = false;
            return;
        }

        _applyButton.Enabled = true;
        _themeName.Text = theme.Name;
        var appearance = theme.Appearance switch { "light" => "白天", "dark" => "黑夜", _ => "自动" };
        _themeMeta.Text = $"{appearance}主题  |  {theme.Subtitle}".TrimEnd(' ', '|');
        _themeStatus.Text = theme.StatusText;
        LoadPreview(theme);
    }

    private void SetPreviewMode(bool task)
    {
        _showTaskPreview = task;
        SetSegmentState(_homePreviewButton, !task);
        SetSegmentState(_taskPreviewButton, task);
        if (_themeList.SelectedItem is ThemeInfo theme) LoadPreview(theme);
    }

    private void LoadPreview(ThemeInfo theme)
    {
        var path = _showTaskPreview && theme.TaskImagePath is not null ? theme.TaskImagePath : theme.HomeImagePath;
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var image = Image.FromStream(stream);
            ReplacePreviewImage(new Bitmap(image));
        }
        catch
        {
            ReplacePreviewImage(null);
            SetFooter("当前图片格式无法在预览中显示，但仍可由 Dream Skin 应用。", true);
        }
    }

    private void ReplacePreviewImage(Image? image)
    {
        var previous = _preview.Image;
        _preview.Image = image;
        previous?.Dispose();
    }

    private async Task ApplySelectedThemeAsync()
    {
        if (_themeList.SelectedItem is not ThemeInfo theme) return;
        SetBusy(true, $"正在应用 {theme.Name}…");
        try
        {
            var result = await RunPowerShellAsync(Path.Combine(_appRoot, "switch-theme.ps1"),
                "-ThemeDirectory", theme.DirectoryPath);
            if (result.ExitCode != 0) throw new InvalidOperationException(LastUsefulLine(result.Error, result.Output));
            SetFooter($"已热切换到 {theme.Name}");
            RefreshConnectionStatus();
        }
        catch (Exception error)
        {
            SetFooter("切换失败：" + error.Message, true);
            MessageBox.Show(this, error.Message, "主题切换失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task StartServiceAsync()
    {
        var script = Path.Combine(_appRoot, "engine", "scripts", "start-dream-skin.ps1");
        SetBusy(true, "正在启动热切换服务…");
        try
        {
            var result = await RunPowerShellAsync(script, "-Port", "9335", "-PromptRestart");
            if (result.ExitCode != 0) throw new InvalidOperationException(LastUsefulLine(result.Error, result.Output));
            SetFooter("热切换服务已启动");
            RefreshConnectionStatus();
        }
        catch (Exception error)
        {
            SetFooter("服务启动失败：" + error.Message, true);
            MessageBox.Show(this, error.Message, "无法启动服务", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private void ImportTheme()
    {
        using var dialog = new FolderBrowserDialog { Description = "选择含 theme.json 的主题文件夹", UseDescriptionForTitle = true, ShowNewFolderButton = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var source = ThemeCatalog.Read(dialog.SelectedPath);
            var folderName = SafeDirectoryName(source.Id);
            var destination = ResolveThemeDestination(folderName);
            if (string.Equals(
                Path.GetFullPath(dialog.SelectedPath).TrimEnd(Path.DirectorySeparatorChar),
                destination.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase))
            {
                SetFooter($"{source.Name} 已在主题库中");
                return;
            }
            if (Directory.Exists(destination))
            {
                var overwrite = MessageBox.Show(this, $"主题“{source.Name}”已存在，是否更新？", "更新主题", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (overwrite != DialogResult.Yes) return;
            }
            CopyDirectory(dialog.SelectedPath, destination);
            RefreshCatalog(false);
            _themeList.SelectedIndex = _themes.ToList().FindIndex(theme => theme.Id == source.Id);
            SetFooter($"已导入 {source.Name}");
        }
        catch (Exception error)
        {
            MessageBox.Show(this, error.Message, "导入失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenThemesDirectory()
    {
        Directory.CreateDirectory(_themesRoot);
        Process.Start(new ProcessStartInfo("explorer.exe", _themesRoot) { UseShellExecute = true });
    }

    private void RefreshConnectionStatus()
    {
        var statePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin", "state.json");
        var connected = false;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(statePath));
            var pid = document.RootElement.GetProperty("injectorPid").GetInt32();
            using var process = Process.GetProcessById(pid);
            connected = !process.HasExited;
        }
        catch { }

        _connection.Text = connected ? "热切换服务已连接" : "热切换服务未连接";
        _connection.BackColor = connected ? SuccessSoft : Color.FromArgb(242, 235, 228);
        _connection.ForeColor = connected ? Success : Color.FromArgb(134, 79, 34);
        _connection.Parent?.PerformLayout();
        _connection.Left = (_connection.Parent?.ClientSize.Width ?? Width) - _connection.Width - 20;
    }

    private string? ReadActiveThemeId()
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin", "active-theme", "theme.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.GetProperty("id").GetString();
        }
        catch { return null; }
    }

    private async Task<(int ExitCode, string Output, string Error)> RunPowerShellAsync(string script, params string[] arguments)
    {
        if (!File.Exists(script)) throw new FileNotFoundException("缺少运行脚本。", script);
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

    private void SetBusy(bool busy, string? message)
    {
        _applyButton.Enabled = !busy && _themeList.SelectedItem is ThemeInfo;
        _serviceButton.Enabled = !busy;
        _themeList.Enabled = !busy;
        _progress.Visible = busy;
        if (!string.IsNullOrWhiteSpace(message)) SetFooter(message);
    }

    private void SetFooter(string message, bool error = false)
    {
        _footerStatus.Text = message;
        _footerStatus.ForeColor = error ? Red : Muted;
    }

    private static string LastUsefulLine(params string[] values) => values
        .SelectMany(value => value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        .Select(line => line.Trim())
        .LastOrDefault(line => line.Length > 0) ?? "未知错误";

    private static string SafeDirectoryName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(character => invalid.Contains(character) ? '-' : character).ToArray())
            .Trim()
            .TrimEnd('.', ' ');
        var reservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned is "." or ".." || reservedNames.Contains(cleaned))
            cleaned = "imported-theme";
        return cleaned.Length > 80 ? cleaned[..80] : cleaned;
    }

    private string ResolveThemeDestination(string folderName)
    {
        var root = Path.GetFullPath(_themesRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var destination = Path.GetFullPath(Path.Combine(root, folderName));
        if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("主题目标路径越过 themes 目录。");
        return destination;
    }

    private static void CopyDirectory(string source, string destination)
    {
        var sourceInfo = new DirectoryInfo(source);
        if ((sourceInfo.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("不支持链接目录。");
        Directory.CreateDirectory(destination);
        foreach (var file in sourceInfo.EnumerateFiles())
        {
            if ((file.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("不支持链接文件。");
            file.CopyTo(Path.Combine(destination, file.Name), true);
        }
        foreach (var directory in sourceInfo.EnumerateDirectories())
        {
            CopyDirectory(directory.FullName, Path.Combine(destination, directory.Name));
        }
    }

    private static Button CreateButton(string text, bool primary, int width)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Red : Surface,
            ForeColor = primary ? Color.White : Ink,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0),
            TabStop = true,
        };
        button.FlatAppearance.BorderColor = primary ? Red : Line;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(111, 31, 33) : Color.FromArgb(247, 246, 245);
        return button;
    }

    private static Button CreateSegmentButton(string text, bool selected)
    {
        var button = CreateButton(text, false, 112);
        button.Height = 34;
        SetSegmentState(button, selected);
        return button;
    }

    private static void SetSegmentState(Button button, bool selected)
    {
        button.BackColor = selected ? RedSoft : Surface;
        button.ForeColor = selected ? Color.FromArgb(111, 31, 33) : Muted;
        button.FlatAppearance.BorderColor = selected ? Color.FromArgb(190, 126, 123) : Line;
    }

    private static void DrawPanelBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel panel) return;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Line);
        e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
    }

    private void DrawThemeItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _themeList.Items.Count) return;
        var theme = (ThemeInfo)_themeList.Items[e.Index];
        var selected = (e.State & DrawItemState.Selected) != 0;
        e.Graphics.FillRectangle(new SolidBrush(selected ? RedSoft : Surface), e.Bounds);
        if (selected) e.Graphics.FillRectangle(new SolidBrush(Red), new Rectangle(e.Bounds.Left, e.Bounds.Top + 6, 3, e.Bounds.Height - 12));
        var nameRect = new Rectangle(e.Bounds.Left + 14, e.Bounds.Top + 9, e.Bounds.Width - 26, 23);
        var metaRect = new Rectangle(e.Bounds.Left + 14, e.Bounds.Top + 32, e.Bounds.Width - 26, 19);
        TextRenderer.DrawText(e.Graphics, theme.Name, Font, nameRect, selected ? Color.FromArgb(111, 31, 33) : Ink, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
        var meta = theme.Appearance switch { "light" => "白天主题", "dark" => "黑夜主题", _ => "自动主题" };
        TextRenderer.DrawText(e.Graphics, meta, new Font(Font.FontFamily, 8.5f), metaRect, Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
        if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _preview.Image?.Dispose();
        base.Dispose(disposing);
    }
}
