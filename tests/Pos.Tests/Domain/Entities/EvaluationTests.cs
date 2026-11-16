using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Domain.Entities;

public class EvaluationTests
{
    [Fact]
    public void ChangeScore_ValidScore_UpdatesScore()
    {
        var (evaluation, rubric, _) = CreateEvaluation();

        evaluation.ChangeScore(new Score(8m), rubric);

        Assert.Equal(new Score(8m), evaluation.Score);
    }

    [Fact]
    public void ChangeScore_ScoreAboveMaximum_ThrowsException()
    {
        var (evaluation, rubric, _) = CreateEvaluation();

        Action action = () =>
            evaluation.ChangeScore(new Score(11m), rubric);

        Assert.Throws<ArgumentOutOfRangeException>(action);

        Assert.Equal(new Score(5m), evaluation.Score);
    }

    [Fact]
    public void ChangeScore_DifferentRubric_ThrowsException()
    {
        var (evaluation, _, _) = CreateEvaluation();

        var anotherRubric = new Rubric(Guid.NewGuid());

        Action action = () =>
            evaluation.ChangeScore(new Score(8m), anotherRubric);

        Assert.Throws<ArgumentException>(action);

        Assert.Equal(new Score(5m), evaluation.Score);
    }

    [Fact]
    public void ChangeScore_CriterionNotInRubric_ThrowsException()
    {
        var (evaluation, rubric, _) = CreateEvaluation();

        var rubricWithoutCriterion = new Rubric(rubric.Id);

        Action action = () =>
            evaluation.ChangeScore(
                new Score(8m),
                rubricWithoutCriterion
            );

        Assert.Throws<KeyNotFoundException>(action);

        Assert.Equal(new Score(5m), evaluation.Score);
    }

    [Fact]
    public void ChangeScore_NullScore_ThrowsException()
    {
        var (evaluation, rubric, _) = CreateEvaluation();

        Action action = () =>
            evaluation.ChangeScore(null!, rubric);

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void ChangeScore_NullRubric_ThrowsException()
    {
        var (evaluation, _, _) = CreateEvaluation();

        Action action = () =>
            evaluation.ChangeScore(new Score(8m), null!);

        Assert.Throws<ArgumentNullException>(action);
    }

    private static (Evaluation Evaluation, Rubric Rubric, Criterion Criterion) CreateEvaluation()
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

        var factory = new EvaluationFactory();

        var evaluation = factory.Create(
            student,
            rubric,
            criterion.Id,
            new Score(5m)
        );

        return (evaluation, rubric, criterion);
    }
}
