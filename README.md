# OnlineGroceryStore · FreshBasket

An original grocery-store implementation of the supplied project plan, built with **ASP.NET Web Forms, VB.NET, .NET Framework 4.8, Microsoft Access, and ADO.NET/OleDb**. The repository name is historical; the application uses grocery terminology throughout.

## Implementation status

Customer and administrator source pages, database schema/setup, shared services, responsive styling, and test scripts are included. **The application has not been compiled or run with Windows/ACE in this Linux workspace.** The 10 portable source-contract checks pass; they are not runtime or security certification.

The binary `GroceryDB.accdb` is **not included**. Create it with the Windows initialization script below. Do not rename an empty text file to `.accdb`.

### Included

- Registration, PBKDF2-SHA256 password hashing, login throttling, Forms Authentication plus sessions, logout confirmation.
- Product catalog, name/brand search, category filters, product details, active/expiry checks.
- Persistent cart with unique customer/product rows, quantity updates, removal, clearing, and totals in INR.
- Cash on Delivery checkout: transaction, stock revalidation/conditional decrement, order-item snapshots, cart clearing, and idempotent replay protection.
- Customer-owned order history, order details, and status tracking.
- Admin dashboard, customer activation/deactivation, category/product management, optimistic stock edits, safe deletion, order transitions, cancellation/restocking, and reports.
- Responsive customer/admin master pages, Bootstrap styling with local CSS fallback, local grocery illustration, safe error messages, CSRF protection, HTTPS-only cookies.

### Intentionally not included

A payment gateway, actual payment collection, real delivery tracking, email/SMS, password recovery, arbitrary file uploads, or production-scale infrastructure. **Cash on Delivery is the only enabled payment method.** The contact page explicitly identifies the site as a demonstration.

## Run on Windows

### 1. Install prerequisites

- Windows 10/11 or Windows Server.
- Visual Studio 2022 with **ASP.NET and web development**, .NET Framework 4.8 targeting/developer tools, and IIS Express.
- Microsoft Access Database Engine / Access Runtime providing `Microsoft.ACE.OLEDB.12.0` (or update the provider consistently to an installed compatible ACE version).
- Windows PowerShell **5.1**, not PowerShell 7, for the initialization/test scripts.

**ACE and the process using it must have matching bitness.** For x64 ACE, use x64 PowerShell and x64 IIS Express. For x86 ACE, use 32-bit PowerShell at `%WINDIR%\SysWOW64\WindowsPowerShell\v1.0\powershell.exe` and x86 IIS Express. Do not force-install conflicting Office runtimes.

### 2. Create the database

From the repository root in matching-bitness Windows PowerShell:

```powershell
.\scripts\Initialize-Database.ps1
```

The script prompts for an administrator email and a secure password of 12–128 characters, creates `OnlineGroceryStore\App_Data\GroceryDB.accdb`, applies tables/indexes/foreign keys/validation rules, and seeds ten categories and twelve demonstration products. There are **no default admin credentials**. Never commit the generated database.

For categories and administrator only:

```powershell
.\scripts\Initialize-Database.ps1 -SkipSampleProducts
```

The script refuses to overwrite an existing file. If it fails, inspect the error and delete **only the partially generated database** before trying again. Schema creation is a one-time operation, not an automatic production migration.

### 3. Open the ASP.NET Web Site

In Visual Studio: **File → Open → Web Site → File System**, select the `OnlineGroceryStore` folder (not the repository root). This is the Web Forms **Web Site** model: `App_Code` and `CodeFile` are compiled by ASP.NET. A `.vbproj`, generated designer files, or a pre-existing solution are not required. Visual Studio may create a local solution for you.

Set the target framework to .NET Framework 4.8 if prompted. Configure IIS Express with **SSL Enabled = True**, trust its local development certificate, and browse its **HTTPS** URL. Cookies require HTTPS by default; plain HTTP login will not persist. Set `Default.aspx` as the start page.

If using full IIS, install ASP.NET 4.x, use a .NET v4 Integrated application pool of matching bitness, configure HTTPS, and grant the pool identity Modify permission **only on `App_Data`** so ACE can create its lock file. Do not expose the database directory through static hosting.

