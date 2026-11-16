using Pos.Application.Abstractions.Export;
using Pos.Application.Abstractions.Repositories;
using Pos.Application.Facades;
using Pos.Application.Services;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Domain.ValueObjects;

namespace Pos.Tests.Application.Support;

internal sealed class ApplicationTestContext
{
    public StudentService StudentService { get; }

    public RubricService RubricService { get; }

    public EvaluationService EvaluationService { get; }

    public GradeReportService GradeReportService { get; }

    public ApplicationTestContext()
    {
        var studentRepository = new InMemoryStudentRepository();
        var rubricRepository = new InMemoryRubricRepository();
        var evaluationRepository = new InMemoryEvaluationRepository();

        var gradeCalculator = new GradeCalculator();

        StudentService = new StudentService(
            studentRepository,
            evaluationRepository);

        RubricService = new RubricService(rubricRepository);

        EvaluationService = new EvaluationService(
            studentRepository,
            rubricRepository,
            evaluationRepository,
            new EvaluationFactory(),
            gradeCalculator);

        GradeReportService = new GradeReportService(
            StudentService,
            RubricService,
            EvaluationService,
            gradeCalculator);
    }

    public PosFacade CreateFacade(
        params IGradeReportExporter[] exporters)
    {
        return new PosFacade(
            StudentService,
            RubricService,
            EvaluationService,
            GradeReportService,
            exporters);
    }

    public (Student Student, Rubric Rubric, Criterion Criterion)
        CreateAssessment()
    {
        var student = StudentService.AddStudent("Иван Иванов");

        var rubric = RubricService.CreateRubric();

        var criterion = RubricService.AddCriterion(
            rubric.Id,
            "Контрольная работа",
            new Score(10m),
            new Weight(1m));

        return (student, rubric, criterion);
    }

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

    private sealed class InMemoryRubricRepository : IRubricRepository
    {
        private readonly Dictionary<Guid, Rubric> _rubrics = new();

        public void Add(Rubric rubric)
        {
            ArgumentNullException.ThrowIfNull(rubric);

            if (!_rubrics.TryAdd(rubric.Id, rubric))
                throw new InvalidOperationException(
                    "Rubric already exists.");
        }

        public void Update(Rubric rubric)
        {
            ArgumentNullException.ThrowIfNull(rubric);

            if (!_rubrics.ContainsKey(rubric.Id))
                throw new KeyNotFoundException(
                    "Rubric does not exist.");

            _rubrics[rubric.Id] = rubric;
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
