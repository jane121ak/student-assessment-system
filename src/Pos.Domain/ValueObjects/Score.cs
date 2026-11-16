namespace Pos.Domain.ValueObjects;

public sealed record Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Score cannot be negative."
            );

        Value = value;
    }
}
