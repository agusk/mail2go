using Mail2Go.Domain.Emails;

namespace Mail2Go.Application.Abstractions;

public interface IEmailMessageRepository
{
    Task AddAsync(EmailMessage message, CancellationToken cancellationToken);

    Task<EmailMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmailMessage>> GetMessagesByMailboxAsync(
        Guid mailboxId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EmailMessage>> GetMessagesByDomainAsync(
        string domain,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<EmailMessage>> GetAllAsync(CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<int> CountMessagesAsync(CancellationToken cancellationToken);
}
