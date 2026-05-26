using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Emails;
using Mail2Go.Application.UseCases.Queries;
using Mail2Go.Web.ViewModels.Compose;
using Mail2Go.Web.Areas.Admin.ViewModels;

namespace Mail2Go.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class MessagesController : Controller
{
    private readonly IEmailMessageRepository _emailRepo;
    private readonly GetMessageDetailQuery _messageDetailQuery;
    private readonly DeleteEmailHandler _deleteHandler;
    private readonly ComposeEmailHandler _composeHandler;
    private readonly ICurrentUserService _currentUserService;

    public MessagesController(
        IEmailMessageRepository emailRepo,
        GetMessageDetailQuery messageDetailQuery,
        DeleteEmailHandler deleteHandler,
        ComposeEmailHandler composeHandler,
        ICurrentUserService currentUserService)
    {
        _emailRepo = emailRepo;
        _messageDetailQuery = messageDetailQuery;
        _deleteHandler = deleteHandler;
        _composeHandler = composeHandler;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index(Guid? selected, CancellationToken ct)
    {
        var messages = await _emailRepo.GetAllAsync(ct);
        var detail = selected.HasValue ? await _messageDetailQuery.HandleAsync(selected.Value, ct) : null;
        var vm = new MessagesIndexViewModel { Messages = messages, Selected = detail, SelectedId = selected };
        return View(vm);
    }

    public async Task<IActionResult> Raw(Guid id, CancellationToken ct)
    {
        var message = await _messageDetailQuery.HandleAsync(id, ct);
        if (message is null) return NotFound();
        return Content(message.RawMime ?? string.Empty, "text/plain");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _deleteHandler.HandleAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Compose(string? to = null, string? subject = null)
    {
        return View(new ComposeViewModel { To = to ?? string.Empty, Subject = subject ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Compose(ComposeViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _currentUserService.UserId;
        if (userId is null) return Challenge();

        var toList = model.To.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var ccList = model.Cc?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [];
        var bccList = model.Bcc?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [];

        var command = new ComposeEmailCommand
        {
            SenderUserId = userId.Value,
            FromAddress = model.From,
            ToAddresses = toList,
            CcAddresses = ccList,
            BccAddresses = bccList,
            Subject = model.Subject,
            TextBody = model.Body,
            HtmlBody = null,
            SkipMailboxOwnerCheck = true
        };

        await _composeHandler.HandleAsync(command, ct);
        return RedirectToAction(nameof(Index));
    }
}
