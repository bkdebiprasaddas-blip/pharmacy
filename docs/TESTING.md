# Test plan and execution record

## Execution status

| Layer | Command / procedure | Status in this workspace |
|---|---|---|
| Source contracts | `python -m unittest discover -s tests -v` | **10 passed** |
| Web Forms/VB precompilation | `scripts\Test-Windows.ps1` | Not run — Windows required |
| Access business integration | `scripts\Test-Business.ps1` | Not run — Windows/ACE required |
| Browser/admin/security workflows | Matrix below | Not run |

The Python suite checks config XML, required files, page/master/event wiring, validator targets, indexes/foreign keys, literal SQL parameter counts, source security/transaction contracts, local links, and credential/database exclusion. These are **structural checks**, not proof of runtime correctness, race safety, or an ASP.NET compile.

The Windows business script creates and deletes its own temporary database. Never repoint tests at a database containing real customer information. It checks hashing, validation, registration uniqueness, search, cart merging/updates, stock revalidation, transaction rollback, checkout, replay safety, ownership, historical snapshots, and concurrent purchase of the last unit. Full admin integration and browser authorization still require the matrix below.

## Preparation on Windows

1. Install prerequisites and create the database as described in README.
2. Run `Test-Windows.ps1`; resolve all markup/compiler errors.
3. Run `Test-Business.ps1` from a PowerShell process matching ACE bitness. All assertions must pass.
4. Launch the site over HTTPS with a trusted development certificate.
5. Use a fresh demonstration database and register two different customers, A and B.
6. Use separate browser profiles for A, B, and the administrator.
7. Record actual results, screenshots, provider/OS versions, and execution date. Do not mark rows passed without executing them.

## Authentication and authorization

| Test | Expected result |
|---|---|
| Valid registration with all fields | Account saved as Customer; redirect to login |
| Same email with changed case/whitespace | Duplicate rejected; one Users row |
| Two simultaneous registrations, same email | One succeeds; unique constraint prevents two accounts |
| Malformed email/phone, blank address, mismatch or short password | Friendly validation error; no account |
| Inspect database password field | Salted `pbkdf2-sha256$600000$...`, not plaintext |
| Valid customer/admin credentials | Customer home / admin dashboard |
| Wrong password or inactive account | Generic login failure |
| Repeated failed requests from new browser sessions | Account/IP throttle eventually rejects attempts |
| Unauthenticated visit to Cart/Checkout/Orders | Login required |
| Customer directly visits every Admin URL or sends a postback | Access denied; no database mutation |
| Expired Forms ticket or missing session with valid ticket | Login required; session alone cannot authorize |
| Admin deactivates an already logged-in customer | Next request signs that customer out |
| Admin's role changed directly in test database | Next admin request denied |
| GET Logout.aspx | Confirmation only; session still active |
| Confirm logout POST, then revisit protected pages | Authentication/session cleared; login required |
| Remove/alter `__csrf` or replay another session's form | Request rejected; no mutation |
| Modify ViewState or forged action argument | ASP.NET validation rejects the request |
| Inspect cookies and response headers | Secure, HttpOnly, SameSite=Lax; frame/content-type protections |
| Customer name/product description contains HTML-like text | Input rejected by request validation or displayed encoded; never executed |

## Catalog and cart

| Test | Expected result |
|---|---|
| Product-name and brand searches | Only matching products |
| Search apostrophes, `%`, `_`, SQL-like text | Literal substring behavior; no injected SQL |
| Category filter with search | Both filters applied |
| Unknown category | Safe default/no unauthorized data |
| Empty catalog/no matches | Clear empty-state text |
| Missing, negative, nonnumeric, oversized product ID | Safe invalid-product message |
| Inactive product/category or yesterday's expiry | Hidden from catalog and not purchasable |
| Expiry today | Purchasable through today if active/in stock |
| Add same product twice | One cart row; combined quantity |
| Two concurrent add requests for same customer/product | Serialized updates or safe retry; no duplicate row |
| Add or update quantity 0, negative, noninteger, beyond stock | Rejected server-side |
| Edit another customer's CartID in a request | No access or mutation |
| Quantity update and removal | Correct line totals and grand total |
| Empty and clear cart | No checkout link; saved cart cleared |
| Product deactivated after being added | Checkout blocked; item can still be removed |
| Attempt remote/path-traversal image path in admin editor | Rejected; executable uploads not offered |

