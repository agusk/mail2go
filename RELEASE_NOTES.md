# Mail2Go v0.1.0 — First Public Release

Mail2Go is a lightweight self-hosted mail server built on ASP.NET Core 10. It receives, stores, and manages inbound emails over standard SMTP, and provides a web-based admin dashboard to browse domains, mailboxes, and messages — all backed by a zero-config SQLite database.

---

## ✨ Highlights

### Embedded SMTP Server
Mail2Go runs an SMTP listener (port 2525) directly inside the web process using SmtpServer 11.x. No external MTA (Postfix, Sendmail, etc.) is required. Drop it on any machine and it starts accepting mail immediately.

### Web Admin Dashboard
A clean ASP.NET Core MVC interface lets administrators manage:
- **Domains** — add and remove accepted mail domains
- **Mailboxes** — create per-domain user mailboxes
- **Messages** — browse, read, and delete stored emails

### Zero-Config SQLite Storage
All data — users, domains, mailboxes, and messages — lives in a single `mail2go.db` file powered by EF Core 10 migrations. No database server to install, no connection string to configure beyond the file path.

### Role-Based Access Control
Built on ASP.NET Core Identity with two roles out of the box:
- **Admin** — full access to all domains and system settings
- **User** — access limited to their own mailbox

### Clean Architecture
The codebase follows a strict layered structure:
```
Mail2Go.Domain          → Entities, value objects, domain rules
Mail2Go.Application     → Use-case handlers (CQRS-style), abstractions
Mail2Go.Infrastructure  → EF Core, Identity, SMTP service, repositories
Mail2Go.Web             → MVC controllers, views, middleware, DI composition
```

### Multi-Language Client Samples
Four ready-to-run examples showing how to send mail to Mail2Go:

| Sample | Location |
|--------|----------|
| .NET — `System.Net.Mail.SmtpClient` | `src/Mail2Go.Clients/SmtpClientSample/` |
| .NET — MailKit | `src/Mail2Go.Clients/MailKitSample/` |
| Python — `smtplib` | `src/Mail2Go.Clients/PythonSmtpSample/` |
| Node.js — nodemailer | `src/Mail2Go.Clients/NodejsSmtpSample/` |

---

## 📦 Downloads

Each archive is a **self-contained, single-file executable**. No .NET runtime required on the target machine.

| Platform | File |
|----------|------|
| Windows x64 | `mail2go-v0.1.0-win-x64.zip` |
| Linux x64 | `mail2go-v0.1.0-linux-x64.zip` |
| macOS x64 | `mail2go-v0.1.0-osx-x64.zip` |

---

## 🚀 Quick Start

### Windows
```bat
:: Extract the zip, then:
Mail2Go.Web.exe
:: Open http://localhost:5050
```

### Linux / macOS
```bash
unzip mail2go-v0.1.0-linux-x64.zip
chmod +x Mail2Go.Web
./Mail2Go.Web
# Open http://localhost:5050
```

### Default Credentials
| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@mail2go.local` | `pass123` |
| User | `user@mail2go.local` | `pass123` |

> **Change the default passwords immediately after first login.**

### Send a Test Email
Point any SMTP client at `localhost:2525` (no TLS, no authentication required by default):
```bash
# Python one-liner
python -c "
import smtplib, email.mime.text as t
m = t.MIMEText('Hello Mail2Go!')
m['Subject'] = 'Test'; m['From'] = 'me@example.com'; m['To'] = 'admin@mail2go.local'
s = smtplib.SMTP('localhost', 2525); s.send_message(m); s.quit()
print('Sent!')
"
```

---

## ⚙️ Configuration

All settings live in `appsettings.json` next to the executable:

```json
{
  "Database": {
    "ConnectionString": "Data Source=data/mail2go.db"
  },
  "Smtp": {
    "Port": 2525,
    "RequireAuthentication": false,
    "AllowAnonymous": true
  }
}
```

The `data/` folder (database + attachments) is created automatically on first run.

---

## ⚠️ Known Limitations

- **No TLS/SSL on SMTP** — intended for local development and internal networks only
- **No outbound delivery** — Mail2Go only receives and stores inbound mail; it does not relay or forward messages
- **SQLite only** — no PostgreSQL/SQL Server support in this release
- **Single-node** — no clustering or replication support

---

## 🔄 What's Changed

- Initial public release — all features listed above are new
