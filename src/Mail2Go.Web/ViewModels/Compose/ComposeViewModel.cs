using System.ComponentModel.DataAnnotations;

namespace Mail2Go.Web.ViewModels.Compose;

public sealed class ComposeViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "From")]
    public string From { get; set; } = string.Empty;

    [Required]
    [Display(Name = "To")]
    public string To { get; set; } = string.Empty;

    [Display(Name = "Cc")]
    public string? Cc { get; set; }

    [Display(Name = "Bcc")]
    public string? Bcc { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.MultilineText)]
    public string Body { get; set; } = string.Empty;
}
