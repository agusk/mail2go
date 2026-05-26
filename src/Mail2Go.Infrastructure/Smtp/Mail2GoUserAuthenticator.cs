using SmtpServer;
using SmtpServer.Authentication;
using Mail2Go.Application.Abstractions;

namespace Mail2Go.Infrastructure.Smtp;

public sealed class Mail2GoUserAuthenticator : UserAuthenticator
{
    private readonly ISmtpAuthenticationService _authService;

    public Mail2GoUserAuthenticator(ISmtpAuthenticationService authService)
    {
        _authService = authService;
    }

    public override async Task<bool> AuthenticateAsync(
        ISessionContext context,
        string user,
        string password,
        CancellationToken cancellationToken)
    {
        var result = await _authService.AuthenticateAsync(user, password, cancellationToken);
        return result.IsAuthenticated;
    }
}
