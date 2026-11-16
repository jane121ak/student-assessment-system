using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Domain.ValueObjects;

namespace Pos.Application.Services;

public sealed class EvaluationService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IRubricRepository _rubricRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly EvaluationFactory _evaluationFactory;
    private readonly GradeCalculator _gradeCalculator;

    public EvaluationService(
        IStudentRepository studentRepository,
        IRubricRepository rubricRepository,
        IEvaluationRepository evaluationRepository,
        EvaluationFactory evaluationFactory,
        GradeCalculator gradeCalculator)
    {
        ArgumentNullException.ThrowIfNull(studentRepository);
        ArgumentNullException.ThrowIfNull(rubricRepository);
        ArgumentNullException.ThrowIfNull(evaluationRepository);
        ArgumentNullException.ThrowIfNull(evaluationFactory);
        ArgumentNullException.ThrowIfNull(gradeCalculator);

        _studentRepository = studentRepository;
        _rubricRepository = rubricRepository;
        _evaluationRepository = evaluationRepository;
        _evaluationFactory = evaluationFactory;
        _gradeCalculator = gradeCalculator;
    }

    public Evaluation AddEvaluation(Guid studentId, Guid rubricId, Guid criterionId, Score score)
    {
        ArgumentNullException.ThrowIfNull(score);

        var student = GetStudent(studentId);
        var rubric = GetRubric(rubricId);

        if (criterionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Criterion ID cannot be empty.",
                nameof(criterionId));
        }

        rubric.GetCriterion(criterionId);

        var existingEvaluations =
            _evaluationRepository.GetByStudentId(studentId);

        if (existingEvaluations.Any(e =>
            e.RubricId == rubricId &&
            e.CriterionId == criterionId))
        {
            throw new InvalidOperationException(
                "Evaluation for this criterion already exists.");
        }

        var evaluation = _evaluationFactory.Create(
            student,
            rubric,
            criterionId,
            score);

        _evaluationRepository.Add(evaluation);

        return evaluation;
    }

    public Evaluation GetEvaluation(Guid evaluationId)
    {
        if (evaluationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Evaluation ID cannot be empty.",
                nameof(evaluationId));
        }

        return _evaluationRepository.GetById(evaluationId)
            ?? throw new KeyNotFoundException(
                $"Evaluation with ID '{evaluationId}' was not found.");
    }

    public IReadOnlyCollection<Evaluation> GetEvaluations(Guid studentId)
    {
        GetStudent(studentId);

        return _evaluationRepository
            .GetByStudentId(studentId)
            .ToArray();
    }

    public Evaluation ChangeEvaluation(Guid evaluationId, Score score)
    {
        ArgumentNullException.ThrowIfNull(score);

        var evaluation = GetEvaluation(evaluationId);
        var rubric = GetRubric(evaluation.RubricId);

        evaluation.ChangeScore(score, rubric);

        _evaluationRepository.Update(evaluation);

        return evaluation;
    }

    public void CancelEvaluation(Guid evaluationId)
    {
        var evaluation = GetEvaluation(evaluationId);

        _evaluationRepository.Remove(evaluation.Id);
    }

    public decimal? GetFinalGrade(Guid studentId, Guid rubricId)
    {
        var student = GetStudent(studentId);
        var rubric = GetRubric(rubricId);

        var evaluations = _evaluationRepository
            .GetByStudentId(student.Id)
            .Where(e => e.RubricId == rubric.Id)
            .ToArray();

        return _gradeCalculator.Calculate(
            student.Id,
            rubric,
            evaluations);
    }

    private Student GetStudent(Guid studentId)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(studentId));
        }

        return _studentRepository.GetById(studentId)
            ?? throw new KeyNotFoundException(
                $"Student with ID '{studentId}' was not found.");
    }

    private Rubric GetRubric(Guid rubricId)
    {
        if (rubricId == Guid.Empty)
        {
            throw new ArgumentException(
                "Rubric ID cannot be empty.",
                nameof(rubricId));
        }

        return _rubricRepository.GetById(rubricId)
            ?? throw new KeyNotFoundException(
                $"Rubric with ID '{rubricId}' was not found.");
    }
}
