using System.ComponentModel.DataAnnotations;

namespace Mail2Go.Web.ViewModels.Admin;

public sealed class CreateDomainViewModel
{
    [Required]
    [Display(Name = "Domain Name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class DomainListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CreateMailboxViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9._%+\-]+$", ErrorMessage = "Only letters, numbers, and . _ % + - are allowed.")]
    [Display(Name = "Email Local Part")]
    public string LocalPart { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Domain")]
    public Guid DomainId { get; set; }

    public List<DomainListItem> Domains { get; set; } = [];
}
