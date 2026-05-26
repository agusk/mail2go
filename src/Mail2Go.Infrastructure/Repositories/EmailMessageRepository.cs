using Microsoft.EntityFrameworkCore;
using Mail2Go.Application.Abstractions;
using Mail2Go.Domain.Emails;
using Mail2Go.Infrastructure.Persistence;

namespace Mail2Go.Infrastructure.Repositories;

public sealed class EmailMessageRepository : IEmailMessageRepository
{
    private readonly AppDbContext _context;

    public EmailMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        _context.EmailMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.EmailMessages
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailMessage>> GetMessagesByMailboxAsync(
        Guid mailboxId,
        CancellationToken cancellationToken)
    {
        var messages = await _context.EmailMessages
            .Include(m => m.Recipients)
            .Where(m => m.Recipients.Any(r => r.MailboxId == mailboxId))
            .ToListAsync(cancellationToken);
        return messages.OrderByDescending(m => m.ReceivedAt).ToList();
    }

    public async Task<IReadOnlyList<EmailMessage>> GetMessagesByDomainAsync(
        string domain,
        CancellationToken cancellationToken)
    {
        var messages = await _context.EmailMessages
            .Include(m => m.Recipients)
            .Where(m => m.Recipients.Any(r => r.Domain == domain.ToLowerInvariant()))
            .ToListAsync(cancellationToken);
        return messages.OrderByDescending(m => m.ReceivedAt).ToList();
    }

    public async Task<IReadOnlyList<EmailMessage>> GetAllAsync(CancellationToken cancellationToken)
    {
        var messages = await _context.EmailMessages
            .Include(m => m.Recipients)
            .ToListAsync(cancellationToken);
        return messages.OrderByDescending(m => m.ReceivedAt).ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await _context.EmailMessages.FindAsync([id], cancellationToken);
        if (message is not null)
        {
            _context.EmailMessages.Remove(message);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountMessagesAsync(CancellationToken cancellationToken)
    {
        return await _context.EmailMessages.CountAsync(cancellationToken);
    }
}
