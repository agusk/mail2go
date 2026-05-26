namespace Mail2Go.Application.Abstractions;

public sealed class SmtpAuthenticationResult
{
    public bool IsAuthenticated { get; private set; }

    public Guid? UserId { get; private set; }

    public string? Username { get; private set; }

    public string? FailureReason { get; private set; }

    private SmtpAuthenticationResult()
    {
    }

    public static SmtpAuthenticationResult Success(Guid userId, string username)
    {
        return new SmtpAuthenticationResult
        {
            IsAuthenticated = true,
            UserId = userId,
            Username = username
        };
    }

    public static SmtpAuthenticationResult Failure(string reason)
    {
        return new SmtpAuthenticationResult
        {
            IsAuthenticated = false,
            FailureReason = reason
        };
    }
}
