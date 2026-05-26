namespace Mail2Go.Domain.Emails;

public sealed class EmailMessage
{
    private readonly List<EmailRecipient> _recipients = new();

    public Guid Id { get; private set; }

    public string? InternetMessageId { get; private set; }

    public string? FromAddress { get; private set; }

    public string? Subject { get; private set; }

    public string? TextBody { get; private set; }

    public string? HtmlBody { get; private set; }

    public string RawMime { get; private set; } = string.Empty;

    public DateTimeOffset ReceivedAt { get; private set; }

    public long SizeBytes { get; private set; }

    public MessageSource Source { get; private set; }

    public Guid? AuthenticatedUserId { get; private set; }

    public IReadOnlyCollection<EmailRecipient> Recipients => _recipients;

    private EmailMessage()
    {
    }

    public EmailMessage(
        Guid id,
        string? internetMessageId,
        string? fromAddress,
        string? subject,
        string? textBody,
        string? htmlBody,
        string rawMime,
        DateTimeOffset receivedAt,
        long sizeBytes,
        MessageSource source,
        Guid? authenticatedUserId)
    {
        Id = id;
        InternetMessageId = internetMessageId;
        FromAddress = fromAddress;
        Subject = subject;
        TextBody = textBody;
        HtmlBody = htmlBody;
        RawMime = rawMime;
        ReceivedAt = receivedAt;
        SizeBytes = sizeBytes;
        Source = source;
        AuthenticatedUserId = authenticatedUserId;
    }

    public void AddRecipient(
        string address,
        string domain,
        RecipientType type,
        Guid? mailboxId)
    {
        _recipients.Add(
            new EmailRecipient(
                Guid.NewGuid(),
                Id,
                address,
                domain,
                type,
                mailboxId));
    }
}
