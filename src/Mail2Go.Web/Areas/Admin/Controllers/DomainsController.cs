using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Domains;

namespace Mail2Go.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class DomainsController : Controller
{
    private readonly CreateDomainHandler _createHandler;
    private readonly UpdateDomainHandler _updateHandler;
    private readonly IMailDomainRepository _domainRepo;

    public DomainsController(
        CreateDomainHandler createHandler,
        UpdateDomainHandler updateHandler,
        IMailDomainRepository domainRepo)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _domainRepo = domainRepo;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var domains = await _domainRepo.GetAllAsync(ct);
        return View(domains);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(nameof(name), "Domain name is required.");
            return View();
        }

        await _createHandler.HandleAsync(new CreateDomainCommand { Name = name }, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var domain = await _domainRepo.GetByIdAsync(id, ct);
        if (domain is null) return NotFound();

        bool enable = !domain.IsEnabled;
        await _updateHandler.HandleAsync(
            new UpdateDomainCommand { DomainId = id, Name = domain.Name, IsEnabled = enable }, ct);
        return RedirectToAction(nameof(Index));
    }
}
