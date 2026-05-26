using System.Net.Sockets;
using Shouldly;
using Mail2Go.IntegrationTests.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using Mail2Go.Application.Abstractions;
using Xunit;

namespace Mail2Go.IntegrationTests.Smtp;

/// <summary>
/// SMTP integration tests.  The factory starts the embedded SmtpServer on
/// port <see cref="CustomWebApplicationFactory.SmtpTestPort"/> (12525).
/// Tests send real SMTP messages and verify they land in the database.
/// </summary>
public sealed class SmtpSendTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly int _port = CustomWebApplicationFactory.SmtpTestPort;

    public SmtpSendTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        // Trigger server initialization by creating any HttpClient
        // (WebApplicationFactory is lazy — this ensures the host is started).
        _ = factory.CreateClient();
    }

    [Fact]
    public async Task SmtpServer_AcceptsConnection_Returns220Greeting()
    {
        // Give the background service a moment to bind.
        await Task.Delay(500);

        using var tcp = new TcpClient();
        await tcp.ConnectAsync("127.0.0.1", _port);

        using var reader = new StreamReader(tcp.GetStream(), leaveOpen: true);
        var greeting = await reader.ReadLineAsync();

        greeting.ShouldNotBeNull();
        greeting!.ShouldStartWith("220");
    }

    [Fact]
    public async Task SmtpServer_SendEmail_IsStoredInDatabase()
    {
        // Give the background service a moment to bind.
        await Task.Delay(500);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Test Sender", "sender@external.com"));
        message.To.Add(new MailboxAddress("Admin", "admin@mail2go.local"));
        message.Subject = $"Integration test {Guid.NewGuid():N}";
        message.Body    = new TextPart("plain") { Text = "Hello from integration test." };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("127.0.0.1", _port, SecureSocketOptions.None);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        // Wait for the async message store handler to finish.
        await Task.Delay(800);

        // Verify the message was persisted via the repository.
        using var scope = _factory.Services.CreateScope();
        var emailRepo = scope.ServiceProvider.GetRequiredService<IEmailMessageRepository>();
        var all = await emailRepo.GetAllAsync(CancellationToken.None);

        all.ShouldContain(m =>
            m.Subject == message.Subject &&
            m.FromAddress == "sender@external.com");
    }
}
