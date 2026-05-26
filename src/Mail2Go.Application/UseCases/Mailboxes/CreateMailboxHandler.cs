using Mail2Go.Domain.Mailboxes;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Mailboxes;

public sealed class CreateMailboxCommand
{
    public required Guid UserId { get; init; }
    public required Guid DomainId { get; init; }
    public required string Address { get; init; }
}

public sealed class CreateMailboxHandler
{
    private readonly IMailboxRepository _mailboxRepository;

    public CreateMailboxHandler(IMailboxRepository mailboxRepository)
    {
        _mailboxRepository = mailboxRepository;
    }

    public async Task<Mailbox> HandleAsync(CreateMailboxCommand command, CancellationToken cancellationToken)
    {
        var address = command.Address.Trim().ToLowerInvariant();
        var exists = await _mailboxRepository.ExistsAsync(address, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Mailbox '{address}' already exists.");
        }

        var mailbox = new Mailbox(Guid.NewGuid(), command.UserId, command.DomainId, address, DateTimeOffset.UtcNow);
        await _mailboxRepository.AddAsync(mailbox, cancellationToken);
        await _mailboxRepository.SaveChangesAsync(cancellationToken);

        return mailbox;
    }
}
