using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Facades;
using Pos.Application.Services;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Infrastructure.DependencyInjection;

var services = new ServiceCollection();

var dataDirectory = Path.Combine(
    Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData),
    "Pos");

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

Console.WriteLine("POS application initialized.");
Console.WriteLine($"Data directory: {dataDirectory}");
