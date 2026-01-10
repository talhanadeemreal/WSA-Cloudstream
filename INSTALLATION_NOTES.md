# CloudStream WSA Installation Notes

## Installation Location

This installer allows you to choose **any location** for installation:

- **Default**: `C:\Program Files\CloudStream WSA`
- **Custom**: Any drive and folder you prefer (e.g., `D:\CloudStream`, `E:\Apps\CloudStream WSA`)

## AppData Folder

The installer automatically creates an `AppData` folder in the **same partition** where you install CloudStream WSA:

### Example Structure

If you install to `D:\CloudStream WSA`, the installer creates:
```
D:\
├── CloudStream WSA\           (Installation folder)
│   ├── WsaClient\
│   ├── WsaService\
│   ├── *.vhdx files
│   └── ...
└── AppData\
    └── CloudStream WSA\       (AppData folder for user data)
```

### Purpose

The `AppData` folder is reserved for:
- User preferences
- Application data
- Cached content
- Configuration files

This keeps user data separate from the system installation and allows for easy backup.

## Installation Steps

1. **Run the Installer**: `CloudStreamSetup.exe`
2. **Choose Location**: 
   - Accept default (`C:\Program Files\CloudStream WSA`)
   - **OR** click "Browse" and choose any location
3. **Proceed**: Click "Next" and complete installation
4. **Launch**: Use the desktop shortcut created automatically

## Disk Space Requirements

| Component | Space Required |
|-----------|---------------|
| Installation Files | ~2.5 GB |
| AppData (reserved) | ~500 MB |
| **Total** | **~3 GB** |

## Uninstallation

When you uninstall:
- ✅ Installation folder is removed completely
- ✅ AppData folder is removed from the partition
- ✅ Desktop shortcut is removed
- ✅ WSA is unregistered from Windows

## Advanced: Multiple Installations

You can install CloudStream WSA to **multiple locations** with different configurations:

```
C:\CloudStream WSA\           → Default installation
D:\CloudStream WSA\           → Secondary installation
E:\Apps\CloudStream WSA\      → Portable installation
```

Each installation will have its own AppData folder in the respective partition.

> **Note**: Only one WSA instance can run at a time. Multiple installations share the same WSA registration but can have different app data.
