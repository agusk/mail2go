using Shouldly;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Emails;
using Mail2Go.Domain.Emails;
using Mail2Go.Domain.Mailboxes;
using NSubstitute;
using Xunit;

namespace Mail2Go.UnitTests.Application;

public sealed class CaptureEmailHandlerTests
{
    private readonly IEmailMessageRepository _emailRepo = Substitute.For<IEmailMessageRepository>();
    private readonly IMailboxRepository _mailboxRepo = Substitute.For<IMailboxRepository>();
    private readonly CaptureIncomingEmailHandler _sut;

    public CaptureEmailHandlerTests()
    {
        _sut = new CaptureIncomingEmailHandler(_emailRepo, _mailboxRepo);
    }

    private static CaptureIncomingEmailCommand BuildCommand(
        string from = "sender@external.com",
        string to   = "alice@mail2go.local",
        string subject = "Test subject") =>
        new()
        {
            InternetMessageId = "<test@id>",
            FromAddress       = from,
            Subject           = subject,
            TextBody          = "Hello world",
            HtmlBody          = null,
            RawMime           = "raw",
            ReceivedAt        = DateTimeOffset.UtcNow,
            SizeBytes         = 100,
            AuthenticatedUserId = null,
            Recipients        =
            [
                new CaptureIncomingEmailCommand.RecipientEntry
                {
                    Address = to,
                    Domain  = "mail2go.local",
                    Type    = RecipientType.To
                }
            ]
        };

    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsEmailToRepository()
    {
        var cmd = BuildCommand();

        _mailboxRepo.GetByAddressAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((Mailbox?)null);

        await _sut.HandleAsync(cmd, CancellationToken.None);

        await _emailRepo.Received(1)
                        .AddAsync(Arg.Is<EmailMessage>(m =>
                            m.FromAddress == cmd.FromAddress &&
                            m.Subject     == cmd.Subject &&
                            m.Source      == MessageSource.Smtp),
                        Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsRecipients()
    {
        var cmd = BuildCommand(to: "bob@mail2go.local");

        _mailboxRepo.GetByAddressAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((Mailbox?)null);

        await _sut.HandleAsync(cmd, CancellationToken.None);

        await _emailRepo.Received(1).AddAsync(
            Arg.Is<EmailMessage>(m => m.Recipients.Any(r => r.Address == "bob@mail2go.local")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_RecipientMatchesLocalMailbox_LinksMailboxId()
    {
        var mailboxId = Guid.NewGuid();
        var mailbox   = new Mailbox(mailboxId, Guid.NewGuid(), Guid.NewGuid(),
                                    "alice@mail2go.local", DateTimeOffset.UtcNow);

        _mailboxRepo.GetByAddressAsync("alice@mail2go.local", Arg.Any<CancellationToken>())
                    .Returns(mailbox);

        var cmd = BuildCommand(to: "alice@mail2go.local");
        await _sut.HandleAsync(cmd, CancellationToken.None);

        await _emailRepo.Received(1).AddAsync(
            Arg.Is<EmailMessage>(m => m.Recipients.Any(r => r.MailboxId == mailboxId)),
            Arg.Any<CancellationToken>());
    }
}
