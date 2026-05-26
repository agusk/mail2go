// Mail2Go — Sample SMTP Client using System.Net.Mail
// Sends a test email to a Mail2Go instance via plain SMTP.
//
// Usage:
//   dotnet run                                    (uses defaults)
//   dotnet run -- <host> <port> <from> <to> <user> <pass>
//
// Defaults:
//   host  = localhost
//   port  = 2525
//   from  = user@mail2go.local
//   to    = admin@mail2go.local
//   user  = user@mail2go.local
//   pass  = pass123

using System.Net;
using System.Net.Mail;

var host  = args.ElementAtOrDefault(0) ?? "localhost";
var port  = int.Parse(args.ElementAtOrDefault(1) ?? "2525");
var from  = args.ElementAtOrDefault(2) ?? "user@mail2go.local";
var to    = args.ElementAtOrDefault(3) ?? "admin@mail2go.local";
var user  = args.ElementAtOrDefault(4) ?? "user@mail2go.local";
var pass  = args.ElementAtOrDefault(5) ?? "pass123";

Console.WriteLine($"Mail2Go — SmtpClient Sample");
Console.WriteLine($"  Server : {host}:{port}");
Console.WriteLine($"  From   : {from}");
Console.WriteLine($"  To     : {to}");
Console.WriteLine();

using var client = new SmtpClient(host, port)
{
    Credentials = new NetworkCredential(user, pass),
    EnableSsl   = false,
    DeliveryMethod = SmtpDeliveryMethod.Network
};

var message = new MailMessage
{
    From    = new MailAddress(from, "Mail2Go Test"),
    Subject = $"[SmtpClient] Test email — {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
    Body    = $"""
               Hello from the Mail2Go SmtpClient sample!

               This message was sent using System.Net.Mail.SmtpClient.

               Sent at : {DateTime.UtcNow:O}
               From    : {from}
               To      : {to}
               Server  : {host}:{port}

               If you can read this, your Mail2Go SMTP server is working correctly.
               """,
    IsBodyHtml = false
};
message.To.Add(new MailAddress(to));

try
{
    await client.SendMailAsync(message);
    Console.WriteLine("✓ Message sent successfully.");
    Console.WriteLine($"  Check your inbox at http://localhost:5050");
}
catch (SmtpException ex)
{
    Console.Error.WriteLine($"✗ SMTP error: {ex.StatusCode} — {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"✗ Error: {ex.Message}");
    return 1;
}

return 0;
