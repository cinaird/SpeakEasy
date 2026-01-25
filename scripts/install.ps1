param(
    [string]$InstallDir = "$env:LOCALAPPDATA\\SpeakEasy",
    [string]$TaskName = "SpeakEasy",
    [switch]$SkipPublish,
    [string]$PublishDir
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
if (-not $PublishDir) {
    $PublishDir = Join-Path $repoRoot "publish"
}

if (-not $SkipPublish) {
    & (Join-Path $PSScriptRoot "publish.ps1") -OutputDir $PublishDir
}

if (-not (Test-Path $PublishDir)) {
    throw "Publish output not found: $PublishDir"
}

New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
Copy-Item -Path (Join-Path $PublishDir "*") -Destination $InstallDir -Recurse -Force

$exePath = Join-Path $InstallDir "SpeakEasy.exe"
if (-not (Test-Path $exePath)) {
    throw "SpeakEasy.exe not found in $InstallDir"
}

$userId = if ($env:USERDOMAIN) { "$env:USERDOMAIN\\$env:USERNAME" } else { $env:USERNAME }
$action = New-ScheduledTaskAction -Execute $exePath
$trigger = New-ScheduledTaskTrigger -AtLogOn
$principal = New-ScheduledTaskPrincipal -UserId $userId -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit 0

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings `
    -Description "SpeakEasy tray app" `
    -Force | Out-Null

Write-Output "Installed to $InstallDir"
Write-Output "Scheduled task '$TaskName' created for user $userId"
