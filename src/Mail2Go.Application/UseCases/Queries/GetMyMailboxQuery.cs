using Mail2Go.Domain.Emails;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Queries;

public sealed class GetMyMailboxResult
{
    public required Guid MailboxId { get; init; }
    public required string Address { get; init; }
    public required IReadOnlyList<EmailMessage> Messages { get; init; }
}

public sealed class GetMyMailboxQuery
{
    private readonly IMailboxRepository _mailboxRepository;
    private readonly IEmailMessageRepository _emailRepository;

    public GetMyMailboxQuery(
        IMailboxRepository mailboxRepository,
        IEmailMessageRepository emailRepository)
    {
        _mailboxRepository = mailboxRepository;
        _emailRepository = emailRepository;
    }

    public async Task<IReadOnlyList<GetMyMailboxResult>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mailboxes = await _mailboxRepository.GetByUserIdAsync(userId, cancellationToken);
        var results = new List<GetMyMailboxResult>();

        foreach (var mailbox in mailboxes)
        {
            var messages = await _emailRepository.GetMessagesByMailboxAsync(mailbox.Id, cancellationToken);
            results.Add(new GetMyMailboxResult
            {
                MailboxId = mailbox.Id,
                Address = mailbox.Address,
                Messages = messages
            });
        }

        return results;
    }
}
