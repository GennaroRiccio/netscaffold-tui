using Terminal.Gui;
using NetScaffoldTui.Models;
using NetScaffoldTui.Views;

namespace NetScaffoldTui;

public static class Program
{
    private static ProjectConfig _config = new();

    public static void Main(string[] args)
    {
        Application.Init();
        var mainWindow = new MainWindow(_config, ShowConfiguration);
        Application.Run(mainWindow);
        Application.Shutdown();
    }

    private static void ShowConfiguration(ProjectConfig config)
    {
        _config = config;
        var window = new ConfigurationWindow(_config, ShowFeatures);
        Application.Run(window);
    }

    private static void ShowFeatures(ProjectConfig config)
    {
        _config = config;
        var window = new FeaturesWindow(_config, ShowPackages);
        Application.Run(window);
    }

    private static void ShowPackages(ProjectConfig config)
    {
        _config = config;
        var window = new PackagesWindow(_config, ShowSummary);
        Application.Run(window);
    }

    private static void ShowSummary(ProjectConfig config)
    {
        _config = config;
        var window = new SummaryWindow(_config, OnComplete);
        Application.Run(window);
    }

    private static void OnComplete()
    {
        Application.RequestStop();
    }
}

public class MainWindow : Window
{
    private ProjectConfig _config;
    private Action<ProjectConfig> _onNext;

    public MainWindow(ProjectConfig config, Action<ProjectConfig> onNext)
    {
        _config = config;
        _onNext = onNext;
        Title = "⬡ NetScaffold TUI - .NET Solution Generator";
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };

        var menuBar = new MenuBar(new MenuBarItem[]
        {
            new("_File", new MenuItem[] { new("E_xit", "", () => Application.RequestStop()) }),
            new("_Help", new MenuItem[] { new("_About", "", () => MessageBox.Query("About", "NetScaffold TUI v1.0\n.NET Solution Generator", "Ok")) })
        });
        Add(menuBar);

        var welcomeLabel = new Label("⚡ Welcome to NetScaffold TUI!") { X = 2, Y = 2 };
        Add(welcomeLabel);

        var btnStart = new Button("▶ Start") { X = 2, Y = 4 };
        btnStart.Clicked += () =>
        {
            var window = new ProjectTypeWindow(_config, OnProjectTypeComplete);
            Application.Run(window);
        };
        Add(btnStart);
    }

    private void OnProjectTypeComplete(ProjectConfig config)
    {
        _config = config;
        _onNext(config);
    }
}