using Pos.Application.Services;
using Pos.Cli;
using Pos.Cli.Menus;
using Pos.Cli.Support;
using Pos.Domain.ValueObjects;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Cli;

public class RubricMenuTests
{
    [Fact]
    public void Run_CreateRubric_CreatesRubric()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "2\n0\n", output);

        menu.Run();

        Assert.Single(context.RubricService.GetRubrics());
        Assert.Contains(
            "Рубрикатор успешно создан",
            output.ToString());
    }

    [Fact]
    public void Run_ListRubrics_DisplaysRubrics()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();
        var menu = CreateMenu(context, "1\n0\n", output);

        menu.Run();

        Assert.Contains(rubric.Id.ToString(), output.ToString());
    }

    [Fact]
    public void Run_ListRubrics_EmptyRepository_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "1\n0\n", output);

        menu.Run();

        Assert.Contains(
            "Рубрикаторы не найдены",
            output.ToString());
    }

    [Fact]
    public void Run_AddCriterion_ValidInput_AddsCriterion()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nКонтрольная работа\n10\n0.5\n0\n",
            output);

        menu.Run();

        var criteria = context.RubricService.GetCriteria(rubric.Id);

        var criterion = Assert.Single(criteria);

        Assert.Equal("Контрольная работа", criterion.Name);
        Assert.Equal(10m, criterion.MaxScore.Value);
        Assert.Equal(0.5m, criterion.Weight.Value);

        Assert.Contains(
            "Критерий успешно добавлен",
            output.ToString());
    }

    [Fact]
    public void Run_AddCriterion_InvalidMaxScore_Reprompts()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nКонтрольная работа\n-1\n10\n0.5\n0\n",
            output);

        menu.Run();

        var criterion = Assert.Single(
            context.RubricService.GetCriteria(rubric.Id));

        Assert.Equal(10m, criterion.MaxScore.Value);

        Assert.Contains(
            "Некорректное число",
            output.ToString());
    }

    [Fact]
    public void Run_AddCriterion_InvalidWeight_Reprompts()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nКонтрольная работа\n10\n1.5\n0.5\n0\n",
            output);

        menu.Run();

        var criterion = Assert.Single(
            context.RubricService.GetCriteria(rubric.Id));

        Assert.Equal(0.5m, criterion.Weight.Value);

        Assert.Contains(
            "Некорректное число",
            output.ToString());
    }

    [Fact]
    public void Run_AddCriterion_EmptyName_Reprompts()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\n\nКонтрольная работа\n10\n0.5\n0\n",
            output);

        menu.Run();

        var criterion = Assert.Single(
            context.RubricService.GetCriteria(rubric.Id));

        Assert.Equal("Контрольная работа", criterion.Name);

        Assert.Contains(
            "Значение не может быть пустым",
            output.ToString());
    }

    [Fact]
    public void Run_AddCriterion_NoRubrics_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "3\n0\n", output);

        menu.Run();

        Assert.Contains(
            "Сначала необходимо создать рубрикатор",
            output.ToString());
    }

    [Fact]
    public void Run_AddCriterion_CancelSelection_DoesNotAddCriterion()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n0\n0\n",
            output);

        menu.Run();

        Assert.Empty(
            context.RubricService.GetCriteria(rubric.Id));
    }

    [Fact]
    public void Run_AddCriterion_CancelMaxScore_DoesNotAddCriterion()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nКонтрольная работа\n0\n0\n",
            output);

        menu.Run();

        Assert.Empty(
            context.RubricService.GetCriteria(rubric.Id));
    }

    [Fact]
    public void Run_ViewCriteria_DisplaysCriteria()
    {
        var context = new ApplicationTestContext();
        var rubric = context.RubricService.CreateRubric();

        context.RubricService.AddCriterion(
            rubric.Id,
            "Контрольная работа",
            new Score(10m),
            new Weight(0.5m));

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "4\n1\n0\n",
            output);

        menu.Run();

        var result = output.ToString();

        Assert.Contains("Контрольная работа", result);
        Assert.Contains("10", result);
        Assert.Contains(0.5m.ToString(), result);
    }

    [Fact]
    public void Run_ViewCriteria_EmptyRubric_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "4\n1\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "В этом рубрикаторе пока нет критериев",
            output.ToString());
    }

    [Fact]
    public void Run_InvalidRubricSelection_Reprompts()
    {
        var context = new ApplicationTestContext();
        context.RubricService.CreateRubric();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "4\n9\n1\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "Введите номер от 1 до 1 или 0 для отмены",
            output.ToString());
    }

    [Fact]
    public void ConsoleApplication_SelectRubrics_OpensRubricMenu()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var application = new ConsoleApplication(
            context.CreateFacade(),
            new StringReader("2\n0\n0\n"),
            output);

        application.Run();

        Assert.Contains(
            "========== РУБРИКАТОРЫ ==========",
            output.ToString());
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
