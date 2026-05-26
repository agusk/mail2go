namespace Mail2Go.Application.Settings;

public sealed class SmtpSettings
{
    public string Host { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 2525;
    public bool EnableTls { get; set; } = false;
    public bool RequireAuthentication { get; set; } = true;
    public bool AllowAnonymous { get; set; } = false;
    public long MaxMessageSizeBytes { get; set; } = 10485760;
}
