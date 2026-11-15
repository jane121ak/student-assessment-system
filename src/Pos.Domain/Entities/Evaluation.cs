using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;

public sealed class Evaluation
{
    public Guid Id { get; }

    public Guid StudentId { get; }

    public Guid CriterionId { get; }

    public Score Score { get; }

    internal Evaluation(Guid id, Guid studentId, Guid criterionId, Score score)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Evaluation ID cannot be empty.",
                nameof(id));

        if (studentId == Guid.Empty)
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));

        if (criterionId == Guid.Empty)
            throw new ArgumentException(
                "Criterion ID cannot be empty.",
                nameof(criterionId));

        ArgumentNullException.ThrowIfNull(score);

        Id = id;
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
    }
}
