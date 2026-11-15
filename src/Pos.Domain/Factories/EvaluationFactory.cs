using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Factories;

public sealed class EvaluationFactory
{
    public Evaluation Create(Student student, Rubric rubric, Guid criterionId, Score score)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(score);

        var criterion = rubric.GetCriterion(criterionId);

        if (!criterion.IsScoreInRange(score))
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score.Value,
                $"Score cannot exceed {criterion.MaxScore.Value}."
            );
        }

        return new Evaluation(Guid.NewGuid(), student.Id, criterion.Id, score);
    }
}
