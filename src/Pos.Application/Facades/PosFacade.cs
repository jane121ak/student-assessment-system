using Pos.Application.Abstractions.Export;
using Pos.Application.Reports;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Application.Facades;

public sealed class PosFacade
{
    private readonly StudentService _studentService;
    private readonly RubricService _rubricService;
    private readonly EvaluationService _evaluationService;
    private readonly GradeReportService _gradeReportService;

    private readonly IReadOnlyDictionary<string, IGradeReportExporter>
        _exporters;

    public PosFacade(
        StudentService studentService,
        RubricService rubricService,
        EvaluationService evaluationService,
        GradeReportService gradeReportService,
        IEnumerable<IGradeReportExporter> exporters)
    {
        ArgumentNullException.ThrowIfNull(studentService);
        ArgumentNullException.ThrowIfNull(rubricService);
        ArgumentNullException.ThrowIfNull(evaluationService);
        ArgumentNullException.ThrowIfNull(gradeReportService);
        ArgumentNullException.ThrowIfNull(exporters);

        _studentService = studentService;
        _rubricService = rubricService;
        _evaluationService = evaluationService;
        _gradeReportService = gradeReportService;

        var exporterList = exporters.ToArray();

        if (exporterList.Any(e => e is null))
            throw new ArgumentException(
                "Exporters cannot contain null.",
                nameof(exporters));

        if (exporterList.Any(e => string.IsNullOrWhiteSpace(e.Format)))
            throw new ArgumentException(
                "Exporter format cannot be empty.",
                nameof(exporters));

        var duplicateFormat = exporterList
            .GroupBy(e => e.Format.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateFormat is not null)
            throw new ArgumentException(
                "Exporter formats must be unique.",
                nameof(exporters));

        _exporters = exporterList.ToDictionary(
            e => e.Format.Trim(),
            e => e,
            StringComparer.OrdinalIgnoreCase);
    }

    public Student AddStudent(string fullName)
    {
        return _studentService.AddStudent(fullName);
    }

    public Student GetStudent(Guid studentId)
    {
        return _studentService.GetStudent(studentId);
    }

    public IReadOnlyCollection<Student> GetStudents()
    {
        return _studentService.GetStudents();
    }

    public void DeleteStudent(Guid studentId)
    {
        _studentService.DeleteStudent(studentId);
    }

    public Rubric CreateRubric()
    {
        return _rubricService.CreateRubric();
    }

    public Rubric GetRubric(Guid rubricId)
    {
        return _rubricService.GetRubric(rubricId);
    }

    public IReadOnlyCollection<Rubric> GetRubrics()
    {
        return _rubricService.GetRubrics();
    }

    public void DeleteRubric(Guid rubricId)
    {
        _ = _rubricService.GetRubric(rubricId);

        if (HasEvaluations(rubricId))
            throw new InvalidOperationException(
                "Нельзя удалить рубрикатор: по нему уже выставлены оценки.");

        _rubricService.DeleteRubric(rubricId);
    }

    public Criterion AddCriterion(
        Guid rubricId,
        string name,
        Score maxScore,
        Weight weight)
    {
        return _rubricService.AddCriterion(
            rubricId,
            name,
            maxScore,
            weight);
    }

    public void DeleteCriterion(
        Guid rubricId,
        Guid criterionId)
    {
        var rubric = _rubricService.GetRubric(rubricId);

        _ = rubric.GetCriterion(criterionId);

        if (HasEvaluations(rubricId, criterionId))
            throw new InvalidOperationException(
                "Нельзя удалить критерий: по нему уже выставлены оценки.");

        _rubricService.DeleteCriterion(
            rubricId,
            criterionId);
    }

    public IReadOnlyCollection<Criterion> GetCriteria(Guid rubricId)
    {
        return _rubricService.GetCriteria(rubricId);
    }

    public Evaluation AddEvaluation(
        Guid studentId,
        Guid rubricId,
        Guid criterionId,
        Score score)
    {
        return _evaluationService.AddEvaluation(
            studentId,
            rubricId,
            criterionId,
            score);
    }

    public Evaluation GetEvaluation(Guid evaluationId)
    {
        return _evaluationService.GetEvaluation(evaluationId);
    }

    public IReadOnlyCollection<Evaluation> GetEvaluations(Guid studentId)
    {
        return _evaluationService.GetEvaluations(studentId);
    }

    public Evaluation ChangeEvaluation(Guid evaluationId, Score score)
    {
        return _evaluationService.ChangeEvaluation(
            evaluationId,
            score);
    }

    public void CancelEvaluation(Guid evaluationId)
    {
        _evaluationService.CancelEvaluation(evaluationId);
    }

    public GradeReport GetGradeReport(Guid studentId, Guid rubricId)
    {
        return _gradeReportService.CreateReport(studentId, rubricId);
    }

    public GradeReport ExportGradeReport(
        Guid studentId,
        Guid rubricId,
        string format,
        string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException(
                "Export format cannot be empty.",
                nameof(format));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException(
                "Destination path cannot be empty.",
                nameof(destinationPath));

        if (!_exporters.TryGetValue(format.Trim(), out var exporter))
            throw new NotSupportedException(
                $"Export format '{format}' is not supported.");

        var report = _gradeReportService.CreateReport(
            studentId,
            rubricId);

        exporter.Export(report, destinationPath);

        return report;
    }

    private bool HasEvaluations(
        Guid rubricId,
        Guid? criterionId = null)
    {
        foreach (var student in _studentService.GetStudents())
        {
            var evaluations = _evaluationService.GetEvaluations(
                student.Id);

            if (evaluations.Any(e =>
                e.RubricId == rubricId &&
                (!criterionId.HasValue ||
                 e.CriterionId == criterionId.Value)))
            {
                return true;
            }
        }

        return false;
    }
}
