using Mail2Go.Domain.Mailboxes;

namespace Mail2Go.Application.Abstractions;

public interface IMailboxRepository
{
    Task AddAsync(Mailbox mailbox, CancellationToken cancellationToken);

    Task<Mailbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Mailbox?> GetByAddressAsync(string address, CancellationToken cancellationToken);

    Task<IReadOnlyList<Mailbox>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Mailbox>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string address, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
