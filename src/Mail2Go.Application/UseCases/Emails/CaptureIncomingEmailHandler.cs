using Mail2Go.Domain.Emails;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Emails;

public sealed class CaptureIncomingEmailCommand
{
    public required string? InternetMessageId { get; init; }
    public required string? FromAddress { get; init; }
    public required string? Subject { get; init; }
    public required string? TextBody { get; init; }
    public required string? HtmlBody { get; init; }
    public required string RawMime { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public required long SizeBytes { get; init; }
    public required Guid? AuthenticatedUserId { get; init; }
    public required IReadOnlyList<RecipientEntry> Recipients { get; init; }

    public sealed class RecipientEntry
    {
        public required string Address { get; init; }
        public required string Domain { get; init; }
        public required RecipientType Type { get; init; }
    }
}

public sealed class CaptureIncomingEmailHandler
{
    private readonly IEmailMessageRepository _emailRepository;
    private readonly IMailboxRepository _mailboxRepository;

    public CaptureIncomingEmailHandler(
        IEmailMessageRepository emailRepository,
        IMailboxRepository mailboxRepository)
    {
        _emailRepository = emailRepository;
        _mailboxRepository = mailboxRepository;
    }

    public async Task HandleAsync(CaptureIncomingEmailCommand command, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            Guid.NewGuid(),
            command.InternetMessageId,
            command.FromAddress,
            command.Subject,
            command.TextBody,
            command.HtmlBody,
            command.RawMime,
            command.ReceivedAt,
            command.SizeBytes,
            MessageSource.Smtp,
            command.AuthenticatedUserId);

        foreach (var recipient in command.Recipients)
        {
            var mailbox = await _mailboxRepository.GetByAddressAsync(recipient.Address, cancellationToken);
            message.AddRecipient(recipient.Address, recipient.Domain, recipient.Type, mailbox?.Id);
        }

        await _emailRepository.AddAsync(message, cancellationToken);
    }
}
