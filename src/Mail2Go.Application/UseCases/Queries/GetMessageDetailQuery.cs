using Mail2Go.Domain.Emails;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Queries;

public sealed class GetMessageDetailQuery
{
    private readonly IEmailMessageRepository _emailRepository;

    public GetMessageDetailQuery(IEmailMessageRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<EmailMessage?> HandleAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return await _emailRepository.GetByIdAsync(messageId, cancellationToken);
    }
}
