using Microsoft.AspNetCore.Identity;
using Mail2Go.Application.Abstractions;
using Mail2Go.Infrastructure.Identity;

namespace Mail2Go.Infrastructure.Smtp;

public sealed class SmtpAuthenticationService : ISmtpAuthenticationService
{
    private readonly UserManager<AppUser> _userManager;

    public SmtpAuthenticationService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<SmtpAuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        AppUser? user = await _userManager.FindByEmailAsync(username)
                     ?? await _userManager.FindByNameAsync(username);

        if (user is null || !user.IsActive)
        {
            return SmtpAuthenticationResult.Failure("Invalid credentials.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            return SmtpAuthenticationResult.Failure("Invalid credentials.");
        }

        return SmtpAuthenticationResult.Success(user.Id, user.UserName!);
    }
}
