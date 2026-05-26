#!/usr/bin/env python3
"""
Mail2Go — Sample SMTP Client using Python smtplib (no dependencies)

Usage:
    python send_mail.py
    python send_mail.py <host> <port> <from> <to> <user> <pass>

Defaults:
    host  = localhost
    port  = 2525
    from  = user@mail2go.local
    to    = admin@mail2go.local
    user  = user@mail2go.local
    pass  = pass123
"""

import smtplib
import sys
from datetime import datetime, timezone
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

args = sys.argv[1:]
host    = args[0] if len(args) > 0 else "localhost"
port    = int(args[1]) if len(args) > 1 else 2525
sender  = args[2] if len(args) > 2 else "user@mail2go.local"
to      = args[3] if len(args) > 3 else "admin@mail2go.local"
user    = args[4] if len(args) > 4 else "user@mail2go.local"
password = args[5] if len(args) > 5 else "pass123"

timestamp = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S UTC")

print("Mail2Go — Python smtplib Sample")
print(f"  Server : {host}:{port}")
print(f"  From   : {sender}")
print(f"  To     : {to}")
print()

text_body = f"""\
Hello from the Mail2Go Python sample!

This message was sent using Python's built-in smtplib.

Sent at : {timestamp}
From    : {sender}
To      : {to}
Server  : {host}:{port}

If you can read this, your Mail2Go SMTP server is working correctly.
"""

html_body = f"""\
<!DOCTYPE html>
<html>
<head><meta charset="utf-8" /></head>
<body style="font-family:sans-serif;max-width:600px;margin:2rem auto;">
  <h2 style="color:#4f8ef7;">Mail2Go Python Sample</h2>
  <p>This message was sent using Python's built-in <strong>smtplib</strong>.</p>
  <table style="border-collapse:collapse;width:100%;">
    <tr><td style="padding:4px 8px;color:#888;">Sent at</td><td style="padding:4px 8px;">{timestamp}</td></tr>
    <tr><td style="padding:4px 8px;color:#888;">From</td><td style="padding:4px 8px;">{sender}</td></tr>
    <tr><td style="padding:4px 8px;color:#888;">To</td><td style="padding:4px 8px;">{to}</td></tr>
    <tr><td style="padding:4px 8px;color:#888;">Server</td><td style="padding:4px 8px;">{host}:{port}</td></tr>
  </table>
  <p style="margin-top:1.5rem;color:#4caf50;font-weight:bold;">
    &#10003; Your Mail2Go SMTP server is working correctly.
  </p>
</body>
</html>
"""

msg = MIMEMultipart("alternative")
msg["Subject"] = f"[Python] Test email — {timestamp}"
msg["From"]    = sender
msg["To"]      = to
msg.attach(MIMEText(text_body, "plain", "utf-8"))
msg.attach(MIMEText(html_body, "html",  "utf-8"))

try:
    with smtplib.SMTP(host, port) as server:
        server.login(user, password)
        server.sendmail(sender, [to], msg.as_string())
    print("✓ Message sent successfully.")
    print("  Check your inbox at http://localhost:5050")
except smtplib.SMTPException as e:
    print(f"✗ SMTP error: {e}", file=sys.stderr)
    sys.exit(1)
except Exception as e:
    print(f"✗ Error: {e}", file=sys.stderr)
    sys.exit(1)
