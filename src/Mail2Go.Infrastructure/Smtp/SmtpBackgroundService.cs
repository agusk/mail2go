using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtpServer;
using SmtpServer.ComponentModel;
using Mail2Go.Application.Settings;

namespace Mail2Go.Infrastructure.Smtp;

public sealed class SmtpBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<SmtpBackgroundService> _logger;

    public SmtpBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<SmtpSettings> smtpSettings,
        ILogger<SmtpBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SMTP server starting on port {Port}", _smtpSettings.Port);

        using var scope = _scopeFactory.CreateScope();
        var messageStore = scope.ServiceProvider.GetRequiredService<Mail2GoMessageStore>();
        var authenticator = scope.ServiceProvider.GetRequiredService<Mail2GoUserAuthenticator>();

        var options = new SmtpServerOptionsBuilder()
            .ServerName("mail2go")
            .Endpoint(builder =>
            {
                builder.Port(_smtpSettings.Port, false);
                builder.AuthenticationRequired(
                    _smtpSettings.RequireAuthentication && !_smtpSettings.AllowAnonymous);
                builder.AllowUnsecureAuthentication();
            })
            .Build();

        var serviceProvider = new SmtpServer.ComponentModel.ServiceProvider();
        serviceProvider.Add(messageStore);
        serviceProvider.Add(authenticator);

        var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);

        try
        {
            await smtpServer.StartAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SMTP server stopped.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP server encountered an error.");
        }
    }
}
