using Pos.Cli.Menus;
using Pos.Cli.Support;
using Pos.Domain.ValueObjects;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Cli;

public class RubricDeletionTests
{
    [Fact]
    public void DeleteRubric_Confirmed_RemovesRubric()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();
        var output = new StringWriter();

        CreateMenu(context, "5\n1\nд\n0\n", output).Run();

        Assert.Empty(context.RubricService.GetRubrics());
        Assert.Contains(
            "Рубрикатор успешно удалён",
            output.ToString());
    }

    [Fact]
    public void DeleteCriterion_Confirmed_RemovesCriterion()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        context.RubricService.AddCriterion(
            rubric.Id,
            "Контрольная работа",
            new Score(10m),
            new Weight(1m));

        var output = new StringWriter();

        CreateMenu(context, "6\n1\n1\nд\n0\n", output).Run();

        Assert.Empty(
            context.RubricService.GetCriteria(rubric.Id));

        Assert.Contains(
            "Критерий успешно удалён",
            output.ToString());
    }

    [Fact]
    public void DeleteRubric_Cancelled_PreservesRubric()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();
        var output = new StringWriter();

        CreateMenu(context, "5\n1\nн\n0\n", output).Run();

        Assert.NotNull(
            context.RubricService.GetRubric(rubric.Id));
    }

    [Fact]
    public void DeleteRubric_WithEvaluations_ThrowsException()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var facade = context.CreateFacade();

        Assert.Throws<InvalidOperationException>(
            () => facade.DeleteRubric(assessment.Rubric.Id));
    }

    [Fact]
    public void DeleteCriterion_WithEvaluations_ThrowsException()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var facade = context.CreateFacade();

        Assert.Throws<InvalidOperationException>(
            () => facade.DeleteCriterion(
                assessment.Rubric.Id,
                assessment.Criterion.Id));
    }

    [Fact]
    public void DeleteCriterion_Cancelled_PreservesCriterion()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        context.RubricService.AddCriterion(
            rubric.Id,
            "Контрольная работа",
            new Score(10m),
            new Weight(1m));

        var output = new StringWriter();

        CreateMenu(context, "6\n1\n1\nн\n0\n", output).Run();

        Assert.Single(
            context.RubricService.GetCriteria(rubric.Id));
    }

    private static RubricMenu CreateMenu(
        ApplicationTestContext context,
        string input,
        StringWriter output)
    {
        return new RubricMenu(
            context.CreateFacade(),
            new ConsoleInput(
                new StringReader(input),
                output),
            output);
    }
}
