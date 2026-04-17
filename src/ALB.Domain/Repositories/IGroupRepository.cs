using ALB.Domain.Entities;

namespace ALB.Domain.Repositories;

public interface IGroupRepository
{
    Task<Group> CreateAsync(Group group);
    Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Group group);
    Task DeleteAsync(Guid id);
    Task<List<Group>> GetAllAsync(CancellationToken ct = default);
    Task<List<AcademicYear>> CreateAcademicYearsAsync(List<AcademicYear> academicYears, CancellationToken ct = default);
    Task<List<AcademicYear>> GetAcademicYearsAsync(CancellationToken ct = default);
    Task<AcademicYear?> GetAcademicYearByIdAsync(Guid id, CancellationToken ct = default);
    Task<AllowedGroupname> CreateAllowedGroupnameAsync(AllowedGroupname allowedGroupname, CancellationToken ct = default);
    Task<List<AllowedGroupname>> GetAllAllowedGroupnamesAsync(CancellationToken ct = default);
    Task<AllowedGroupname?> GetAllowedGroupnameByIdAsync(Guid id, CancellationToken ct = default);
    Task AddChildrenToGroupAsync(Guid groupId, IEnumerable<Guid> childIds, CancellationToken ct);
    Task RemoveChildrenFromGroupAsync(Guid groupId, IEnumerable<Guid> childIds, CancellationToken ct);

}