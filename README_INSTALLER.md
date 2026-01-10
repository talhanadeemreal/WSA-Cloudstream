# CloudStream WSA Zero-Configuration Installer

## Overview

This installer packages **Windows Subsystem for Android (WSA)** with **CloudStream** pre-configured for a completely automated, zero-configuration installation experience.

## What This Installer Does

1. ✅ Installs Windows Subsystem for Android (WSA) with CloudStream pre-installed
2. ✅ Creates a desktop shortcut for instant launching
3. ✅ Configures all required Windows features
4. ✅ Handles all dependencies automatically

**No manual configuration required!**

## System Requirements

| Requirement | Details |
|------------|---------|
| **Operating System** | Windows 10 Build 19041+ or Windows 11 |
| **Architecture** | 64-bit processor with virtualization support |
| **RAM** | 8GB minimum (16GB recommended) |
| **Disk Space** | 3GB free space |
| **Privileges** | Administrator access required |
| **Internet** | Recommended for initial setup |

## Installation Instructions

### For End Users

1. **Download** `CloudStreamSetup.exe`
2. **Right-click** the installer and select **"Run as Administrator"**
3. **Follow the wizard**:
   - Review system requirements
   - Choose installation directory (default: `C:\Program Files\CloudStream WSA`)
   - Click "Install"
4. **Wait** for installation to complete (10-15 minutes)
5. **Launch CloudStream** using the desktop shortcut

> **Note**: First launch may take 30-60 seconds as WSA initializes.

### First Launch

After installation:
1. Look for the **CloudStream** shortcut on your desktop
2. Double-click to launch
3. Wait for WSA to start (shows Android logo)
4. CloudStream will open automatically

### Potential Restart

If the **Virtual Machine Platform** feature is not enabled on your system, the installer will enable it automatically. **You may be prompted to restart Windows** to complete the installation.

## Building the Installer from Source

### Prerequisites

1. **Inno Setup 6** or later
   - Download from: https://jrsoftware.org/isinfo.php
   - Install with default options

2. **Complete WSA Project Files**
   - All files in the `WSA_Project` directory
   - Including `cloudstream.apk` (64MB+)

### Build Steps

1. **Open Command Prompt** in the `WSA_Project` directory:
   ```cmd
   cd "d:\Softwares\CloudStream (WSA)\WSA_Project"
   ```

2. **Compile with Inno Setup**:
   ```cmd
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" CloudStreamInstaller.iss
   ```

3. **Locate Output**:
   - Installer will be created at: `.\Output\CloudStreamSetup.exe`
   - File size: ~2GB (includes complete WSA + CloudStream)

### Build via Inno Setup GUI

Alternatively:
1. Open **Inno Setup Compiler**
2. Open `CloudStreamInstaller.iss`
3. Click **Build** → **Compile**
4. Find output in `Output` folder

## Troubleshooting

### Installation Issues

**Problem**: "Installation failed" error
- **Solution**: Ensure you're running as Administrator
- **Solution**: Check that Virtual Machine Platform can be enabled
- **Solution**: Verify at least 3GB free disk space

**Problem**: System restart required
- **Solution**: This is normal if Virtual Machine Platform wasn't enabled. Restart and installation will continue.

**Problem**: Installation takes very long
- **Solution**: WSA installation is large (~2GB). Wait 10-15 minutes on average systems.

### Launch Issues

**Problem**: Desktop shortcut doesn't work
- **Solution**: Manually start WSA from Start Menu, then use shortcut
- **Solution**: Reinstall using the uninstaller first

**Problem**: CloudStream doesn't appear
- **Solution**: Open WSA Settings and check installed apps
- **Solution**: Manually install APK from: `C:\Program Files\CloudStream WSA\cloudstream.apk`

**Problem**: WSA won't start
- **Solution**: Check Windows Update for latest WSA updates
- **Solution**: Ensure virtualization is enabled in BIOS

### Uninstallation

To completely remove CloudStream WSA:

1. **Via Control Panel**:
   - Open "Apps & Features"
   - Find "CloudStream for Windows"
   - Click "Uninstall"

2. **Manual Cleanup** (if needed):
   ```powershell
   # Remove WSA
   Get-AppxPackage -Name "MicrosoftCorporationII.WindowsSubsystemForAndroid" | Remove-AppxPackage
   
   # Remove desktop shortcut
   Remove-Item "$env:USERPROFILE\Desktop\CloudStream.lnk" -Force
   
   # Remove installation folder
   Remove-Item "C:\Program Files\CloudStream WSA" -Recurse -Force
   ```

## Technical Details

### Installation Components

The installer packages:

| Component | Size | Purpose |
|-----------|------|---------|
| WSA System Files | ~1.8GB | Android subsystem with CloudStream pre-installed |
| Dependencies | ~12MB | Microsoft VCLibs, UI.Xaml |
| Scripts | ~20KB | Installation automation |
| Assets | ~500KB | Icons and branding |

### Installation Flow

```
1. Extract files to Program Files
   ↓
2. Run Install.ps1 (WSA registration)
   ↓
3. Enable Virtual Machine Platform
   ↓
4. Register WSA with Windows
   ↓
5. Create desktop shortcut
   ↓
6. Complete!
```

### Files Created

After installation:

- **Installation Directory**: `C:\Program Files\CloudStream WSA\`
  - All WSA files
  - CloudStream APK
  - Installation scripts
  
- **Desktop**: `CloudStream.lnk` shortcut

- **Start Menu**: Uninstaller shortcut

- **Windows Apps**: WSA registered as system package

### Security Notes

- ✅ All scripts are signed and transparent (PowerShell source included)
- ✅ No telemetry or data collection
- ✅ Local installation only
- ✅ Open source CloudStream application
- ✅ Official Microsoft WSA components

## FAQ

**Q: Is this safe to install?**  
A: Yes. This installer uses official Microsoft WSA components and the open-source CloudStream application. All scripts are included and can be reviewed.

**Q: Do I need Developer Mode enabled?**  
A: No. The installer handles all necessary registry changes automatically.

**Q: Can I use other Android apps with this WSA?**  
A: Yes. WSA Settings allows you to install other APKs once installed.

**Q: Will this affect my existing WSA installation?**  
A: If you already have WSA installed, the installer will prompt to uninstall it first.

**Q: Does this require an internet connection?**  
A: Not strictly required, but recommended for WSA's initial Android setup.

**Q: How much disk space after installation?**  
A: Approximately 2.5-3GB total.

**Q: Can I move the installation to another drive?**  
A: You can choose the installation directory during setup, but WSA itself registers with Windows and has some fixed locations.

## Support & Links

- **CloudStream GitHub**: https://github.com/recloudstream/cloudstream
- **WSA Documentation**: https://learn.microsoft.com/windows/android/wsa/

## License

- **WSA**: Licensed by Microsoft Corporation
- **CloudStream**: Open source (Check CloudStream repository for license)
- **This Installer**: Free to use and distribute

---

**Version**: 1.0.0  
**Last Updated**: January 2026
