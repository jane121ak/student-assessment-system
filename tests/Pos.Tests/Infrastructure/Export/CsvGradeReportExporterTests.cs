using Pos.Application.Reports;
using Pos.Infrastructure.Export;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Export;

public class CsvGradeReportExporterTests
{
    [Fact]
    public void Export_ValidReport_CreatesCsvFile()
    {
        using var directory = new TemporaryDirectory();

        var exporter = new CsvGradeReportExporter();
        var report = CreateReport();
        var path = directory.GetFilePath("report.csv");

        exporter.Export(report, path);

        var lines = File.ReadAllLines(path);

        Assert.Equal(3, lines.Length);
        Assert.Contains("studentFullName", lines[0]);
        Assert.Contains("\"Иван, Иванов\"", lines[1]);
        Assert.Contains("0.8", lines[1]);
    }

    [Fact]
    public void Export_QuotesAndCommas_EscapesFields()
    {
        using var directory = new TemporaryDirectory();

        var exporter = new CsvGradeReportExporter();
        var report = CreateReport();
        var path = directory.GetFilePath("report.csv");

        exporter.Export(report, path);

        var content = File.ReadAllText(path);

        Assert.Contains("\"Иван, Иванов\"", content);
        Assert.Contains("\"Контрольная \"\"1\"\"\"", content);
    }

    [Fact]
    public void Export_AfterRejectedPath_ValidPathSucceeds()
    {
        using var directory = new TemporaryDirectory();

        var exporter = new CsvGradeReportExporter();
        var report = CreateReport();

        Assert.Throws<ArgumentException>(() =>
            exporter.Export(report, " "));

        var path = directory.GetFilePath("report.csv");

        exporter.Export(report, path);

        Assert.True(File.Exists(path));
    }

    private static GradeReport CreateReport()
    {
        return new GradeReport(
            Guid.NewGuid(),
            "Иван, Иванов",
            Guid.NewGuid(),
            new[]
            {
                new CriterionGradeReport(
                    Guid.NewGuid(),
                    "Контрольная \"1\"",
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
