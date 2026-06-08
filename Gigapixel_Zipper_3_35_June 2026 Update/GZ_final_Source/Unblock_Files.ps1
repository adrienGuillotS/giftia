# Run this once after extracting the zip, BEFORE opening in Visual Studio.
# Right-click this file -> "Run with PowerShell"

$folder = Split-Path -Parent $MyInvocation.MyCommand.Path
Get-ChildItem -Path $folder -Recurse | Unblock-File
Write-Host ""
Write-Host "All files unblocked successfully." -ForegroundColor Green
Write-Host "You can now open Gigapixel Zipper.sln in Visual Studio and press Ctrl+Shift+B to build." -ForegroundColor Green
Write-Host ""
Pause
