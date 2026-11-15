using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Domain.Entities;

public class CriterionTests
{
    [Fact]
    public void Constructor_ValidData_CreatesCriterion()
    {
        var id = Guid.NewGuid();
        var maxScore = new Score(10m);
        var weight = new Weight(0.2m);

        var criterion = new Criterion(
            id,
            "Домашние задания",
            maxScore,
            weight
        );

        Assert.Equal(id, criterion.Id);
        Assert.Equal("Домашние задания", criterion.Name);
        Assert.Equal(maxScore, criterion.MaxScore);
        Assert.Equal(weight, criterion.Weight);
    }

    [Fact]
    public void Constructor_EmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Criterion(
                Guid.Empty,
                "Домашние задания",
                new Score(10m),
                new Weight(0.2m)
            )
        );
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Criterion(
                Guid.NewGuid(),
                " ",
                new Score(10m),
                new Weight(0.2m)
            )
        );
    }

    [Fact]
    public void IsScoreInRange_ScoreInsideRange_ReturnsTrue()
    {
        var criterion = CreateCriterion();

        Assert.True(criterion.IsScoreInRange(new Score(5m)));
    }

    [Fact]
    public void IsScoreInRange_Zero_ReturnsTrue()
    {
        var criterion = CreateCriterion();

        Assert.True(criterion.IsScoreInRange(new Score(0m)));
    }

    [Fact]
    public void IsScoreInRange_MaximumScore_ReturnsTrue()
    {
        var criterion = CreateCriterion();

        Assert.True(criterion.IsScoreInRange(new Score(10m)));
    }

    [Fact]
    public void IsScoreInRange_AboveMaximum_ReturnsFalse()
    {
        var criterion = CreateCriterion();

        Assert.False(criterion.IsScoreInRange(new Score(11m)));
    }

    private static Criterion CreateCriterion()
    {
        return new Criterion(
            Guid.NewGuid(),
            "Домашние задания",
            new Score(10m),
            new Weight(0.2m)
        );
    }
}
