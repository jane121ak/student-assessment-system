using Pos.Application.Abstractions.Repositories;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.ValueObjects;

namespace Pos.Tests.Application.Services;

public class StudentServiceTests
{
    [Fact]
    public void AddStudent_ValidName_ReturnsAndStoresStudent()
    {
        var context = CreateContext();

        var student = context.Service.AddStudent("Иван Иванов");

        Assert.Equal("Иван Иванов", student.FullName);
        Assert.Same(student, context.Students.GetById(student.Id));
    }

    [Fact]
    public void AddStudent_InvalidName_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentException>(
            () => context.Service.AddStudent(" "));

        Assert.Empty(context.Students.GetAll());
    }

    [Fact]
    public void AddStudent_AfterRejectedName_ValidNameIsAccepted()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentException>(
            () => context.Service.AddStudent(" "));

        var student = context.Service.AddStudent("Иван Иванов");

        Assert.Equal("Иван Иванов", student.FullName);
        Assert.Single(context.Students.GetAll());
    }

    [Fact]
    public void GetStudent_ExistingId_ReturnsStudent()
    {
        var context = CreateContext();

        var created = context.Service.AddStudent("Иван Иванов");

        var result = context.Service.GetStudent(created.Id);

        Assert.Same(created, result);
    }

    [Fact]
    public void GetStudent_UnknownId_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<KeyNotFoundException>(
            () => context.Service.GetStudent(Guid.NewGuid()));
    }

    [Fact]
    public void GetStudent_EmptyId_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentException>(
            () => context.Service.GetStudent(Guid.Empty));
    }

    [Fact]
    public void GetStudents_ReturnsAllStudents()
    {
        var context = CreateContext();

        context.Service.AddStudent("Иван Иванов");
        context.Service.AddStudent("Пётр Петров");

        var students = context.Service.GetStudents();

        Assert.Equal(2, students.Count);
    }

    [Fact]
    public void DeleteStudent_ExistingStudent_RemovesStudent()
    {
        var context = CreateContext();

        var student = context.Service.AddStudent("Иван Иванов");

        context.Service.DeleteStudent(student.Id);

        Assert.Null(context.Students.GetById(student.Id));
        Assert.Empty(context.Students.GetAll());
    }

    [Fact]
    public void DeleteStudent_UnknownId_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<KeyNotFoundException>(
            () => context.Service.DeleteStudent(Guid.NewGuid()));
    }

    [Fact]
    public void DeleteStudent_EmptyId_ThrowsException()
    {
        var context = CreateContext();

        Assert.Throws<ArgumentException>(
            () => context.Service.DeleteStudent(Guid.Empty));
    }

    [Fact]
    public void DeleteStudent_HasEvaluations_ThrowsException()
    {
        var context = CreateContext();

        var student = context.Service.AddStudent("Иван Иванов");
        var evaluation = CreateEvaluation(student);

        context.Evaluations.Add(evaluation);

        Assert.Throws<InvalidOperationException>(
            () => context.Service.DeleteStudent(student.Id));

        Assert.NotNull(context.Students.GetById(student.Id));
        Assert.Single(context.Evaluations.GetByStudentId(student.Id));
    }

    [Fact]
    public void DeleteStudent_AfterEvaluationsRemoved_DeletesStudent()
    {
        var context = CreateContext();

        var student = context.Service.AddStudent("Иван Иванов");
        var evaluation = CreateEvaluation(student);

        context.Evaluations.Add(evaluation);

        Assert.Throws<InvalidOperationException>(
            () => context.Service.DeleteStudent(student.Id));

        context.Evaluations.Remove(evaluation.Id);

        context.Service.DeleteStudent(student.Id);

        Assert.Null(context.Students.GetById(student.Id));
        Assert.Empty(context.Students.GetAll());
    }

    private static TestContext CreateContext()
    {
        var students = new InMemoryStudentRepository();
        var evaluations = new InMemoryEvaluationRepository();

        var service = new StudentService(students, evaluations);

        return new TestContext(service, students, evaluations);
    }

    private static Evaluation CreateEvaluation(Student student)
    {
        var rubric = new Rubric(Guid.NewGuid());

        var criterion = new Criterion(
            Guid.NewGuid(),
            "Контрольная работа",
            new Score(10m),
            new Weight(1m));

        rubric.AddCriterion(criterion);

        return new EvaluationFactory().Create(
            student,
            rubric,
            criterion.Id,
            new Score(8m));
    }

    private sealed record TestContext(
        StudentService Service,
        InMemoryStudentRepository Students,
        InMemoryEvaluationRepository Evaluations);

    private sealed class InMemoryStudentRepository : IStudentRepository
    {
        private readonly Dictionary<Guid, Student> _students = new();

        public void Add(Student student)
        {
            ArgumentNullException.ThrowIfNull(student);

            if (!_students.TryAdd(student.Id, student))
                throw new InvalidOperationException(
                    "Student already exists.");
        }

        public Student? GetById(Guid id)
        {
            return _students.GetValueOrDefault(id);
        }

        public IReadOnlyCollection<Student> GetAll()
        {
            return _students.Values.ToArray();
        }

        public void Remove(Guid id)
        {
            if (!_students.Remove(id))
                throw new KeyNotFoundException(
                    "Student does not exist.");
        }
    }

    private sealed class InMemoryEvaluationRepository : IEvaluationRepository
    {
        private readonly Dictionary<Guid, Evaluation> _evaluations = new();

        public void Add(Evaluation evaluation)
        {
            ArgumentNullException.ThrowIfNull(evaluation);

            if (!_evaluations.TryAdd(evaluation.Id, evaluation))
                throw new InvalidOperationException(
                    "Evaluation already exists.");
        }

        public void Update(Evaluation evaluation)
        {
            ArgumentNullException.ThrowIfNull(evaluation);

            if (!_evaluations.ContainsKey(evaluation.Id))
                throw new KeyNotFoundException(
                    "Evaluation does not exist.");

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
                throw new KeyNotFoundException(
                    "Evaluation does not exist.");
        }
    }
}
