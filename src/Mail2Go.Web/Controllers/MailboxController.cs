using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mail2Go.Application.Abstractions;
using Mail2Go.Application.UseCases.Emails;
using Mail2Go.Application.UseCases.Queries;
using Mail2Go.Web.ViewModels.Compose;

namespace Mail2Go.Web.Controllers;

[Authorize]
public sealed class MailboxController : Controller
{
    private readonly GetMyMailboxQuery _myMailboxQuery;
    private readonly GetMessageDetailQuery _messageDetailQuery;
    private readonly DeleteEmailHandler _deleteHandler;
    private readonly ComposeEmailHandler _composeHandler;
    private readonly ICurrentUserService _currentUserService;

    public MailboxController(
        GetMyMailboxQuery myMailboxQuery,
        GetMessageDetailQuery messageDetailQuery,
        DeleteEmailHandler deleteHandler,
        ComposeEmailHandler composeHandler,
        ICurrentUserService currentUserService)
    {
        _myMailboxQuery = myMailboxQuery;
        _messageDetailQuery = messageDetailQuery;
        _deleteHandler = deleteHandler;
        _composeHandler = composeHandler;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Challenge();

        var results = await _myMailboxQuery.HandleAsync(userId.Value, ct);
        return View(results);
    }

    public async Task<IActionResult> Message(Guid id, CancellationToken ct)
    {
        var message = await _messageDetailQuery.HandleAsync(id, ct);
        if (message is null) return NotFound();
        return View(message);
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
    public IActionResult Compose()
    {
        return View(new ComposeViewModel());
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
            HtmlBody = null
        };

        await _composeHandler.HandleAsync(command, ct);
        return RedirectToAction(nameof(Index));
    }
}
