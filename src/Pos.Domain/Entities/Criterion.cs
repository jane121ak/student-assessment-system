using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;

public sealed class Criterion
{
    public Guid Id { get; }

    public string Name { get; }

    public Score MaxScore { get; }

    public Weight Weight { get; }

    public Criterion(Guid id, string name, Score maxScore, Weight weight)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Criterion ID cannot be empty.",
                nameof(id)
            );
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Criterion name cannot be empty.",
                nameof(name)
            );
        }

        ArgumentNullException.ThrowIfNull(maxScore);
        ArgumentNullException.ThrowIfNull(weight);

        Id = id;
        Name = name;
        MaxScore = maxScore;
        Weight = weight;
    }

    public bool IsScoreInRange(Score score)
    {
        ArgumentNullException.ThrowIfNull(score);

        return score.Value <= MaxScore.Value;
    }
}
