using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Emails;

public sealed class DeleteEmailHandler
{
    private readonly IEmailMessageRepository _emailRepository;

    public DeleteEmailHandler(IEmailMessageRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task HandleAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await _emailRepository.DeleteAsync(messageId, cancellationToken);
    }
}
