using ALB.Domain.Entities;

namespace ALB.Domain.Repositories;

public interface IChildRepository
{
    Task<Child> CreateAsync(Child child, CancellationToken ct);
    Task<Child?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Child> AddGuardiansToChildAsync(Guid childId, List<Guid> guardianIds, CancellationToken ct);
    Task<List<Child>> TakeChildrenByCursor(Guid? cursor, int limit, CancellationToken ct = default);
    Task UpdateAsync(Child child);
    Task DeleteAsync(Guid id);
    Task<List<Child>> GetByParentId(Guid parentId);

}