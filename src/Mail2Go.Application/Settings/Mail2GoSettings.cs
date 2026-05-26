namespace Mail2Go.Application.Settings;

public sealed class Mail2GoSettings
{
    public string ApplicationName { get; set; } = "Mail2Go";
    public bool AllowUnknownDomains { get; set; } = true;
    public bool AllowUnknownRecipients { get; set; } = true;
    public bool RequirePasswordChangeForSeedUsers { get; set; } = true;
}
