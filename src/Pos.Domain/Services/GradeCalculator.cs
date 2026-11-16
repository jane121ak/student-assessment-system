using Pos.Domain.Entities;

namespace Pos.Domain.Services;

public sealed class GradeCalculator
{
    public decimal? Calculate(
        Guid studentId,
        Rubric rubric,
        IEnumerable<Evaluation> evaluations)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));
        }

        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(evaluations);

        decimal weightedScoreSum = 0m;
        decimal totalWeight = 0m;

        var processedCriteria = new HashSet<Guid>();

        foreach (var evaluation in evaluations)
        {
            if (evaluation is null)
            {
                throw new ArgumentException(
                    "Evaluations cannot contain null.",
                    nameof(evaluations));
            }

            if (evaluation.StudentId != studentId)
            {
                throw new ArgumentException(
                    "Evaluation belongs to another student.",
                    nameof(evaluations));
            }

            if (evaluation.RubricId != rubric.Id)
            {
                throw new ArgumentException(
                    "Evaluation belongs to another rubric.",
                    nameof(evaluations));
            }

            if (!processedCriteria.Add(evaluation.CriterionId))
            {
                throw new InvalidOperationException(
                    "Student already has an evaluation for this criterion.");
            }

            var criterion = rubric.GetCriterion(
                evaluation.CriterionId);

            if (!criterion.IsScoreInRange(evaluation.Score))
            {
                throw new InvalidOperationException(
                    "Evaluation score is outside the criterion range.");
            }

            var normalizedScore =
                evaluation.Score.Value / criterion.MaxScore.Value;

            weightedScoreSum +=
                normalizedScore * criterion.Weight.Value;

            totalWeight += criterion.Weight.Value;
        }

        if (totalWeight == 0m)
        {
            return null;
        }

        return weightedScoreSum / totalWeight;
    }
}
