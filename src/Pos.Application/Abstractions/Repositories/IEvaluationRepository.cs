using Pos.Domain.Entities;

namespace Pos.Application.Abstractions.Repositories;

public interface IEvaluationRepository
{
    void Add(Evaluation evaluation);

    void Update(Evaluation evaluation);

    Evaluation? GetById(Guid id);

    IReadOnlyCollection<Evaluation> GetByStudentId(Guid studentId);

    void Remove(Guid id);
}
