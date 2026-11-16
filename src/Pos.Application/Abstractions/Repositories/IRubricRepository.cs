using Pos.Domain.Entities;

namespace Pos.Application.Abstractions.Repositories;

public interface IRubricRepository
{
    void Add(Rubric rubric);

    void Update(Rubric rubric);

    Rubric? GetById(Guid id);

    IReadOnlyCollection<Rubric> GetAll();
}
