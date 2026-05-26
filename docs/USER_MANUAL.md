# Mail2Go — User Manual

> This guide is for both **Admin** and **User** role accounts.



## Table of Contents

1. [Logging In](#1-logging-in)
2. [Admin: Dashboard](#2-admin-dashboard)
3. [Admin: Domain Management](#3-admin-domain-management)
4. [Admin: Mailbox Management](#4-admin-mailbox-management)
5. [Admin: Messages (Webmail)](#5-admin-messages-webmail)
6. [User: My Mailbox](#6-user-my-mailbox)
7. [Composing an Email](#7-composing-an-email)
8. [Reading a Message](#8-reading-a-message)
9. [Deleting Messages](#9-deleting-messages)
10. [Changing Theme (Dark / Light)](#10-changing-theme-dark--light)



## 1. Logging In

Open your browser and navigate to the Mail2Go web address (e.g. `http://localhost:5050`).

You will be redirected to the **Login** page.

Enter your credentials:

| Field | Description |
|---|---|
| **Username or Email** | Your Mail2Go username (e.g. `admin`) or full email (e.g. `admin@mail2go.local`) |
| **Password** | Your account password |

Click **Sign In**.

- **Admin** accounts are redirected to the Admin Dashboard.
- **User** accounts are redirected to their Mailbox.

> **Default credentials (change after first login):**
> - Admin → `admin` / `pass123`
> - Test user → `user` / `pass123`



## 2. Admin: Dashboard

The Dashboard gives a real-time overview of the system.

### Stats Cards

| Card | Shows |
|---|---|
| **Domains** | Number of registered test domains |
| **Mailboxes** | Number of active mailboxes |
| **Messages** | Total captured messages |
| **Users** | Total registered users |

### SMTP Status

The SMTP status card shows whether the built-in SMTP server is running and on which port.

Click **Test Connection** to verify the SMTP server is reachable. A green success message confirms it is accepting connections.

### Sidebar Navigation

The left sidebar provides quick access to all admin features:

| Menu Item | Purpose |
|---|---|
| Dashboard | System overview |
| Domains | Manage test email domains |
| Mailboxes | Manage user mailboxes |
| Messages | Browse all captured emails |



## 3. Admin: Domain Management

Go to **Domains** in the sidebar.

### View Domains

The domains list shows:

- Domain name (e.g. `mail2go.local`)
- Status (Active / Disabled)
- Number of mailboxes under the domain
- Created date

### Add a Domain

1. Click **Add Domain**.
2. Enter the domain name (e.g. `qa.internal`).
3. Click **Save**.

Domain names are normalized to lowercase automatically.

### Edit a Domain

Click **Edit** next to a domain to update its name or status.

### Enable / Disable a Domain

Disabling a domain prevents new mailboxes from being assigned to it. Existing mailboxes and messages are not affected.

### Delete a Domain

Domains can only be deleted when they have no mailboxes assigned.



## 4. Admin: Mailbox Management

Go to **Mailboxes** in the sidebar.

### View Mailboxes

The mailbox list shows:

- Email address (e.g. `alice@mail2go.local`)
- Owner username
- Associated domain
- Created date

### Create a Mailbox

1. Click **Create Mailbox**.
2. Fill in the form:

| Field | Description |
|---|---|
| **Username** | Login username for the new account |
| **Display Name** | Friendly name shown in the UI |
| **Email Local Part** | The part before `@` (e.g. `alice`) |
| **Domain** | Select a registered domain from the dropdown |
| **Password** | Minimum 6 characters |

3. Click **Create**.

The full email address is assembled automatically as `[local-part]@[domain]` (e.g. `alice@mail2go.local`).

> New accounts are created with the **User** role by default.

### Edit / Delete a Mailbox

Use the action buttons in the mailbox list to edit details or remove a mailbox.



## 5. Admin: Messages (Webmail)

Go to **Messages** in the sidebar.

This view shows **all captured messages** across all domains and mailboxes.

### Split-Pane Layout

The Messages page uses a two-panel layout:

- **Left panel** — message list with sender, subject, and timestamp.
- **Right panel** — message detail view.

Click any message in the left panel to load it in the right panel.

### Resize Panels

Drag the **vertical divider** between the two panels left or right to adjust their widths.

- Minimum left panel width: 180 px
- Maximum left panel width: 600 px

### Message List Columns

| Column | Description |
|---|---|
| **From** | Sender address |
| **Subject** | Email subject line |
| **Received** | Date and time captured |

### Actions

- **Compose** — Open the compose form to write a new test email.
- **Reply** (in detail panel) — Pre-fills the compose form with the original sender as recipient.
- **Delete** (in detail panel) — Permanently removes the message.



## 6. User: My Mailbox

Regular users see only their own mailbox after login.

The mailbox page shows messages where the user's email address appears as a recipient.

Click a message row to open the full message detail.



## 7. Composing an Email

Both Admin and User roles can compose emails.

### Opening the Compose Form

- From the Messages page: click **Compose**.
- From a message detail: click **Reply**.

### Compose Form Fields

| Field | Required | Description |
|---|:---:|---|
| **From** | ✅ | Sender address. Admins can pick any managed mailbox. Users are limited to their own address. |
| **To** | ✅ | Recipient email address(es), comma-separated |
| **Cc** | — | Carbon copy recipients |
| **Bcc** | — | Blind carbon copy recipients |
| **Subject** | ✅ | Email subject |
| **Body** | ✅ | Message text |

### Send

Click **Send**. The message is stored locally and appears in the recipient's mailbox if the address matches a registered mailbox.

> Mail2Go does **not** deliver emails externally. All composed messages stay within the system.



## 8. Reading a Message

Click any message to open it in the detail panel (or on a separate page for User role).

### Detail View Sections

| Section | Description |
|---|---|
| **From** | Sender address |
| **To / Cc / Bcc** | Recipient list |
| **Subject** | Email subject |
| **Date** | Received or sent timestamp |
| **Body** | Rendered HTML body, or plain text fallback |
| **Raw MIME** | Full raw MIME source (collapsed by default — click to expand) |



## 9. Deleting Messages

To delete a message:

1. Open the message in the detail panel.
2. Click **Delete**.
3. Confirm when prompted.

Deletion is **permanent** — there is no recycle bin.



## 10. Changing Theme (Dark / Light)

Mail2Go supports both dark and light themes.

Click the **sun / moon icon** in the top-right corner of the topbar to toggle between dark and light mode.

The selected theme is saved in your browser session.
