
namespace Pos.Domain.ValueObjects;

public sealed record Weight
{
    public decimal Value { get; }

    public Weight(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Weight must be greater than zero."
            );
        }

        Value = value;
    }
}
