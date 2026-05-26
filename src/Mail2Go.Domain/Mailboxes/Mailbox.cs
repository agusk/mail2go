namespace Mail2Go.Domain.Mailboxes;

public sealed class Mailbox
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid DomainId { get; private set; }

    public string Address { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Mailbox()
    {
    }

    public Mailbox(
        Guid id,
        Guid userId,
        Guid domainId,
        string address,
        DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        DomainId = domainId;
        Address = address.Trim().ToLowerInvariant();
        IsEnabled = true;
        CreatedAt = createdAt;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
