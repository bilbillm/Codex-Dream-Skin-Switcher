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
                pass = true,
                root = appRoot,
                count = themes.Count,
                themes = themes.Select(theme => new { theme.Id, theme.Name, theme.Appearance })
            }));
            Environment.ExitCode = 0;
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new DashboardForm(appRoot));
    }
}
