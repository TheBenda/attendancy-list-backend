using ALB.Domain.Entities;

namespace ALB.Domain.Repositories;

public interface IInviteUsersRepository
{
    Task CreateAsync(InviteUser inviteUser, CancellationToken ct = default);
    Task<InviteUser?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task DeleteAsync(InviteUser inviteUser, CancellationToken ct = default);
    Task<InviteUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
}