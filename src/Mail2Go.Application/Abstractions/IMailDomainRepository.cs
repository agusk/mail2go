using Mail2Go.Domain.Domains;

namespace Mail2Go.Application.Abstractions;

public interface IMailDomainRepository
{
    Task AddAsync(MailDomain domain, CancellationToken cancellationToken);

    Task<MailDomain?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MailDomain?> GetByNameAsync(string name, CancellationToken cancellationToken);

    Task<IReadOnlyList<MailDomain>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
