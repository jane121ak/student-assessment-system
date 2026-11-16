using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

public sealed class StudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEvaluationRepository _evaluationRepository;

    public StudentService(
        IStudentRepository studentRepository,
        IEvaluationRepository evaluationRepository)
    {
        ArgumentNullException.ThrowIfNull(studentRepository);
        ArgumentNullException.ThrowIfNull(evaluationRepository);

        _studentRepository = studentRepository;
        _evaluationRepository = evaluationRepository;
    }

    public Student AddStudent(string fullName)
    {
        var student = new Student(Guid.NewGuid(), fullName);

        _studentRepository.Add(student);

        return student;
    }

    public Student GetStudent(Guid studentId)
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

    public IReadOnlyCollection<Student> GetStudents()
    {
        return _studentRepository.GetAll();
    }

    public void DeleteStudent(Guid studentId)
    {
        var student = GetStudent(studentId);

        var evaluations =
            _evaluationRepository.GetByStudentId(student.Id);

        if (evaluations.Count > 0)
        {
            throw new InvalidOperationException(
                "Cannot delete a student who has evaluations. Cancel evaluations first.");
        }

        _studentRepository.Remove(student.Id);
    }
}
