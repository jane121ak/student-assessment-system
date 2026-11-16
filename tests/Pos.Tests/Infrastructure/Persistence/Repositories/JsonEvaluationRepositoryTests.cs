using System.Text.Json;
using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Tests.Infrastructure.Support;

namespace Pos.Tests.Infrastructure.Persistence.Repositories;

public class JsonEvaluationRepositoryTests
{
    [Fact]
    public void Add_ValidEvaluation_PersistsEvaluation()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);

        var evaluation = context.Factory.Create(
            context.Student,
            context.Rubric,
            context.Criterion.Id,
            new Score(8m));

        var repository = CreateRepository(context);

        repository.Add(evaluation);

        var reloadedRepository = CreateRepository(context);
        var result = reloadedRepository.GetById(evaluation.Id);

        Assert.NotNull(result);
        Assert.Equal(evaluation.Id, result.Id);
        Assert.Equal(evaluation.StudentId, result.StudentId);
        Assert.Equal(evaluation.RubricId, result.RubricId);
        Assert.Equal(8m, result.Score.Value);
    }

    [Fact]
    public void Update_ChangedScore_PersistsChanges()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);
        var repository = CreateRepository(context);

        var evaluation = context.Factory.Create(
            context.Student,
            context.Rubric,
            context.Criterion.Id,
            new Score(5m));

        repository.Add(evaluation);

        evaluation.ChangeScore(new Score(8m), context.Rubric);

        repository.Update(evaluation);

        var result = CreateRepository(context)
            .GetById(evaluation.Id);

        Assert.NotNull(result);
        Assert.Equal(8m, result.Score.Value);
    }

    [Fact]
    public void Remove_ExistingEvaluation_DeletesEvaluation()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);
        var repository = CreateRepository(context);

        var evaluation = context.Factory.Create(
            context.Student,
            context.Rubric,
            context.Criterion.Id,
            new Score(8m));

        repository.Add(evaluation);

        repository.Remove(evaluation.Id);

        Assert.Null(
            CreateRepository(context).GetById(evaluation.Id));
    }

    [Fact]
    public void Add_DuplicateCriterion_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);
        var repository = CreateRepository(context);

        var first = context.Factory.Create(
            context.Student,
            context.Rubric,
            context.Criterion.Id,
            new Score(5m));

        var second = context.Factory.Create(
            context.Student,
            context.Rubric,
            context.Criterion.Id,
            new Score(8m));

        repository.Add(first);

        Assert.Throws<InvalidOperationException>(() =>
            repository.Add(second));
    }

    [Fact]
    public void GetByStudentId_ReturnsOnlyStudentEvaluations()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);
        var repository = CreateRepository(context);

        var evaluation = context.Factory.Create(
            context.Student,
            context.Rubric,
            context.Criterion.Id,
            new Score(8m));

        repository.Add(evaluation);

        var result = repository.GetByStudentId(context.Student.Id);

        Assert.Single(result);
        Assert.Equal(evaluation.Id, result.Single().Id);
    }

    [Fact]
    public void GetById_InvalidStoredScore_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);
        var evaluationId = Guid.NewGuid();

        var record = new[]
        {
            new
            {
                Id = evaluationId,
                StudentId = context.Student.Id,
                RubricId = context.Rubric.Id,
                CriterionId = context.Criterion.Id,
                Score = 11m
            }
        };

        File.WriteAllText(
            context.EvaluationPath,
            JsonSerializer.Serialize(record));

        var repository = CreateRepository(context);

        Assert.Throws<InvalidDataException>(() =>
            repository.GetById(evaluationId));
    }

    [Fact]
    public void GetById_MissingRubric_ThrowsException()
    {
        using var directory = new TemporaryDirectory();

        var context = CreateContext(directory);
        var evaluationId = Guid.NewGuid();

        var record = new[]
        {
            new
            {
                Id = evaluationId,
                StudentId = context.Student.Id,
                RubricId = Guid.NewGuid(),
                CriterionId = context.Criterion.Id,
                Score = 8m
            }
        };

        File.WriteAllText(
            context.EvaluationPath,
            JsonSerializer.Serialize(record));

        var repository = CreateRepository(context);

        Assert.Throws<InvalidDataException>(() =>
            repository.GetById(evaluationId));
    }

    private static JsonEvaluationRepository CreateRepository(
        TestContext context)
    {
        return new JsonEvaluationRepository(
            context.EvaluationPath,
            context.RubricRepository,
            context.Factory);
    }

    private static TestContext CreateContext(
        TemporaryDirectory directory)
    {
        var rubricRepository = new JsonRubricRepository(
            directory.GetFilePath("rubrics.json"));

        var rubric = new Rubric(Guid.NewGuid());

        var criterion = new Criterion(
            Guid.NewGuid(),
            "Контрольная работа",
            new Score(10m),
            new Weight(1m));

        rubric.AddCriterion(criterion);

        rubricRepository.Add(rubric);

        return new TestContext(
            rubricRepository,
            rubric,
            criterion,
            new Student(Guid.NewGuid(), "Иван Иванов"),
            new EvaluationFactory(),
            directory.GetFilePath("evaluations.json"));
    }

    private sealed record TestContext(
        IRubricRepository RubricRepository,
        Rubric Rubric,
        Criterion Criterion,
        Student Student,
        EvaluationFactory Factory,
        string EvaluationPath);
}
