using Terminal.Gui;
using NetScaffoldTui.Models;

namespace NetScaffoldTui.Views;

public class ConfigurationWindow : Window
{
    private readonly TextField _solutionNameField;
    private readonly TextField _outputPathField;
    private readonly ProjectConfig _config;
    private readonly Action<ProjectConfig> _onComplete;

    public ConfigurationWindow(ProjectConfig config, Action<ProjectConfig> onComplete)
    {
        _config = config;
        _onComplete = onComplete;
        Title = "⚙ Configure Solution";
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = CreateColorScheme();

        var label = new Label("▸ Solution Configuration") { X = 2, Y = 2 };
        Add(label);

        var nameLabel = new Label("Solution Name:") { X = 2, Y = 4 };
        Add(nameLabel);

        _solutionNameField = new TextField(config.SolutionName) { X = 2, Y = 5, Width = 40 };
        Add(_solutionNameField);

        var outputLabel = new Label("Output Path (empty = current):") { X = 2, Y = 7 };
        Add(outputLabel);

        _outputPathField = new TextField(config.OutputPath) { X = 2, Y = 8, Width = 40 };
        Add(_outputPathField);

        var btnNext = new Button("Next >") { X = 20, Y = 10 };
        btnNext.Clicked += () =>
        {
            var name = _solutionNameField.Text.ToString().Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.ErrorQuery("Error", "Solution name cannot be empty.", "Ok");
                return;
            }
            if (!char.IsLetter(name[0]))
            {
                MessageBox.ErrorQuery("Error", "Solution name must start with a letter.", "Ok");
                return;
            }
            _config.SolutionName = name;
            _config.OutputPath = _outputPathField.Text.ToString().Trim();
            Application.RequestStop();
            _onComplete(_config);
        };
        Add(btnNext);

        var btnBack = new Button("< Back") { X = 2, Y = 10 };
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