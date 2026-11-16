namespace Pos.Domain.Entities;

public sealed class Student
{
    public Guid Id { get; }
    public string FullName { get; }

    public Student(Guid id, string fullName)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Student ID cannot be empty.",
                nameof(id));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "Student full name cannot be empty.",
                nameof(fullName));

        Id = id;
        FullName = fullName;
    }
}
