using Pos.Application.Reports;
using Pos.Domain.Services;

namespace Pos.Application.Services;

public sealed class GradeReportService
{
    private readonly StudentService _studentService;
    private readonly RubricService _rubricService;
    private readonly EvaluationService _evaluationService;
    private readonly GradeCalculator _gradeCalculator;

    public GradeReportService(
        StudentService studentService,
        RubricService rubricService,
        EvaluationService evaluationService,
        GradeCalculator gradeCalculator)
    {
        ArgumentNullException.ThrowIfNull(studentService);
        ArgumentNullException.ThrowIfNull(rubricService);
        ArgumentNullException.ThrowIfNull(evaluationService);
        ArgumentNullException.ThrowIfNull(gradeCalculator);

        _studentService = studentService;
        _rubricService = rubricService;
        _evaluationService = evaluationService;
        _gradeCalculator = gradeCalculator;
    }

    public GradeReport CreateReport(Guid studentId, Guid rubricId)
    {
        var student = _studentService.GetStudent(studentId);
        var rubric = _rubricService.GetRubric(rubricId);

        var evaluations = _evaluationService
            .GetEvaluations(studentId)
            .Where(e => e.RubricId == rubricId)
            .ToArray();

        var finalGrade = _gradeCalculator.Calculate(
            student.Id,
            rubric,
            evaluations);

        var scoresByCriterion = evaluations.ToDictionary(
            e => e.CriterionId,
            e => e.Score.Value);

        var criteria = rubric.Criteria
            .Select(criterion => new CriterionGradeReport(
                criterion.Id,
                criterion.Name,
                criterion.MaxScore.Value,
                criterion.Weight.Value,
                scoresByCriterion.TryGetValue(
                    criterion.Id,
                    out var score)
                        ? score
                        : null))
            .ToArray();

        return new GradeReport(
            student.Id,
            student.FullName,
            rubric.Id,
            Array.AsReadOnly(criteria),
            finalGrade);
    }
}
