using Microsoft.EntityFrameworkCore;
using Mail2Go.Application.Abstractions;
using Mail2Go.Domain.Domains;
using Mail2Go.Infrastructure.Persistence;

namespace Mail2Go.Infrastructure.Repositories;

public sealed class MailDomainRepository : IMailDomainRepository
{
    private readonly AppDbContext _context;

    public MailDomainRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MailDomain domain, CancellationToken cancellationToken)
    {
        _context.MailDomains.Add(domain);
        await Task.CompletedTask;
    }

    public async Task<MailDomain?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.MailDomains.FindAsync([id], cancellationToken);
    }

    public async Task<MailDomain?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.MailDomains
            .FirstOrDefaultAsync(d => d.Name == name.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<MailDomain>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.MailDomains
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.MailDomains
            .AnyAsync(d => d.Name == name.ToLowerInvariant(), cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
