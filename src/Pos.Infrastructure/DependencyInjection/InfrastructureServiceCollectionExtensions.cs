using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Abstractions.Export;
using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Factories;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence.Repositories;

namespace Pos.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPosInfrastructure(
        this IServiceCollection services,
        string dataDirectory)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(dataDirectory))
            throw new ArgumentException(
                "Data directory cannot be empty.",
                nameof(dataDirectory));

        var fullDataDirectory = Path.GetFullPath(dataDirectory);

        services.AddSingleton<IStudentRepository>(_ =>
            new JsonStudentRepository(
                Path.Combine(fullDataDirectory, "students.json")));

        services.AddSingleton<IRubricRepository>(_ =>
            new JsonRubricRepository(
                Path.Combine(fullDataDirectory, "rubrics.json")));

        services.AddSingleton<IEvaluationRepository>(provider =>
            new JsonEvaluationRepository(
                Path.Combine(fullDataDirectory, "evaluations.json"),
                provider.GetRequiredService<IRubricRepository>(),
                provider.GetRequiredService<EvaluationFactory>()));

        services.AddSingleton<IGradeReportExporter,
            CsvGradeReportExporter>();

        services.AddSingleton<IGradeReportExporter,
            JsonGradeReportExporter>();

        return services;
    }
}
