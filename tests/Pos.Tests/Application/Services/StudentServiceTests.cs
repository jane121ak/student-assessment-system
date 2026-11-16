using Pos.Application.Abstractions.Repositories;
using Pos.Application.Services;
using Pos.Domain.Entities;

namespace Pos.Tests.Application.Services;

public class StudentServiceTests
{
    [Fact]
    public void AddStudent_ValidName_ReturnsAndStoresStudent()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        var student = service.AddStudent("Иван Иванов");

        Assert.Equal("Иван Иванов", student.FullName);
        Assert.Same(student, repository.GetById(student.Id));
    }

    [Fact]
    public void AddStudent_InvalidName_ThrowsException()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        Assert.Throws<ArgumentException>(
            () => service.AddStudent(" "));

        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void AddStudent_AfterRejectedName_ValidNameIsAccepted()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        Assert.Throws<ArgumentException>(
            () => service.AddStudent(" "));

        var student = service.AddStudent("Иван Иванов");

        Assert.Equal("Иван Иванов", student.FullName);
        Assert.Single(repository.GetAll());
    }

    [Fact]
    public void GetStudent_ExistingId_ReturnsStudent()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        var created = service.AddStudent("Иван Иванов");

        var result = service.GetStudent(created.Id);

        Assert.Same(created, result);
    }

    [Fact]
    public void GetStudent_UnknownId_ThrowsException()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        Assert.Throws<KeyNotFoundException>(
            () => service.GetStudent(Guid.NewGuid()));
    }

    [Fact]
    public void GetStudent_EmptyId_ThrowsException()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        Assert.Throws<ArgumentException>(
            () => service.GetStudent(Guid.Empty));
    }

    [Fact]
    public void GetStudents_ReturnsAllStudents()
    {
        var repository = new InMemoryStudentRepository();
        var service = new StudentService(repository);

        service.AddStudent("Иван Иванов");
        service.AddStudent("Пётр Петров");

        var students = service.GetStudents();

        Assert.Equal(2, students.Count);
    }

    private sealed class InMemoryStudentRepository : IStudentRepository
    {
        private readonly Dictionary<Guid, Student> _students = new();

        public void Add(Student student)
        {
            ArgumentNullException.ThrowIfNull(student);

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
}
