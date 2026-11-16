using Pos.Cli.Support;

namespace Pos.Tests.Cli.Support;

public class ConsoleInputTests
{
    [Fact]
    public void ReadRequiredText_BlankThenValid_ReturnsText()
    {
        var output = new StringWriter();
        var input = new ConsoleInput(
            new StringReader("\n   \n Иван Иванов \n"),
            output);

        var result = input.ReadRequiredText("ФИО: ");

        Assert.Equal("Иван Иванов", result);
        Assert.Contains(
            "Значение не может быть пустым",
            output.ToString());
    }

    [Fact]
    public void ReadRequiredText_EndOfInput_ReturnsNull()
    {
        var input = new ConsoleInput(
            new StringReader(""),
            new StringWriter());

        Assert.Null(input.ReadRequiredText("ФИО: "));
    }

    [Fact]
    public void ReadDecimal_CommaSeparator_ReturnsDecimal()
    {
        var input = new ConsoleInput(
            new StringReader("0,75\n"),
            new StringWriter());

        var result = input.ReadDecimal(
            "Вес: ",
            0m,
            1m);

        Assert.Equal(0.75m, result);
    }

    [Fact]
    public void ReadDecimal_InvalidAndOutOfRangeThenValid_Reprompts()
    {
        var output = new StringWriter();
        var input = new ConsoleInput(
            new StringReader("abc\n1.5\n0.5\n"),
            output);

        var result = input.ReadDecimal(
            "Балл: ",
            0m,
            1m);

        Assert.Equal(0.5m, result);

        Assert.Equal(
            2,
            output.ToString().Split(
                "Некорректное число").Length - 1);
    }

    [Fact]
    public void ReadDecimal_ExclusiveMinimum_RejectsZero()
    {
        var output = new StringWriter();
        var input = new ConsoleInput(
            new StringReader("0\n0.01\n"),
            output);

        var result = input.ReadDecimal(
            "Вес: ",
            0m,
            minimumInclusive: false);

        Assert.Equal(0.01m, result);
        Assert.Contains("Некорректное число", output.ToString());
    }

    [Fact]
    public void ReadSelection_InvalidThenValid_ReturnsSelection()
    {
        var output = new StringWriter();
        var input = new ConsoleInput(
            new StringReader("0\n5\n2\n"),
            output);

        var result = input.ReadSelection(
            "Выберите: ",
            3);

        Assert.Equal(2, result);
        Assert.Contains("Введите номер от 1 до 3", output.ToString());
    }

    [Theory]
    [InlineData("да", true)]
    [InlineData("д", true)]
    [InlineData("yes", true)]
    [InlineData("нет", false)]
    [InlineData("н", false)]
    [InlineData("no", false)]
    public void ReadConfirmation_ValidInput_ReturnsResult(
        string value,
        bool expected)
    {
        var input = new ConsoleInput(
            new StringReader(value),
            new StringWriter());

        Assert.Equal(
            expected,
            input.ReadConfirmation("Продолжить? "));
    }

    [Fact]
    public void ReadConfirmation_InvalidThenValid_Reprompts()
    {
        var output = new StringWriter();
        var input = new ConsoleInput(
            new StringReader("maybe\nда\n"),
            output);

        var result = input.ReadConfirmation("Продолжить? ");

        Assert.True(result);
        Assert.Contains("Введите «д» или «н»", output.ToString());
    }
}
