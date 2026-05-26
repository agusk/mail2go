using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mail2Go.Infrastructure.Persistence;
using Mail2Go.Infrastructure.Seeding;

namespace Mail2Go.IntegrationTests.Helpers;

/// <summary>
/// Custom factory that spins up the full ASP.NET Core app using a per-test-run
/// SQLite temp file and an unused SMTP port, so tests are isolated.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Pick a high port that is unlikely to conflict with the real server.
    public const int SmtpTestPort = 12525;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"mail2go-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = $"Data Source={_dbPath}",
                ["Smtp:Port"]                 = SmtpTestPort.ToString(),
                ["Smtp:RequireAuthentication"] = "false",
                ["Smtp:AllowAnonymous"]        = "true",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Nothing extra — DatabaseSeeder.SeedAsync is called by Program.cs
            // and will create+migrate the temp SQLite DB automatically.
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
