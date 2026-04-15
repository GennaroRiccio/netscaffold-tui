using System.Text;
using NetScaffoldTui.Models;

namespace NetScaffoldTui.Services;

public class ScaffoldingService
{
    public async Task GenerateSolutionAsync(ProjectConfig config)
    {
        var basePath = string.IsNullOrEmpty(config.OutputPath)
            ? Directory.GetCurrentDirectory()
            : config.OutputPath;
        
        var solutionPath = Path.Combine(basePath, config.SolutionName);
        Directory.CreateDirectory(solutionPath);
        
        var srcPath = Path.Combine(solutionPath, "src");
        var testsPath = Path.Combine(solutionPath, "tests");
        
        Directory.CreateDirectory(srcPath);
        Directory.CreateDirectory(testsPath);
        
        var slnContent = GenerateSolutionFile(config.SolutionName);
        await File.WriteAllTextAsync(Path.Combine(basePath, $"{config.SolutionName}.sln"), slnContent);
        
        GenerateProject(srcPath, testsPath, config);
        
        GenerateBuildProps(basePath, config);
        
        GenerateGitIgnore(solutionPath);
        
        if (config.FeatureToggles["Serilog"])
            GenerateSerilogConfig(srcPath, config);
        
        GenerateDockerfile(solutionPath, config);
        GenerateGitHubWorkflow(solutionPath, config);
        
        GenerateAppsettings(srcPath, config);
        GenerateLaunchSettings(srcPath, config);
    }

    private string GenerateSolutionFile(string solutionName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");
        sb.AppendLine("VisualStudioVersion = 17.0.31903.59");
        sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
        return sb.ToString();
    }

    private void GenerateProject(string srcPath, string testsPath, ProjectConfig config)
    {
        var projects = new List<(string name, string layer)>
        {
            ( $"{config.SolutionName}.Domain", "Domain"),
            ( $"{config.SolutionName}.Application", "Application"),
            ( $"{config.SolutionName}.Infrastructure", "Infrastructure")
        };
        
        switch (config.ProjectType)
        {
            case ProjectType.WebApi:
                projects.Add(($"{config.SolutionName}.Api", "Api"));
                break;
            case ProjectType.Worker:
                projects.Add(($"{config.SolutionName}.Worker", "Worker"));
                break;
            case ProjectType.Console:
                projects.Add(($"{config.SolutionName}.Console", "Console"));
                break;
        }
        
        foreach (var (name, layer) in projects)
        {
            var projectPath = Path.Combine(srcPath, name);
            Directory.CreateDirectory(projectPath);
            GenerateCsproj(projectPath, name, config, layer);
            GenerateFolderStructure(projectPath, name, layer, config);
        }
        
        var testProjects = new List<string>
        {
            $"{config.SolutionName}.Domain.Tests",
            $"{config.SolutionName}.Application.Tests"
        };
        
        foreach (var testProject in testProjects)
        {
            var projectPath = Path.Combine(testsPath, testProject);
            Directory.CreateDirectory(projectPath);
            GenerateTestCsproj(projectPath, testProject, config);
        }
    }

