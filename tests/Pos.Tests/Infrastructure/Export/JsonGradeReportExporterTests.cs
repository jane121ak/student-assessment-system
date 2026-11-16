using System.Text.Json;
using Pos.Application.Reports;
using Pos.Infrastructure.Export;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Export;

public class JsonGradeReportExporterTests
{
    [Fact]
    public void Export_ValidReport_CreatesJsonFile()
    {
        using var directory = new TemporaryDirectory();

        var exporter = new JsonGradeReportExporter();
        var report = CreateReport();
        var path = directory.GetFilePath("report.json");

        exporter.Export(report, path);

        using var document = JsonDocument.Parse(
            File.ReadAllText(path));

        var root = document.RootElement;

        Assert.Equal(
            report.StudentId.ToString(),
            root.GetProperty("studentId").GetString());

        Assert.Equal(
            "Иван Иванов",
            root.GetProperty("studentFullName").GetString());

        Assert.Equal(
            0.8m,
            root.GetProperty("finalGrade").GetDecimal());
    }

    [Fact]
    public void Export_UnratedCriterion_PreservesNullScore()
    {
        using var directory = new TemporaryDirectory();

        var exporter = new JsonGradeReportExporter();
        var report = CreateReport();
        var path = directory.GetFilePath("report.json");

        exporter.Export(report, path);

        using var document = JsonDocument.Parse(
            File.ReadAllText(path));

        var criteria = document.RootElement
            .GetProperty("criteria");

        Assert.Equal(2, criteria.GetArrayLength());

        Assert.Equal(
            JsonValueKind.Null,
            criteria[1].GetProperty("score").ValueKind);
    }

    [Fact]
    public void Export_AfterRejectedPath_ValidPathSucceeds()
    {
        using var directory = new TemporaryDirectory();

        var exporter = new JsonGradeReportExporter();
        var report = CreateReport();

        Assert.Throws<ArgumentException>(() =>
            exporter.Export(report, " "));

        var path = directory.GetFilePath("report.json");

        exporter.Export(report, path);

        Assert.True(File.Exists(path));

        using var document = JsonDocument.Parse(
            File.ReadAllText(path));

        Assert.Equal(
            report.StudentId.ToString(),
            document.RootElement
                .GetProperty("studentId")
                .GetString());
    }

    private static GradeReport CreateReport()
    {
        return new GradeReport(
            Guid.NewGuid(),
            "Иван Иванов",
            Guid.NewGuid(),
            new[]
            {
                new CriterionGradeReport(
                    Guid.NewGuid(),
                    "Контрольная работа",
                    10m,
                    0.5m,
                    8m),

                new CriterionGradeReport(
                    Guid.NewGuid(),
                    "Домашняя работа",
                    20m,
                    0.5m,
                    null)
            },
            0.8m);
    }
}
