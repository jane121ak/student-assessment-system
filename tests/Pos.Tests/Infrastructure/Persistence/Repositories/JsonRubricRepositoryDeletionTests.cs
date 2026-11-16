using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Persistence.Repositories;

public class JsonRubricRepositoryDeletionTests
{
    [Fact]
    public void Remove_ExistingRubric_RemovesFromStorage()
    {
        using var directory = new TemporaryDirectory();

        var path = directory.GetFilePath("rubrics.json");
        var repository = new JsonRubricRepository(path);

        var rubric = new Rubric(Guid.NewGuid());

        repository.Add(rubric);
        repository.Remove(rubric.Id);

        var reloadedRepository = new JsonRubricRepository(path);

        Assert.Null(reloadedRepository.GetById(rubric.Id));
        Assert.Empty(reloadedRepository.GetAll());
    }

    [Fact]
    public void Remove_UnknownRubric_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var repository = new JsonRubricRepository(
            directory.GetFilePath("rubrics.json"));

        Assert.Throws<KeyNotFoundException>(
            () => repository.Remove(Guid.NewGuid()));
    }
}
