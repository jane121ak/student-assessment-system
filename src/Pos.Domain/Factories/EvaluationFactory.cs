using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Factories;

public sealed class EvaluationFactory
{
    public Evaluation Create(Student student, Rubric rubric, Guid criterionId, Score score)
    {
        ArgumentNullException.ThrowIfNull(student);

        return Reconstitute(
            Guid.NewGuid(),
            student.Id,
            rubric,
            criterionId,
            score);
    }

    public Evaluation Reconstitute(
        Guid evaluationId,
        Guid studentId,
        Rubric rubric,
        Guid criterionId,
        Score score)
    {
        if (evaluationId == Guid.Empty)
            throw new ArgumentException(
                "Evaluation ID cannot be empty.",
                nameof(evaluationId));

        if (studentId == Guid.Empty)
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));

        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(score);

        var criterion = rubric.GetCriterion(criterionId);

        if (!criterion.IsScoreInRange(score))
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score.Value,
                $"Score cannot exceed {criterion.MaxScore.Value}.");

        return new Evaluation(
            evaluationId,
            studentId,
            rubric.Id,
            criterion.Id,
            score);
    }
}
