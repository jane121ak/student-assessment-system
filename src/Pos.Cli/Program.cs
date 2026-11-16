using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Facades;
using Pos.Application.Services;
using Pos.Cli;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Infrastructure.DependencyInjection;

var services = new ServiceCollection();

var dataDirectory = ResolveDataDirectory();

Directory.CreateDirectory(dataDirectory);

services.AddSingleton<EvaluationFactory>();
services.AddSingleton<GradeCalculator>();

services.AddPosInfrastructure(dataDirectory);

services.AddSingleton<StudentService>();
services.AddSingleton<RubricService>();
services.AddSingleton<EvaluationService>();
services.AddSingleton<GradeReportService>();

services.AddSingleton<PosFacade>();

using var serviceProvider = services.BuildServiceProvider(
    new ServiceProviderOptions
    {
        ValidateOnBuild = true,
        ValidateScopes = true
    });

var facade = serviceProvider.GetRequiredService<PosFacade>();

var application = new ConsoleApplication(
    facade,
    Console.In,
    Console.Out);

application.Run();

static string ResolveDataDirectory()
{
    DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);

    while (current is not null)
    {
        var gitPath = Path.Combine(current.FullName, ".git");

        if (Directory.Exists(gitPath) || File.Exists(gitPath))
            return Path.Combine(current.FullName, "data");

        current = current.Parent;
    }

    return Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "Pos");
}
