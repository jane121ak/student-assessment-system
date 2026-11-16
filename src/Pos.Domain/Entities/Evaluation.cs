using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;

public sealed class Evaluation
{
    public Guid Id { get; }

    public Guid StudentId { get; }

    public Guid RubricId { get; }

    public Guid CriterionId { get; }

    public Score Score { get; private set; }

    internal Evaluation(Guid id, Guid studentId, Guid rubricId, Guid criterionId, Score score)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Evaluation ID cannot be empty.",
                nameof(id));

        if (studentId == Guid.Empty)
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));

        if (rubricId == Guid.Empty)
            throw new ArgumentException(
                "Rubric ID cannot be empty.",
                nameof(rubricId));

        if (criterionId == Guid.Empty)
            throw new ArgumentException(
                "Criterion ID cannot be empty.",
                nameof(criterionId));

        ArgumentNullException.ThrowIfNull(score);

        Id = id;
        StudentId = studentId;
        RubricId = rubricId;
        CriterionId = criterionId;
        Score = score;
    }

    public void ChangeScore(Score score, Rubric rubric)
    {
        ArgumentNullException.ThrowIfNull(score);
        ArgumentNullException.ThrowIfNull(rubric);

        if (rubric.Id != RubricId)
        {
            throw new ArgumentException(
                "Rubric does not match this evaluation.",
                nameof(rubric));
        }

        var criterion = rubric.GetCriterion(CriterionId);

        if (!criterion.IsScoreInRange(score))
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score.Value,
                $"Score cannot exceed {criterion.MaxScore.Value}.");
        }

        Score = score;
    }
}
