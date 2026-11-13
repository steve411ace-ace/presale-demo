# Deploy PresalesDemo to IIS on the PC. Run as Administrator from this project folder.
# Idempotent: publishes over D:\App\PresalesDemo only, creates the DB if missing,
# creates/updates ONE IIS site (PresalesDemo, port 99). It NEVER deletes D:\App\thuoc-vui
# or any other folder. It does NOT delete the publish folder before publishing.
param(
  [string]$SiteName = "PresalesDemo",
  [int]$Port = 99,
  [string]$PublishPath = "D:\App\PresalesDemo",
  [string]$SqlInstance = "localhost\SQLEXPRESS",
  [string]$Database = "PresalesDemo",
  [string]$ConnectionString = ""
)
$ErrorActionPreference = "Stop"
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw "Run this script as Administrator." }
if (-not (Test-Path "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")) { throw "ASP.NET Core Hosting Bundle not found. Install it first, then rerun." }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "dotnet SDK not found on PATH." }
Import-Module WebAdministration -ErrorAction Stop
# Stop only this site/app pool, never any other site.
# Use Test-Path (not Get-WebAppPoolState/Get-Website) for existence checks:
# those cmdlets throw ItemNotFound for a missing site/pool even with SilentlyContinue
# when $ErrorActionPreference is Stop.
if (Test-Path "IIS:\Sites\$SiteName") { Stop-Website -Name $SiteName -ErrorAction SilentlyContinue }
if (Test-Path "IIS:\AppPools\$SiteName") { Stop-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue }
# Publish (overwrites this site's folder only).
Write-Host "dotnet publish `"$PSScriptRoot\PresalesDemo.csproj`" -c Release -o `"$PublishPath`""
dotnet publish "$PSScriptRoot\PresalesDemo.csproj" -c Release -o $PublishPath
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
if ($ConnectionString) {
  $cfg = Join-Path $PublishPath "appsettings.json"
  $json = Get-Content $cfg -Raw | ConvertFrom-Json
  $json.ConnectionStrings.PresalesDemo = $ConnectionString
  $json | ConvertTo-Json -Depth 5 | Set-Content $cfg -Encoding UTF8
  Write-Host "Connection string written to published appsettings.json."
} else {
  Write-Warning "No -ConnectionString supplied. Fill ConnectionStrings:PresalesDemo in $PublishPath\appsettings.json (SQL auth, no Trusted_Connection) before the site can use SQL Server."
}
# Create database if missing (sqlcmd if available; otherwise the app's EnsureCreated() creates it on first run if the login has rights).
if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
  sqlcmd -S $SqlInstance -Q "IF DB_ID('$Database') IS NULL CREATE DATABASE [$Database];"
  Write-Host "Database check done: $Database on $SqlInstance"
} else {
  Write-Warning "sqlcmd not found. The app will try EnsureCreated() on first run; check the DB manually if the API fails."
}
# App pool: No Managed Code.
if (-not (Test-Path "IIS:\AppPools\$SiteName")) { New-WebAppPool -Name $SiteName }
Set-ItemProperty "IIS:\AppPools\$SiteName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$SiteName" -Name processModel.identityType -Value ApplicationPoolIdentity
# Site: create or update, port 99, physical path D:\App\PresalesDemo.
if (Test-Path "IIS:\Sites\$SiteName") {
  Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PublishPath
  Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $SiteName
} else {
  New-Website -Name $SiteName -Port $Port -PhysicalPath $PublishPath -ApplicationPool $SiteName
}
Start-WebAppPool -Name $SiteName
Start-Website -Name $SiteName
Start-Sleep -Seconds 3
try {
  $r = Invoke-WebRequest -Uri "http://localhost:$Port/api/health" -UseBasicParsing -TimeoutSec 15
  Write-Host "OK: http://localhost:$Port/api/health returned $($r.StatusCode)"
  Write-Host "Open the demo at http://localhost:$Port/ (or via Tailscale/LAN hostname, same port, HTTP local only)."
} catch {
  Write-Warning "Site started but health check failed: $($_.Exception.Message). Check $PublishPath\appsettings.json and IIS logs."
}
