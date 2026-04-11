namespace NetScaffoldTui.Models;

public enum ProjectType
{
    Console,
    WebApi,
    Worker
}

public class ProjectConfig
{
    public string SolutionName { get; set; } = "MySolution";
    public ProjectType ProjectType { get; set; } = ProjectType.WebApi;
    public bool UseMinimalApis { get; set; } = true;
    public Dictionary<string, bool> FeatureToggles { get; set; } = new()
    {
        ["Swagger"] = true,
        ["Serilog"] = true,
        ["MediatR"] = false,
        ["HealthChecks"] = true,
        ["FluentValidation"] = true,
        ["EntityFrameworkCore"] = true,
        ["Mapster"] = true
    };
    public List<string> AdditionalPackages { get; set; } = new();
    public string OutputPath { get; set; } = "";
}