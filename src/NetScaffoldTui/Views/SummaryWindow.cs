using Terminal.Gui;
using NetScaffoldTui.Models;
using NetScaffoldTui.Services;

namespace NetScaffoldTui.Views;

public class SummaryWindow : Window
{
    private readonly ProjectConfig _config;
    private readonly ScaffoldingService _scaffoldingService;
    private readonly Action _onComplete;

    public SummaryWindow(ProjectConfig config, Action onComplete)
    {
        _config = config;
        _onComplete = onComplete;
        _scaffoldingService = new ScaffoldingService();
        Title = "📋 Summary";
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = CreateColorScheme();

        var label = new Label("Review Configuration") { X = 2, Y = 2 };
        Add(label);

        int y = 4;

        Add(new Label($"Solution Name: {_config.SolutionName}") { X = 2, Y = y++ });
        Add(new Label($"Project Type: {_config.ProjectType}") { X = 2, Y = y++ });
        
        if (_config.ProjectType == ProjectType.WebApi)
            Add(new Label($"API Type: {(_config.UseMinimalApis ? "Minimal APIs" : "Controller-based")}") { X = 2, Y = y++ });
        
        Add(new Label($"Output Path: {(string.IsNullOrEmpty(_config.OutputPath) ? "(current directory)" : _config.OutputPath)}") { X = 2, Y = y++ });

        y++;
        Add(new Label("Features:") { X = 2, Y = y++ });
        foreach (var feature in _config.FeatureToggles)
        {
            Add(new Label($"  {(feature.Value ? "[x]" : "[ ]")} {feature.Key}") { X = 2, Y = y++ });
        }

        if (_config.AdditionalPackages.Count > 0)
        {
            y++;
            Add(new Label("Additional Packages:") { X = 2, Y = y++ });
            foreach (var pkg in _config.AdditionalPackages)
            {
                Add(new Label($"  - {pkg}") { X = 2, Y = y++ });
            }
        }

        y++;
        var btnGenerate = new Button("Generate Solution") { X = 2, Y = y + 2 };
        btnGenerate.Clicked += async () =>
        {
            try
            {
                btnGenerate.Enabled = false;
                await _scaffoldingService.GenerateSolutionAsync(_config);
                MessageBox.Query("Success", $"Solution '{_config.SolutionName}' generated successfully!", "Ok");
                _onComplete();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to generate solution: {ex.Message}", "Ok");
                btnGenerate.Enabled = true;
            }
        };
        Add(btnGenerate);

        var btnBack = new Button("< Back") { X = 20, Y = y + 2 };
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