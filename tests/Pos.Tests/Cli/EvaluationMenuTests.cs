using Pos.Cli.Menus;
using Pos.Cli.Support;
using Pos.Domain.ValueObjects;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Cli;

public class EvaluationMenuTests
{
    [Fact]
    public void Run_AddEvaluation_NoStudents_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        CreateMenu(context, "2\n0\n", output).Run();

        Assert.Contains("Студенты не найдены", output.ToString());
    }

    [Fact]
    public void Run_AddEvaluation_NoRubrics_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();

        CreateMenu(context, "2\n1\n0\n", output).Run();

        Assert.Contains("Рубрикаторы не найдены", output.ToString());
    }

    [Fact]
    public void Run_AddEvaluation_NoCriteria_ShowsMessage()
    {
        var context = new ApplicationTestContext();

        context.StudentService.AddStudent("Иван Иванов");
        context.RubricService.CreateRubric();

        var output = new StringWriter();

        CreateMenu(context, "2\n1\n1\n0\n", output).Run();

        Assert.Contains(
            "В выбранном рубрикаторе нет критериев",
            output.ToString());
    }

    [Fact]
    public void Run_AddEvaluation_CreatesEvaluation()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var output = new StringWriter();

        CreateMenu(
            context,
            "2\n1\n1\n1\n8\n0\n",
            output).Run();

        var evaluation = Assert.Single(
            context.EvaluationService.GetEvaluations(
                assessment.Student.Id));

        Assert.Equal(8m, evaluation.Score.Value);

        Assert.Contains(
            "Оценка успешно выставлена",
            output.ToString());
    }

    [Fact]
    public void Run_AddEvaluation_ZeroScore_IsValid()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var output = new StringWriter();

        CreateMenu(
            context,
            "2\n1\n1\n1\n0\n0\n",
            output).Run();

        var evaluation = Assert.Single(
            context.EvaluationService.GetEvaluations(
                assessment.Student.Id));

        Assert.Equal(0m, evaluation.Score.Value);
    }

    [Fact]
    public void Run_AddEvaluation_InvalidScore_Reprompts()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var output = new StringWriter();

        CreateMenu(
            context,
            "2\n1\n1\n1\n11\n7\n0\n",
            output).Run();

        var evaluation = Assert.Single(
            context.EvaluationService.GetEvaluations(
                assessment.Student.Id));

        Assert.Equal(7m, evaluation.Score.Value);

        Assert.Contains(
            "Некорректное число",
            output.ToString());
    }

    [Fact]
    public void Run_AddEvaluation_Duplicate_ShowsError()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(5m));

        var output = new StringWriter();

        CreateMenu(
            context,
            "2\n1\n1\n1\n8\n0\n",
            output).Run();

        Assert.Single(
            context.EvaluationService.GetEvaluations(
                assessment.Student.Id));

        Assert.Contains("Ошибка:", output.ToString());
    }

    [Fact]
    public void Run_ListEvaluations_DisplaysEvaluations()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var output = new StringWriter();

        CreateMenu(context, "1\n1\n0\n", output).Run();

        var result = output.ToString();

        Assert.Contains("Иван Иванов", result);
        Assert.Contains("Контрольная работа", result);
        Assert.Contains("8 / 10", result);
    }

    [Fact]
    public void Run_ListEvaluations_EmptyList_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();

        CreateMenu(context, "1\n1\n0\n", output).Run();

        Assert.Contains("Оценки не найдены", output.ToString());
    }

    [Fact]
    public void Run_ChangeEvaluation_ChangesScore()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        var evaluation = context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(4m));

        var output = new StringWriter();

        CreateMenu(
            context,
            "3\n1\n1\n8\n0\n",
            output).Run();

        var updated = context.EvaluationService.GetEvaluation(
            evaluation.Id);

        Assert.Equal(8m, updated.Score.Value);

        Assert.Contains(
            "Оценка успешно изменена",
            output.ToString());
    }

    [Fact]
    public void Run_DeleteEvaluation_Confirmed_RemovesEvaluation()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var output = new StringWriter();

        CreateMenu(
            context,
            "4\n1\n1\nд\n0\n",
            output).Run();

        Assert.Empty(
            context.EvaluationService.GetEvaluations(
                assessment.Student.Id));

        Assert.Contains(
            "Оценка успешно удалена",
            output.ToString());
    }

    [Fact]
    public void Run_DeleteEvaluation_Rejected_KeepsEvaluation()
    {
        var context = new ApplicationTestContext();
        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var output = new StringWriter();

        CreateMenu(
            context,
            "4\n1\n1\nн\n0\n",
            output).Run();

        Assert.Single(
            context.EvaluationService.GetEvaluations(
                assessment.Student.Id));

        Assert.Contains(
            "Удаление отменено",
            output.ToString());
    }

    [Fact]
    public void Run_InvalidChoice_Reprompts()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        CreateMenu(context, "abc\n0\n", output).Run();

        Assert.Contains(
            "Введите номер от 0 до 4",
            output.ToString());
    }

    [Fact]
    public void Run_EndOfInput_Returns()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        CreateMenu(context, "", output).Run();

        Assert.Empty(context.StudentService.GetStudents());
    }

    private static EvaluationMenu CreateMenu(
        ApplicationTestContext context,
        string input,
        StringWriter output)
    {
        return new EvaluationMenu(
            context.CreateFacade(),
            new ConsoleInput(
                new StringReader(input),
                output),
            output);
    }
}
