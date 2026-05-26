using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.Settings;
using Mail2Go.Application.UseCases.Domains;
using Mail2Go.Application.UseCases.Emails;
using Mail2Go.Application.UseCases.Mailboxes;
using Mail2Go.Application.UseCases.Queries;
using Mail2Go.Infrastructure.Identity;
using Mail2Go.Infrastructure.Persistence;
using Mail2Go.Infrastructure.Repositories;
using Mail2Go.Infrastructure.Seeding;
using Mail2Go.Infrastructure.Services;
using Mail2Go.Infrastructure.Smtp;

var builder = WebApplication.CreateBuilder(args);

// Settings
builder.Services.Configure<Mail2GoSettings>(builder.Configuration.GetSection("Mail2Go"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// Database
var connectionString = builder.Configuration["Database:ConnectionString"]
    ?? "Data Source=data/mail2go.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Repositories
builder.Services.AddScoped<IEmailMessageRepository, EmailMessageRepository>();
builder.Services.AddScoped<IMailDomainRepository, MailDomainRepository>();
builder.Services.AddScoped<IMailboxRepository, MailboxRepository>();

// Application services
builder.Services.AddScoped<ISmtpAuthenticationService, SmtpAuthenticationService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Use cases
builder.Services.AddScoped<CaptureIncomingEmailHandler>();
builder.Services.AddScoped<ComposeEmailHandler>();
builder.Services.AddScoped<DeleteEmailHandler>();
builder.Services.AddScoped<CreateDomainHandler>();
builder.Services.AddScoped<UpdateDomainHandler>();
builder.Services.AddScoped<CreateMailboxHandler>();
builder.Services.AddScoped<GetMyMailboxQuery>();
builder.Services.AddScoped<GetDomainInboxQuery>();
builder.Services.AddScoped<GetMessageDetailQuery>();
builder.Services.AddScoped<GetDashboardQuery>();

// Seed
builder.Services.AddScoped<DatabaseSeeder>();

// SMTP background service
builder.Services.AddSingleton<Mail2GoMessageStore>();
builder.Services.AddScoped<Mail2GoUserAuthenticator>();
builder.Services.AddHostedService<SmtpBackgroundService>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }

