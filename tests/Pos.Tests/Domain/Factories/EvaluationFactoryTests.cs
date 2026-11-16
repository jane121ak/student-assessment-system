using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Domain.Factories;

public class EvaluationFactoryTests
{
    [Fact]
    public void Create_ValidData_CreatesEvaluation()
    {
        var (student, rubric, criterion) = CreateValidContext();

        var factory = new EvaluationFactory();

        var evaluation = factory.Create(
            student,
            rubric,
            criterion.Id,
            new Score(8m)
        );

        Assert.NotEqual(Guid.Empty, evaluation.Id);
        Assert.Equal(student.Id, evaluation.StudentId);
        Assert.Equal(rubric.Id, evaluation.RubricId);
        Assert.Equal(criterion.Id, evaluation.CriterionId);
        Assert.Equal(new Score(8m), evaluation.Score);
    }

    [Fact]
    public void Create_UnknownCriterion_ThrowsException()
    {
        var (student, rubric, _) = CreateValidContext();
        var factory = new EvaluationFactory();

        Action action = () => factory.Create(
            student,
            rubric,
            Guid.NewGuid(),
            new Score(5m)
        );

        Assert.Throws<KeyNotFoundException>(action);
    }

    [Fact]
    public void Create_ScoreAboveMaximum_ThrowsException()
    {
        var (student, rubric, criterion) = CreateValidContext();
        var factory = new EvaluationFactory();

        Action action = () => factory.Create(
            student,
            rubric,
            criterion.Id,
            new Score(11m)
        );

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Create_ZeroScore_CreatesEvaluation()
    {
        var (student, rubric, criterion) = CreateValidContext();
        var factory = new EvaluationFactory();

        var evaluation = factory.Create(
            student,
            rubric,
            criterion.Id,
            new Score(0m)
        );

        Assert.Equal(new Score(0m), evaluation.Score);
    }

    [Fact]
    public void Create_MaximumScore_CreatesEvaluation()
    {
        var (student, rubric, criterion) = CreateValidContext();
        var factory = new EvaluationFactory();

        var evaluation = factory.Create(
            student,
            rubric,
            criterion.Id,
            new Score(10m)
        );

        Assert.Equal(new Score(10m), evaluation.Score);
    }

    [Fact]
    public void Create_NullStudent_ThrowsException()
    {
        var (_, rubric, criterion) = CreateValidContext();
        var factory = new EvaluationFactory();

        Action action = () => factory.Create(
            null!,
            rubric,
            criterion.Id,
            new Score(5m)
        );

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Create_NullRubric_ThrowsException()
    {
        var (student, _, criterion) = CreateValidContext();
        var factory = new EvaluationFactory();

        Action action = () => factory.Create(
            student,
            null!,
            criterion.Id,
            new Score(5m)
        );

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Create_NullScore_ThrowsException()
    {
        var (student, rubric, criterion) = CreateValidContext();
        var factory = new EvaluationFactory();

        Action action = () => factory.Create(
            student,
            rubric,
            criterion.Id,
            null!
        );

        Assert.Throws<ArgumentNullException>(action);
    }

    private static (Student Student, Rubric Rubric, Criterion Criterion) CreateValidContext()
    {
        var student = new Student(
            Guid.NewGuid(),
            "Иван Иванов"
        );

        var rubric = new Rubric(Guid.NewGuid());

        var criterion = new Criterion(
            Guid.NewGuid(),
            "Домашние задания",
            new Score(10m),
            new Weight(0.2m)
        );

        rubric.AddCriterion(criterion);

        return (student, rubric, criterion);
    }
}
