using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Domains;

public sealed class UpdateDomainCommand
{
    public required Guid DomainId { get; init; }
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
}

public sealed class UpdateDomainHandler
{
    private readonly IMailDomainRepository _domainRepository;

    public UpdateDomainHandler(IMailDomainRepository domainRepository)
    {
        _domainRepository = domainRepository;
    }

    public async Task HandleAsync(UpdateDomainCommand command, CancellationToken cancellationToken)
    {
        var domain = await _domainRepository.GetByIdAsync(command.DomainId, cancellationToken)
            ?? throw new InvalidOperationException($"Domain '{command.DomainId}' not found.");

        domain.UpdateName(command.Name);

        if (command.IsEnabled)
            domain.Enable();
        else
            domain.Disable();

        await _domainRepository.SaveChangesAsync(cancellationToken);
    }
}
