using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Queries;

public sealed class DashboardResult
{
    public required int TotalMessages { get; init; }
    public required int TotalDomains { get; init; }
    public required int TotalMailboxes { get; init; }
}

public sealed class GetDashboardQuery
{
    private readonly IEmailMessageRepository _emailRepository;
    private readonly IMailDomainRepository _domainRepository;
    private readonly IMailboxRepository _mailboxRepository;

    public GetDashboardQuery(
        IEmailMessageRepository emailRepository,
        IMailDomainRepository domainRepository,
        IMailboxRepository mailboxRepository)
    {
        _emailRepository = emailRepository;
        _domainRepository = domainRepository;
        _mailboxRepository = mailboxRepository;
    }

    public async Task<DashboardResult> HandleAsync(CancellationToken cancellationToken)
    {
        var totalMessages = await _emailRepository.CountMessagesAsync(cancellationToken);
        var domains = await _domainRepository.GetAllAsync(cancellationToken);
        var mailboxes = await _mailboxRepository.GetAllAsync(cancellationToken);

        return new DashboardResult
        {
            TotalMessages = totalMessages,
            TotalDomains = domains.Count,
            TotalMailboxes = mailboxes.Count
        };
    }
}
