using System.Text.Json;

namespace CodexThemeSwitcher;

internal sealed record ThemeInfo(
    string Id,
    string Name,
    string Appearance,
    string DirectoryPath,
    string HomeImagePath,
    string? TaskImagePath,
    string Subtitle,
    string StatusText)
{
    public override string ToString() => Name;
}

internal static class ThemeCatalog
{
    public static IReadOnlyList<ThemeInfo> Load(string themesRoot)
    {
        Directory.CreateDirectory(themesRoot);
        var result = new List<ThemeInfo>();
        foreach (var directory in Directory.EnumerateDirectories(themesRoot).OrderBy(path => path, StringComparer.CurrentCultureIgnoreCase))
        {
            try
            {
                result.Add(Read(directory));
            }
            catch
            {
                // Invalid folders remain visible on disk but do not enter the switchable catalog.
            }
        }
        return result.OrderBy(theme => theme.Appearance == "light" ? 0 : 1).ThenBy(theme => theme.Name).ToArray();
    }

    public static ThemeInfo Read(string directory)
    {
        var root = Path.GetFullPath(directory);
        var themePath = Path.Combine(root, "theme.json");
        if (!File.Exists(themePath)) throw new InvalidDataException("主题目录缺少 theme.json。");

        using var document = JsonDocument.Parse(File.ReadAllText(themePath));
        var json = document.RootElement;
        var id = RequiredString(json, "id");
        var name = RequiredString(json, "name");
        var image = RequiredString(json, "image");
        var appearance = OptionalString(json, "appearance")?.ToLowerInvariant() ?? "auto";
        var taskImage = OptionalString(json, "taskImage");
        var homePath = ResolveChild(root, image);
        var taskPath = string.IsNullOrWhiteSpace(taskImage) ? null : ResolveChild(root, taskImage);
        if (!File.Exists(homePath)) throw new FileNotFoundException("首页图片不存在。", homePath);
        if (taskPath is not null && !File.Exists(taskPath)) throw new FileNotFoundException("任务页图片不存在。", taskPath);

        return new ThemeInfo(
            id,
            name,
            appearance,
            root,
            homePath,
            taskPath,
            OptionalString(json, "brandSubtitle") ?? string.Empty,
            OptionalString(json, "statusText") ?? string.Empty);
    }

    private static string RequiredString(JsonElement element, string name)
    {
        var value = OptionalString(element, name);
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"theme.json 缺少 {name}。");
        return value;
    }

    private static string? OptionalString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static string ResolveChild(string root, string child)
    {
        if (Path.IsPathRooted(child)) throw new InvalidDataException("主题图片必须使用相对路径。");
        var full = Path.GetFullPath(Path.Combine(root, child));
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("主题图片路径越过主题目录。");
        return full;
    }
}
