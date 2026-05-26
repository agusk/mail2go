// Mail2Go — Sample SMTP Client using Node.js (nodemailer)
//
// Usage:
//   node send_mail.js
//   node send_mail.js <host> <port> <from> <to> <user> <pass>
//
// Defaults:
//   host  = localhost
//   port  = 2525
//   from  = user@mail2go.local
//   to    = admin@mail2go.local
//   user  = user@mail2go.local
//   pass  = pass123
//
// Install dependency first:
//   npm install

import nodemailer from 'nodemailer';

const [, , host = 'localhost', portStr = '2525', from = 'user@mail2go.local',
  to = 'admin@mail2go.local', user = 'user@mail2go.local', pass = 'pass123'] = process.argv;

const port = parseInt(portStr, 10);
const timestamp = new Date().toISOString();

console.log('Mail2Go — Node.js nodemailer Sample');
console.log(`  Server : ${host}:${port}`);
console.log(`  From   : ${from}`);
console.log(`  To     : ${to}`);
console.log();

const transporter = nodemailer.createTransport({
  host,
  port,
  secure: false,         // no TLS
  auth: { user, pass },
  tls: { rejectUnauthorized: false }
});

const textBody = `Hello from the Mail2Go Node.js sample!

This message was sent using nodemailer.

Sent at : ${timestamp}
From    : ${from}
To      : ${to}
Server  : ${host}:${port}

If you can read this, your Mail2Go SMTP server is working correctly.`;

const htmlBody = `<!DOCTYPE html>
<html>
<head><meta charset="utf-8" /></head>
<body style="font-family:sans-serif;max-width:600px;margin:2rem auto;">
  <h2 style="color:#4f8ef7;">Mail2Go Node.js Sample</h2>
  <p>This message was sent using <strong>nodemailer</strong>.</p>
  <table style="border-collapse:collapse;width:100%;">
    <tr><td style="padding:4px 8px;color:#888;">Sent at</td><td style="padding:4px 8px;">${timestamp}</td></tr>
    <tr><td style="padding:4px 8px;color:#888;">From</td><td style="padding:4px 8px;">${from}</td></tr>
    <tr><td style="padding:4px 8px;color:#888;">To</td><td style="padding:4px 8px;">${to}</td></tr>
    <tr><td style="padding:4px 8px;color:#888;">Server</td><td style="padding:4px 8px;">${host}:${port}</td></tr>
  </table>
  <p style="margin-top:1.5rem;color:#4caf50;font-weight:bold;">
    &#10003; Your Mail2Go SMTP server is working correctly.
  </p>
</body>
</html>`;

try {
  const info = await transporter.sendMail({
    from: `"Mail2Go Test" <${from}>`,
    to,
    subject: `[Node.js] Test email — ${timestamp}`,
    text: textBody,
    html: htmlBody
  });

  console.log('✓ Message sent successfully.');
  console.log(`  Message ID : ${info.messageId}`);
  console.log('  Check your inbox at http://localhost:5050');
} catch (err) {
  console.error(`✗ Error: ${err.message}`);
  process.exit(1);
}
