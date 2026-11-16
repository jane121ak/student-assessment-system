using Pos.Domain.ValueObjects;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Application.Services;

public class GradeReportServiceTests
{
    [Fact]
    public void CreateReport_ValidEvaluation_ReturnsReport()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var report = context.GradeReportService.CreateReport(
            assessment.Student.Id,
            assessment.Rubric.Id);

        Assert.Equal(assessment.Student.Id, report.StudentId);
        Assert.Equal("Иван Иванов", report.StudentFullName);
        Assert.Equal(assessment.Rubric.Id, report.RubricId);
        Assert.Equal(0.8m, report.FinalGrade!.Value);

        var criterion = Assert.Single(report.Criteria);

        Assert.Equal(8m, criterion.Score!.Value);
        Assert.Equal(10m, criterion.MaxScore);
        Assert.Equal(1m, criterion.Weight);
    }

    [Fact]
    public void CreateReport_IncludesCriteriaWithoutEvaluation()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var secondCriterion = context.RubricService.AddCriterion(
            assessment.Rubric.Id,
            "Домашняя работа",
            new Score(20m),
            new Weight(0.5m));

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var report = context.GradeReportService.CreateReport(
            assessment.Student.Id,
            assessment.Rubric.Id);

        Assert.Equal(2, report.Criteria.Count);

        var ungraded = report.Criteria.Single(
            c => c.CriterionId == secondCriterion.Id);

        Assert.Null(ungraded.Score);
    }

    [Fact]
    public void CreateReport_NoEvaluations_ReturnsNullFinalGrade()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var report = context.GradeReportService.CreateReport(
            assessment.Student.Id,
            assessment.Rubric.Id);

        Assert.Null(report.FinalGrade);
        Assert.Null(Assert.Single(report.Criteria).Score);
    }

    [Fact]
    public void CreateReport_UnknownStudent_ThrowsException()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        Assert.Throws<KeyNotFoundException>(() =>
            context.GradeReportService.CreateReport(
                Guid.NewGuid(),
                assessment.Rubric.Id));
    }

    [Fact]
    public void CreateReport_UnknownRubric_ThrowsException()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        Assert.Throws<KeyNotFoundException>(() =>
            context.GradeReportService.CreateReport(
                assessment.Student.Id,
                Guid.NewGuid()));
    }

    [Fact]
    public void CreateReport_AfterRejectedEvaluation_ReportsAcceptedScore()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            context.EvaluationService.AddEvaluation(
                assessment.Student.Id,
                assessment.Rubric.Id,
                assessment.Criterion.Id,
                new Score(11m)));

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(7m));

        var report = context.GradeReportService.CreateReport(
            assessment.Student.Id,
            assessment.Rubric.Id);

        Assert.Equal(7m, Assert.Single(report.Criteria).Score!.Value);
        Assert.Equal(0.7m, report.FinalGrade!.Value);
    }
}
