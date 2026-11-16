using Pos.Domain.Entities;

namespace Pos.Application.Abstractions.Repositories;

public interface IStudentRepository
{
    void Add(Student student);

    Student? GetById(Guid id);

    IReadOnlyCollection<Student> GetAll();
}
