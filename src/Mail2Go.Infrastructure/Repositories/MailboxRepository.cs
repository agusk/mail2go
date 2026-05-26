using Microsoft.EntityFrameworkCore;
using Mail2Go.Application.Abstractions;
using Mail2Go.Domain.Mailboxes;
using Mail2Go.Infrastructure.Persistence;

namespace Mail2Go.Infrastructure.Repositories;

public sealed class MailboxRepository : IMailboxRepository
{
    private readonly AppDbContext _context;

    public MailboxRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Mailbox mailbox, CancellationToken cancellationToken)
    {
        _context.Mailboxes.Add(mailbox);
        await Task.CompletedTask;
    }

    public async Task<Mailbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Mailboxes.FindAsync([id], cancellationToken);
    }

    public async Task<Mailbox?> GetByAddressAsync(string address, CancellationToken cancellationToken)
    {
        return await _context.Mailboxes
            .FirstOrDefaultAsync(m => m.Address == address.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<Mailbox>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Mailboxes
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Address)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Mailbox>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Mailboxes
            .OrderBy(m => m.Address)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string address, CancellationToken cancellationToken)
    {
        return await _context.Mailboxes
            .AnyAsync(m => m.Address == address.ToLowerInvariant(), cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
