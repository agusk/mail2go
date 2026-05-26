using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mail2Go.Domain.Domains;
using Mail2Go.Domain.Mailboxes;
using Mail2Go.Infrastructure.Identity;
using Mail2Go.Infrastructure.Persistence;

namespace Mail2Go.Infrastructure.Seeding;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DatabaseSeeder(
        AppDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);

        await SeedRolesAsync();
        await SeedDefaultDomainAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
        await SeedDefaultUserAsync(cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.NewGuid() });
            }
        }
    }

    private async Task SeedDefaultDomainAsync(CancellationToken cancellationToken)
    {
        const string defaultDomain = "mail2go.local";

        if (!await _context.MailDomains.AnyAsync(d => d.Name == defaultDomain, cancellationToken))
        {
            var domain = new MailDomain(Guid.NewGuid(), defaultDomain, DateTimeOffset.UtcNow);
            _context.MailDomains.Add(domain);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@mail2go.local";

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = adminEmail,
            DisplayName = "Administrator",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(admin, "pass123");
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private async Task SeedDefaultUserAsync(CancellationToken cancellationToken)
    {
        const string userEmail = "user@mail2go.local";

        if (await _userManager.FindByEmailAsync(userEmail) is not null)
            return;

        var domain = await _context.MailDomains
            .FirstOrDefaultAsync(d => d.Name == "mail2go.local", cancellationToken);

        if (domain is null)
            return;

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "user",
            Email = userEmail,
            DisplayName = "Test User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, "pass123");
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");

            var mailboxExists = await _context.Mailboxes
                .AnyAsync(m => m.Address == userEmail, cancellationToken);

            if (!mailboxExists)
            {
                var mailbox = new Mailbox(Guid.NewGuid(), user.Id, domain.Id, userEmail, DateTimeOffset.UtcNow);
                _context.Mailboxes.Add(mailbox);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
