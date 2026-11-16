using Pos.Application.Abstractions.Export;
using Pos.Application.Facades;
using Pos.Application.Reports;
using Pos.Cli.Menus;
using Pos.Cli.Support;
using Pos.Domain.ValueObjects;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Cli;

public class ReportMenuTests
{
    [Fact]
    public void Run_ViewReport_DisplaysStudentCriteriaAndFinalGrade()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();
        var facade = context.CreateFacade();

        facade.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var output = new StringWriter();
        var menu = CreateMenu(
            facade,
            "1\n1\n1\n0\n",
            output);

        menu.Run();

        var result = output.ToString();

        Assert.Contains("Иван Иванов", result);
        Assert.Contains("Контрольная работа", result);
        Assert.Contains("8 / 10", result);
        Assert.Contains("80", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Run_ViewReport_WithoutEvaluations_ShowsNotCalculatedMessage()
    {
        var context = new ApplicationTestContext();
        context.CreateAssessment();

        var output = new StringWriter();
        var menu = CreateMenu(
            context.CreateFacade(),
            "1\n1\n1\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "ещё нет выставленных оценок",
            output.ToString());
    }

    [Fact]
    public void Run_WithoutStudents_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();
        var menu = CreateMenu(
            context.CreateFacade(),
            "1\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "Студенты не найдены",
            output.ToString());
    }

    [Fact]
    public void Run_WithoutRubrics_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();
        var menu = CreateMenu(
            context.CreateFacade(),
            "1\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "Рубрикаторы не найдены",
            output.ToString());
    }

    [Theory]
    [InlineData("csv", "1", "report.csv")]
    [InlineData("json", "2", "report.json")]
    public void Run_ExportReport_UsesSelectedExporter(
        string format,
        string formatChoice,
        string destinationPath)
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();
        var exporter = new RecordingExporter(format);
        var facade = context.CreateFacade(exporter);

        facade.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var output = new StringWriter();
        var menu = CreateMenu(
            facade,
            $"2\n1\n1\n{formatChoice}\n{destinationPath}\n0\n",
            output);

        menu.Run();

        Assert.NotNull(exporter.ExportedReport);
        Assert.Equal(0.8m, exporter.ExportedReport!.FinalGrade);
        Assert.Equal(destinationPath, exporter.DestinationPath);
        Assert.Contains(
            "Отчёт успешно экспортирован",
            output.ToString());
    }

    [Fact]
    public void Run_InvalidMenuChoice_Reprompts()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();
        var menu = CreateMenu(
            context.CreateFacade(),
            "abc\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "Введите номер от 0 до 2",
            output.ToString());
    }

    [Fact]
    public void Run_EndOfInput_Returns()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();
        var menu = CreateMenu(
            context.CreateFacade(),
            "",
            output);

        menu.Run();

        Assert.Contains(
            "ОТЧЁТЫ И РЕЗУЛЬТАТЫ",
            output.ToString());
    }

    private static ReportMenu CreateMenu(
        PosFacade facade,
        string input,
        StringWriter output)
    {
        return new ReportMenu(
            facade,
            new ConsoleInput(
                new StringReader(input),
                output),
            output);
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

        public void Export(
            GradeReport report,
            string destinationPath)
        {
            ExportedReport = report;
            DestinationPath = destinationPath;
        }
    }
}