    private void GenerateCsproj(string path, string name, ProjectConfig config, string layer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        sb.AppendLine("  <PropertyGroup>");
        
        var targetFramework = layer == "Api" || config.ProjectType == ProjectType.WebApi
            ? "<TargetFramework>net10.0</TargetFramework>"
            : "<TargetFramework>net10.0</TargetFramework>";
        
        sb.AppendLine($"    {targetFramework}");
        
        if (layer == "Api")
            sb.AppendLine("    <Nullable>enable</Nullable>");
        
        if (layer == "Domain" || layer == "Application")
            sb.AppendLine("  <ImplicitUsings>enable</ImplicitUsings>");
        else
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        
        if (layer != "Domain")
            sb.AppendLine("  </PropertyGroup>");
        else
            sb.AppendLine("  </PropertyGroup>");
        
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        
        if (layer == "Application")
        {
            sb.AppendLine($"    <ProjectReference Include=\"../{config.SolutionName}.Domain/{config.SolutionName}.Domain.csproj\" />");
        }
        else if (layer == "Infrastructure")
        {
            sb.AppendLine($"    <ProjectReference Include=\"../{config.SolutionName}.Application/{config.SolutionName}.Application.csproj\" />");
            sb.AppendLine($"    <ProjectReference Include=\"../{config.SolutionName}.Domain/{config.SolutionName}.Domain.csproj\" />");
        }
        else if (layer == "Api" || config.ProjectType == ProjectType.WebApi)
        {
            if (config.FeatureToggles["EntityFrameworkCore"])
                sb.AppendLine("    <PackageReference Include=\"Microsoft.EntityFrameworkCore\" Version=\"9.0.0\" />");
        }
        
        if (config.FeatureToggles["Serilog"] && (layer == "Api" || layer == "Console" || layer == "Worker"))
        {
            sb.AppendLine("    <PackageReference Include=\"Serilog\" Version=\"4.0.0\" />");
            sb.AppendLine("    <PackageReference Include=\"Serilog.Sinks.Console\" Version=\"6.0.0\" />");
            sb.AppendLine("    <PackageReference Include=\"Serilog.Sinks.File\" Version=\"6.0.0\" />");
        }
        
        if (config.FeatureToggles["FluentValidation"] && layer == "Application")
        {
            sb.AppendLine("    <PackageReference Include=\"FluentValidation\" Version=\"11.9.0\" />");
        }
        
        if (config.FeatureToggles["Mapster"] && layer != "Domain")
        {
            sb.AppendLine("    <PackageReference Include=\"Mapster\" Version=\"7.4.0\" />");
        }
        
        if (config.ProjectType == ProjectType.WebApi && layer == "Api")
        {
            if (config.FeatureToggles["Swagger"])
            {
                sb.AppendLine("    <PackageReference Include=\"Microsoft.AspNetCore.OpenApi\" Version=\"9.0.0\" />");
                sb.AppendLine("    <PackageReference Include=\"Swashbuckle.AspNetCore\" Version=\"7.1.0\" />");
            }
            
            if (config.FeatureToggles["HealthChecks"])
            {
                sb.AppendLine("    <PackageReference Include=\"AspNetCore.HealthChecks.Uris\" Version=\"9.0.0\" />");
            }
        }
        
        if (config.FeatureToggles["MediatR"] && (layer == "Application" || layer == "Api"))
        {
            sb.AppendLine("    <PackageReference Include=\"MediatR\" Version=\"12.2.0\" />");
        }
        
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("</Project>");
        
        File.WriteAllText(Path.Combine(path, $"{name}.csproj"), sb.ToString());
    }

    private void GenerateTestCsproj(string path, string name, ProjectConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <IsPackable>false</IsPackable>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.11.0\" />");
        sb.AppendLine("    <PackageReference Include=\"xunit\" Version=\"2.8.0\" />");
        sb.AppendLine("    <PackageReference Include=\"xunit.runner.visualstudio\" Version=\"2.8.0\">");
        sb.AppendLine("      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitives</IncludeAssets>");
        sb.AppendLine("      <PrivateAssets>all</PrivateAssets>");
        sb.AppendLine("    </PackageReference>");
        sb.AppendLine("    <PackageReference Include=\"coverlet.collector\" Version=\"6.0.0\">");
        sb.AppendLine("      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitives</IncludeAssets>");
        sb.AppendLine("      <PrivateAssets>all</PrivateAssets>");
        sb.AppendLine("    </PackageReference>");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("</Project>");
        
