namespace Mail2Go.Application.Abstractions;

public interface ISmtpAuthenticationService
{
    Task<SmtpAuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken);
}
