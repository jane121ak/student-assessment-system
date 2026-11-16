using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Persistence.Repositories;

public class JsonStudentRepositoryTests
{
    [Fact]
    public void Add_ValidStudent_PersistsStudent()
    {
        using var directory = new TemporaryDirectory();

        var path = directory.GetFilePath("students.json");
        var repository = new JsonStudentRepository(path);

        var student = new Student(
            Guid.NewGuid(),
            "Иван Иванов");

        repository.Add(student);

        var reloadedRepository = new JsonStudentRepository(path);
        var result = reloadedRepository.GetById(student.Id);

        Assert.NotNull(result);
        Assert.Equal(student.Id, result.Id);
        Assert.Equal(student.FullName, result.FullName);
    }

    [Fact]
    public void Add_DuplicateId_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var repository = new JsonStudentRepository(
            directory.GetFilePath("students.json"));

        var id = Guid.NewGuid();

        repository.Add(new Student(id, "Иван Иванов"));

        Assert.Throws<InvalidOperationException>(() =>
            repository.Add(new Student(id, "Пётр Петров")));
    }

    [Fact]
    public void GetAll_ReturnsAllStudents()
    {
        using var directory = new TemporaryDirectory();

        var repository = new JsonStudentRepository(
            directory.GetFilePath("students.json"));

        repository.Add(new Student(Guid.NewGuid(), "Иван Иванов"));
        repository.Add(new Student(Guid.NewGuid(), "Пётр Петров"));

        var result = repository.GetAll();

        Assert.Equal(2, result.Count);
    }
}
