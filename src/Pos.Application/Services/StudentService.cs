using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

public sealed class StudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        ArgumentNullException.ThrowIfNull(studentRepository);

        _studentRepository = studentRepository;
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
}
