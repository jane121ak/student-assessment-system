
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Domain.ValueObjects;

public class WeightTests
{
    [Fact]
    public void Constructor_ValidValue_CreatesWeight()
    {
        var weight = new Weight(0.2m);

        Assert.Equal(0.2m, weight.Value);
    }

    [Fact]
    public void Constructor_One_AllowsWeight()
    {
        var weight = new Weight(1.0m);

        Assert.Equal(1.0m, weight.Value);
    }

    [Fact]
    public void Constructor_Zero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Weight(0m)
        );
    }

    [Fact]
    public void Constructor_NegativeValue_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Weight(-0.2m)
        );
    }

    [Fact]
    public void Constructor_GreaterThanOne_CreatesWeight()
    {
        var weight = new Weight(1.1m);

        Assert.Equal(1.1m, weight.Value);
    }

    [Fact]
    public void TwoWeights_SameValue_AreEqual()
    {
        var first = new Weight(0.2m);
        var second = new Weight(0.2m);

        Assert.Equal(first, second);
    }
}
