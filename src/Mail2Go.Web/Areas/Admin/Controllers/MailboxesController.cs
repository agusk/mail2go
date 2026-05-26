using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Mailboxes;
using Mail2Go.Infrastructure.Identity;
using Mail2Go.Web.ViewModels.Admin;

namespace Mail2Go.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class MailboxesController : Controller
{
    private readonly CreateMailboxHandler _createHandler;
    private readonly IMailboxRepository _mailboxRepo;
    private readonly IMailDomainRepository _domainRepo;
    private readonly UserManager<AppUser> _userManager;

    public MailboxesController(
        CreateMailboxHandler createHandler,
        IMailboxRepository mailboxRepo,
        IMailDomainRepository domainRepo,
        UserManager<AppUser> userManager)
    {
        _createHandler = createHandler;
        _mailboxRepo = mailboxRepo;
        _domainRepo = domainRepo;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var mailboxes = await _mailboxRepo.GetAllAsync(ct);
        return View(mailboxes);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var domains = await _domainRepo.GetAllAsync(ct);
        var vm = new CreateMailboxViewModel
        {
            Domains = domains.Select(d => new DomainListItem
            {
                Id = d.Id,
                Name = d.Name,
                Status = d.IsEnabled ? "Active" : "Disabled",
                CreatedAt = d.CreatedAt
            }).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMailboxViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var domains = await _domainRepo.GetAllAsync(ct);
            model.Domains = domains.Select(d => new DomainListItem
            {
                Id = d.Id,
                Name = d.Name,
                Status = d.IsEnabled ? "Active" : "Disabled",
                CreatedAt = d.CreatedAt
            }).ToList();
            return View(model);
        }

        // Create Identity user
        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.LocalPart.Trim().ToLowerInvariant() + "@" + "placeholder",
            DisplayName = model.DisplayName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            var domainsRetry = await _domainRepo.GetAllAsync(ct);
            model.Domains = domainsRetry.Select(d => new DomainListItem
            {
                Id = d.Id,
                Name = d.Name,
                Status = d.IsEnabled ? "Active" : "Disabled",
                CreatedAt = d.CreatedAt
            }).ToList();
            return View(model);
        }

        var selectedDomain = await _domainRepo.GetByIdAsync(model.DomainId, ct);
        var address = model.LocalPart.Trim().ToLowerInvariant() + "@" + (selectedDomain?.Name ?? string.Empty);

        // Update Identity user email to the real composed address
        user.Email = address;
        user.NormalizedEmail = _userManager.NormalizeEmail(address);
        await _userManager.UpdateAsync(user);

        await _createHandler.HandleAsync(new CreateMailboxCommand
        {
            UserId = user.Id,
            DomainId = model.DomainId,
            Address = address
        }, ct);

        return RedirectToAction(nameof(Index));
    }
}

