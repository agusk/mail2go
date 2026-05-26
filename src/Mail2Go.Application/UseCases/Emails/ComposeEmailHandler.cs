using Mail2Go.Domain.Emails;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Emails;

public sealed class ComposeEmailCommand
{
    public required Guid SenderUserId { get; init; }
    public required string FromAddress { get; init; }
    public required IReadOnlyList<string> ToAddresses { get; init; }
    public required IReadOnlyList<string> CcAddresses { get; init; }
    public required IReadOnlyList<string> BccAddresses { get; init; }
    public required string? Subject { get; init; }
    public required string? TextBody { get; init; }
    public required string? HtmlBody { get; init; }
    /// <summary>When true, skips mailbox ownership validation (admin use).</summary>
    public bool SkipMailboxOwnerCheck { get; init; }
}

public sealed class ComposeEmailHandler
{
    private readonly IEmailMessageRepository _emailRepository;
    private readonly IMailboxRepository _mailboxRepository;

    public ComposeEmailHandler(
        IEmailMessageRepository emailRepository,
        IMailboxRepository mailboxRepository)
    {
        _emailRepository = emailRepository;
        _mailboxRepository = mailboxRepository;
    }

    public async Task HandleAsync(ComposeEmailCommand command, CancellationToken cancellationToken)
    {
        if (!command.SkipMailboxOwnerCheck)
        {
            var senderMailboxes = await _mailboxRepository.GetByUserIdAsync(command.SenderUserId, cancellationToken);
            var senderAddress = command.FromAddress.Trim().ToLowerInvariant();
            var isOwner = senderMailboxes.Any(m => m.Address == senderAddress);

            if (!isOwner)
                throw new InvalidOperationException("User does not own the specified sender mailbox.");
        }

        var rawMime = BuildSimpleMime(command);

        var message = new EmailMessage(
            Guid.NewGuid(),
            GenerateMessageId(),
            command.FromAddress,
            command.Subject,
            command.TextBody,
            command.HtmlBody,
            rawMime,
            DateTimeOffset.UtcNow,
            rawMime.Length,
            MessageSource.WebCompose,
            command.SenderUserId);

        foreach (var addr in command.ToAddresses)
        {
            var domain = ExtractDomain(addr);
            var mailbox = await _mailboxRepository.GetByAddressAsync(addr.Trim().ToLowerInvariant(), cancellationToken);
            message.AddRecipient(addr, domain, RecipientType.To, mailbox?.Id);
        }

        foreach (var addr in command.CcAddresses)
        {
            var domain = ExtractDomain(addr);
            var mailbox = await _mailboxRepository.GetByAddressAsync(addr.Trim().ToLowerInvariant(), cancellationToken);
            message.AddRecipient(addr, domain, RecipientType.Cc, mailbox?.Id);
        }

        foreach (var addr in command.BccAddresses)
        {
            var domain = ExtractDomain(addr);
            var mailbox = await _mailboxRepository.GetByAddressAsync(addr.Trim().ToLowerInvariant(), cancellationToken);
            message.AddRecipient(addr, domain, RecipientType.Bcc, mailbox?.Id);
        }

        await _emailRepository.AddAsync(message, cancellationToken);
    }

    private static string ExtractDomain(string address)
    {
        var idx = address.IndexOf('@');
        return idx >= 0 ? address[(idx + 1)..].Trim().ToLowerInvariant() : string.Empty;
    }

    private static string GenerateMessageId()
    {
        return $"<{Guid.NewGuid()}@mail2go.local>";
    }

    private static string BuildSimpleMime(ComposeEmailCommand command)
    {
        var to = string.Join(", ", command.ToAddresses);
        return $"From: {command.FromAddress}\r\nTo: {to}\r\nSubject: {command.Subject}\r\n\r\n{command.TextBody}";
    }
}