## Checkout and orders

| Test | Expected result |
|---|---|
| Empty cart direct checkout | No order created |
| Valid cart/address/city/COD | One Pending order, correct items/total/stock, cart cleared |
| Tamper browser price/total fields | Server uses database prices, not browser totals |
| Submit unsupported Online Payment value | Rejected; no fictitious payment recorded |
| Lower stock from admin after cart was loaded | Checkout revalidates; fails safely if insufficient |
| Two customers submit for final unit | Only one succeeds; stock never negative; loser retains cart |
| Replay identical successful checkout form | Existing owned order returned; no duplicate decrement/order |
| Two checkout tabs with different tokens | One consumes cart; other gets empty-cart error |
| Force a late failure in a disposable database transaction | No partial order/items/stock/cart mutation persists |
| B requests A's order ID | No header, customer information, address, or items disclosed |
| Edit catalog price/name/unit after ordering | Historical details preserve purchased values |
| Visit order confirmation/history | ID/date/address/payment/status/total correct |

For a **late-checkout rollback test**, use only an isolated database: temporarily configure an OrderItems column validation rule that rejects one test item's insert, then submit a two-item cart. Verify Users.CartVersion, Orders, OrderItems, Products.StockQuantity/Version, and Cart match the pre-test snapshot. Restore the rule before other tests. This proves rollback after work has already begun, not merely an early input check.

## Administrator and reports

| Test | Expected result |
|---|---|
| Add/edit/deactivate categories | Validated changes; inactive categories hidden |
| Duplicate category name | Friendly error |
| Delete empty vs populated category | Empty deleted; populated refused |
| Add product with active category and valid values | Product saved and visible |
| Negative price/stock, >2-decimal price, invalid expiry | Rejected server-side |
| Activate expired product / select inactive category | Rejected |
| Edit stock while a second session completes checkout | Stale Version prevents editor from overwriting stock |
| Delete unused vs cart/order-referenced product | Unused deleted; referenced refused, deactivate suggested |
| Customer search/status actions | Only customers listed; no password data or role promotion controls |
| Pending → Confirmed → Packed → Out for Delivery → Delivered | Each transition succeeds in order |
| Pending → Delivered, backwards transition, unknown status | Rejected |
| Cancel from Pending/Confirmed/Packed | Stock restored and status committed together |
| Repeat or concurrently submit cancellation | Stock restored exactly once |
| Cancel Delivered/Out for Delivery | Rejected by defined policy |
| Dashboard counts | Match database, pending count, recent ten orders |
| Delivered revenue/top-selling | Exclude cancelled/pending orders |
| Inclusive date-range sales report | Includes both boundary dates, excludes outside dates |
| Invalid or >366-day report range | Validation message |
| Configurable low-stock threshold | Products at or below threshold shown |
| Browser print/PDF | Navigation/actions hidden, reports readable |

## Operational and UI checks

- Test desktop and narrow mobile viewports. Navigation wraps/scrolls; tables scroll; form labels and keyboard focus remain usable.
- Disconnect the Bootstrap CDN: local styling remains usable and no shopping behavior depends on JavaScript.
- Rename the **test** database temporarily, remove its write permission, or create a lock conflict: no raw provider exception/SQL/path should reach the browser.
- Configure a server-side Trace listener and verify that detailed errors are captured only there. Never log passwords or full payment details.
- Test HTTP versus HTTPS: secure login is expected to work only on HTTPS.
- Check that IIS blocks direct requests to `/App_Data/GroceryDB.accdb` and never serves code-behind/config files.
- Verify safe backup/restore and close exclusive Microsoft Access sessions before serving the application.

## Suggested demonstration sequence

Create customer → search groceries → add twice → adjust cart → COD checkout → view confirmation/history → admin confirms/packs/dispatches/delivers → inspect delivered revenue → create a second order → cancel before dispatch → verify stock restored → deactivate a product → confirm it disappears from shopping.
