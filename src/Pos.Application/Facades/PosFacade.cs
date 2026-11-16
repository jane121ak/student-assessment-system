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
}
