# OneNow Presales Demo — Server version

Single ASP.NET Core 8 web app: static frontend in `wwwroot/` + JSON API under `/api/`.
Data is persisted in SQL Server, not browser storage. Internal use only, HTTP local first
(Tailscale / LAN). No authentication in v1 — do not expose it publicly.

## Logo (official OneNow, not a redraw, not ezHR)

- `wwwroot/images/onenow-logo.png` — the official OneNow wordmark, copied from the file
  supplied by the parent (`~/workspace/onenow-wordmark.png`, downloaded from
  `https://onenow.ezhrsolution.com/brand/onenow-wordmark.png`, HTTP 200, PNG 1200x308, RGBA).
  It shows “One” in dark navy + “Now” in bright blue, with a blue puzzle-piece icon.
- It is used in the site header, on the industry-selection screen (same global header),
  in the print header, and is referenced in text exports.
- If the official logo changes, overwrite that exact file with the new official file.
  If the file is missing, the header falls back to the text wordmark “OneNow” in
  navy/royal-blue — that fallback is NOT the official logo. See
  `wwwroot/images/README-LOGO.md`.

## Build and publish (on the PC, which has SDK 8.0.427 / runtime 8.0.33)

```powershell
cd D:\path\to\presales-demo-server
dotnet build
dotnet publish -c Release -o D:\App\PresalesDemo
```

This VM has no dotnet SDK, so `dotnet build` was NOT verified here. It must be run
on the PC. Package versions are pinned to EF Core 8.0.8 (same as TaskApi) to avoid
the 10.x mismatch seen before.

## Configuration (never hardcode a real password)

Edit `appsettings.json` on the PC (or pass `-ConnectionString` to the deploy script):

```json
{ "ConnectionStrings": { "PresalesDemo": "Server=localhost\\SQLEXPRESS;Database=PresalesDemo;User Id=presales_app;Password=__FILL_ON_PC__;TrustServerCertificate=True" } }
```

Rules taken from TaskApi: SQL authentication, and do NOT add `Trusted_Connection=True`,
because it would override User Id/Password. The shipped value is a placeholder only.
Create a SQL login for the app, or use a login the parent already owns, and fill it in
on the PC. The app calls `EnsureCreated()` on startup and seeds only if no profiles exist.
`sql/CreateDatabase.sql` is the idempotent table script as an audit alternative.

## Deploy to IIS (on the PC, as Administrator)

```powershell
.\deploy-to-iis.ps1 -ConnectionString "Server=localhost\SQLEXPRESS;Database=PresalesDemo;User Id=presales_app;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

The script publishes to `D:\App\PresalesDemo`, creates the `PresalesDemo` database if
missing (via sqlcmd if available), and creates/updates ONE IIS site `PresalesDemo` on
port 99 with physical path `D:\App\PresalesDemo` and an app pool with No Managed Code.
It is idempotent and NEVER deletes `D:\App\thuoc-vui` or any other folder, and it does
not delete the publish folder before publishing. Then open `http://localhost:99/`
(or the Tailscale/LAN hostname, same port).

## API endpoints

- `GET /api/health`
- `GET /api/industries` — static industry content (personas, audience, objections, presenter stages, closing questions)
- `GET /api/profiles` · `GET /api/profiles/{id}` (full profile with discovery, priorities, scenarios, Fit-Gap, scorecard, actions, follow-ups, checklist)
- `POST /api/profiles` (body: name, industry, locations, employeeCount; copies the industry default template)
- `POST /api/profiles/{id}/duplicate` · `PUT /api/profiles/{id}` · `DELETE /api/profiles/{id}` (default profiles cannot be deleted)
- `PUT /api/profiles/{id}/discovery/{itemId}` · `PUT /api/profiles/{id}/fitgap/{itemId}` · `POST /api/profiles/{id}/fitgap` · `DELETE /api/profiles/{id}/fitgap/{itemId}`
- `PUT /api/profiles/{id}/scorecard` (limits 25/25/20/15/15) · `POST /api/profiles/{id}/actions` · `PUT /api/profiles/{id}/checklist/{itemId}`
- `GET /api/profiles/{id}/exports/{discovery|matrix|action|summary}` — downloadable .txt

Fit-Gap has exactly 4 states: Đáp ứng sẵn, Đáp ứng qua cấu hình, Cần phát triển thêm,
Chưa đáp ứng. It is shown as a single coloured line (green/blue/yellow/red) and each
dropdown option is coloured per its own status. Changing a Fit-Gap row regenerates
follow-ups from rows in “Cần phát triển thêm” or “Chưa đáp ứng”.

## Seed profiles (exactly 4 defaults, demo sample data only)

On first run, if no profiles exist, the app seeds one default per industry:

1. `[MẪU] Logistic - Hồ sơ demo mặc định` — 3 locations, 850 employees
2. `[MẪU] FnB - Hồ sơ demo mặc định` — 12 locations, 620 employees
3. `[MẪU] Hotel - Hồ sơ demo mặc định` — 1 location, 340 employees
4. `[MẪU] Sản xuất - Hồ sơ demo mặc định` — 2 locations, 1,400 employees

Each is pre-filled with that industry's specific Discovery answers (8 Playbook groups),
3 priorities, 3 Pain–Solution–Outcome–Value scenarios with OneNow screen flows
(Dashboard, ESS, Approvals, Attendance, Leave balance, Payroll 23 components, Contract,
Recruitment, Reports), Fit-Gap rows, a Scorecard, actions and checklist (18 + 3 OneNow
mandatory items). No real customer data, no ACE Company test tickets, no prices
(pricing/timeline/contact are placeholders, per the thuyết minh).

## Presenter, estimator, health check

- Presenter: 00–05, 05–15, 15–45 (3×10 min, per-scenario timer, checkpoint question after
  each scenario), 45–55, 55–60, plus the 3 closing questions.
- Estimator: uses only customer-entered Discovery numbers, shows a stated-assumption
  range, and never claims a reduction from 4 days to 1 day.
- Demo-health check (5 conditions, self-assessed in the UI): clean demo tenant,
  no stuck/overdue Dashboard tickets, demo accounts for each role work, fallback plan
  for slow network/system, sample data matches the industry. The app does NOT connect
  to a live OneNow tenant to verify these.

## What was NOT verified here

- `dotnet build` / `dotnet publish` / IIS deployment: not run in this VM (no SDK).
  Must be verified on the PC after filling the connection string.
- No live OneNow login was used for this build. OneNow flows listed above are the
  flows already verified in earlier OneNow testing (leave balance on approve/reject/
  cancel, attendance-explanation Applied, Dashboard overdue flag); nothing beyond
  those flows is claimed.
- Native `<select>` option colours depend on the browser: Chrome/Edge/Firefox on
  Windows honour option colours; some mobile browsers use the system dropdown UI
  and may not show them.
