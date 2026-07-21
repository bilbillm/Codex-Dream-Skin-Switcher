namespace CodexThemeSwitcher;

internal sealed class SettingsForm : Form
{
    private static readonly Color Canvas = Color.FromArgb(246, 247, 248);
    private static readonly Color Ink = Color.FromArgb(37, 36, 38);
    private static readonly Color Muted = Color.FromArgb(106, 103, 102);
    private static readonly Color Line = Color.FromArgb(218, 216, 213);
    private static readonly Color Red = Color.FromArgb(158, 47, 46);
    private readonly RadioButton _minimizeToTray;
    private readonly RadioButton _keepOpen;

    public DashboardSettings Settings => new(_minimizeToTray.Checked);

    public SettingsForm(DashboardSettings settings)
    {
        Text = "控制台设置";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(438, 230);
        BackColor = Canvas;
        ForeColor = Ink;
        Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        var title = new Label
        {
            Text = "启动成功后",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
            Location = new Point(24, 24),
        };
        Controls.Add(title);

        var hint = new Label
        {
            Text = "选择控制台在 Codex 启动完成后的显示方式。",
            AutoSize = true,
            ForeColor = Muted,
            Location = new Point(24, 54),
        };
        Controls.Add(hint);

        _minimizeToTray = new RadioButton
        {
            Text = "最小化到系统托盘",
            AutoSize = true,
            Location = new Point(28, 96),
            Checked = settings.MinimizeToTrayAfterLaunch,
        };
        _keepOpen = new RadioButton
        {
            Text = "保持控制台打开",
            AutoSize = true,
            Location = new Point(28, 129),
            Checked = !settings.MinimizeToTrayAfterLaunch,
        };
        Controls.Add(_minimizeToTray);
        Controls.Add(_keepOpen);

        var cancel = CreateButton("取消", false, 88);
        cancel.DialogResult = DialogResult.Cancel;
        cancel.Location = new Point(230, 171);
        Controls.Add(cancel);

        var save = CreateButton("保存", true, 92);
        save.DialogResult = DialogResult.OK;
        save.Location = new Point(326, 171);
        Controls.Add(save);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static Button CreateButton(string text, bool primary, int width)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Red : Color.White,
            ForeColor = primary ? Color.White : Ink,
            Cursor = Cursors.Hand,
        };
        button.FlatAppearance.BorderColor = primary ? Red : Line;
        button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(111, 31, 33) : Color.FromArgb(247, 246, 245);
        return button;
    }
}
