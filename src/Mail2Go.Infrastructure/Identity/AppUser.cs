using Microsoft.AspNetCore.Identity;

namespace Mail2Go.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
}
