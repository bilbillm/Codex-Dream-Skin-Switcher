using System.Text.Json;

namespace CodexThemeSwitcher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var appRoot = Path.GetFullPath(AppContext.BaseDirectory);
        if (args.Any(arg => string.Equals(arg, "--self-test", StringComparison.OrdinalIgnoreCase)))
        {
            var themes = ThemeCatalog.Load(Path.Combine(appRoot, "themes"));
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                pass = themes.Count >= 2,
                root = appRoot,
                count = themes.Count,
                themes = themes.Select(theme => new { theme.Id, theme.Name, theme.Appearance })
            }));
            Environment.ExitCode = themes.Count >= 2 ? 0 : 1;
            return;
        }

        ApplicationConfiguration.Initialize();
        var launchMode = args.Any(arg => string.Equals(arg, "--launch", StringComparison.OrdinalIgnoreCase));
        Application.Run(launchMode ? new LauncherForm(appRoot) : new MainForm(appRoot));
    }
}
