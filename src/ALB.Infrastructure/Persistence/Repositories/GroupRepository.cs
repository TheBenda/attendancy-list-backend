using ALB.Domain.Entities;
using ALB.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ALB.Infrastructure.Persistence.Repositories;

public class GroupRepository(ApplicationDbContext dbContext) : IGroupRepository
{
    public async Task<Group> CreateAsync(Group group)
    {
        dbContext.Groups.Add(group);
        await dbContext.SaveChangesAsync();
        return group;
    }

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Group group)
    {
        dbContext.Groups.Update(group);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var group = await dbContext.Groups.FindAsync(id);
        if (group is not null)
        {
            dbContext.Groups.Remove(group);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<Group>> GetAllAsync(CancellationToken ct = default)
         => await dbContext.Groups.ToListAsync(ct);

    public async Task<List<AcademicYear>> CreateAcademicYearsAsync(List<AcademicYear> academicYears, CancellationToken ct = default)
    {
        dbContext.AcademicYears.AddRange(academicYears);
        await dbContext.SaveChangesAsync(ct);
        return academicYears;
    }

    public async Task<List<AcademicYear>> GetAcademicYearsAsync(CancellationToken ct = default)
        => await dbContext.AcademicYears.ToListAsync(ct);

    public async Task<AcademicYear?> GetAcademicYearByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.AcademicYears.FindAsync(id, ct);


    public async Task<AllowedGroupname> CreateAllowedGroupnameAsync(AllowedGroupname allowedGroupname, CancellationToken ct = default)
    {
        dbContext.AllowedGroupnames.Add(allowedGroupname);
        await dbContext.SaveChangesAsync(ct);
        return allowedGroupname;
    }

    public async Task<List<AllowedGroupname>> GetAllAllowedGroupnamesAsync(CancellationToken ct = default)
        => await dbContext.AllowedGroupnames.ToListAsync(ct);

    public async Task<AllowedGroupname?> GetAllowedGroupnameByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.AllowedGroupnames.FindAsync(id, ct);

    public async Task AddChildrenToGroupAsync(Guid groupId, IEnumerable<Guid> childIds, CancellationToken ct)
    {
        var group = await dbContext.Groups
            .Include(g => g.Children)
            .FirstOrDefaultAsync(g => g.Id == groupId, ct);

        if (group is null) throw new Exception("Group not found");

        var children = await dbContext.Children
            .Where(c => childIds.Contains(c.Id))
            .ToListAsync(ct);

        foreach (var child in children)
            if (!group.Children.Contains(child))
                group.Children.Add(child);

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveChildrenFromGroupAsync(Guid groupId, IEnumerable<Guid> childIds, CancellationToken ct)
    {
        var group = await dbContext.Groups
            .Include(g => g.Children)
            .FirstOrDefaultAsync(g => g.Id == groupId, ct);

        if (group is null) throw new Exception("Group not found");

        var childrenToRemove = group.Children
            .Where(c => childIds.Contains(c.Id))
            .ToList();

        foreach (var child in childrenToRemove) group.Children.Remove(child);

        await dbContext.SaveChangesAsync(ct);
    }
}