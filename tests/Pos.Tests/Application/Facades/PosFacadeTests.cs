using Pos.Application.Abstractions.Export;
using Pos.Application.Reports;
using Pos.Domain.ValueObjects;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Application.Facades;

public class PosFacadeTests
{
    [Fact]
    public void AssessmentWorkflow_UsesFacade()
    {
        var context = new ApplicationTestContext();
        var facade = context.CreateFacade();

        var student = facade.AddStudent("Пётр Петров");
        var rubric = facade.CreateRubric();

        var criterion = facade.AddCriterion(
            rubric.Id,
            "Экзамен",
            new Score(10m),
            new Weight(1m));

        facade.AddEvaluation(
            student.Id,
            rubric.Id,
            criterion.Id,
            new Score(9m));

        var report = facade.GetGradeReport(
            student.Id,
            rubric.Id);

        Assert.Single(facade.GetStudents());
        Assert.Single(facade.GetRubrics());
        Assert.Single(facade.GetCriteria(rubric.Id));
        Assert.Equal(0.9m, report.FinalGrade!.Value);
    }

    [Fact]
    public void ExportGradeReport_UsesMatchingExporter()
    {
        var context = new ApplicationTestContext();
        var exporter = new RecordingExporter("csv");
        var facade = context.CreateFacade(exporter);

        var assessment = context.CreateAssessment();

        facade.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var report = facade.ExportGradeReport(
            assessment.Student.Id,
            assessment.Rubric.Id,
            "CSV",
            "report.csv");

        Assert.Same(report, exporter.ExportedReport);
        Assert.Equal("report.csv", exporter.DestinationPath);
    }

    [Fact]
    public void ExportGradeReport_UnknownFormat_ThrowsException()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var facade = context.CreateFacade(
            new RecordingExporter("csv"));

        Assert.Throws<NotSupportedException>(() =>
            facade.ExportGradeReport(
                assessment.Student.Id,
                assessment.Rubric.Id,
                "xml",
                "report.xml"));
    }

    [Fact]
    public void ExportGradeReport_EmptyPath_ThrowsException()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var facade = context.CreateFacade(
            new RecordingExporter("csv"));

        Assert.Throws<ArgumentException>(() =>
            facade.ExportGradeReport(
                assessment.Student.Id,
                assessment.Rubric.Id,
                "csv",
                " "));
    }

    [Fact]
    public void Constructor_DuplicateExporterFormats_ThrowsException()
    {
        var context = new ApplicationTestContext();

        Assert.Throws<ArgumentException>(() =>
            context.CreateFacade(
                new RecordingExporter("csv"),
                new RecordingExporter("CSV")));
    }

    [Fact]
    public void CancelEvaluation_UpdatesGradeReport()
    {
        var context = new ApplicationTestContext();
        var facade = context.CreateFacade();
        var assessment = context.CreateAssessment();

        var evaluation = facade.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        Assert.Equal(
            0.8m,
            facade.GetGradeReport(
                assessment.Student.Id,
                assessment.Rubric.Id).FinalGrade!.Value);

        facade.CancelEvaluation(evaluation.Id);

        var report = facade.GetGradeReport(
            assessment.Student.Id,
            assessment.Rubric.Id);

        Assert.Null(report.FinalGrade);
        Assert.Null(Assert.Single(report.Criteria).Score);
    }

    private sealed class RecordingExporter : IGradeReportExporter
    {
        public string Format { get; }

        public GradeReport? ExportedReport { get; private set; }

        public string? DestinationPath { get; private set; }

        public RecordingExporter(string format)
        {
            Format = format;
        }

        public void Export(GradeReport report, string destinationPath)
        {
            ExportedReport = report;
            DestinationPath = destinationPath;
        }
    }
}
