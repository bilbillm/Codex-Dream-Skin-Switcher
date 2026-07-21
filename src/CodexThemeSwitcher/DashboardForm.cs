using System.Diagnostics;
using System.Text.Json;

namespace CodexThemeSwitcher;

internal sealed class DashboardForm : Form
{
    private static readonly Color Canvas = Color.FromArgb(246, 247, 248);
    private static readonly Color Surface = Color.White;
    private static readonly Color Ink = Color.FromArgb(37, 36, 38);
    private static readonly Color Muted = Color.FromArgb(106, 103, 102);
    private static readonly Color Line = Color.FromArgb(218, 216, 213);
    private static readonly Color Red = Color.FromArgb(158, 47, 46);
    private static readonly Color Success = Color.FromArgb(31, 122, 74);
    private static readonly Color SuccessSoft = Color.FromArgb(229, 244, 235);
    private readonly string _appRoot;
    private readonly string _themesRoot;
    private readonly FlowLayoutPanel _themeTiles = new();
    private readonly PictureBox _preview = new();
    private readonly Label _themeName = new();
    private readonly Label _themeMeta = new();
    private readonly Label _themeStatus = new();
    private readonly Label _connection = new();
    private readonly Label _footerStatus = new();
    private readonly Button _homePreviewButton;
    private readonly Button _taskPreviewButton;
    private readonly Button _applyButton;
    private readonly Button _launchButton;
    private readonly Button _managementButton;
    private readonly Button _settingsButton;
    private readonly ProgressBar _progress = new();
    private readonly NotifyIcon _notifyIcon = new();
    private readonly ContextMenuStrip _managementMenu = new();
    private IReadOnlyList<ThemeInfo> _themes = Array.Empty<ThemeInfo>();
    private ThemeInfo? _selectedTheme;
    private DashboardSettings _settings;
    private bool _showTaskPreview;
    private bool _busy;
    private bool _allowExit;

    public DashboardForm(string appRoot)
    {
        _appRoot = appRoot;
        _themesRoot = Path.Combine(_appRoot, "themes");
        _settings = DashboardSettingsStore.Load();
        Text = "Codex 自定义主题";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 640);
        Size = new Size(1050, 720);
        BackColor = Canvas;
        ForeColor = Ink;
        Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        _homePreviewButton = CreateButton("首页预览", false, 100, 34);
        _taskPreviewButton = CreateButton("任务页预览", false, 100, 34);
        _homePreviewButton.Click += (_, _) => SetPreviewMode(false);
        _taskPreviewButton.Click += (_, _) => SetPreviewMode(true);

        _applyButton = CreateButton("应用主题", false, 132, 44);
        _applyButton.Click += async (_, _) => await ApplySelectedThemeAsync();
        _launchButton = CreateButton("启动 Codex", true, 166, 44);
        _launchButton.Click += async (_, _) => await LaunchCodexAsync();
        _managementButton = CreateButton("主题管理", false, 100, 34);
        _managementButton.Click += (_, _) => ShowManagementMenu();
        _settingsButton = CreateButton("设置", false, 72, 34);
        _settingsButton.Click += (_, _) => ShowSettings();

