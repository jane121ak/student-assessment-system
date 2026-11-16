using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;

public sealed class Rubric
{
    private readonly List<Criterion> _criteria = new();

    public Guid Id { get; }

    public IReadOnlyCollection<Criterion> Criteria =>
        _criteria.AsReadOnly();

    public Rubric(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Rubric ID cannot be empty.",
                nameof(id));
        }

        Id = id;
    }

    public void AddCriterion(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);

        if (_criteria.Exists(c => c.Id == criterion.Id))
        {
            throw new InvalidOperationException(
                "Criterion already exists in this rubric.");
        }

        _criteria.Add(criterion);
    }

    public void RemoveCriterion(Guid criterionId)
    {
        if (criterionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Criterion ID cannot be empty.",
                nameof(criterionId));
        }

        var criterion = _criteria.Find(c => c.Id == criterionId);

        if (criterion is null)
        {
            throw new KeyNotFoundException(
                $"Criterion with ID '{criterionId}' was not found.");
        }

        _criteria.Remove(criterion);
    }

    public bool ContainsCriterion(Guid id)
    {
        return _criteria.Exists(c => c.Id == id);
    }

    public Criterion GetCriterion(Guid id)
    {
        var criterion = _criteria.Find(c => c.Id == id);

        return criterion ?? throw new KeyNotFoundException(
            $"Criterion with ID '{id}' was not found.");
    }
}
