using Pos.Cli.Menus;
using Pos.Cli.Support;
using Pos.Tests.Application.Support;

namespace Pos.Tests.Cli;

public class StudentMenuTests
{
    [Fact]
    public void Run_EmptyList_ShowsMessage()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "1\n0\n", output);

        menu.Run();

        Assert.Contains("Студенты не найдены", output.ToString());
    }

    [Fact]
    public void Run_AddStudent_CreatesStudent()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "2\nИван Иванов\n0\n",
            output);

        menu.Run();

        Assert.Contains(
            "Студент успешно добавлен",
            output.ToString());

        Assert.Single(context.StudentService.GetStudents());
    }

    [Fact]
    public void Run_BlankNameThenValidName_AcceptsStudent()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "2\n \nИван Иванов\n0\n",
            output);

        menu.Run();

        var result = output.ToString();

        Assert.Contains("Значение не может быть пустым", result);
        Assert.Contains("Студент успешно добавлен", result);
        Assert.Single(context.StudentService.GetStudents());
    }

    [Fact]
    public void Run_ListStudents_DisplaysSortedStudents()
    {
        var context = new ApplicationTestContext();

        context.StudentService.AddStudent("Пётр Петров");
        context.StudentService.AddStudent("Иван Иванов");

        var output = new StringWriter();
        var menu = CreateMenu(context, "1\n0\n", output);

        menu.Run();

        var result = output.ToString();

        Assert.True(
            result.IndexOf("Иван Иванов", StringComparison.Ordinal) <
            result.IndexOf("Пётр Петров", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_InvalidChoice_Reprompts()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "abc\n0\n", output);

        menu.Run();

        Assert.Contains(
            "Введите номер от 0 до 3",
            output.ToString());
    }

    [Fact]
    public void Run_EndOfInput_Returns()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "", output);

        menu.Run();

        Assert.Empty(context.StudentService.GetStudents());
    }


    [Fact]
    public void Run_ZeroAsName_CancelsAddingStudent()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(context, "2\n0\n0\n", output);

        menu.Run();

        Assert.Empty(context.StudentService.GetStudents());
        Assert.Contains("Ввод отменён", output.ToString());
    }

    [Fact]
    public void Run_NumericNameThenValidName_AcceptsStudent()
    {
        var context = new ApplicationTestContext();
        var output = new StringWriter();

        var menu = CreateMenu(
            context,
            "2\n123\nИван Иванов\n0\n",
            output);

        menu.Run();

        var result = output.ToString();

        Assert.Contains(
            "ФИО должно содержать хотя бы одну букву",
            result);

        Assert.Contains(
            "Студент успешно добавлен",
            result);

        Assert.Single(context.StudentService.GetStudents());
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
