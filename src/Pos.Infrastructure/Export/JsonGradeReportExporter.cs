using System.Text.Json;
using Pos.Application.Abstractions.Export;
using Pos.Application.Reports;

namespace Pos.Infrastructure.Export;

public sealed class JsonGradeReportExporter : IGradeReportExporter
{
    public string Format => "json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Export(GradeReport report, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(report);

        var json = JsonSerializer.Serialize(report, Options);

        ReportFileWriter.Write(destinationPath, json);
    }
}
