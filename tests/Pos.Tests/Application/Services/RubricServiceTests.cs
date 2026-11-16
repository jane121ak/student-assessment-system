using Pos.Application.Abstractions.Repositories;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Application.Services;

public class RubricServiceTests
{
    private readonly InMemoryRubricRepository _repository = new();
    private readonly RubricService _service;

    public RubricServiceTests()
    {
        _service = new RubricService(_repository);
    }

    [Fact]
    public void CreateRubric_ValidData_ReturnsAndStoresRubric()
    {
        var rubric = _service.CreateRubric();

        Assert.NotEqual(Guid.Empty, rubric.Id);
        Assert.Same(rubric, _repository.GetById(rubric.Id));
    }

    [Fact]
    public void GetRubric_ExistingId_ReturnsRubric()
    {
        var rubric = _service.CreateRubric();

        var result = _service.GetRubric(rubric.Id);

        Assert.Same(rubric, result);
    }

    [Fact]
    public void GetRubric_UnknownId_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(
            () => _service.GetRubric(Guid.NewGuid()));
    }

    [Fact]
    public void GetRubric_EmptyId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(
            () => _service.GetRubric(Guid.Empty));
    }

    [Fact]
    public void AddCriterion_ValidData_AddsAndStoresCriterion()
    {
        var rubric = _service.CreateRubric();

        var criterion = _service.AddCriterion(
            rubric.Id,
            "Контрольная работа",
            new Score(10m),
            new Weight(0.3m));

        Assert.Equal("Контрольная работа", criterion.Name);
        Assert.Single(_service.GetCriteria(rubric.Id));
        Assert.Equal(
            criterion.Id,
            _repository.GetById(rubric.Id)!.Criteria.Single().Id);
    }

    [Fact]
    public void AddCriterion_UnknownRubric_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            _service.AddCriterion(
                Guid.NewGuid(),
                "Контрольная работа",
                new Score(10m),
                new Weight(0.3m)));
    }

    [Fact]
    public void AddCriterion_InvalidMaxScore_ThrowsException()
    {
        var rubric = _service.CreateRubric();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.AddCriterion(
                rubric.Id,
                "Контрольная работа",
                new Score(0m),
                new Weight(0.3m)));

        Assert.Empty(_service.GetCriteria(rubric.Id));
    }

    [Fact]
    public void AddCriterion_AfterRejectedCriterion_ValidCriterionIsAccepted()
    {
        var rubric = _service.CreateRubric();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.AddCriterion(
                rubric.Id,
                "Контрольная работа",
                new Score(0m),
                new Weight(0.3m)));

        var criterion = _service.AddCriterion(
            rubric.Id,
            "Контрольная работа",
            new Score(10m),
            new Weight(0.3m));

        Assert.Equal("Контрольная работа", criterion.Name);
        Assert.Single(_service.GetCriteria(rubric.Id));
    }

    [Fact]
    public void GetCriteria_ReturnsAllCriteria()
    {
        var rubric = _service.CreateRubric();

        _service.AddCriterion(
            rubric.Id, "Контрольная", new Score(10m), new Weight(0.3m));

        _service.AddCriterion(
            rubric.Id, "Домашняя работа", new Score(20m), new Weight(0.7m));

        var criteria = _service.GetCriteria(rubric.Id);

        Assert.Equal(2, criteria.Count);
    }

    [Fact]
    public void GetRubrics_ReturnsAllRubrics()
    {
        _service.CreateRubric();
        _service.CreateRubric();

        Assert.Equal(2, _service.GetRubrics().Count);
    }

    private sealed class InMemoryRubricRepository : IRubricRepository
    {
        private readonly Dictionary<Guid, Rubric> _rubrics = new();

        public void Add(Rubric rubric)
        {
            ArgumentNullException.ThrowIfNull(rubric);

            if (!_rubrics.TryAdd(rubric.Id, rubric))
            {
                throw new InvalidOperationException(
                    "Rubric already exists.");
            }
        }

        public void Update(Rubric rubric)
        {
            ArgumentNullException.ThrowIfNull(rubric);

            if (!_rubrics.ContainsKey(rubric.Id))
            {
                throw new KeyNotFoundException(
                    "Rubric does not exist.");
            }

            _rubrics[rubric.Id] = rubric;
        }

        public void Remove(Guid id)
        {
            if (!_rubrics.Remove(id))
                throw new KeyNotFoundException(
                    "Rubric does not exist.");
        }


        public Rubric? GetById(Guid id)
        {
            return _rubrics.GetValueOrDefault(id);
        }

        public IReadOnlyCollection<Rubric> GetAll()
        {
            return _rubrics.Values.ToArray();
        }
    }
}
