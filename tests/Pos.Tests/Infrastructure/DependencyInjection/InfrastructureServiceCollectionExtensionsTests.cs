using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions.Export;
using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Infrastructure.DependencyInjection;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.DependencyInjection;

public class InfrastructureServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPosInfrastructure_RegistersRepositoriesAndExporters()
    {
        using var directory = new TemporaryDirectory();

        var services = new ServiceCollection();

        services.AddSingleton<EvaluationFactory>();
        services.AddPosInfrastructure(directory.DirectoryPath);

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.IsType<JsonStudentRepository>(
            provider.GetRequiredService<IStudentRepository>());

        Assert.IsType<JsonRubricRepository>(
            provider.GetRequiredService<IRubricRepository>());

        Assert.IsType<JsonEvaluationRepository>(
            provider.GetRequiredService<IEvaluationRepository>());

        var exporters = provider
            .GetServices<IGradeReportExporter>()
            .ToDictionary(e => e.Format);

        Assert.Equal(2, exporters.Count);
        Assert.IsType<CsvGradeReportExporter>(exporters["csv"]);
        Assert.IsType<JsonGradeReportExporter>(exporters["json"]);
    }

    [Fact]
    public void AddPosInfrastructure_UsesProvidedDataDirectory()
    {
        using var directory = new TemporaryDirectory();

        var services = new ServiceCollection();

        services.AddSingleton<EvaluationFactory>();
        services.AddPosInfrastructure(directory.DirectoryPath);

        using var provider = services.BuildServiceProvider();

        var repository = provider
            .GetRequiredService<IStudentRepository>();

        repository.Add(new Student(
            Guid.NewGuid(),
            "Иван Иванов"));

        Assert.True(File.Exists(
            Path.Combine(directory.DirectoryPath, "students.json")));
    }

    [Fact]
    public void AddPosInfrastructure_EmptyDirectory_ThrowsException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() =>
            services.AddPosInfrastructure(" "));
    }
}
