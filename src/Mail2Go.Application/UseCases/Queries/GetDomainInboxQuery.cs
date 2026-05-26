using Mail2Go.Domain.Emails;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Queries;

public sealed class GetDomainInboxQuery
{
    private readonly IEmailMessageRepository _emailRepository;

    public GetDomainInboxQuery(IEmailMessageRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<IReadOnlyList<EmailMessage>> HandleAsync(string domain, CancellationToken cancellationToken)
    {
        return await _emailRepository.GetMessagesByDomainAsync(domain, cancellationToken);
    }
}
