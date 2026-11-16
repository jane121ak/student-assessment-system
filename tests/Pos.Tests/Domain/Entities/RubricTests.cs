using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Domain.Entities;

public class RubricTests
{
    [Fact]
    public void Constructor_ValidId_CreatesRubric()
    {
        var id = Guid.NewGuid();

        var rubric = new Rubric(id);

        Assert.Equal(id, rubric.Id);
        Assert.Empty(rubric.Criteria);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(
            () => new Rubric(Guid.Empty)
        );
    }

    [Fact]
    public void AddCriterion_ValidCriterion_AddsCriterion()
    {
        var rubric = CreateRubric();
        var criterion = CreateCriterion();

        rubric.AddCriterion(criterion);

        Assert.Single(rubric.Criteria);
        Assert.Contains(criterion, rubric.Criteria);
    }

    [Fact]
    public void AddCriterion_DuplicateId_ThrowsException()
    {
        var rubric = CreateRubric();
        var id = Guid.NewGuid();

        rubric.AddCriterion(CreateCriterion(id));

        Action action = () => rubric.AddCriterion(CreateCriterion(id));

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddCriterion_Null_ThrowsException()
    {
        var rubric = CreateRubric();

        Action action = () => rubric.AddCriterion(null!);

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void ContainsCriterion_ExistingId_ReturnsTrue()
    {
        var rubric = CreateRubric();
        var criterion = CreateCriterion();

        rubric.AddCriterion(criterion);

        Assert.True(rubric.ContainsCriterion(criterion.Id));
    }

    [Fact]
    public void ContainsCriterion_UnknownId_ReturnsFalse()
    {
        var rubric = CreateRubric();

        Assert.False(
            rubric.ContainsCriterion(Guid.NewGuid())
        );
    }

    [Fact]
    public void GetCriterion_ExistingId_ReturnsCriterion()
    {
        var rubric = CreateRubric();
        var criterion = CreateCriterion();

        rubric.AddCriterion(criterion);

        var result = rubric.GetCriterion(criterion.Id);

        Assert.Same(criterion, result);
    }

    [Fact]
    public void GetCriterion_UnknownId_ThrowsException()
    {
        var rubric = CreateRubric();

        Action action = () => rubric.GetCriterion(Guid.NewGuid());

        Assert.Throws<KeyNotFoundException>(action);
    }

    private static Rubric CreateRubric()
    {
        return new Rubric(Guid.NewGuid());
    }

    private static Criterion CreateCriterion(Guid? id = null)
    {
        return new Criterion(
            id ?? Guid.NewGuid(),
            "Домашние задания",
            new Score(10m),
            new Weight(0.2m)
        );
    }
}
