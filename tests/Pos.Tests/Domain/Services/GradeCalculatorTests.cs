using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests.Domain.Services;

public class GradeCalculatorTests
{
    private readonly GradeCalculator _calculator = new();

    [Fact]
    public void Calculate_ValidEvaluations_ReturnsWeightedAverage()
    {
        var student = CreateStudent();
        var rubric = CreateRubric();

        var first = AddCriterion(rubric, 10m, 0.2m);
        var second = AddCriterion(rubric, 20m, 0.3m);
        var third = AddCriterion(rubric, 20m, 0.5m);

        var evaluations = new[]
        {
            CreateEvaluation(student, rubric, first, 8m),
            CreateEvaluation(student, rubric, second, 15m),
            CreateEvaluation(student, rubric, third, 18m)
        };

        var result = _calculator.Calculate(
            student.Id, rubric, evaluations);

        Assert.Equal(0.835m, result);
    }

    [Fact]
    public void Calculate_MissingEvaluation_ExcludesItsWeight()
    {
        var student = CreateStudent();
        var rubric = CreateRubric();

        var first = AddCriterion(rubric, 10m, 0.2m);
        AddCriterion(rubric, 20m, 0.8m);

        var evaluations = new[]
        {
            CreateEvaluation(student, rubric, first, 8m)
        };

        var result = _calculator.Calculate(
            student.Id, rubric, evaluations);

        Assert.Equal(0.8m, result);
    }

    [Fact]
    public void Calculate_NoEvaluations_ReturnsNull()
    {
        var student = CreateStudent();
        var rubric = CreateRubric();

        var result = _calculator.Calculate(
            student.Id,
            rubric,
            Array.Empty<Evaluation>());

        Assert.Null(result);
    }

    [Fact]
    public void Calculate_WrongStudent_ThrowsException()
    {
        var student = CreateStudent();
        var anotherStudent = CreateStudent();
        var rubric = CreateRubric();

        var criterion = AddCriterion(rubric, 10m, 0.2m);

        var evaluations = new[]
        {
            CreateEvaluation(student, rubric, criterion, 8m)
        };

        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(
                anotherStudent.Id, rubric, evaluations));
    }

    [Fact]
    public void Calculate_WrongRubric_ThrowsException()
    {
        var student = CreateStudent();
        var rubric = CreateRubric();
        var anotherRubric = CreateRubric();

        var criterion = AddCriterion(rubric, 10m, 0.2m);

        var evaluations = new[]
        {
            CreateEvaluation(student, rubric, criterion, 8m)
        };

        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(
                student.Id, anotherRubric, evaluations));
    }

    [Fact]
    public void Calculate_DuplicateCriterion_ThrowsException()
    {
        var student = CreateStudent();
        var rubric = CreateRubric();

        var criterion = AddCriterion(rubric, 10m, 0.2m);

        var evaluations = new[]
        {
            CreateEvaluation(student, rubric, criterion, 8m),
            CreateEvaluation(student, rubric, criterion, 9m)
        };

        Assert.Throws<InvalidOperationException>(() =>
            _calculator.Calculate(
                student.Id, rubric, evaluations));
    }

    [Fact]
    public void Calculate_AfterRejectedStudent_CorrectStudentIsAccepted()
    {
        var student = CreateStudent();
        var anotherStudent = CreateStudent();
        var rubric = CreateRubric();

        var criterion = AddCriterion(rubric, 10m, 0.2m);

        var evaluations = new[]
        {
        CreateEvaluation(student, rubric, criterion, 8m)
    };

        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(
                anotherStudent.Id,
                rubric,
                evaluations));

        var result = _calculator.Calculate(
            student.Id,
            rubric,
            evaluations);

        Assert.Equal(0.8m, result);
    }

    private static Student CreateStudent()
    {
        return new Student(Guid.NewGuid(), "Иван Иванов");
    }

    private static Rubric CreateRubric()
    {
        return new Rubric(Guid.NewGuid());
    }

    private static Criterion AddCriterion(
        Rubric rubric,
        decimal maxScore,
        decimal weight)
    {
        var criterion = new Criterion(
            Guid.NewGuid(),
            "Критерий",
            new Score(maxScore),
            new Weight(weight));

        rubric.AddCriterion(criterion);

        return criterion;
    }

    private static Evaluation CreateEvaluation(
        Student student,
        Rubric rubric,
        Criterion criterion,
        decimal score)
    {
        return new EvaluationFactory().Create(
            student,
            rubric,
            criterion.Id,
            new Score(score));
    }
}
