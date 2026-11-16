using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Application.Services;

public sealed class RubricService
{
    private readonly IRubricRepository _rubricRepository;

    public RubricService(IRubricRepository rubricRepository)
    {
        ArgumentNullException.ThrowIfNull(rubricRepository);

        _rubricRepository = rubricRepository;
    }

    public Rubric CreateRubric()
    {
        var rubric = new Rubric(Guid.NewGuid());

        _rubricRepository.Add(rubric);

        return rubric;
    }

    public Criterion AddCriterion(
        Guid rubricId,
        string name,
        Score maxScore,
        Weight weight)
    {
        if (rubricId == Guid.Empty)
        {
            throw new ArgumentException(
                "Rubric ID cannot be empty.",
                nameof(rubricId));
        }

        ArgumentNullException.ThrowIfNull(maxScore);
        ArgumentNullException.ThrowIfNull(weight);

        var rubric = GetRubric(rubricId);

        var criterion = new Criterion(
            Guid.NewGuid(),
            name,
            maxScore,
            weight);

        rubric.AddCriterion(criterion);

        _rubricRepository.Update(rubric);

        return criterion;
    }

    public Rubric GetRubric(Guid rubricId)
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

    public IReadOnlyCollection<Rubric> GetRubrics()
    {
        return _rubricRepository.GetAll();
    }

    public IReadOnlyCollection<Criterion> GetCriteria(Guid rubricId)
    {
        return GetRubric(rubricId).Criteria;
    }
}
