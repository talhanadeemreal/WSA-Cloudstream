; CloudStream WSA Zero-Configuration Installer
; Inno Setup Script for packaging WSA with CloudStream

#define MyAppName "CloudStream for Windows"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "CloudStream Community"
#define MyAppURL "https://github.com/recloudstream/cloudstream"
#define MyAppExeName "CloudStream.lnk"

[Setup]
; Basic Application Information
AppId={{E8F9A2C1-4D3B-4E8F-9A1C-2D3E4F5A6B7C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation Settings
DefaultDirName={autopf}\CloudStream WSA
DefaultGroupName={#MyAppName}
DisableDirPage=no
DisableProgramGroupPage=yes
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
DirExistsWarning=auto
UsePreviousAppDir=yes

; Output Settings
OutputDir=Output
OutputBaseFilename=CloudStreamSetup_Online
SetupIconFile=Assets\app.ico
UninstallDisplayIcon={app}\Assets\app.ico

; Compression
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumBlockThreads=2

; Visual Settings
WizardStyle=modern

; Compatibility
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=10.0.19041

; Uninstall
UninstallDisplayName={#MyAppName}
UninstallFilesDir={app}\Uninstall

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel2=This will install CloudStream on Windows Subsystem for Android (WSA) on your computer.%n%nThis is an online installer which will download WSA components (~2GB) during installation.%n%nIMPORTANT:%n- Administrator privileges are required%n- High-speed internet connection required%n- System restart may be required%n- Requires ~3GB free disk space

[Files]
; Copy all WSA files EXCEPT large VHDX images which are downloaded at runtime
Source: "*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "Output,CloudStreamInstaller.iss,README_INSTALLER.md,BuildInstaller.ps1,*.vhdx"

[Icons]
; We don't create Start Menu shortcuts here as the desktop shortcut is created by the script
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Run]
; Run the installation script with admin privileges
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -WindowStyle Normal -File ""{app}\InstallCloudStream.ps1"""; Flags: runhidden waituntilterminated; StatusMsg: "Installing WSA and CloudStream..."

[UninstallRun]
; Uninstall WSA when uninstalling
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -Command ""Get-AppxPackage -Name 'MicrosoftCorporationII.WindowsSubsystemForAndroid' | Remove-AppxPackage"""; Flags: runhidden waituntilterminated; StatusMsg: "Removing Windows Subsystem for Android..."

[UninstallDelete]
; Clean up desktop shortcut
Type: files; Name: "{userdesktop}\CloudStream.lnk"
; Clean up AppData folder (using code section to get dynamic drive)
Type: filesandordirs; Name: "{app}\..\..\..\AppData\CloudStream WSA"

[Code]
var
  DownloadPage: TDownloadWizardPage;
  RequirementsPage: TOutputMsgMemoWizardPage;
  AppDataDir: String;

function GetInstallDrive(): String;
var
  InstallPath: String;
begin
  InstallPath := ExpandConstant('{app}');
  Result := ExtractFileDrive(InstallPath);
end;

procedure CreateAppDataFolder();
var
  Drive: String;
  AppDataPath: String;
begin
  Drive := GetInstallDrive();
  AppDataPath := Drive + '\AppData\CloudStream WSA';
  if not DirExists(AppDataPath) then
  begin
    if CreateDir(AppDataPath) then
      Log('Created AppData folder: ' + AppDataPath)
    else
      Log('Failed to create AppData folder: ' + AppDataPath);
  end
  else
  begin
    Log('AppData folder already exists: ' + AppDataPath);
  end;
  AppDataDir := AppDataPath;
end;

function InitializeSetup(): Boolean;
var
  Version: TWindowsVersion;
  ErrorMessage: String;
begin
  Result := True;
  GetWindowsVersionEx(Version);
  
  // Check Windows version (10.0.19041 = Windows 10 Build 19041 or later)
  if (Version.Major < 10) or ((Version.Major = 10) and (Version.Build < 19041)) then
  begin
    ErrorMessage := 'CloudStream WSA Installer requires Windows 10 Build 19041 or later, or Windows 11.' + #13#10#13#10 +
                    'Your current version: ' + IntToStr(Version.Major) + '.' + IntToStr(Version.Minor) + ' Build ' + IntToStr(Version.Build) + #13#10#13#10 +
                    'Please update Windows and try again.';
    MsgBox(ErrorMessage, mbError, MB_OK);
    Result := False;
  end;
end;

procedure OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64);
begin
  if ProgressMax <> 0 then
    Log(Format('  %d of %d bytes downloaded.', [Progress, ProgressMax]));
end;

procedure InitializeWizard();
begin
  // Create download page
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), 'Downloading WSA Components...', nil);
  
  // Create requirements page
  RequirementsPage := CreateOutputMsgMemoPage(wpWelcome,
    'System Requirements', 
    'Please review the requirements before continuing',
    'This installer will set up CloudStream with Windows Subsystem for Android (WSA). ' +
    'Please ensure your system meets the following requirements:',
    ''
  );
  
  RequirementsPage.RichEditViewer.Lines.Add('');
  RequirementsPage.RichEditViewer.Lines.Add('SYSTEM REQUIREMENTS:');
  RequirementsPage.RichEditViewer.Lines.Add('  • Windows 10 Build 19041 or later, or Windows 11');
  RequirementsPage.RichEditViewer.Lines.Add('  • 64-bit processor with virtualization support');
  RequirementsPage.RichEditViewer.Lines.Add('  • At least 8GB RAM (16GB recommended)');
  RequirementsPage.RichEditViewer.Lines.Add('  • 3GB free disk space');
  RequirementsPage.RichEditViewer.Lines.Add('  • Administrator privileges');
  RequirementsPage.RichEditViewer.Lines.Add('');
  RequirementsPage.RichEditViewer.Lines.Add('INSTALLATION NOTES:');
  RequirementsPage.RichEditViewer.Lines.Add('  • Virtual Machine Platform will be enabled automatically');
  RequirementsPage.RichEditViewer.Lines.Add('  • System restart may be required');
  RequirementsPage.RichEditViewer.Lines.Add('  • Installation takes approximately 10-15 minutes');
  RequirementsPage.RichEditViewer.Lines.Add('  • High-speed internet required for downloading components');
  RequirementsPage.RichEditViewer.Lines.Add('');
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpReady then begin
    DownloadPage.Clear;
    { REPLACE THESE URLs WITH YOUR ACTUAL GITHUB RELEASE LINKS }
    DownloadPage.Add('https://github.com/recloudstream/cloudstream/releases/download/v1.0/system.vhdx', 'system.vhdx', '');
    DownloadPage.Add('https://github.com/recloudstream/cloudstream/releases/download/v1.0/system_ext.vhdx', 'system_ext.vhdx', '');
    DownloadPage.Add('https://github.com/recloudstream/cloudstream/releases/download/v1.0/product.vhdx', 'product.vhdx', '');
    DownloadPage.Add('https://github.com/recloudstream/cloudstream/releases/download/v1.0/vendor.vhdx', 'vendor.vhdx', '');
    DownloadPage.Add('https://github.com/recloudstream/cloudstream/releases/download/v1.0/userdata.vhdx', 'userdata.vhdx', '');
    
    DownloadPage.Show;
    try
      try
        DownloadPage.Download;
        Result := True;
      except
        SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
        Result := False;
      end;
    finally
      DownloadPage.Hide;
    end;
  end else
    Result := True;
end;

procedure CopyDownloadedFiles;
begin
  { Move downloaded files from tmp to app dir }
  FileCopy(ExpandConstant('{tmp}\system.vhdx'), ExpandConstant('{app}\system.vhdx'), False);
  FileCopy(ExpandConstant('{tmp}\system_ext.vhdx'), ExpandConstant('{app}\system_ext.vhdx'), False);
  FileCopy(ExpandConstant('{tmp}\product.vhdx'), ExpandConstant('{app}\product.vhdx'), False);
  FileCopy(ExpandConstant('{tmp}\vendor.vhdx'), ExpandConstant('{app}\vendor.vhdx'), False);
  FileCopy(ExpandConstant('{tmp}\userdata.vhdx'), ExpandConstant('{app}\userdata.vhdx'), False);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Result := '';
  if Exec('powershell.exe', 
    '-ExecutionPolicy Bypass -Command "if ((Get-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform).State -eq ''Enabled'') { exit 0 } else { exit 1 }"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode <> 0 then NeedsRestart := True;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    CopyDownloadedFiles();
    CreateAppDataFolder();
  end;
end;

function UnInstallNeedsRestart(): Boolean;
begin
  // Uninstall typically doesn't need restart
  Result := False;
end;
