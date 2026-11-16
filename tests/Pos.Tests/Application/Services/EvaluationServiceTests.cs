using Pos.Application.Abstractions.Repositories;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Domain.ValueObjects;

namespace Pos.Tests.Application.Services;

public class EvaluationServiceTests
{
    private readonly InMemoryStudentRepository _students = new();
    private readonly InMemoryRubricRepository _rubrics = new();
    private readonly InMemoryEvaluationRepository _evaluations = new();

    private readonly EvaluationService _service;

    public EvaluationServiceTests()
    {
        _service = new EvaluationService(
            _students,
            _rubrics,
            _evaluations,
            new EvaluationFactory(),
            new GradeCalculator());
    }

    [Fact]
    public void AddEvaluation_ValidData_ReturnsAndStoresEvaluation()
    {
        var context = CreateContext();

        var evaluation = AddEvaluation(context, 8m);

        Assert.Equal(context.Student.Id, evaluation.StudentId);
        Assert.Equal(context.Rubric.Id, evaluation.RubricId);
        Assert.Equal(context.Criterion.Id, evaluation.CriterionId);
        Assert.Equal(8m, evaluation.Score.Value);
        Assert.Same(evaluation, _evaluations.GetById(evaluation.Id));
    }

    [Fact]
    public void AddEvaluation_UnknownStudent_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<KeyNotFoundException>(() =>
            _service.AddEvaluation(
                Guid.NewGuid(),
                context.Rubric.Id,
                context.Criterion.Id,
                new Score(8m)));

