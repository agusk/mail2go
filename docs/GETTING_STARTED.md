# Mail2Go — Getting Started

This guide covers everything you need to install, configure, and run Mail2Go for the first time.



## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Installation](#2-installation)
3. [Configuration](#3-configuration)
4. [Running the Application](#4-running-the-application)
5. [Running with Docker](#5-running-with-docker)
6. [Default Accounts](#6-default-accounts)
7. [Connecting an SMTP Client](#7-connecting-an-smtp-client)
8. [Verifying Everything Works](#8-verifying-everything-works)
9. [Next Steps](#9-next-steps)



## 1. Prerequisites

| Requirement | Minimum Version | Notes |
|---|---|---|
| **.NET SDK** | 10.0 | [Download](https://dotnet.microsoft.com/download) |
| **Git** | Any | For cloning the repository |
| **Docker** *(optional)* | 24+ | Only needed for Docker deployment |
| **OS** | Windows / Linux / macOS | Cross-platform |

Verify your .NET version:

```bash
dotnet --version
# Expected: 10.x.x
```



## 2. Installation

### Clone the Repository

```bash
git clone https://github.com/agusk/mail2go.git
cd mail2go
```

### Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages including EF Core, SmtpServer, and ASP.NET Core Identity.



## 3. Configuration

All configuration lives in `src/Mail2Go.Web/appsettings.json`.

### Default Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=data/mail2go.db"
  },
  "Smtp": {
    "Host": "0.0.0.0",
    "Port": 2525,
    "EnableTls": false,
    "RequireAuthentication": true,
    "AllowAnonymous": false,
    "MaxMessageSizeBytes": 10485760
  },
  "Mail2Go": {
    "AllowUnknownDomains": true,
    "AllowUnknownRecipients": true
  }
}
```

### Common Configuration Changes

#### Change the SMTP Port

```json
"Smtp": {
  "Port": 1025
}
```

> On Linux/macOS, ports below 1024 require root privileges. Use a port ≥ 1025.

#### Allow Unauthenticated SMTP (for quick testing)

```json
"Smtp": {
  "RequireAuthentication": false,
  "AllowAnonymous": true
}
```

#### Change the Database Path

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=/var/mail2go/mail2go.db"
}
```

#### Override via Environment Variables

You can override any setting using environment variables with the `__` separator:

```bash
# Linux / macOS
export Smtp__Port=1025
export Smtp__AllowAnonymous=true

# Windows PowerShell
$env:Smtp__Port = "1025"
$env:Smtp__AllowAnonymous = "true"
```

#### Override via `appsettings.Development.json`

For local development, create `src/Mail2Go.Web/appsettings.Development.json`:

```json
{
  "Smtp": {
    "Port": 2525,
    "AllowAnonymous": true,
    "RequireAuthentication": false
  }
}
```



## 4. Running the Application

### Development Mode

```bash
dotnet run --project src/Mail2Go.Web
```

Expected output:

```
info: Mail2Go starting...
info: Migrations applied.
info: Seed data applied.
info: SMTP server listening on 0.0.0.0:2525
info: Now listening on: http://localhost:5050
```

Open your browser at **http://localhost:5050**.

### Production Build

```bash
dotnet publish src/Mail2Go.Web -c Release -o ./publish
dotnet ./publish/Mail2Go.Web.dll
```

### Change the Web Port

```bash
dotnet run --project src/Mail2Go.Web --urls "http://0.0.0.0:8080"
```

Or in `appsettings.json`:

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://0.0.0.0:8080"
    }
  }
}
```



## 5. Running with Docker

Mail2Go ships with a multi-stage **Alpine-based Dockerfile** and a ready-to-use `docker-compose.yml`.

### Build and Start (Docker Compose)

```bash
docker compose up --build
```

This builds the image from source and starts the container. The database is persisted in a Docker named volume (`mail2go_data`).

- Web UI → **http://localhost:5050**
- SMTP → **localhost:2525**

### Start in Background

```bash
docker compose up -d
```

### Stop

```bash
docker compose down
```

### View Logs

```bash
docker compose logs -f mail2go
```

### Build Image Manually

```bash
docker build -t mail2go:latest .
```

### Run Image Manually

```bash
docker run -d \
  --name mail2go \
  -p 5050:5050 \
  -p 2525:2525 \
  -v mail2go_data:/app/data \
  mail2go:latest
```

### Environment Variable Overrides

Override any setting via environment variables using the `__` separator:

| Variable | Default | Description |
|---|---|---|
| `Smtp__Port` | `2525` | SMTP listen port |
| `Smtp__AllowAnonymous` | `false` | Allow unauthenticated SMTP |
| `Smtp__RequireAuthentication` | `true` | Require AUTH before MAIL FROM |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/mail2go.db` | SQLite path |
| `Mail2Go__AllowUnknownDomains` | `true` | Capture mail to unregistered domains |

Example override:

```bash
docker run -d \
  --name mail2go \
  -p 5050:5050 \
  -p 2525:2525 \
  -v mail2go_data:/app/data \
  -e Smtp__AllowAnonymous=true \
  -e Smtp__RequireAuthentication=false \
  mail2go:latest
```

### Data Persistence

The SQLite database is stored at `/app/data/mail2go.db` inside the container. The compose file maps this to a Docker named volume so data survives container restarts.

> The database and seed data are created automatically on first startup. No manual migration step is needed.


## 6. Default Accounts

Mail2Go creates seed data on first startup. These accounts are ready to use immediately.

### Admin Account

| Field | Value |
|---|---|
| Username | `admin` |
| Email | `admin@mail2go.local` |
| Password | `pass123` |
| Role | Admin |

> **Change the admin password after first login.**

### Default Test User

| Field | Value |
|---|---|
| Username | `user` |
| Email | `user@mail2go.local` |
| Password | `pass123` |
| Role | User |
| Mailbox | `user@mail2go.local` |

### Default Domain

```
mail2go.local
```



## 7. Connecting an SMTP Client

Configure your application (or email client) to use Mail2Go as the SMTP server.

### SMTP Settings

| Setting | Value |
|---|---|
| Host | `localhost` (or the server IP) |
| Port | `2525` |
| Encryption | None / Plain |
| Authentication | Required (unless `AllowAnonymous = true`) |
| Username | `user@mail2go.local` |
| Password | `pass123` |

### Example: .NET `SmtpClient`

> A runnable console sample is available in [`src/Mail2Go.Clients/SmtpClientSample/`](../src/Mail2Go.Clients/SmtpClientSample/).
> Run it with: `dotnet run --project src/Mail2Go.Clients/SmtpClientSample`

```csharp
using System.Net;
using System.Net.Mail;

var client = new SmtpClient("localhost", 2525)
{
    Credentials = new NetworkCredential("user@mail2go.local", "pass123"),
    EnableSsl = false
};

var message = new MailMessage
{
    From = new MailAddress("user@mail2go.local"),
    Subject = "Test from my app",
    Body = "Hello from Mail2Go!"
};
message.To.Add("admin@mail2go.local");

await client.SendMailAsync(message);
```

### Example: MailKit (.NET)

> A runnable console sample is available in [`src/Mail2Go.Clients/MailKitSample/`](../src/Mail2Go.Clients/MailKitSample/).
> Run it with: `dotnet run --project src/Mail2Go.Clients/MailKitSample`

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

var message = new MimeMessage();
message.From.Add(new MailboxAddress("Test User", "user@mail2go.local"));
message.To.Add(new MailboxAddress("Admin", "admin@mail2go.local"));
message.Subject = "Hello from MailKit";
message.Body = new TextPart("plain") { Text = "Test message." };

using var smtp = new SmtpClient();
await smtp.ConnectAsync("localhost", 2525, SecureSocketOptions.None);
await smtp.AuthenticateAsync("user@mail2go.local", "pass123");
await smtp.SendAsync(message);
await smtp.DisconnectAsync(true);
```

### Example: Node.js (nodemailer)

> A runnable sample is available in [`src/Mail2Go.Clients/NodejsSmtpSample/`](../src/Mail2Go.Clients/NodejsSmtpSample/).
> Run it with: `npm install && node send_mail.js`

```js
const nodemailer = require('nodemailer');

const transporter = nodemailer.createTransport({
  host: 'localhost',
  port: 2525,
  secure: false,
  auth: {
    user: 'user@mail2go.local',
    pass: 'pass123'
  }
});

await transporter.sendMail({
  from: 'user@mail2go.local',
  to: 'admin@mail2go.local',
  subject: 'Hello from Node.js',
  text: 'Test message from nodemailer.'
});
```

### Example: Python (smtplib)

```python
import smtplib
from email.mime.text import MIMEText

msg = MIMEText("Test message from Python")
msg["Subject"] = "Hello from Python"
msg["From"] = "user@mail2go.local"
msg["To"] = "admin@mail2go.local"

with smtplib.SMTP("localhost", 2525) as server:
    server.login("user@mail2go.local", "pass123")
    server.sendmail("user@mail2go.local", ["admin@mail2go.local"], msg.as_string())
```



## 8. Verifying Everything Works

### Check the Web UI

1. Open **http://localhost:5050**.
2. Log in as `admin` / `pass123`.
3. The Dashboard should show domain, mailbox, and message counts.

### Test SMTP via the Dashboard

1. Log in as Admin.
2. Go to **Dashboard**.
3. Click **Test Connection** on the SMTP card.
4. You should see a green "SMTP server is reachable" confirmation.

### Send a Test Email

Send an email from your app (or use the code examples above). Then:

1. Open the Mail2Go web UI.
2. Go to **Messages**.
3. The email should appear in the message list within seconds.

### Run the Test Suite

```bash
# Unit tests (fast, no external dependencies)
dotnet test tests/Mail2Go.UnitTests/

# Integration tests (starts a full app instance)
dotnet test tests/Mail2Go.IntegrationTests/
```

Expected output:

```
Passed! - Failed: 0, Passed: 9, Skipped: 0
```



## 9. Next Steps

| Task | Resource |
|---|---|
| Understand the architecture | [Technical Reference](TECHNICAL.md) |
| Learn the UI features | [User Manual](USER_MANUAL.md) |
| Add test domains and mailboxes | Admin → Domains / Mailboxes |
| Connect your app to Mail2Go | [Section 7 — SMTP Client](#7-connecting-an-smtp-client) |
| Change admin password | Admin profile (top-right menu) |
