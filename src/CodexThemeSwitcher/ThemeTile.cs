namespace CodexThemeSwitcher;

internal sealed class ThemeTile : Panel
{
    private static readonly Color Surface = Color.White;
    private static readonly Color Ink = Color.FromArgb(37, 36, 38);
    private static readonly Color Muted = Color.FromArgb(106, 103, 102);
    private static readonly Color Line = Color.FromArgb(218, 216, 213);
    private static readonly Color Red = Color.FromArgb(158, 47, 46);
    private static readonly Color RedSoft = Color.FromArgb(246, 234, 233);
    private readonly PictureBox _thumbnail = new();
    private readonly Label _name = new();
    private readonly Label _appearance = new();
    private bool _selected;

    public ThemeInfo Theme { get; }
    public event Action<ThemeInfo>? Activated;

    public ThemeTile(ThemeInfo theme)
    {
        Theme = theme;
        Size = new Size(224, 78);
        Margin = new Padding(0, 0, 12, 10);
        BackColor = Surface;
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleName = $"选择主题 {theme.Name}";
        SetStyle(ControlStyles.Selectable, true);

        _thumbnail.Location = new Point(8, 8);
        _thumbnail.Size = new Size(76, 62);
        _thumbnail.BackColor = Color.FromArgb(33, 37, 41);
        _thumbnail.SizeMode = PictureBoxSizeMode.Zoom;
        _thumbnail.Image = LoadThumbnail(theme.HomeImagePath);
        Controls.Add(_thumbnail);

        _name.AutoEllipsis = true;
        _name.Font = new Font("Microsoft YaHei UI", 9.5f, FontStyle.Bold, GraphicsUnit.Point);
        _name.Location = new Point(96, 15);
        _name.Size = new Size(120, 25);
        _name.Text = theme.Name;
        Controls.Add(_name);

        _appearance.AutoEllipsis = true;
        _appearance.ForeColor = Muted;
        _appearance.Location = new Point(96, 43);
        _appearance.Size = new Size(120, 21);
        _appearance.Text = theme.Appearance switch
        {
            "light" => "白天主题",
            "dark" => "黑夜主题",
            _ => "自动主题",
        };
        Controls.Add(_appearance);

        Click += (_, _) => ActivateTheme();
        _thumbnail.Click += (_, _) => ActivateTheme();
        _name.Click += (_, _) => ActivateTheme();
        _appearance.Click += (_, _) => ActivateTheme();
        KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode is Keys.Enter or Keys.Space)
            {
                ActivateTheme();
                eventArgs.Handled = true;
            }
        };
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        BackColor = selected ? RedSoft : Surface;
        _name.ForeColor = selected ? Color.FromArgb(111, 31, 33) : Ink;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);
        using var pen = new Pen(_selected ? Red : Line, _selected ? 2 : 1);
        var inset = _selected ? 1 : 0;
        eventArgs.Graphics.DrawRectangle(pen, inset, inset, Width - 1 - (inset * 2), Height - 1 - (inset * 2));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _thumbnail.Image?.Dispose();
        base.Dispose(disposing);
    }

    private void ActivateTheme()
    {
        Focus();
        Activated?.Invoke(Theme);
    }

    private static Image? LoadThumbnail(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }
        catch
        {
            return null;
        }
    }
}
