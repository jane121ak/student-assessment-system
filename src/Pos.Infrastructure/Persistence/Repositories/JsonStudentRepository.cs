using Pos.Application.Abstractions.Repositories;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Persistence.Repositories;

public sealed class JsonStudentRepository : IStudentRepository
{
    private readonly JsonFileStore<StudentRecord> _store;

    public JsonStudentRepository(string filePath)
    {
        _store = new JsonFileStore<StudentRecord>(filePath);
    }

    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        var records = _store.ReadAll();
        var existing = records.Select(ToDomain).ToArray();

        if (existing.Any(s => s.Id == student.Id))
            throw new InvalidOperationException(
                "Student already exists.");

        records.Add(new StudentRecord
        {
            Id = student.Id,
            FullName = student.FullName
        });

        _store.WriteAll(records);
    }

    public Student? GetById(Guid id)
    {
        return LoadAll().FirstOrDefault(s => s.Id == id);
    }

    public IReadOnlyCollection<Student> GetAll()
    {
        return LoadAll();
    }

    public void Remove(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(id));

        var records = _store.ReadAll();

        var index = records.FindIndex(
            record => ToDomain(record).Id == id);

        if (index < 0)
            throw new KeyNotFoundException(
                $"Student with ID '{id}' was not found.");

        records.RemoveAt(index);

        _store.WriteAll(records);
    }

    private Student[] LoadAll()
    {
        return _store.ReadAll()
            .Select(ToDomain)
            .ToArray();
    }

    private static Student ToDomain(StudentRecord record)
    {
        if (record is null)
            throw new InvalidDataException(
                "Student record cannot be null.");

        try
        {
            return new Student(record.Id, record.FullName);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException(
                "Stored student contains invalid data.",
                exception);
        }
    }

    private sealed class StudentRecord
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public StudentRecord()
        {
        }
    }
}