        File.WriteAllText(Path.Combine(path, $"{name}.csproj"), sb.ToString());
    }

    private void GenerateFolderStructure(string path, string name, string layer, ProjectConfig config)
    {
        var directories = layer switch
        {
            "Domain" => new[] { "Entities", "ValueObjects", "Interfaces" },
            "Application" => new[] { "UseCases", "DTOs", "Interfaces", "Services" },
            "Infrastructure" => new[] { "Data", "Services" },
            "Api" => new[] { "Controllers", "Endpoints", "Middleware" },
            "Worker" => new[] { "Services" },
            "Console" => new[] { "Services" },
            _ => Array.Empty<string>()
        };
        
        foreach (var dir in directories)
        {
            Directory.CreateDirectory(Path.Combine(path, dir));
        }
        
        var entityName = config.SolutionName.Replace(".", "") + "Entity";
        
        if (layer == "Domain")
        {
            var entityContent = $@"namespace {name}.Entities;

public class {entityName}
{{
    public Guid Id {{ get; set; }} = Guid.NewGuid();
    public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;
}}
";
            File.WriteAllText(Path.Combine(path, "Entities", $"{entityName}.cs"), entityContent);
            
            var repoInterface = $"I{config.SolutionName}Repository";
            var repoContent = $@"namespace {name}.Interfaces;

public interface I{config.SolutionName}Repository
{{
    Task<IEnumerable<Entities.{entityName}>> GetAllAsync();
    Task<Entities.{entityName}?> GetByIdAsync(Guid id);
    Task AddAsync(Entities.{entityName} entity);
    Task UpdateAsync(Entities.{entityName} entity);
    Task DeleteAsync(Guid id);
}}
";
            File.WriteAllText(Path.Combine(path, "Interfaces", $"{repoInterface}.cs"), repoContent);
        }
        
        if (layer == "Application")
        {
            var dtoName = config.SolutionName.Replace(".", "") + "Dto";
            var dtoContent = $@"namespace {name}.DTOs;

public record {dtoName}(Guid Id, DateTime CreatedAt);
";
            File.WriteAllText(Path.Combine(path, "DTOs", $"{dtoName}.cs"), dtoContent);
        }
        
        if (layer == "Api")
        {
            GenerateApiProgram(config, path);
        }
    }

    private void GenerateApiProgram(ProjectConfig config, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
        sb.AppendLine();
        
        if (config.FeatureToggles["Serilog"])
        {
            sb.AppendLine("builder.Host.UseSerilog();");
        }
        
        sb.AppendLine("builder.Services.AddEndpointsApiBrowser();");
        
        if (config.FeatureToggles["MediatR"])
        {
            sb.AppendLine("builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));");
        }
        
        if (config.FeatureToggles["Swagger"])
        {
            sb.AppendLine("builder.Services.AddOpenApi();");
        }
        
        if (config.FeatureToggles["HealthChecks"])
        {
            sb.AppendLine("builder.Services.AddHealthChecks()");
            sb.AppendLine("    .AddUrlGroup(new Uri(\"https://google.com\"), name: \"google\");");
        }
        
        sb.AppendLine();
        sb.AppendLine("var app = builder.Build();");
        sb.AppendLine();
        
        if (config.FeatureToggles["Swagger"])
        {
            sb.AppendLine("app.MapOpenApi();");
        }
        
        if (config.FeatureToggles["HealthChecks"])
        {
            sb.AppendLine("app.MapHealthChecks(\"/health\");");
        }
        
        sb.AppendLine("app.MapGet(\"/\", () => \"Hello from NetScaffold TUI!\");");
        sb.AppendLine();
        sb.AppendLine("app.Run();");
        
        File.WriteAllText(Path.Combine(path, "Program.cs"), sb.ToString());
    }

    private void GenerateBuildProps(string basePath, ProjectConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Project>");
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>");
        sb.AppendLine("    <Nullable>enable</Nullable>");
        sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <PackageVersion Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.11.0\" />");
        sb.AppendLine("    <PackageVersion Include=\"xunit\" Version=\"2.8.0\" />");
        sb.AppendLine("    <PackageVersion Include=\"xunit.runner.visualstudio\" Version=\"2.8.0\" />");
        sb.AppendLine("    <PackageVersion Include=\"coverlet.collector\" Version=\"6.0.0\" />");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("</Project>");
        
        File.WriteAllText(Path.Combine(basePath, "Directory.Build.props"), sb.ToString());
    }

    private void GenerateGitIgnore(string path)
    {
        var content = @"bin/
obj/
.vs/
.vscode/
*.user
*.suo
*.userosscache
*.sln.docstates
*.log
.DS_Store
Thumbs.db
";
        File.WriteAllText(Path.Combine(path, ".gitignore"), content);
    }

    private void GenerateDockerfile(string path, ProjectConfig config)
    {
        var projectSuffix = config.ProjectType switch
        {
            ProjectType.WebApi => "Api",
            ProjectType.Worker => "Worker",
            ProjectType.Console => "Console",
            _ => "Api"
        };
        var projectName = $"{config.SolutionName}.{projectSuffix}";
        var baseImage = config.ProjectType == ProjectType.WebApi
            ? "mcr.microsoft.com/dotnet/aspnet:9.0"
            : "mcr.microsoft.com/dotnet/runtime:9.0";

        var sb = new StringBuilder();
        sb.AppendLine("FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build");
        sb.AppendLine("WORKDIR /src");
        sb.AppendLine("COPY *.sln .");
        sb.AppendLine($"COPY src/{projectName}/{projectName}.csproj ./src/{projectName}/");
        sb.AppendLine("RUN dotnet restore");
        sb.AppendLine("COPY . .");
        sb.AppendLine("RUN dotnet build -c Release -o /app/build");
        sb.AppendLine();
        sb.AppendLine("FROM build AS publish");
        sb.AppendLine("RUN dotnet publish -c Release -o /app/publish");
        sb.AppendLine();
        sb.AppendLine($"FROM {baseImage} AS final");
        sb.AppendLine("WORKDIR /app");
        sb.AppendLine("COPY --from=publish /app/publish .");
        if (config.ProjectType == ProjectType.WebApi)
            sb.AppendLine("EXPOSE 8080");
        sb.AppendLine($"ENTRYPOINT [\"dotnet\", \"{projectName}.dll\"]");
        
        File.WriteAllText(Path.Combine(path, "Dockerfile"), sb.ToString());
    }

    private void GenerateGitHubWorkflow(string path, ProjectConfig config)
    {
        var workflowDir = Path.Combine(path, ".github", "workflows");
        Directory.CreateDirectory(workflowDir);
        
        var sb = new StringBuilder();
        sb.AppendLine("name: .NET");
        sb.AppendLine();
        sb.AppendLine("on:");
        sb.AppendLine("  push:");
        sb.AppendLine("    branches: [ main ]");
        sb.AppendLine("  pull_request:");
        sb.AppendLine("    branches: [ main ]");
        sb.AppendLine();
        sb.AppendLine("jobs:");
        sb.AppendLine("  build:");
        sb.AppendLine("    runs-on: ubuntu-latest");
        sb.AppendLine("    steps:");
        sb.AppendLine("    - uses: actions/checkout@v4");
        sb.AppendLine("    - name: Setup .NET");
        sb.AppendLine("      uses: actions/setup-dotnet@v4");
        sb.AppendLine("      with:");
        sb.AppendLine("        dotnet-version: '9.0.x'");
        sb.AppendLine("    - name: Restore");
        sb.AppendLine("      run: dotnet restore");
        sb.AppendLine("    - name: Build");
        sb.AppendLine("      run: dotnet build --no-restore");
        sb.AppendLine("    - name: Test");
        sb.AppendLine("      run: dotnet test --no-build --verbosity normal");
        
        File.WriteAllText(Path.Combine(workflowDir, "dotnet.yml"), sb.ToString());
    }

    private void GenerateAppsettings(string srcPath, ProjectConfig config)
    {
        var projectSuffix = config.ProjectType switch
        {
            ProjectType.WebApi => "Api",
            ProjectType.Worker => "Worker",
            ProjectType.Console => "Console",
            _ => "Api"
        };
        var projectPath = Path.Combine(srcPath, $"{config.SolutionName}.{projectSuffix}");
        
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"Logging\": {");
        sb.AppendLine("    \"LogLevel\": {");
        sb.AppendLine("      \"Default\": \"Information\",");
        sb.AppendLine("      \"Microsoft.AspNetCore\": \"Warning\"");
        sb.AppendLine("    }");
        sb.AppendLine("  },");
        
        if (config.FeatureToggles.Count > 0)
        {
            sb.AppendLine("  \"FeatureToggles\": {");
            foreach (var feature in config.FeatureToggles)
            {
                sb.AppendLine($"    \"{feature.Key}\": {feature.Value.ToString().ToLower()},");
            }
            sb.AppendLine("  }");
        }
        
        sb.AppendLine("}");
        
        File.WriteAllText(Path.Combine(projectPath, "appsettings.json"), sb.ToString());
    }

    private void GenerateLaunchSettings(string srcPath, ProjectConfig config)
    {
        var projectSuffix = config.ProjectType switch
        {
            ProjectType.WebApi => "Api",
            ProjectType.Worker => "Worker",
            ProjectType.Console => "Console",
            _ => "Api"
        };
        var projectPath = Path.Combine(srcPath, $"{config.SolutionName}.{projectSuffix}");
        var propsPath = Path.Combine(projectPath, "Properties");
        Directory.CreateDirectory(propsPath);
        
        var content = @"{
  ""profiles"": {
    ""http"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""applicationUrl"": ""http://localhost:5000"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    }
  }
}
";
        File.WriteAllText(Path.Combine(propsPath, "launchSettings.json"), content);
    }

    private void GenerateSerilogConfig(string srcPath, ProjectConfig config)
    {
        var projectSuffix = config.ProjectType switch
        {
            ProjectType.WebApi => "Api",
            ProjectType.Worker => "Worker",
            ProjectType.Console => "Console",
            _ => "Api"
        };
        var projectPath = Path.Combine(srcPath, $"{config.SolutionName}.{projectSuffix}");
        
        var content = $@"using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.{(config.FeatureToggles["Serilog"] == false ? "Information" : "Debug")}()
    .MinimumLevel.Override(""Microsoft"", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(""logs/{config.SolutionName}.txt"", rollingInterval: RollingInterval.Day)
    .CreateLogger();
";
        
        File.WriteAllText(Path.Combine(projectPath, "SerilogConfiguration.cs"), content);
    }
}