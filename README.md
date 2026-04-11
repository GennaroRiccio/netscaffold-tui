# ⬡ NetScaffold TUI

A terminal-based user interface for scaffolding production-ready .NET solutions with Clean Architecture patterns.

## ⬡ Overview

NetScaffold TUI helps .NET developers quickly generate standardized solutions with:

* ⬡ Clean Architecture layers (Domain, Application, Infrastructure, API/Worker/Console)
* ⚙️ Configurable features (Swagger, Serilog, MediatR, Health Checks, etc.)
* 🐳 CI-ready output (Dockerfile, GitHub Actions, .gitignore)
* 📝 Automatic C# naming conventions

## ⬡ Quick Start

```bash
# Clone and run
git clone https://github.com/yourrepo/netscaffold-tui
cd netscaffold-tui
dotnet run
```

## ⬡ Usage

1. **▸ Select Project Type**: Console, Web API, or Worker Service
2. **⚙️ Configure Solution**: Set name and output path
3. **☑ Enable Features**: Toggle Swagger, Serilog, MediatR, Health Checks, FluentValidation, EF Core, Mapster
4. **📦 Add Packages**: Select from curated list or add custom NuGet packages
5. **▶ Generate**: Creates complete solution structure

## ⬡ Architecture

```
SolutionName/
├── src/
│   ├── SolutionName.Domain/         # Entities, Interfaces
│   ├── SolutionName.Application/   # Use Cases, DTOs
│   ├── SolutionName.Infrastructure/ # Data Access
│   ├── SolutionName.Api/         # Controllers, Endpoints
│   └── SolutionName.Tests/       # Unit Tests
└── tests/
    └── SolutionName.*.Tests/
```

## ⬡ Features

| Feature | Default | Description |
|--------|---------|-------------|
| ⬡ Swagger | Enabled | OpenAPI documentation |
| 📝 Serilog | Enabled | Structured logging |
| 🧩 MediatR | Disabled | CQRS pattern |
| ❤️ Health Checks | Enabled | Liveness endpoints |
| ✓ FluentValidation | Enabled | Request validation |
| 🔗 Entity Framework Core | Enabled | ORM support |
| 🔄 Mapster | Enabled | Object mapping |

## ⬡ CI/CD

Generated solutions include:

* 🐳 `Dockerfile` - Multi-stage build
* ⚡ `.github/workflows/dotnet.yml` - GitHub Actions
* 📝 `.gitignore` - .NET defaults
* 📋 `Directory.Build.props` - Centralized packages

## ⬡ Requirements

* ⬡ .NET 10.0 SDK
* ⬡ Terminal.Gui v1.19.0

## ⬡ Author 
* `Gennaro Riccio`

## ⬡ License

MIT