        BuildLayout();
        InitializeTray();
        Shown += (_, _) => RefreshCatalog(true);
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Canvas,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Canvas,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20, 16, 20, 16),
        };
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 16));
        content.Controls.Add(BuildPreviewCard(), 0, 0);
        content.Controls.Add(BuildThemeLibraryCard(), 0, 1);
        content.Controls.Add(BuildActionCard(), 0, 2);
        root.Controls.Add(content, 0, 1);
        root.Controls.Add(BuildFooter(), 0, 2);
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(20, 0, 20, 0) };
        header.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Line);
            eventArgs.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        var title = new Label
        {
            Text = "Codex 自定义主题",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 16f, FontStyle.Bold),
            Location = new Point(20, 13),
        };
        var subtitle = new Label
        {
            Text = "选择主题，然后一键启动或热切换 Codex",
            AutoSize = true,
            ForeColor = Muted,
            Location = new Point(22, 46),
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        _connection.AutoSize = true;
        _connection.Padding = new Padding(10, 5, 10, 5);
        _connection.Text = "正在检查服务";

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Padding = new Padding(0),
            Margin = new Padding(0),
        };
        actions.Controls.Add(_connection);
        actions.Controls.Add(_managementButton);
        actions.Controls.Add(_settingsButton);
        header.Controls.Add(actions);
        header.Resize += (_, _) => actions.Location = new Point(header.Width - actions.Width - 20, 22);
        return header;
    }

    private Control BuildPreviewCard()
    {
        var card = CreateCard();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Surface,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(16),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39));
        card.Controls.Add(layout);

        _preview.Dock = DockStyle.Fill;
        _preview.BackColor = Color.FromArgb(30, 33, 36);
        _preview.SizeMode = PictureBoxSizeMode.Zoom;
        _preview.Margin = new Padding(0, 0, 18, 0);
        _preview.AccessibleName = "当前主题预览";
        layout.Controls.Add(_preview, 0, 0);

        var information = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(6, 10, 0, 0) };
        layout.Controls.Add(information, 1, 0);

        var eyebrow = new Label { Text = "当前选择", AutoSize = true, ForeColor = Muted, Location = new Point(6, 10) };
        information.Controls.Add(eyebrow);
        _themeName.AutoEllipsis = true;
        _themeName.Font = new Font(Font.FontFamily, 16f, FontStyle.Bold);
        _themeName.Location = new Point(4, 40);
        _themeName.Size = new Size(300, 34);
        information.Controls.Add(_themeName);
        _themeMeta.AutoEllipsis = true;
        _themeMeta.ForeColor = Muted;
        _themeMeta.Location = new Point(6, 82);
        _themeMeta.Size = new Size(300, 48);
        information.Controls.Add(_themeMeta);
        _themeStatus.AutoEllipsis = true;
        _themeStatus.ForeColor = Muted;
        _themeStatus.Location = new Point(6, 142);
        _themeStatus.Size = new Size(300, 40);
        information.Controls.Add(_themeStatus);

        var previewModes = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Location = new Point(6, 205),
        };
        previewModes.Controls.Add(_homePreviewButton);
        previewModes.Controls.Add(_taskPreviewButton);
        information.Controls.Add(previewModes);
        information.Resize += (_, _) => previewModes.Location = new Point(6, information.Height - previewModes.Height - 8);
        return card;
    }

    private Control BuildThemeLibraryCard()
    {
        var card = CreateCard();
        var title = new Label
        {
            Text = "主题库",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 11f, FontStyle.Bold),
            Location = new Point(16, 14),
        };
        card.Controls.Add(title);
        var hint = new Label
        {
            Text = "选择不会立即影响 Codex，应用或启动时才生效。",
            AutoSize = true,
            ForeColor = Muted,
            Location = new Point(84, 16),
        };
        card.Controls.Add(hint);

        _themeTiles.Dock = DockStyle.Bottom;
        _themeTiles.Height = 96;
        _themeTiles.Padding = new Padding(16, 8, 16, 8);
        _themeTiles.BackColor = Surface;
        _themeTiles.AutoScroll = true;
        _themeTiles.WrapContents = false;
        _themeTiles.FlowDirection = FlowDirection.LeftToRight;
        card.Controls.Add(_themeTiles);
        return card;
    }

    private Control BuildActionCard()
    {
        var card = CreateCard();
        var description = new Label
        {
            Text = "应用主题会立即更新已连接的 Codex；未连接时会保存为下次启动主题。",
            AutoEllipsis = true,
            ForeColor = Muted,
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(16, 17),
            Size = new Size(card.Width - 340, 42),
        };
        card.Controls.Add(description);

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
        };
        actions.Controls.Add(_applyButton);
        actions.Controls.Add(_launchButton);
        card.Controls.Add(actions);
        card.Resize += (_, _) =>
        {
            actions.Location = new Point(card.Width - actions.Width - 16, 18);
            description.Size = new Size(Math.Max(180, actions.Left - 32), 42);
        };
        return card;
    }

    private Control BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(20, 0, 20, 0) };
        footer.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Line);
            eventArgs.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };
        _footerStatus.Text = "准备就绪";
        _footerStatus.ForeColor = Muted;
        _footerStatus.AutoSize = true;
        _footerStatus.Location = new Point(20, 13);
        footer.Controls.Add(_footerStatus);
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 24;
        _progress.Size = new Size(160, 4);
        _progress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _progress.Visible = false;
        footer.Controls.Add(_progress);
        footer.Resize += (_, _) => _progress.Location = new Point(footer.Width - 180, 20);
        return footer;
    }

    private void InitializeTray()
    {
        _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? Application.ExecutablePath) ?? SystemIcons.Application;
        _notifyIcon.Text = "Codex 自定义主题控制台";
        _notifyIcon.Visible = false;
        var menu = new ContextMenuStrip();
        var open = new ToolStripMenuItem("打开控制台");
        open.Click += (_, _) => RestoreFromTray();
        var exit = new ToolStripMenuItem("退出控制台");
        exit.Click += (_, _) => ExitConsole();
        menu.Items.Add(open);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exit);
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => RestoreFromTray();
    }

    private void RefreshCatalog(bool selectActive)
    {
        var selectedId = _selectedTheme?.Id;
        _themes = ThemeCatalog.Load(_themesRoot);
        foreach (Control tile in _themeTiles.Controls) tile.Dispose();
        _themeTiles.Controls.Clear();
        foreach (var theme in _themes)
        {
            var tile = new ThemeTile(theme);
            tile.Activated += SelectTheme;
            _themeTiles.Controls.Add(tile);
        }

        if (selectActive) selectedId = ReadActiveThemeId() ?? selectedId;
        var themeToSelect = _themes.FirstOrDefault(theme => theme.Id == selectedId) ?? _themes.FirstOrDefault();
        if (themeToSelect is not null) SelectTheme(themeToSelect);
        else ClearSelection();
        RefreshConnectionStatus();
        SetFooter(_themes.Count == 0 ? "未找到可用主题。" : $"已加载 {_themes.Count} 个主题");
    }

    private void SelectTheme(ThemeInfo theme)
    {
        _selectedTheme = theme;
        foreach (var tile in _themeTiles.Controls.OfType<ThemeTile>()) tile.SetSelected(tile.Theme.Id == theme.Id);
        _themeName.Text = theme.Name;
        var appearance = theme.Appearance switch { "light" => "白天主题", "dark" => "黑夜主题", _ => "自动主题" };
        _themeMeta.Text = string.IsNullOrWhiteSpace(theme.Subtitle) ? appearance : $"{appearance} · {theme.Subtitle}";
        _themeStatus.Text = string.IsNullOrWhiteSpace(theme.StatusText) ? "选择后点击应用主题或启动 Codex。" : theme.StatusText;
        _applyButton.Enabled = !_busy;
        _launchButton.Enabled = !_busy;
        LoadPreview(theme);
    }

    private void ClearSelection()
    {
        _selectedTheme = null;
        _themeName.Text = "没有可用主题";
        _themeMeta.Text = "把含 theme.json 的主题文件夹放进 themes 目录。";
        _themeStatus.Text = string.Empty;
        ReplacePreviewImage(null);
        _applyButton.Enabled = false;
        _launchButton.Enabled = false;
    }

    private void SetPreviewMode(bool task)
    {
        _showTaskPreview = task;
        SetSegmentState(_homePreviewButton, !task);
        SetSegmentState(_taskPreviewButton, task);
        if (_selectedTheme is not null) LoadPreview(_selectedTheme);
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
            SetFooter("当前图片无法在预览中显示，但仍可由 Dream Skin 应用。", true);
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
        if (_selectedTheme is null) return;
        SetBusy(true, $"正在保存 {_selectedTheme.Name}…");
        try
        {
            await SaveSelectedThemeAsync(_selectedTheme);
            if (await IsThemeSessionReadyAsync())
            {
                SetFooter($"正在热切换到 {_selectedTheme.Name}…");
                await ApplyLiveThemeAsync(_selectedTheme);
                SetFooter($"已热切换到 {_selectedTheme.Name}");
            }
            else
            {
                SetFooter($"已保存 {_selectedTheme.Name}，下次启动 Codex 时会自动应用。");
            }
            RefreshConnectionStatus();
        }
        catch (Exception error)
        {
            ReportError("主题应用失败", error);
        }
        finally
        {
            SetBusy(false, null);
        }
    }

    private async Task LaunchCodexAsync()
    {
        if (_selectedTheme is null) return;
        var minimizeAfterLaunch = false;
        SetBusy(true, $"正在准备 {_selectedTheme.Name}…");
        try
        {
            await SaveSelectedThemeAsync(_selectedTheme);
            if (await IsThemeSessionReadyAsync())
            {
                SetFooter($"正在热切换到 {_selectedTheme.Name}…");
                await ApplyLiveThemeAsync(_selectedTheme);
            }
            else
            {
                SetFooter("正在连接 Dream Skin 并启动 Codex…");
                var script = Path.Combine(_appRoot, "engine", "scripts", "start-dream-skin.ps1");
                var result = await RunPowerShellAsync(script, "-PromptRestart");
                if (result.ExitCode != 0 && IsDelayedInjectorExit(result.Error, result.Output))
                {
                    SetFooter("旧热切换服务正在退出，即将自动重试…");
                    await Task.Delay(2500);
                    result = await RunPowerShellAsync(script, "-PromptRestart");
                }
                if (result.ExitCode != 0) throw new InvalidOperationException(LastUsefulLine(result.Error, result.Output));
                if (result.Output.Contains("cancelled", StringComparison.OrdinalIgnoreCase))
                    throw new OperationCanceledException("启动已取消，Codex 未被更改。");
                if (!IsThemeServiceRunning())
                    throw new InvalidOperationException("Codex 已启动，但热切换服务未能保持连接。");
            }

            ActivateCodexWindow();
            RefreshConnectionStatus();
            SetFooter($"启动完成，Codex 已连接 {_selectedTheme.Name}。");
            minimizeAfterLaunch = _settings.MinimizeToTrayAfterLaunch;
        }
        catch (Exception error)
        {
            ReportError("无法启动 Codex", error);
        }
        finally
        {
            SetBusy(false, null);
        }

        if (minimizeAfterLaunch) MinimizeToTray();
    }

    private async Task SaveSelectedThemeAsync(ThemeInfo theme)
    {
        var result = await RunPowerShellAsync(
            Path.Combine(_appRoot, "switch-theme.ps1"),
            "-ThemeDirectory", theme.DirectoryPath,
            "-SaveOnly");
        if (result.ExitCode != 0) throw new InvalidOperationException(LastUsefulLine(result.Error, result.Output));
    }

    private async Task ApplyLiveThemeAsync(ThemeInfo theme)
    {
        var result = await RunPowerShellAsync(
            Path.Combine(_appRoot, "switch-theme.ps1"),
            "-ThemeDirectory", theme.DirectoryPath);
        if (result.ExitCode != 0) throw new InvalidOperationException(LastUsefulLine(result.Error, result.Output));
    }

    private void ShowManagementMenu()
    {
        if (_busy) return;
        _managementMenu.Items.Clear();
        var import = new ToolStripMenuItem("导入主题");
        import.Click += (_, _) => ImportTheme();
        var refresh = new ToolStripMenuItem("刷新主题库");
        refresh.Click += (_, _) => RefreshCatalog(false);
        var open = new ToolStripMenuItem("打开主题目录");
        open.Click += (_, _) => OpenThemesDirectory();
        _managementMenu.Items.Add(import);
        _managementMenu.Items.Add(refresh);
        _managementMenu.Items.Add(new ToolStripSeparator());
        _managementMenu.Items.Add(open);
        _managementMenu.Show(_managementButton, new Point(0, _managementButton.Height));
    }

    private void ShowSettings()
    {
        if (_busy) return;
        using var dialog = new SettingsForm(_settings);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _settings = dialog.Settings;
        try
        {
            DashboardSettingsStore.Save(_settings);
            SetFooter("设置已保存。");
        }
        catch (Exception error)
        {
            ReportError("无法保存设置", error);
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
            var imported = _themes.FirstOrDefault(theme => theme.Id == source.Id);
            if (imported is not null) SelectTheme(imported);
            SetFooter($"已导入 {source.Name}");
        }
        catch (Exception error)
        {
            ReportError("导入主题失败", error);
        }
    }

    private void OpenThemesDirectory()
    {
        Directory.CreateDirectory(_themesRoot);
        Process.Start(new ProcessStartInfo("explorer.exe", _themesRoot) { UseShellExecute = true });
    }

    private void RefreshConnectionStatus()
    {
        var connected = IsThemeServiceRunning();
        _connection.Text = connected ? "热切换服务已连接" : "等待启动 Codex";
        _connection.BackColor = connected ? SuccessSoft : Color.FromArgb(242, 235, 228);
        _connection.ForeColor = connected ? Success : Color.FromArgb(134, 79, 34);
        _connection.Parent?.PerformLayout();
    }

    private void SetBusy(bool busy, string? message)
    {
        _busy = busy;
        _applyButton.Enabled = !busy && _selectedTheme is not null;
        _launchButton.Enabled = !busy && _selectedTheme is not null;
        _managementButton.Enabled = !busy;
        _settingsButton.Enabled = !busy;
        _themeTiles.Enabled = !busy;
        _progress.Visible = busy;
        if (!string.IsNullOrWhiteSpace(message)) SetFooter(message);
    }

    private void SetFooter(string message, bool error = false)
    {
        _footerStatus.Text = message;
        _footerStatus.ForeColor = error ? Red : Muted;
    }

    private void ReportError(string title, Exception error)
    {
        SetFooter(error.Message, true);
        MessageBox.Show(this, error.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void MinimizeToTray()
    {
        _notifyIcon.Visible = true;
        Hide();
        _notifyIcon.ShowBalloonTip(1800, "Codex 自定义主题", "Codex 已启动。双击此图标可返回控制台。", ToolTipIcon.Info);
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
        _notifyIcon.Visible = false;
    }

    private void ExitConsole()
    {
        _allowExit = true;
        _notifyIcon.Visible = false;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs eventArgs)
    {
        if (_busy && !_allowExit)
        {
            eventArgs.Cancel = true;
            SetFooter("操作进行中，请等待完成后再关闭控制台。", true);
            return;
        }
        _notifyIcon.Visible = false;
        base.OnFormClosing(eventArgs);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _preview.Image?.Dispose();
            _managementMenu.Dispose();
            _notifyIcon.Dispose();
        }
        base.Dispose(disposing);
    }

    private static Panel CreateCard()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0, 0, 0, 12) };
        card.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Line);
            eventArgs.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        return card;
    }

    private static Button CreateButton(string text, bool primary, int width, int height)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = height,
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

    private static void SetSegmentState(Button button, bool selected)
    {
        button.BackColor = selected ? Color.FromArgb(246, 234, 233) : Surface;
        button.ForeColor = selected ? Color.FromArgb(111, 31, 33) : Muted;
        button.FlatAppearance.BorderColor = selected ? Color.FromArgb(190, 126, 123) : Line;
    }

    private static string LastUsefulLine(params string[] values) => values
        .SelectMany(value => value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        .Select(line => line.Trim())
        .LastOrDefault(line => line.Length > 0) ?? "未知错误";

    private static bool IsDelayedInjectorExit(params string[] values) => values.Any(value =>
        value.Contains("recorded Dream Skin injector did not stop", StringComparison.OrdinalIgnoreCase));

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

    private string? ReadActiveThemeId()
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin", "active-theme", "theme.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.GetProperty("id").GetString();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsThemeServiceRunning()
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin", "state.json");
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
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin", "state.json");
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
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin", "state.json");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var executable = document.RootElement.GetProperty("codexExe").GetString();
            var processName = Path.GetFileNameWithoutExtension(executable);
            if (string.IsNullOrWhiteSpace(processName)) return;

            foreach (var process in Process.GetProcessesByName(processName))
            {
                using (process)
                {
                    if (process.MainWindowHandle == IntPtr.Zero) continue;
                    NativeWindow.ShowWindowAsync(process.MainWindowHandle, 9);
                    NativeWindow.SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }
        catch
        {
        }
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
}
