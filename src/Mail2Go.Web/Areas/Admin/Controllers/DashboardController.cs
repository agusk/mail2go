using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mail2Go.Application.UseCases.Queries;
using System.Net.Sockets;

namespace Mail2Go.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class DashboardController : Controller
{
    private readonly GetDashboardQuery _dashboardQuery;

    public DashboardController(GetDashboardQuery dashboardQuery)
    {
        _dashboardQuery = dashboardQuery;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var stats = await _dashboardQuery.HandleAsync(ct);
        return View(stats);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SmtpPing()
    {
        try
        {
            using var tcp = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await tcp.ConnectAsync("127.0.0.1", 2525, cts.Token);
            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, error = ex.Message });
        }
    }
}
