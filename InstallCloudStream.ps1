# CloudStream WSA Zero-Configuration Installer
# This script automates the installation of WSA with CloudStream pre-installed

$Host.UI.RawUI.WindowTitle = "Installing CloudStream on WSA...."
$ErrorActionPreference = "Continue"

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Create-CloudStreamShortcut {
    Write-ColorOutput "`nCreating desktop shortcut..." "Cyan"
    
    try {
        $WshShell = New-Object -ComObject WScript.Shell
        $DesktopPath = [Environment]::GetFolderPath("Desktop")
        $ShortcutPath = Join-Path $DesktopPath "CloudStream.lnk"
        
        $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
        $Shortcut.TargetPath = "wsa://com.lagradost.cloudstream3"
        
        # Try to set icon if available
        $IconPath = Join-Path $PSScriptRoot "Assets\app.ico"
        if (Test-Path $IconPath) {
            $Shortcut.IconLocation = $IconPath
        }
        
        $Shortcut.Description = "Launch CloudStream on Windows Subsystem for Android"
        $Shortcut.Save()
        
        Write-ColorOutput "✓ Desktop shortcut created successfully!" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "Failed to create desktop shortcut: $_" "Red"
        return $false
    }
}

# Main Installation Flow
Clear-Host
Write-ColorOutput "================================================" "Cyan"
Write-ColorOutput "   CloudStream WSA Zero-Config Installer" "Cyan"
Write-ColorOutput "================================================" "Cyan"
Write-ColorOutput ""

# Step 1: Run the original WSA installation
Write-ColorOutput "[Step 1/2] Installing Windows Subsystem for Android..." "Cyan"
Write-ColorOutput "CloudStream is pre-installed and will be available after setup." "Yellow"
Write-ColorOutput "This may take several minutes. Please be patient..." "Yellow"
Write-ColorOutput ""

$InstallScriptPath = Join-Path $PSScriptRoot "Install.ps1"
if (Test-Path $InstallScriptPath) {
    try {
        # Run the original install script
        & $InstallScriptPath
        Write-ColorOutput "`n✓ WSA installation completed!" "Green"
    }
    catch {
        Write-ColorOutput "WSA installation encountered an issue: $_" "Red"
        Write-ColorOutput "Please check the installation log." "Yellow"
        exit 1
    }
}
else {
    Write-ColorOutput "Error: Install.ps1 not found!" "Red"
    exit 1
}

# Step 2: Create desktop shortcut
Write-ColorOutput "`n[Step 2/2] Creating Desktop Shortcut..." "Cyan"
$ShortcutCreated = Create-CloudStreamShortcut

# Final message
Write-ColorOutput "`n================================================" "Green"
Write-ColorOutput "   Installation Complete!" "Green"
Write-ColorOutput "================================================" "Green"
Write-ColorOutput ""
Write-ColorOutput "CloudStream has been installed successfully!" "Cyan"
Write-ColorOutput ""
Write-ColorOutput "Next Steps:" "Yellow"
Write-ColorOutput "  1. Look for the 'CloudStream' shortcut on your desktop" "White"
Write-ColorOutput "  2. Double-click the shortcut to launch CloudStream" "White"
Write-ColorOutput "  3. Wait a few seconds for WSA to start" "White"
Write-ColorOutput ""
Write-ColorOutput "Note: First launch may take 30-60 seconds as WSA initializes" "Yellow"
Write-ColorOutput ""

