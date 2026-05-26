using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mail2Go.Domain.Domains;
using Mail2Go.Domain.Emails;
using Mail2Go.Domain.Mailboxes;
using Mail2Go.Infrastructure.Identity;

namespace Mail2Go.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MailDomain> MailDomains => Set<MailDomain>();
    public DbSet<Mailbox> Mailboxes => Set<Mailbox>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MailDomain>(entity =>
        {
            entity.ToTable("MailDomains");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<Mailbox>(entity =>
        {
            entity.ToTable("Mailboxes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(320);
            entity.HasIndex(e => e.Address).IsUnique();
        });

        builder.Entity<EmailMessage>(entity =>
        {
            entity.ToTable("EmailMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RawMime).IsRequired();

            entity.HasMany(e => e.Recipients)
                  .WithOne()
                  .HasForeignKey(r => r.EmailMessageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmailRecipient>(entity =>
        {
            entity.ToTable("EmailRecipients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(320);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
        });
    }
}
