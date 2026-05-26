using System.Net;
using Shouldly;
using Mail2Go.IntegrationTests.Helpers;
using Xunit;

namespace Mail2Go.IntegrationTests.Web;

/// <summary>
/// Integration tests for the Admin Dashboard HTTP endpoints.
/// The factory starts the full app (SQLite temp DB + SMTP on port 12525).
/// </summary>
public sealed class DashboardTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DashboardTests(CustomWebApplicationFactory factory)
    {
        // AllowAutoRedirect = false so we can assert 302 redirects explicitly.
        _client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Dashboard_Unauthenticated_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/Admin/Dashboard");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().ShouldContain("/Account/Login");
    }

    [Fact]
    public async Task AdminArea_Unauthenticated_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/Admin/Mailboxes");

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().ShouldContain("/Account/Login");
    }

    [Fact]
    public async Task LoginPage_Get_ReturnsOk()
    {
        var response = await _client.GetAsync("/Account/Login");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginPage_Post_WithValidCredentials_RedirectsToDashboard()
    {
        // First GET the login page to obtain the antiforgery token.
        var getResp   = await _client.GetAsync("/Account/Login");
        var html      = await getResp.Content.ReadAsStringAsync();
        var token     = ExtractAntiForgeryToken(html);
        var cookies   = getResp.Headers.GetValues("Set-Cookie").FirstOrDefault() ?? string.Empty;

        var formData = new Dictionary<string, string>
        {
            ["UsernameOrEmail"] = "admin",
            ["Password"]        = "pass123",
            ["__RequestVerificationToken"] = token
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/Account/Login")
        {
            Content = new FormUrlEncodedContent(formData)
        };

        // Forward the antiforgery cookie from the GET response.
        request.Headers.Add("Cookie", cookies);

        var postResp = await _client.SendAsync(request);

        // Successful login redirects away from the login page.
        postResp.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        postResp.Headers.Location?.ToString().ShouldNotContain("/Account/Login");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string ExtractAntiForgeryToken(string html)
    {
        // Parse the hidden __RequestVerificationToken input value.
        const string marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return string.Empty;

        start += marker.Length;
        var end = html.IndexOf('"', start);
        return end < 0 ? string.Empty : html[start..end];
    }
}
