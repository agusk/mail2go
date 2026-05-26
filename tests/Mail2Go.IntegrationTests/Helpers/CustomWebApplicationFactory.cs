using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mail2Go.Infrastructure.Persistence;

namespace Mail2Go.IntegrationTests.Helpers;

/// <summary>
/// Custom factory that spins up the full ASP.NET Core app using a per-test-run
/// SQLite temp file and an OS-assigned free SMTP port, so tests are isolated.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>The SMTP port that this factory instance is using (OS-assigned free port).</summary>
    public int SmtpTestPort { get; } = GetFreeTcpPort();

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"mail2go-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override SMTP settings — use a free OS-assigned port so parallel
        // test class fixtures don't collide.
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Port"]                  = SmtpTestPort.ToString(),
                ["Smtp:RequireAuthentication"] = "false",
                ["Smtp:AllowAnonymous"]        = "true",
            });
        });

        // Replace the DbContext registration so it uses the temp SQLite file.
        // ConfigureAppConfiguration alone is not reliable in the .NET 6+ minimal
        // hosting model because Program.cs reads the connection string at builder
        // time, before the factory's config overrides are applied.
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContextOptions<AppDbContext> registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register a new DbContext pointing at the temp file
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Temp file may still be locked by SQLite after the host shuts down.
        // Silently ignore — the OS will clean up the temp directory eventually.
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch (IOException) { }
    }

    /// <summary>Asks the OS for a free TCP port by binding to port 0.</summary>
    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
