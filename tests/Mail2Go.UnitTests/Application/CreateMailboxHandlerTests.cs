using Shouldly;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Mailboxes;
using Mail2Go.Domain.Mailboxes;
using NSubstitute;
using Xunit;

namespace Mail2Go.UnitTests.Application;

public sealed class CreateMailboxHandlerTests
{
    private readonly IMailboxRepository _mailboxRepo = Substitute.For<IMailboxRepository>();
    private readonly CreateMailboxHandler _sut;

    public CreateMailboxHandlerTests()
    {
        _sut = new CreateMailboxHandler(_mailboxRepo);
    }

    [Fact]
    public async Task HandleAsync_NewAddress_CreatesAndSavesMailbox()
    {
        // Arrange
        var cmd = new CreateMailboxCommand
        {
            UserId   = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            Address  = "alice@mail2go.local"
        };

        _mailboxRepo.ExistsAsync("alice@mail2go.local", Arg.Any<CancellationToken>())
                    .Returns(false);

        // Act
        var result = await _sut.HandleAsync(cmd, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldBe("alice@mail2go.local");
        result.UserId.ShouldBe(cmd.UserId);
        result.DomainId.ShouldBe(cmd.DomainId);

        await _mailboxRepo.Received(1).AddAsync(Arg.Any<Mailbox>(), Arg.Any<CancellationToken>());
        await _mailboxRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NormalisesAddressToLowercase()
    {
        var cmd = new CreateMailboxCommand
        {
            UserId   = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            Address  = "Alice@Mail2Go.Local"
        };

        _mailboxRepo.ExistsAsync("alice@mail2go.local", Arg.Any<CancellationToken>())
                    .Returns(false);

        var result = await _sut.HandleAsync(cmd, CancellationToken.None);

        result.Address.ShouldBe("alice@mail2go.local");
    }

    [Fact]
    public async Task HandleAsync_DuplicateAddress_ThrowsInvalidOperationException()
    {
        var cmd = new CreateMailboxCommand
        {
            UserId   = Guid.NewGuid(),
            DomainId = Guid.NewGuid(),
            Address  = "existing@mail2go.local"
        };

        _mailboxRepo.ExistsAsync("existing@mail2go.local", Arg.Any<CancellationToken>())
                    .Returns(true);

        Func<Task> act = () => _sut.HandleAsync(cmd, CancellationToken.None);

        var ex = await Should.ThrowAsync<InvalidOperationException>(act);
        ex.Message.ShouldContain("already exists");

        await _mailboxRepo.DidNotReceive().AddAsync(Arg.Any<Mailbox>(), Arg.Any<CancellationToken>());
    }
}
