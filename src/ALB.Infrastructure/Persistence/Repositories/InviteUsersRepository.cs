using ALB.Domain.Entities;
using ALB.Domain.Repositories;
using ALB.MailgunApi.Adapters;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALB.Infrastructure.Persistence.Repositories;

public class InviteUsersRepository(ApplicationDbContext dbContext, IMailgunApiAdapter mailgunApiAdapter, ILogger<InviteUsersRepository> logger): IInviteUsersRepository
{
    public async Task CreateAsync(InviteUser inviteUser, CancellationToken ct = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            dbContext.InviteUsers.Add(inviteUser);
            await dbContext.SaveChangesAsync(ct);
            await mailgunApiAdapter.SendInvitationEmailAsync(inviteUser, ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(e, "Failed to create invite user");
            throw;
        }
    }

    public async Task<InviteUser?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await dbContext.InviteUsers
            .Where(iu => iu.Token == token)
            .SingleOrDefaultAsync(ct);

    public async Task DeleteAsync(InviteUser inviteUser, CancellationToken ct = default)
    {
        dbContext.InviteUsers.Remove(inviteUser);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<InviteUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext
            .InviteUsers
            .Where(iu => iu.Id == id)
            .SingleOrDefaultAsync(ct);
}