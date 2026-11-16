using Pos.Cli.Menus;
using Pos.Cli.Support;
using Pos.Tests.Application.Support;
using Pos.Domain.ValueObjects;

namespace Pos.Tests.Cli;

public class StudentDeletionMenuTests
{
    [Fact]
    public void Run_DeleteStudent_Confirmed_RemovesStudent()
    {
        var context = new ApplicationTestContext();

        var student = context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nда\n0\n",
            output);

        menu.Run();

        Assert.Empty(context.StudentService.GetStudents());

        Assert.Contains(
            "Студент успешно удалён",
            output.ToString());
    }

    [Fact]
    public void Run_DeleteStudent_Cancelled_KeepsStudent()
    {
        var context = new ApplicationTestContext();

        var student = context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nн\n0\n",
            output);

        menu.Run();

        Assert.Single(context.StudentService.GetStudents());

        Assert.Contains(
            "Удаление отменено",
            output.ToString());
    }

    [Fact]
    public void Run_DeleteStudent_WithEvaluations_RejectsDeletion()
    {
        var context = new ApplicationTestContext();

        var assessment = context.CreateAssessment();

        context.EvaluationService.AddEvaluation(
            assessment.Student.Id,
            assessment.Rubric.Id,
            assessment.Criterion.Id,
            new Score(8m));

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n1\nда\n0\n",
            output);

        menu.Run();

        Assert.Single(context.StudentService.GetStudents());

        Assert.Contains(
            "Cannot delete a student who has evaluations",
            output.ToString());
    }

    [Fact]
    public void Run_DeleteStudent_EmptyList_ShowsMessage()
    {
        var context = new ApplicationTestContext();

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "Студенты не найдены",
            output.ToString());
    }

    [Fact]
    public void Run_DeleteStudent_InvalidSelection_Reprompts()
    {
        var context = new ApplicationTestContext();

        context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "3\n5\n1\nда\n0\n",
            output);

        menu.Run();

        Assert.Empty(context.StudentService.GetStudents());

        Assert.Contains(
            "Введите номер от 0 до 1",
            output.ToString());
    }

    private static StudentMenu CreateMenu(
        ApplicationTestContext context,
        string input,
        StringWriter output)
    {
        return new StudentMenu(
            context.CreateFacade(),
            new ConsoleInput(
                new StringReader(input),
                output),
            output);
    }
}
