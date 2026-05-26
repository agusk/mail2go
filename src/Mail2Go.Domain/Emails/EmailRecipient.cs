namespace Mail2Go.Domain.Emails;

public sealed class EmailRecipient
{
    public Guid Id { get; private set; }

    public Guid EmailMessageId { get; private set; }

    public string Address { get; private set; } = string.Empty;

    public string Domain { get; private set; } = string.Empty;

    public RecipientType Type { get; private set; }

    public Guid? MailboxId { get; private set; }

    private EmailRecipient()
    {
    }

    public EmailRecipient(
        Guid id,
        Guid emailMessageId,
        string address,
        string domain,
        RecipientType type,
        Guid? mailboxId)
    {
        Id = id;
        EmailMessageId = emailMessageId;
        Address = address.Trim().ToLowerInvariant();
        Domain = domain.Trim().ToLowerInvariant();
        Type = type;
        MailboxId = mailboxId;
    }
}
