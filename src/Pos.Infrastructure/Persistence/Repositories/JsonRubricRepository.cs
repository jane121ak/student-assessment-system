using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Persistence.Repositories;

public sealed class JsonRubricRepository : IRubricRepository
{
    private readonly JsonFileStore<RubricRecord> _store;

    public JsonRubricRepository(string filePath)
    {
        _store = new JsonFileStore<RubricRecord>(filePath);
    }

    public void Add(Rubric rubric)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        var records = _store.ReadAll();
        var existing = records.Select(ToDomain).ToArray();

        if (existing.Any(r => r.Id == rubric.Id))
            throw new InvalidOperationException(
                "Rubric already exists.");

        records.Add(ToRecord(rubric));

        _store.WriteAll(records);
    }

    public void Update(Rubric rubric)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        var records = _store.ReadAll();
        var existing = records.Select(ToDomain).ToArray();

        var index = existing.ToList()
            .FindIndex(r => r.Id == rubric.Id);

        if (index < 0)
            throw new KeyNotFoundException(
                "Rubric does not exist.");

        records[index] = ToRecord(rubric);

        _store.WriteAll(records);
    }

    public Rubric? GetById(Guid id)
    {
        return LoadAll().FirstOrDefault(r => r.Id == id);
    }

    public IReadOnlyCollection<Rubric> GetAll()
    {
        return LoadAll();
    }

    private Rubric[] LoadAll()
    {
        return _store.ReadAll()
            .Select(ToDomain)
            .ToArray();
    }

    private static Rubric ToDomain(RubricRecord record)
    {
        if (record is null || record.Criteria is null)
            throw new InvalidDataException(
                "Rubric record contains invalid data.");

        try
        {
            var rubric = new Rubric(record.Id);

            foreach (var criterionRecord in record.Criteria)
            {
                if (criterionRecord is null)
                    throw new InvalidDataException(
                        "Criterion record cannot be null.");

                var criterion = new Criterion(
                    criterionRecord.Id,
                    criterionRecord.Name,
                    new Score(criterionRecord.MaxScore),
                    new Weight(criterionRecord.Weight));

                rubric.AddCriterion(criterion);
            }

            return rubric;
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            throw new InvalidDataException(
                "Stored rubric contains invalid data.",
                exception);
        }
    }

    private static RubricRecord ToRecord(Rubric rubric)
    {
        return new RubricRecord
        {
            Id = rubric.Id,
            Criteria = rubric.Criteria
                .Select(c => new CriterionRecord
                {
                    Id = c.Id,
                    Name = c.Name,
                    MaxScore = c.MaxScore.Value,
                    Weight = c.Weight.Value
                })
                .ToList()
        };
    }

    private sealed class RubricRecord
    {
        public Guid Id { get; set; }

        public List<CriterionRecord> Criteria { get; set; } = new();

        public RubricRecord()
        {
        }
    }

    private sealed class CriterionRecord
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal MaxScore { get; set; }

        public decimal Weight { get; set; }

        public CriterionRecord()
        {
        }
    }
}
