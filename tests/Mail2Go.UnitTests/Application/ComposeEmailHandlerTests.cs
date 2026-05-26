using Shouldly;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Emails;
using Mail2Go.Domain.Emails;
using Mail2Go.Domain.Mailboxes;
using NSubstitute;
using Xunit;

namespace Mail2Go.UnitTests.Application;

public sealed class ComposeEmailHandlerTests
{
    private readonly IEmailMessageRepository _emailRepo    = Substitute.For<IEmailMessageRepository>();
    private readonly IMailboxRepository      _mailboxRepo  = Substitute.For<IMailboxRepository>();
    private readonly ComposeEmailHandler     _sut;

    public ComposeEmailHandlerTests()
    {
        _sut = new ComposeEmailHandler(_emailRepo, _mailboxRepo);
    }

    private static Mailbox MakeMailbox(Guid userId, string address) =>
        new(Guid.NewGuid(), userId, Guid.NewGuid(), address, DateTimeOffset.UtcNow);

    [Fact]
    public async Task HandleAsync_OwnerSendsFromOwnMailbox_PersistsMessage()
    {
        var userId = Guid.NewGuid();
        var mailbox = MakeMailbox(userId, "alice@mail2go.local");

        _mailboxRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
                    .Returns(new[] { mailbox });
        _mailboxRepo.GetByAddressAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((Mailbox?)null);

        var cmd = new ComposeEmailCommand
        {
            SenderUserId   = userId,
            FromAddress    = "alice@mail2go.local",
            ToAddresses    = ["bob@mail2go.local"],
            CcAddresses    = [],
            BccAddresses   = [],
            Subject        = "Hello",
            TextBody       = "World",
            HtmlBody       = null
        };

        await _sut.HandleAsync(cmd, CancellationToken.None);

        await _emailRepo.Received(1).AddAsync(
            Arg.Is<EmailMessage>(m =>
                m.FromAddress == "alice@mail2go.local" &&
                m.Subject     == "Hello" &&
                m.Source      == MessageSource.WebCompose),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SenderNotMailboxOwner_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();

        _mailboxRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
                    .Returns(Array.Empty<Mailbox>());

        var cmd = new ComposeEmailCommand
        {
            SenderUserId = userId,
            FromAddress  = "notmine@mail2go.local",
            ToAddresses  = ["someone@mail2go.local"],
            CcAddresses  = [],
            BccAddresses = [],
            Subject      = "Hi",
            TextBody     = "Test",
            HtmlBody     = null
        };

        Func<Task> act = () => _sut.HandleAsync(cmd, CancellationToken.None);

        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldContain("does not own");

        await _emailRepo.DidNotReceive().AddAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SkipOwnerCheck_DoesNotValidateOwnership()
    {
        _mailboxRepo.GetByAddressAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((Mailbox?)null);

        var cmd = new ComposeEmailCommand
        {
            SenderUserId       = Guid.NewGuid(),
            FromAddress        = "admin@mail2go.local",
            ToAddresses        = ["user@mail2go.local"],
            CcAddresses        = [],
            BccAddresses       = [],
            Subject            = "Admin msg",
            TextBody           = "From admin",
            HtmlBody           = null,
            SkipMailboxOwnerCheck = true
        };

        await _sut.HandleAsync(cmd, CancellationToken.None);

        // GetByUserIdAsync must NOT be called when skip flag is set
        await _mailboxRepo.DidNotReceive()
                          .GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _emailRepo.Received(1).AddAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }
}
