using Terminal.Gui;
using NetScaffoldTui.Models;

namespace NetScaffoldTui.Views;

public class ProjectTypeWindow : Window
{
    private readonly ProjectConfig _config;
    private readonly Action<ProjectConfig> _onComplete;

    public ProjectTypeWindow(ProjectConfig config, Action<ProjectConfig> onComplete)
    {
        _config = config;
        _onComplete = onComplete;
        Title = "◉ Select Project Type";
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = CreateColorScheme();

        var label = new Label("▸ Choose your project type:") { X = 2, Y = 2 };
        Add(label);

        var radioGroup = new RadioGroup(new NStack.ustring[] { "Console Application", "Web API", "Worker Service" })
        {
            X = 2,
            Y = 4,
            SelectedItem = (int)config.ProjectType
        };
        Add(radioGroup);

        var radioApi = new RadioGroup(new NStack.ustring[] { "Minimal APIs", "Controller-based" })
        {
            X = 4,
            Y = 8,
            SelectedItem = config.UseMinimalApis ? 0 : 1
        };
        radioApi.Visible = config.ProjectType == ProjectType.WebApi;
        Add(radioApi);

        radioGroup.SelectedItemChanged += (args) =>
        {
            radioApi.Visible = args.SelectedItem == 1;
        };

        var btnNext = new Button("Next >") { X = 20, Y = 12 };
        btnNext.Clicked += () =>
        {
            _config.ProjectType = (ProjectType)radioGroup.SelectedItem;
            _config.UseMinimalApis = radioApi.SelectedItem == 0;
            Application.RequestStop();
            _onComplete(_config);
        };
        Add(btnNext);

        var btnBack = new Button("< Back") { X = 2, Y = 12 };
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