param(
    [string]$InstallDir = "$env:LOCALAPPDATA\\SpeakEasy",
    [string]$TaskName = "SpeakEasy",
    [switch]$KeepFiles
)

$ErrorActionPreference = "Stop"

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($task) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
}

if (-not $KeepFiles -and (Test-Path $InstallDir)) {
    Remove-Item -Path $InstallDir -Recurse -Force
}

Write-Output "Removed scheduled task '$TaskName'"
if (-not $KeepFiles) {
    Write-Output "Removed install folder $InstallDir"
}
