using System.Text.Json;

namespace CodexThemeSwitcher;

internal sealed record DashboardSettings(bool MinimizeToTrayAfterLaunch = true);

internal static class DashboardSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CodexDreamSkin",
        "switcher-settings.json");

    public static DashboardSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new DashboardSettings();
            return JsonSerializer.Deserialize<DashboardSettings>(File.ReadAllText(SettingsPath), SerializerOptions) ??
                new DashboardSettings();
        }
        catch
        {
            return new DashboardSettings();
        }
    }

    public static void Save(DashboardSettings settings)
    {
        var directory = Path.GetDirectoryName(SettingsPath) ?? throw new InvalidOperationException("无法确定设置目录。");
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".switcher-settings-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, SerializerOptions), new System.Text.UTF8Encoding(false));
            File.Move(temporaryPath, SettingsPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
