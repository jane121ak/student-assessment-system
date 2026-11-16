using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Persistence.Repositories;

public class JsonRubricRepositoryTests
{
    [Fact]
    public void Add_ValidRubric_PersistsRubricAndCriteria()
    {
        using var directory = new TemporaryDirectory();

        var path = directory.GetFilePath("rubrics.json");
        var repository = new JsonRubricRepository(path);

        var rubric = CreateRubric();

        repository.Add(rubric);

        var reloadedRepository = new JsonRubricRepository(path);
        var result = reloadedRepository.GetById(rubric.Id);

        Assert.NotNull(result);
        Assert.Equal(rubric.Id, result.Id);

        var criterion = Assert.Single(result.Criteria);

        Assert.Equal("Контрольная работа", criterion.Name);
        Assert.Equal(10m, criterion.MaxScore.Value);
        Assert.Equal(0.5m, criterion.Weight.Value);
    }

    [Fact]
    public void Update_ChangedRubric_PersistsChanges()
    {
        using var directory = new TemporaryDirectory();

        var path = directory.GetFilePath("rubrics.json");
        var repository = new JsonRubricRepository(path);

        var rubric = CreateRubric();

        repository.Add(rubric);

        rubric.AddCriterion(
            new Criterion(
                Guid.NewGuid(),
                "Экзамен",
                new Score(20m),
                new Weight(0.5m)));

        repository.Update(rubric);

        var result = new JsonRubricRepository(path)
            .GetById(rubric.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.Criteria.Count);
    }

    [Fact]
    public void Add_DuplicateId_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var repository = new JsonRubricRepository(
            directory.GetFilePath("rubrics.json"));

        var rubric = CreateRubric();

        repository.Add(rubric);

        Assert.Throws<InvalidOperationException>(() =>
            repository.Add(new Rubric(rubric.Id)));
    }

    private static Rubric CreateRubric()
    {
        var rubric = new Rubric(Guid.NewGuid());

        rubric.AddCriterion(
            new Criterion(
                Guid.NewGuid(),
                "Контрольная работа",
                new Score(10m),
                new Weight(0.5m)));

        return rubric;
    }
}
