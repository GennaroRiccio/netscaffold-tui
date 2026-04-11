using Terminal.Gui;
using NetScaffoldTui.Models;

namespace NetScaffoldTui.Views;

public class FeaturesWindow : Window
{
    private readonly List<CheckBox> _checkBoxes = new();
    private readonly ProjectConfig _config;
    private readonly Action<ProjectConfig> _onComplete;

    public FeaturesWindow(ProjectConfig config, Action<ProjectConfig> onComplete)
    {
        _config = config;
        _onComplete = onComplete;
        Title = "☑ Feature Toggles";
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = CreateColorScheme();

        var label = new Label("Enable/Disable Features") { X = 2, Y = 2 };
        Add(label);

        int y = 4;
        foreach (var feature in config.FeatureToggles)
        {
            var checkBox = new CheckBox(feature.Key, feature.Value) { X = 2, Y = y++ };
            _checkBoxes.Add(checkBox);
            Add(checkBox);
        }

        var btnNext = new Button("Next >") { X = 2, Y = y + 2 };
        btnNext.Clicked += () =>
        {
            for (int i = 0; i < _checkBoxes.Count; i++)
            {
                var key = _config.FeatureToggles.Keys.ElementAt(i);
                _config.FeatureToggles[key] = _checkBoxes[i].Checked;
            }
            Application.RequestStop();
            _onComplete(_config);
        };
        Add(btnNext);

        var btnBack = new Button("< Back") { X = 12, Y = y + 2 };
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