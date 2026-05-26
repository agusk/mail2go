namespace Mail2Go.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Username { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