        Assert.Empty(_evaluations.GetByStudentId(context.Student.Id));
    }

    [Fact]
    public void AddEvaluation_UnknownRubric_ThrowsException()
    {
        var student = CreateStudent();

        Assert.Throws<KeyNotFoundException>(() =>
            _service.AddEvaluation(
                student.Id,
                Guid.NewGuid(),
                Guid.NewGuid(),
                new Score(8m)));
    }

    [Fact]
    public void AddEvaluation_UnknownCriterion_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<KeyNotFoundException>(() =>
            _service.AddEvaluation(
                context.Student.Id,
                context.Rubric.Id,
                Guid.NewGuid(),
                new Score(8m)));

        Assert.Empty(_evaluations.GetByStudentId(context.Student.Id));
    }

    [Fact]
    public void AddEvaluation_ScoreAboveMaximum_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AddEvaluation(context, 11m));

        Assert.Empty(_evaluations.GetByStudentId(context.Student.Id));
    }

    [Fact]
    public void AddEvaluation_DuplicateCriterion_ThrowsException()
    {
        var context = CreateContext();

        AddEvaluation(context, 8m);

        Assert.Throws<InvalidOperationException>(() =>
            AddEvaluation(context, 9m));

        Assert.Single(_evaluations.GetByStudentId(context.Student.Id));
    }

    [Fact]
    public void AddEvaluation_AfterRejectedScore_ValidScoreIsAccepted()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AddEvaluation(context, 11m));

        var evaluation = AddEvaluation(context, 8m);

        Assert.Equal(8m, evaluation.Score.Value);
        Assert.Single(_evaluations.GetByStudentId(context.Student.Id));
    }

    [Fact]
    public void GetEvaluation_ExistingId_ReturnsEvaluation()
    {
        var context = CreateContext();
        var evaluation = AddEvaluation(context, 8m);

        var result = _service.GetEvaluation(evaluation.Id);

        Assert.Same(evaluation, result);
    }

    [Fact]
    public void GetEvaluation_UnknownId_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(
            () => _service.GetEvaluation(Guid.NewGuid()));
    }

    [Fact]
    public void GetEvaluations_ReturnsOnlyRequestedStudentEvaluations()
    {
        var first = CreateContext();
        var second = CreateContext();

        AddEvaluation(first, 8m);
        AddEvaluation(second, 6m);

        var result = _service.GetEvaluations(first.Student.Id);

        Assert.Single(result);
        Assert.All(result, e =>
            Assert.Equal(first.Student.Id, e.StudentId));
    }

    [Fact]
    public void ChangeEvaluation_ValidScore_UpdatesEvaluation()
    {
        var context = CreateContext();
        var evaluation = AddEvaluation(context, 5m);

        var result = _service.ChangeEvaluation(
            evaluation.Id,
            new Score(8m));

        Assert.Equal(8m, result.Score.Value);
        Assert.Equal(8m, _evaluations.GetById(evaluation.Id)!.Score.Value);
    }

    [Fact]
    public void ChangeEvaluation_InvalidScore_LeavesOriginalScoreUnchanged()
    {
        var context = CreateContext();
        var evaluation = AddEvaluation(context, 5m);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.ChangeEvaluation(
                evaluation.Id,
                new Score(11m)));

        Assert.Equal(5m, evaluation.Score.Value);
    }

    [Fact]
    public void ChangeEvaluation_AfterRejectedScore_ValidScoreIsAccepted()
    {
        var context = CreateContext();
        var evaluation = AddEvaluation(context, 5m);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.ChangeEvaluation(
                evaluation.Id,
                new Score(11m)));

        var result = _service.ChangeEvaluation(
            evaluation.Id,
            new Score(8m));

        Assert.Equal(8m, result.Score.Value);
    }

    [Fact]
    public void CancelEvaluation_ExistingEvaluation_RemovesIt()
    {
        var context = CreateContext();
        var evaluation = AddEvaluation(context, 8m);

        _service.CancelEvaluation(evaluation.Id);

        Assert.Null(_evaluations.GetById(evaluation.Id));
        Assert.Empty(_evaluations.GetByStudentId(context.Student.Id));
    }

    [Fact]
    public void CancelEvaluation_UnknownId_ThrowsException()
    {
        Assert.Throws<KeyNotFoundException>(
            () => _service.CancelEvaluation(Guid.NewGuid()));
    }

    [Fact]
    public void GetFinalGrade_ValidEvaluations_ReturnsWeightedAverage()
    {
        var student = CreateStudent();
        var rubric = new Rubric(Guid.NewGuid());

        var first = CreateCriterion(10m, 0.25m);
        var second = CreateCriterion(20m, 0.75m);

        rubric.AddCriterion(first);
        rubric.AddCriterion(second);

        _rubrics.Add(rubric);

        _service.AddEvaluation(
            student.Id, rubric.Id, first.Id, new Score(8m));

        _service.AddEvaluation(
            student.Id, rubric.Id, second.Id, new Score(10m));

        var result = _service.GetFinalGrade(student.Id, rubric.Id);

        Assert.Equal(0.575m, result);
    }

    [Fact]
    public void GetFinalGrade_NoEvaluations_ReturnsNull()
    {
        var context = CreateContext();

        var result = _service.GetFinalGrade(
            context.Student.Id,
            context.Rubric.Id);

        Assert.Null(result);
    }

    private (Student Student, Rubric Rubric, Criterion Criterion)
        CreateContext()
    {
        var student = CreateStudent();
        var rubric = new Rubric(Guid.NewGuid());
        var criterion = CreateCriterion(10m, 1m);

        rubric.AddCriterion(criterion);
        _rubrics.Add(rubric);

        return (student, rubric, criterion);
    }

    private Student CreateStudent()
    {
        var student = new Student(
            Guid.NewGuid(),
            "Иван Иванов");

        _students.Add(student);

        return student;
    }

    private static Criterion CreateCriterion(
        decimal maxScore,
        decimal weight)
    {
        return new Criterion(
            Guid.NewGuid(),
            "Контрольная работа",
            new Score(maxScore),
            new Weight(weight));
    }

    private Evaluation AddEvaluation(
        (Student Student, Rubric Rubric, Criterion Criterion) context,
        decimal score)
    {
        return _service.AddEvaluation(
            context.Student.Id,
            context.Rubric.Id,
            context.Criterion.Id,
            new Score(score));
    }

    private sealed class InMemoryStudentRepository : IStudentRepository
    {
        private readonly Dictionary<Guid, Student> _students = new();

        public void Add(Student student)
        {
            if (!_students.TryAdd(student.Id, student))
            {
                throw new InvalidOperationException(
                    "Student already exists.");
            }
        }

        public Student? GetById(Guid id)
        {
            return _students.GetValueOrDefault(id);
        }

        public IReadOnlyCollection<Student> GetAll()
        {
            return _students.Values.ToArray();
        }
    }

    private sealed class InMemoryRubricRepository : IRubricRepository
    {
        private readonly Dictionary<Guid, Rubric> _rubrics = new();

        public void Add(Rubric rubric)
        {
            if (!_rubrics.TryAdd(rubric.Id, rubric))
            {
                throw new InvalidOperationException(
                    "Rubric already exists.");
            }
        }

        public void Update(Rubric rubric)
        {
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

    private sealed class InMemoryEvaluationRepository : IEvaluationRepository
    {
        private readonly Dictionary<Guid, Evaluation> _evaluations = new();

        public void Add(Evaluation evaluation)
        {
            if (!_evaluations.TryAdd(evaluation.Id, evaluation))
            {
                throw new InvalidOperationException(
                    "Evaluation already exists.");
            }
        }

        public void Update(Evaluation evaluation)
        {
            if (!_evaluations.ContainsKey(evaluation.Id))
            {
                throw new KeyNotFoundException(
                    "Evaluation does not exist.");
            }

            _evaluations[evaluation.Id] = evaluation;
        }

        public Evaluation? GetById(Guid id)
        {
            return _evaluations.GetValueOrDefault(id);
        }

        public IReadOnlyCollection<Evaluation> GetByStudentId(Guid studentId)
        {
            return _evaluations.Values
                .Where(e => e.StudentId == studentId)
                .ToArray();
        }

        public void Remove(Guid id)
        {
            if (!_evaluations.Remove(id))
            {
                throw new KeyNotFoundException(
                    "Evaluation does not exist.");
            }
        }
    }
}
