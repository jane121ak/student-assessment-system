using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Persistence.Repositories;

public sealed class JsonEvaluationRepository : IEvaluationRepository
{
    private readonly JsonFileStore<EvaluationRecord> _store;
    private readonly IRubricRepository _rubricRepository;
    private readonly EvaluationFactory _evaluationFactory;

    public JsonEvaluationRepository(
        string filePath,
        IRubricRepository rubricRepository,
        EvaluationFactory evaluationFactory)
    {
        ArgumentNullException.ThrowIfNull(rubricRepository);
        ArgumentNullException.ThrowIfNull(evaluationFactory);

        _store = new JsonFileStore<EvaluationRecord>(filePath);
        _rubricRepository = rubricRepository;
        _evaluationFactory = evaluationFactory;
    }

    public void Add(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var records = _store.ReadAll();
        var existing = records.Select(ToDomain).ToArray();

        if (existing.Any(e => e.Id == evaluation.Id))
            throw new InvalidOperationException(
                "Evaluation already exists.");

        EnsureUniqueCriterion(existing, evaluation);

        Validate(evaluation);

        records.Add(ToRecord(evaluation));

        _store.WriteAll(records);
    }

    public void Update(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var records = _store.ReadAll();
        var existing = records.Select(ToDomain).ToArray();

        var index = existing.ToList()
            .FindIndex(e => e.Id == evaluation.Id);

        if (index < 0)
            throw new KeyNotFoundException(
                "Evaluation does not exist.");

        EnsureUniqueCriterion(
            existing.Where(e => e.Id != evaluation.Id),
            evaluation);

        Validate(evaluation);

        records[index] = ToRecord(evaluation);

        _store.WriteAll(records);
    }

    public Evaluation? GetById(Guid id)
    {
        return LoadAll().FirstOrDefault(e => e.Id == id);
    }

    public IReadOnlyCollection<Evaluation> GetByStudentId(Guid studentId)
    {
        return LoadAll()
            .Where(e => e.StudentId == studentId)
            .ToArray();
    }

    public void Remove(Guid id)
    {
        var records = _store.ReadAll();
        var existing = records.Select(ToDomain).ToArray();

        var index = existing.ToList()
            .FindIndex(e => e.Id == id);

        if (index < 0)
            throw new KeyNotFoundException(
                "Evaluation does not exist.");

        records.RemoveAt(index);

        _store.WriteAll(records);
    }

    private Evaluation[] LoadAll()
    {
        return _store.ReadAll()
            .Select(ToDomain)
            .ToArray();
    }

    private Evaluation ToDomain(EvaluationRecord record)
    {
        if (record is null)
            throw new InvalidDataException(
                "Evaluation record cannot be null.");

        var rubric = _rubricRepository.GetById(record.RubricId);

        if (rubric is null)
            throw new InvalidDataException(
                $"Rubric '{record.RubricId}' was not found.");

        try
        {
            return _evaluationFactory.Reconstitute(
                record.Id,
                record.StudentId,
                rubric,
                record.CriterionId,
                new Score(record.Score));
        }
        catch (Exception exception) when (
            exception is ArgumentException or KeyNotFoundException)
        {
            throw new InvalidDataException(
                "Stored evaluation contains invalid data.",
                exception);
        }
    }

    private void Validate(Evaluation evaluation)
    {
        var rubric = _rubricRepository.GetById(evaluation.RubricId)
            ?? throw new InvalidOperationException(
                "Evaluation rubric does not exist.");

        try
        {
            _evaluationFactory.Reconstitute(
                evaluation.Id,
                evaluation.StudentId,
                rubric,
                evaluation.CriterionId,
                evaluation.Score);
        }
        catch (Exception exception) when (
            exception is ArgumentException or KeyNotFoundException)
        {
            throw new InvalidOperationException(
                "Evaluation does not satisfy domain invariants.",
                exception);
        }
    }

    private static void EnsureUniqueCriterion(
        IEnumerable<Evaluation> existing,
        Evaluation evaluation)
    {
        if (existing.Any(e =>
            e.StudentId == evaluation.StudentId &&
            e.RubricId == evaluation.RubricId &&
            e.CriterionId == evaluation.CriterionId))
        {
            throw new InvalidOperationException(
                "Evaluation for this criterion already exists.");
        }
    }

    private static EvaluationRecord ToRecord(Evaluation evaluation)
    {
        return new EvaluationRecord
        {
            Id = evaluation.Id,
            StudentId = evaluation.StudentId,
            RubricId = evaluation.RubricId,
            CriterionId = evaluation.CriterionId,
            Score = evaluation.Score.Value
        };
    }

    private sealed class EvaluationRecord
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }

        public Guid RubricId { get; set; }

        public Guid CriterionId { get; set; }

        public decimal Score { get; set; }

        public EvaluationRecord()
        {
        }
    }
}
