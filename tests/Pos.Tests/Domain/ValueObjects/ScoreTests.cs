using Pos.Domain.ValueObjects;

namespace Pos.Tests.Domain.ValueObjects;

public class ScoreTests
{
    [Fact]
    public void Constructor_ValidValue_CreatesScore()
    {
        var score = new Score(8.5m);

        Assert.Equal(8.5m, score.Value);
    }

    [Fact]
    public void Constructor_NegativeValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Score(-1m)
        );
    }
}
