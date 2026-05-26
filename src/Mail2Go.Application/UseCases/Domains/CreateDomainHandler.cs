using Mail2Go.Domain.Domains;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Application.UseCases.Domains;

public sealed class CreateDomainCommand
{
    public required string Name { get; init; }
}

public sealed class CreateDomainHandler
{
    private readonly IMailDomainRepository _domainRepository;

    public CreateDomainHandler(IMailDomainRepository domainRepository)
    {
        _domainRepository = domainRepository;
    }

    public async Task<MailDomain> HandleAsync(CreateDomainCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim().ToLowerInvariant();
        var exists = await _domainRepository.ExistsAsync(name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Domain '{name}' already exists.");
        }

        var domain = new MailDomain(Guid.NewGuid(), name, DateTimeOffset.UtcNow);
        await _domainRepository.AddAsync(domain, cancellationToken);
        await _domainRepository.SaveChangesAsync(cancellationToken);

        return domain;
    }
}
