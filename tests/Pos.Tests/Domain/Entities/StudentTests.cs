using Pos.Domain.Entities;
using Xunit;

namespace Pos.Tests.Domain.Entities;

public class StudentTests
{
    [Fact]
    public void Constructor_ValidData_CreatesStudent()
    {
        var id = Guid.NewGuid();
        const string fullName = "Иван Иванов";

        var student = new Student(id, fullName);

        Assert.Equal(id, student.Id);
        Assert.Equal(fullName, student.FullName);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsException()
    {
        Action action = () =>
            new Student(Guid.Empty, "Иван Иванов");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Constructor_NullFullName_ThrowsException()
    {
        Action action = () =>
            new Student(Guid.NewGuid(), null!);

        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_EmptyOrWhitespaceFullName_ThrowsException(
        string fullName)
    {
        Action action = () =>
            new Student(Guid.NewGuid(), fullName);

        Assert.Throws<ArgumentException>(action);
    }
}
