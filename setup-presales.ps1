$ErrorActionPreference = "Stop"
# Generate a strong password ON THIS PC only. It is never printed and never leaves this PC.
$bytes = New-Object byte[] 24
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$pwd = ([Convert]::ToBase64String($bytes) -replace '[+/=]', 'x').Substring(0, 24)
if ($pwd -notmatch '[A-Z]') { $pwd = 'A' + $pwd.Substring(1) }
if ($pwd -notmatch '[0-9]') { $pwd = $pwd.Substring(0, 23) + '7' }
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF DB_ID('PresalesDemo') IS NULL CREATE DATABASE [PresalesDemo];"
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'presales_app') CREATE LOGIN [presales_app] WITH PASSWORD = '$pwd', CHECK_POLICY = OFF; ELSE ALTER LOGIN [presales_app] WITH PASSWORD = '$pwd', CHECK_POLICY = OFF;"
sqlcmd -S "localhost\SQLEXPRESS" -E -d PresalesDemo -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'presales_app') CREATE USER [presales_app] FOR LOGIN [presales_app]; ALTER ROLE [db_owner] ADD MEMBER [presales_app];"
$cs = "Server=localhost\SQLEXPRESS;Database=PresalesDemo;User Id=presales_app;Password=$pwd;TrustServerCertificate=True"
Set-Location "D:\Source\PresalesDemo"
.\deploy-to-iis.ps1 -ConnectionString $cs
Write-Host "DEPLOY_DONE: site PresalesDemo on port 99, database PresalesDemo, SQL login presales_app created. Password is stored only in D:\App\PresalesDemo\appsettings.json on this PC and is not printed here."
