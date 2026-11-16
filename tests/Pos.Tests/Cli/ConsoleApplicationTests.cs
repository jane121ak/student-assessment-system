using Pos.Cli;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Cli;

public class ConsoleApplicationTests
{
    [Fact]
    public void Run_ExitChoice_StopsApplication()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var application = new ConsoleApplication(
            context.CreateFacade(),
            new StringReader("0"),
            output);

        application.Run();

        Assert.Contains("Завершение работы", output.ToString());
    }

    [Fact]
    public void Run_AddStudentAndList_DisplaysStudent()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var input = new StringReader(
            "1\n2\nИван Иванов\n1\n0\n0\n");

        var application = new ConsoleApplication(
            context.CreateFacade(),
            input,
            output);

        application.Run();

        Assert.Contains("Иван Иванов", output.ToString());
        Assert.Contains("Студент успешно добавлен", output.ToString());
    }

    [Fact]
    public void Run_AfterRejectedName_AcceptsValidStudent()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var input = new StringReader(
            "1\n2\n \nИван Иванов\n1\n0\n0\n");

        var application = new ConsoleApplication(
            context.CreateFacade(),
            input,
            output);

        application.Run();

        var result = output.ToString();

        Assert.Contains("Значение не может быть пустым.", result);
        Assert.Contains("Студент успешно добавлен", result);
        Assert.Contains("Иван Иванов", result);

        Assert.Single(context.StudentService.GetStudents());
    }

    [Fact]
    public void Run_InvalidMenuChoice_Reprompts()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var application = new ConsoleApplication(
            context.CreateFacade(),
            new StringReader("abc\n0\n"),
            output);

        application.Run();

        Assert.Contains(
            "Введите номер от 0 до 4",
            output.ToString());
    }

    [Fact]
    public void Run_EvaluationSection_OpensEvaluationMenu()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var application = new ConsoleApplication(
            context.CreateFacade(),
            new StringReader("3\n0\n0\n"),
            output);

        application.Run();

        Assert.Contains(
            "========== ОЦЕНКИ ==========",
            output.ToString());
    }

    [Fact]
    public void Run_UnimplementedSection_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var application = new ConsoleApplication(
            context.CreateFacade(),
            new StringReader("4\n0\n"),
            output);

        application.Run();

        Assert.Contains(
            "Раздел пока не реализован",
            output.ToString());
    }

    [Fact]
    public void Run_EndOfInput_StopsApplication()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var application = new ConsoleApplication(
            context.CreateFacade(),
            new StringReader(""),
            output);

        application.Run();

        Assert.Contains(
            "Завершение работы",
            output.ToString());
    }
}
