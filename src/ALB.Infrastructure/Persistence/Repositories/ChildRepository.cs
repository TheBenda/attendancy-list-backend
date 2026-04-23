using ALB.Domain.Entities;
using ALB.Domain.Identity;
using ALB.Domain.Repositories;
using ALB.Domain.Values;
//using ServiceDefaults.Activities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ALB.Infrastructure.Persistence.Repositories;

public class ChildRepository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : IChildRepository
{
    public async Task<Child> CreateAsync(Child child, CancellationToken ct)
    {
        dbContext.Children.Add(child);
        await dbContext.SaveChangesAsync(ct);
        return child;
    }

    public async Task<Child?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Children
            .Include(c => c.Guardians)
            .SingleOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Child> AddGuardiansToChildAsync(Guid childId, List<Guid> guardianIds, CancellationToken ct)
    {
        //using var activity = ActivitySources.BackendActivitySource.StartActivity("Add Guardians to Child");
        var child = await dbContext.Children.FindAsync(childId, ct);

        if (child is null) throw new Exception("Child not found");

        foreach (var guardianId in guardianIds)
        {
            var foundUser = await userManager.FindByIdAsync(guardianId.ToString());
            if (foundUser is null)
            {
                var ex = new Exception($"Guardian with id {guardianId} not found");
                //activity?.AddTag("guardian_Id", guardianId.ToString());
                //activity?.AddTag("child_Id", childId.ToString());
                //activity?.AddException(ex);
                throw ex;
            }

            var userRoles = await userManager.GetRolesAsync(foundUser);

            if (userRoles.Contains(SystemRoles.Parent))
            {
                child.Guardians.Add(foundUser);
            }
        }

        await dbContext.SaveChangesAsync(ct);

        return child;
    }

    public async Task<List<Child>> TakeChildrenByCursor(Guid? cursor, int limit, CancellationToken ct = default)
    {
        var childrenUnordered = await dbContext.Children.ToListAsync(ct);

        var childrenOrdered = await dbContext.Children
            .OrderByDescending(c => c.Id)
            .ToListAsync(ct);

        var childrenOrderedLimited = await dbContext.Children
            .OrderByDescending(c => c.Id)
            .Take(limit + 1)
            .ToListAsync(ct);

        var cursored = await dbContext.Children
            .OrderByDescending(c => c.Id)
            .Where(c => c.Id <= cursor)
            .Take(limit + 1)
            .ToListAsync(ct);

        return cursor is null ?
            await dbContext.Children
                .OrderByDescending(c => c.Id)
                .Take(limit + 1)
                .ToListAsync(ct) :
            await dbContext.Children
                .OrderByDescending(c => c.Id)
                .Where(c => c.Id <= cursor)
                .Take(limit + 1)
                .ToListAsync(ct);
    }


    public async Task UpdateAsync(Child child)
    {
        dbContext.Children.Update(child);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var child = await dbContext.Children.FindAsync(id);
        if (child is not null)
        {
            dbContext.Children.Remove(child);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Child>> GetByCohortAsync(Guid cohortId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Child>> GetByParentId(Guid parentId)
    {
        return await dbContext.Children
            .Include(c => c.Guardians)
            .Where(child => child.Guardians.Any(p => p.Id == parentId))
            .ToListAsync();
    }
}