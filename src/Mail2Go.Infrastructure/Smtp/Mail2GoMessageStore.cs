using System.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using Mail2Go.Application.UseCases.Emails;

namespace Mail2Go.Infrastructure.Smtp;

public sealed class Mail2GoMessageStore : MessageStore
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Mail2GoMessageStore> _logger;

    public Mail2GoMessageStore(
        IServiceScopeFactory scopeFactory,
        ILogger<Mail2GoMessageStore> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override async Task<SmtpResponse> SaveAsync(
        ISessionContext context,
        IMessageTransaction transaction,
        ReadOnlySequence<byte> buffer,
        CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream(buffer.ToArray());
            var mimeMessage = await MimeMessage.LoadAsync(stream, cancellationToken);

            var rawMime = System.Text.Encoding.UTF8.GetString(buffer.ToArray());

            // AuthenticatedUserId is tracked separately via audit records in future versions.
            // SMTP auth context only provides username, not user GUID.
            Guid? authenticatedUserId = null;

            var recipients = BuildRecipients(transaction, mimeMessage);

            var command = new CaptureIncomingEmailCommand
            {
                InternetMessageId = mimeMessage.MessageId,
                FromAddress = mimeMessage.From.Mailboxes.FirstOrDefault()?.Address,
                Subject = mimeMessage.Subject,
                TextBody = mimeMessage.TextBody,
                HtmlBody = mimeMessage.HtmlBody,
                RawMime = rawMime,
                ReceivedAt = DateTimeOffset.UtcNow,
                SizeBytes = buffer.Length,
                AuthenticatedUserId = authenticatedUserId,
                Recipients = recipients
            };

            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<CaptureIncomingEmailHandler>();
            await handler.HandleAsync(command, cancellationToken);

            _logger.LogInformation("Email captured from {From} with subject '{Subject}'",
                command.FromAddress, command.Subject);

            return SmtpResponse.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture incoming email.");
            return SmtpResponse.TransactionFailed;
        }
    }

    private static List<CaptureIncomingEmailCommand.RecipientEntry> BuildRecipients(
        IMessageTransaction transaction,
        MimeMessage mimeMessage)
    {
        var recipients = new List<CaptureIncomingEmailCommand.RecipientEntry>();

        foreach (var to in mimeMessage.To.Mailboxes)
        {
            recipients.Add(new CaptureIncomingEmailCommand.RecipientEntry
            {
                Address = to.Address.ToLowerInvariant(),
                Domain = to.Domain.ToLowerInvariant(),
                Type = Domain.Emails.RecipientType.To
            });
        }

        foreach (var cc in mimeMessage.Cc.Mailboxes)
        {
            recipients.Add(new CaptureIncomingEmailCommand.RecipientEntry
            {
                Address = cc.Address.ToLowerInvariant(),
                Domain = cc.Domain.ToLowerInvariant(),
                Type = Domain.Emails.RecipientType.Cc
            });
        }

        foreach (var bcc in mimeMessage.Bcc.Mailboxes)
        {
            recipients.Add(new CaptureIncomingEmailCommand.RecipientEntry
            {
                Address = bcc.Address.ToLowerInvariant(),
                Domain = bcc.Domain.ToLowerInvariant(),
                Type = Domain.Emails.RecipientType.Bcc
            });
        }

        // Fallback: use SMTP RCPT TO addresses if MIME headers have none
        if (recipients.Count == 0)
        {
            foreach (var rcpt in transaction.To)
            {
                var addr = $"{rcpt.User}@{rcpt.Host}".ToLowerInvariant();
                var domain = rcpt.Host.ToLowerInvariant();
                recipients.Add(new CaptureIncomingEmailCommand.RecipientEntry
                {
                    Address = addr,
                    Domain = domain,
                    Type = Domain.Emails.RecipientType.To
                });
            }
        }

        return recipients;
    }
}
