#Requires -Version 5.1
Write-Host ">> Restarting Docker Desktop..."
Get-Process "*docker*" | Stop-Process -Force -ErrorAction SilentlyContinue
wsl --shutdown
Start-Sleep -Seconds 3
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
Write-Host ">> Waiting for engine..."
$timeout = 90
$elapsed = 0
while ($elapsed -lt $timeout) {
    Start-Sleep -Seconds 5
    $elapsed += 5
    $result = docker info 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host ">> Docker engine is up! ($elapsed s)"
        exit 0
    }
}
Write-Host ">> Timed out waiting for Docker engine." -ForegroundColor Red
exit 1
