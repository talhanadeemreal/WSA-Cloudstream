# Quick Build Script for CloudStream Installer
# Run this script to build the installer quickly

param(
    [string]$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)

$ErrorActionPreference = "Stop"

Write-Host "CloudStream Installer Build Script" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Check if Inno Setup is installed
if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "ERROR: Inno Setup not found at: $InnoSetupPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Inno Setup 6 from:" -ForegroundColor Yellow
    Write-Host "https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or specify the correct path using:" -ForegroundColor Yellow
    Write-Host '  .\BuildInstaller.ps1 -InnoSetupPath "C:\Your\Path\ISCC.exe"' -ForegroundColor Yellow
    exit 1
}

# Check if CloudStreamInstaller.iss exists
$IssPath = Join-Path $PSScriptRoot "CloudStreamInstaller.iss"
if (-not (Test-Path $IssPath)) {
    Write-Host "ERROR: CloudStreamInstaller.iss not found!" -ForegroundColor Red
    Write-Host "Expected location: $IssPath" -ForegroundColor Yellow
    exit 1
}

Write-Host "Starting build process..." -ForegroundColor Green
Write-Host ""

try {
    # Run Inno Setup compiler
    $process = Start-Process -FilePath $InnoSetupPath `
        -ArgumentList "`"$IssPath`"" `
        -NoNewWindow `
        -Wait `
        -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Host ""
        Write-Host "===================================" -ForegroundColor Green
        Write-Host "Build completed successfully!" -ForegroundColor Green
        Write-Host "===================================" -ForegroundColor Green
        Write-Host ""
        
        $OutputPath = Join-Path $PSScriptRoot "Output\CloudStreamSetup.exe"
        if (Test-Path $OutputPath) {
            $FileInfo = Get-Item $OutputPath
            Write-Host "Installer created at:" -ForegroundColor Cyan
            Write-Host "  $OutputPath" -ForegroundColor White
            Write-Host ""
            Write-Host "File size: $([math]::Round($FileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
            Write-Host ""
            
            # Ask if user wants to open output folder
            $open = Read-Host "Open output folder? (Y/n)"
            if ($open -ne "n" -and $open -ne "N") {
                Start-Process explorer.exe -ArgumentList "/select,`"$OutputPath`""
            }
        }
    }
    else {
        Write-Host ""
        Write-Host "Build failed with exit code: $($process.ExitCode)" -ForegroundColor Red
        Write-Host "Check the output above for errors." -ForegroundColor Yellow
        exit $process.ExitCode
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
