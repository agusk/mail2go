using Mail2Go.Domain.Emails;

namespace Mail2Go.Web.Areas.Admin.ViewModels;

public sealed class MessagesIndexViewModel
{
    public IReadOnlyList<EmailMessage> Messages { get; init; } = [];
    public EmailMessage? Selected { get; init; }
    public Guid? SelectedId { get; init; }
}
