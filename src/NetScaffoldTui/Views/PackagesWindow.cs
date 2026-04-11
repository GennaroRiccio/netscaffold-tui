using Terminal.Gui;
using NetScaffoldTui.Models;

namespace NetScaffoldTui.Views;

public class PackagesWindow : Window
{
    private readonly ListView _availablePackagesListView;
    private readonly List<string> _selectedPackages = new();
    private readonly ProjectConfig _config;
    private readonly Action<ProjectConfig> _onComplete;

    private static readonly List<string> AvailablePackages = new()
    {
        "AutoMapper", "Polly", "YamlDotNet", "Dapper", "NHibernate",
        "MassTransit", "Quartz", "Hangfire", "Redis", "MongoDB.Driver",
        "Azure.Storage.Blobs", "AWS.SDK.S3", "SendGrid", "MailKit",
        "GraphQL", "Newtonsoft.Json"
    };

    public PackagesWindow(ProjectConfig config, Action<ProjectConfig> onComplete)
    {
        _config = config;
        _onComplete = onComplete;
        Title = "📦 Additional NuGet Packages";
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = CreateColorScheme();

        var label = new Label("Select additional packages (Space to toggle):") { X = 2, Y = 2 };
        Add(label);

        _availablePackagesListView = new ListView(AvailablePackages)
        {
            X = 2, Y = 4, Width = 30, Height = 15, AllowsMarking = true
        };
        Add(_availablePackagesListView);

        var btnGenerate = new Button("Generate Solution >") { X = 2, Y = 20 };
        btnGenerate.Clicked += () =>
        {
            _config.AdditionalPackages = new List<string>(_selectedPackages);
            Application.RequestStop();
            _onComplete(_config);
        };
        Add(btnGenerate);

        var btnBack = new Button("< Back") { X = 2, Y = 21 };
        btnBack.Clicked += () =>
        {
            Application.RequestStop();
        };
        Add(btnBack);
    }

    private static ColorScheme CreateColorScheme()
    {
        return new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
            Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
        };
    }
}