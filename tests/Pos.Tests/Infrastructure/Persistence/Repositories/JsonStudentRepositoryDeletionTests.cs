using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Persistence.Repositories;

public class JsonStudentRepositoryDeletionTests
{
    [Fact]
    public void Remove_ExistingStudent_PersistsRemoval()
    {
        using var directory = new TemporaryDirectory();

        var path = directory.GetFilePath("students.json");
        var repository = new JsonStudentRepository(path);

        var student = new Student(
            Guid.NewGuid(),
            "Иван Иванов");

        repository.Add(student);

        repository.Remove(student.Id);

        var reloadedRepository = new JsonStudentRepository(path);

        Assert.Null(reloadedRepository.GetById(student.Id));
        Assert.Empty(reloadedRepository.GetAll());
    }

    [Fact]
    public void Remove_UnknownId_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var repository = new JsonStudentRepository(
            directory.GetFilePath("students.json"));

        Assert.Throws<KeyNotFoundException>(
            () => repository.Remove(Guid.NewGuid()));
    }

    [Fact]
    public void Remove_EmptyId_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var repository = new JsonStudentRepository(
            directory.GetFilePath("students.json"));

        Assert.Throws<ArgumentException>(
            () => repository.Remove(Guid.Empty));
    }
}
