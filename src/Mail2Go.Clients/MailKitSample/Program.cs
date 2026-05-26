// Mail2Go — Sample SMTP Client using MailKit
// Sends a test email (plain text + HTML) to a Mail2Go instance.
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

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

var host  = args.ElementAtOrDefault(0) ?? "localhost";
var port  = int.Parse(args.ElementAtOrDefault(1) ?? "2525");
var from  = args.ElementAtOrDefault(2) ?? "user@mail2go.local";
var to    = args.ElementAtOrDefault(3) ?? "admin@mail2go.local";
var user  = args.ElementAtOrDefault(4) ?? "user@mail2go.local";
var pass  = args.ElementAtOrDefault(5) ?? "pass123";

Console.WriteLine("Mail2Go — MailKit Sample");
Console.WriteLine($"  Server : {host}:{port}");
Console.WriteLine($"  From   : {from}");
Console.WriteLine($"  To     : {to}");
Console.WriteLine();

var timestamp = DateTimeOffset.UtcNow;

var textBody = $"""
               Hello from the Mail2Go MailKit sample!

               This message was sent using MailKit (MimeKit).

               Sent at : {timestamp:O}
               From    : {from}
               To      : {to}
               Server  : {host}:{port}

               If you can read this, your Mail2Go SMTP server is working correctly.
               """;

var htmlBody = $"""
               <!DOCTYPE html>
               <html>
               <head><meta charset="utf-8" /></head>
               <body style="font-family:sans-serif;max-width:600px;margin:2rem auto;">
                 <h2 style="color:#4f8ef7;">Mail2Go MailKit Sample</h2>
                 <p>This message was sent using <strong>MailKit</strong> (MimeKit).</p>
                 <table style="border-collapse:collapse;width:100%;">
                   <tr><td style="padding:4px 8px;color:#888;">Sent at</td><td style="padding:4px 8px;">{timestamp:O}</td></tr>
                   <tr><td style="padding:4px 8px;color:#888;">From</td><td style="padding:4px 8px;">{from}</td></tr>
                   <tr><td style="padding:4px 8px;color:#888;">To</td><td style="padding:4px 8px;">{to}</td></tr>
                   <tr><td style="padding:4px 8px;color:#888;">Server</td><td style="padding:4px 8px;">{host}:{port}</td></tr>
                 </table>
                 <p style="margin-top:1.5rem;color:#4caf50;font-weight:bold;">
                   ✓ Your Mail2Go SMTP server is working correctly.
                 </p>
               </body>
               </html>
               """;

var message = new MimeMessage();
message.From.Add(new MailboxAddress("Mail2Go Test", from));
message.To.Add(new MailboxAddress(to, to));
message.Subject = $"[MailKit] Test email — {timestamp:yyyy-MM-dd HH:mm:ss} UTC";

var bodyBuilder = new BodyBuilder
{
    TextBody = textBody,
    HtmlBody = htmlBody
};
message.Body = bodyBuilder.ToMessageBody();

try
{
    using var smtp = new SmtpClient();
    await smtp.ConnectAsync(host, port, SecureSocketOptions.None);
    await smtp.AuthenticateAsync(user, pass);
    await smtp.SendAsync(message);
    await smtp.DisconnectAsync(true);

    Console.WriteLine("✓ Message sent successfully.");
    Console.WriteLine($"  Check your inbox at http://localhost:5050");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"✗ Error: {ex.GetType().Name} — {ex.Message}");
    return 1;
}

return 0;
