#Requires -RunAsAdministrator
<#
    SS Traders Stock Management — Installer
    Run as Administrator: Right-click Install.ps1 → Run with PowerShell
#>

$AppName    = "SS Traders Stock Management"
$Publisher  = "SS Traders"
$Version    = "1.0.0"
$InstallDir = "$env:ProgramFiles\SS Traders\StockManagement"
$SourceDir  = "$PSScriptRoot\..\publish\StockManagementApp"
$ExeName    = "StockManagementApp.exe"
$UninstKey  = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\SSTraders_StockMgmt"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   SS Traders Stock Management Installer  " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Confirm source exists
if (-not (Test-Path "$SourceDir\$ExeName")) {
    Write-Host "ERROR: Source files not found at: $SourceDir" -ForegroundColor Red
    Write-Host "Make sure you are running Install.ps1 from the 'setup' folder." -ForegroundColor Yellow
    pause; exit 1
}

# Stop running instance
$running = Get-Process -Name "StockManagementApp" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "Stopping running instance..." -ForegroundColor Yellow
    $running | Stop-Process -Force
    Start-Sleep -Seconds 1
}

# Copy files
Write-Host "Installing to: $InstallDir" -ForegroundColor Green
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
Copy-Item "$SourceDir\*" $InstallDir -Recurse -Force
Write-Host "Files copied." -ForegroundColor Green

# Desktop shortcut
$WshShell = New-Object -ComObject WScript.Shell
$DesktopShortcut = $WshShell.CreateShortcut("$env:PUBLIC\Desktop\$AppName.lnk")
$DesktopShortcut.TargetPath       = "$InstallDir\$ExeName"
$DesktopShortcut.WorkingDirectory = $InstallDir
$DesktopShortcut.Description      = $AppName
$DesktopShortcut.Save()
Write-Host "Desktop shortcut created." -ForegroundColor Green

# Start Menu shortcut
$StartMenuDir = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\SS Traders"
New-Item -ItemType Directory -Force -Path $StartMenuDir | Out-Null
$StartShortcut = $WshShell.CreateShortcut("$StartMenuDir\$AppName.lnk")
$StartShortcut.TargetPath       = "$InstallDir\$ExeName"
$StartShortcut.WorkingDirectory = $InstallDir
$StartShortcut.Description      = $AppName
$StartShortcut.Save()
Write-Host "Start Menu shortcut created." -ForegroundColor Green

# Uninstall registry entry
$UninstallScript = "$InstallDir\Uninstall.ps1"
@"
#Requires -RunAsAdministrator
`$InstallDir = "$InstallDir"
`$StartMenuDir = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\SS Traders"
`$DesktopLink  = "$env:PUBLIC\Desktop\$AppName.lnk"
`$UninstKey    = "$UninstKey"
Stop-Process -Name "StockManagementApp" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Remove-Item `$InstallDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item `$StartMenuDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item `$DesktopLink  -Force -ErrorAction SilentlyContinue
Remove-ItemProperty -Path `$UninstKey -Name * -ErrorAction SilentlyContinue
Remove-Item `$UninstKey -ErrorAction SilentlyContinue
Write-Host "SS Traders Stock Management has been uninstalled." -ForegroundColor Green
pause
"@ | Set-Content $UninstallScript -Encoding UTF8

New-Item -Path $UninstKey -Force | Out-Null
Set-ItemProperty -Path $UninstKey -Name "DisplayName"      -Value $AppName
Set-ItemProperty -Path $UninstKey -Name "DisplayVersion"   -Value $Version
Set-ItemProperty -Path $UninstKey -Name "Publisher"        -Value $Publisher
Set-ItemProperty -Path $UninstKey -Name "InstallLocation"  -Value $InstallDir
Set-ItemProperty -Path $UninstKey -Name "UninstallString"  -Value "powershell -ExecutionPolicy Bypass -File `"$UninstallScript`""
Set-ItemProperty -Path $UninstKey -Name "NoModify"         -Value 1 -Type DWord
Set-ItemProperty -Path $UninstKey -Name "NoRepair"         -Value 1 -Type DWord
Write-Host "Uninstall entry registered in Windows Settings." -ForegroundColor Green

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   Installation complete!" -ForegroundColor Cyan
Write-Host "   Shortcut added to Desktop & Start Menu" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$launch = Read-Host "Launch SS Traders Stock Management now? (Y/N)"
if ($launch -eq "Y" -or $launch -eq "y") {
    Start-Process "$InstallDir\$ExeName"
}
