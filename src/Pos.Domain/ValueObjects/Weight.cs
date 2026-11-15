
namespace Pos.Domain.ValueObjects;

public sealed record Weight
{
    public decimal Value { get; }

    public Weight(decimal value)
    {
        if (value <= 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Weight must be greater than 0 and less than or equal to 1."
            );
        }

        Value = value;
    }
}