### 4. Compile and test

```powershell
# Precompile all Web Forms markup and VB source.
.\scripts\Test-Windows.ps1

# Create a temporary Access database, compile and run business tests, then clean it up.
.\scripts\Test-Business.ps1
```

Run these on Windows and fix any provider/compiler/runtime errors before considering the application verified. Then execute the browser/security/admin matrix in [docs/TESTING.md](docs/TESTING.md).

Portable source checks (Python 3.9+):

```sh
python -m unittest discover -s tests -v
```

## Structure

```text
OnlineGroceryStore/
  App_Code/          Database, security, authorization, cart/order/admin services
  App_Data/          Generated GroceryDB.accdb (ignored by Git)
  Admin/             Dashboard and management/report pages + AdminMaster.master
  Content/           Responsive CSS and local SVG illustration
  Images/Products/   Optional local JPG/PNG/WebP product images
  Site.Master        Shared customer layout
  *.aspx[.vb]        Customer pages and VB code-behind
  Web.config         ACE connection, HTTPS cookies, Forms Authentication, sessions
  Global.asax        Server-side error logging hook
scripts/
  Initialize-Database.ps1
  Test-Windows.ps1
  Test-Business.ps1
database/schema.sql
tests/               Portable checks + Windows/ACE business integration tests
docs/                Analysis, architecture, decisions, and acceptance tests
```

## Important design decisions

- `PasswordHash` replaces the plan's ambiguous `Password` field. Passwords use a random 16-byte salt and 600,000 PBKDF2-HMAC-SHA256 iterations.
- `CartVersion` serializes a customer's cart writes and checkout. `Products.Version` detects stale admin edits. `CheckoutToken` prevents repeated submission from creating a second order.
- Orders preserve purchased product **name, unit, and price**. Referenced products/categories cannot be hard-deleted.
- Cancellation is allowed from Pending, Confirmed, or Packed only. Cancelled/Delivered are terminal; cancellation restores stock in the same transaction as the guarded status change.
- Revenue means **delivered-order value**, not all placed orders and not payment-gateway settlements.
- Expiry is inclusive of the stored date. Expired/inactive products or inactive categories cannot be purchased.
- Online payments remain a future integration, not a misleading dropdown option.
- Admin product images are manually copied into `Images/Products`; the editor accepts a constrained local path, not remote URLs or executable uploads.

See [docs/ANALYSIS.md](docs/ANALYSIS.md) for the requirement analysis, schema changes, limitations, and ER diagram.

## Troubleshooting

| Symptom | Check |
|---|---|
| ACE provider is not registered | Provider installation and PowerShell/IIS process bitness |
| Database missing or invalid format | Run initializer successfully; verify connection string and genuine `.accdb` file |
| Database is read-only / cannot lock | App pool identity needs Modify on `App_Data`; close exclusive Access sessions |
| Login redirects back to login | Use HTTPS; allow cookies; inspect Forms/session cookie configuration |
| Form expired after logout/session timeout | Reload the form; CSRF tokens intentionally expire with the session |
| “Product or stock changed” | Reload the edit page before saving; do not overwrite a concurrent checkout |
| Frequent lock conflicts | Retry after refresh; keep database local and transactions short; Access is not a high-concurrency server |
| Styling unavailable offline | Local CSS keeps the pages usable; Bootstrap's CDN enhancement needs internet |

Before any real deployment: configure durable server logs (the app uses `Trace.TraceError`), backups, least-privilege ACLs, recovery policies, delivery/contact information, durable rate limiting, and a production database/payment strategy. Never turn detailed public errors or debug mode on in production.

## UI demonstration video

A **2:18 captioned 1080p MP4** walkthrough was generated at `deliverables/FreshBasket_UI_Feature_Demo.mp4`, with a matching SRT file. It shows source-based previews and illustrative sample data, **not verified live Windows/Access operations**. See [scripts/video/README.md](scripts/video/README.md) for scope, verification, and regeneration instructions. Rendered media and temporary video tooling are intentionally excluded from Git.
