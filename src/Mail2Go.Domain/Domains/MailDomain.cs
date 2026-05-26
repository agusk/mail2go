namespace Mail2Go.Domain.Domains;

public sealed class MailDomain
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private MailDomain()
    {
    }

    public MailDomain(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name.Trim().ToLowerInvariant();
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

    public void UpdateName(string name)
    {
        Name = name.Trim().ToLowerInvariant();
    }
}
