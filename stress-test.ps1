#Requires -Version 5.1
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'

$Port = 8080
$env:RUN_ID = (Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')
$ReportDir  = "stress-reports\$($env:RUN_ID)"

# --- kill host processes on the API port ---
Write-Host ">> Checking port $Port..."
$pids = netstat -ano |
    Select-String ":$Port\s" |
    ForEach-Object { ($_ -split '\s+')[-1] } |
    Sort-Object -Unique

if ($pids) {
    Write-Host "   Killing PIDs on port ${Port}: $($pids -join ', ')"
    $pids | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }
} else {
    Write-Host "   Port $Port is free"
}

# --- switch to the Linux engine context (Docker Desktop on Windows) ---
$ctx = docker context show 2>$null
if ($ctx -ne 'desktop-linux') {
    Write-Host ">> Switching Docker context to desktop-linux..."
    docker context use desktop-linux | Out-Null
}

# --- tear down any previous stack ---
Write-Host ">> Tearing down previous stack..."
docker compose down --remove-orphans

# --- build & start the full stack (mongo + API + stress-tester) ---
Write-Host ">> Building and starting stack (run: $($env:RUN_ID))..."
docker compose up --build --abort-on-container-exit --exit-code-from stress-tester

# --- show stress-tester output cleanly ---
Write-Host ""
Write-Host ">> Stress-tester results:"
docker compose logs stress-tester

# --- report location ---
Write-Host ""
Write-Host ">> Report saved -> $ReportDir\report.html"
Write-Host ">> Done. API is still up at http://localhost:$Port"
Write-Host "   Run 'docker compose down' to stop everything."

Write-Host ""
Write-Host ">> Done. API is still up at http://localhost:$Port"
Write-Host "   Run 'docker compose down' to stop everything."
