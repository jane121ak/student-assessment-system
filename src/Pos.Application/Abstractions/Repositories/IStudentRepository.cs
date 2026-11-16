using Pos.Domain.Entities;

namespace Pos.Application.Abstractions.Repositories;

public interface IStudentRepository
{
    void Add(Student student);

    Student? GetById(Guid id);

    IReadOnlyCollection<Student> GetAll();

    void Remove(Guid id)
    {
        throw new NotSupportedException(
            "Student removal is not supported by this repository.");
    }
}
