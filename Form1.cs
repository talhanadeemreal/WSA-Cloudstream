using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Management;
using System.Reflection;

namespace CloudStreamInstaller;

public partial class Form1 : Form
{
    private WebView2 webView;
    private Point dragStartPoint;
    private bool isDragging = false;
    private string tempSetupFolder = string.Empty;
    private bool virtualizationEnabled = false;
    private string installPath = @"C:\Program Files\CloudStream";
    private string logFilePath = string.Empty;

    public Form1()
    {
        InitializeComponent();
        
        // Load embedded icon for the form (taskbar/title bar)
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("CloudStreamInstaller.CloudLogo.ico"))
            {
                if (stream != null)
                {
                    this.Icon = new Icon(stream);
                }
            }
        }
        catch { /* Ignore icon loading errors */ }

        InitializeUI();
        InitializeWebView();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        
        // Form1
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(700, 550);
        this.FormBorderStyle = FormBorderStyle.None;
        this.Name = "Form1";
        this.Text = "CloudStream Installer";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.Black;
        this.ShowIcon = true; // Ensure icon is shown in taskbar
        
        this.ResumeLayout(false);
    }

    private void InitializeUI()
    {
        // Setup window dragging
        this.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        };

        this.MouseMove += (s, e) =>
        {
            if (isDragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - dragStartPoint.X, p.Y - dragStartPoint.Y);
            }
        };

        this.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        };
    }

    private async void InitializeWebView()
    {
        try
        {
            // Extract all embedded resources first
            ExtractEmbeddedResources();

            // Initialize WebView2
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);

            await webView.EnsureCoreWebView2Async(null);

            // Load the HTML file from temp folder (Online Installer)
            string htmlPath = Path.Combine(tempSetupFolder, "Online Installer.html");
            webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);

            // Setup message handler
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize installer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void ExtractEmbeddedResources()
    {
        try
        {
            // Create temp folder
            tempSetupFolder = Path.Combine(Path.GetTempPath(), "CS_Setup");
            if (Directory.Exists(tempSetupFolder))
            {
                Directory.Delete(tempSetupFolder, true);
            }
            Directory.CreateDirectory(tempSetupFolder);

            // Get all embedded resources (Online installer: NO data.7z)
            var assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = new[]
            {
                "Online Installer.html",
                "image_417f8e.png",
                "image_417cc4.png",
                "7za.exe",
                "CloudLogo.ico"
            };

            foreach (var resourceName in resourceNames)
            {
                var fullResourceName = $"CloudStreamInstallerOnline.{resourceName}";
                using (Stream resourceStream = assembly.GetManifestResourceStream(fullResourceName)!)
                {
                    if (resourceStream == null)
                        throw new Exception($"Could not find embedded resource: {resourceName}");

                    string outputPath = Path.Combine(tempSetupFolder, resourceName);
                    using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to extract resources: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string message = e.TryGetWebMessageAsString();

        if (message == "StartInstall" || message == "StartOnlineInstall")
        {
            HandleStartInstall();
        }
        else if (message.StartsWith("CheckProcess:"))
        {
            string processName = message.Substring("CheckProcess:".Length);
            HandleCheckProcess(processName);
        }
        else if (message.StartsWith("BrowseFolder"))
        {
            HandleBrowseFolder();
        }
        else if (message == "CloseApp")
        {
            Application.Exit();
        }
    }

    private async void HandleStartInstall()
    {
        // Create log file in temp directory
        logFilePath = Path.Combine(Path.GetTempPath(), "CloudStream_Install_Log.txt");
        
        try
        {
            File.WriteAllText(logFilePath, $"=== CloudStream Installation Log ===\n");
            File.AppendAllText(logFilePath, $"Started at: {DateTime.Now}\n\n");

            // Send immediate feedback to show installation has started
            Log("Sending initial progress (0%)");
            SendMessageToWebView("Progress:0");
            await Task.Delay(200);
            
            Log("Sending 5% progress");
            SendMessageToWebView("Progress:5");
            await Task.Delay(200);

            // Run installation steps directly (no Task.Run needed - operations are already async)
            try
            {
                // 1. Check virtualization firmware
                Log("Step 1: Checking virtualization status...");
                SendInstallLog("✓ Checking system virtualization...");
                virtualizationEnabled = CheckVirtualizationStatus();
                Log($"Virtualization enabled: {virtualizationEnabled}");

                // 2. Enable Hyper-V features
                Log("Step 2: Enabling Hyper-V features...");
                SendInstallLog("✓ Configuring Windows features...");
                SendMessageToWebView("Progress:5");
                await Task.Delay(500);
                await EnableHyperVFeatures();
                Log("Hyper-V features enabled successfully");
                SendInstallLog("✓ Features configured successfully");

                // 3. Download data.7z from GitHub (NEW for online installer)
                Log("Step 3: Downloading data.7z from GitHub...");
                SendInstallLog("⬇ Connecting to GitHub...");
                SendMessageToWebView("Progress:10");
                await Task.Delay(500);
                await DownloadDataArchive();
                Log("Download completed successfully");
                SendInstallLog("✓ All files downloaded");

                // 4. Extract data.7z with progress updates
                Log("Step 4: Extracting data archive...");
                SendInstallLog("✓ Extracting CloudStream files (630 MB)...");
                SendMessageToWebView("Progress:40");
                await Task.Delay(500);
                await ExtractDataArchive();
                Log("Data archive extracted successfully");
                SendInstallLog("✓ Extraction complete");

                // 5. Run Install.ps1 from extracted folder
                Log("Step 5: Running installation script...");
                SendInstallLog("✓ Registering Windows Subsystem for Android...");
                SendMessageToWebView("Progress:70");
                await Task.Delay(500);
                await RunInstallScript();
                Log("Installation script completed successfully");
                SendInstallLog("✓ WSA registered successfully");

                // 6. Create desktop shortcut (copy from Start Menu)
                Log("Step 6: Creating desktop shortcut...");
                SendInstallLog("✓ Creating desktop shortcut...");
                SendMessageToWebView("Progress:85");
                await Task.Delay(500);
                CreateDesktopShortcut();
                Log("Desktop shortcut created successfully");
                SendInstallLog("✓ Shortcut created");

                // 7. Send completion
                Log("Step 7: Finalizing installation...");
                SendMessageToWebView("Progress:100");
                await Task.Delay(500);
                Log("Installation completed successfully!");

                // 7. Show BIOS alert if needed
                if (!virtualizationEnabled)
                {
                    MessageBox.Show(
                        "Please enable Virtualization from BIOS for a better experience.",
                        "Optimization",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log($"ERROR in installation: {ex.Message}");
                Log($"Stack trace: {ex.StackTrace}");
                
                MessageBox.Show(
                    $"Installation failed: {ex.Message}\n\nLog file saved at:\n{logFilePath}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            Log($"CRITICAL ERROR: {ex.Message}");
            Log($"Stack trace: {ex.StackTrace}");
            
            MessageBox.Show(
                $"Installation failed: {ex.Message}\n\nLog file saved at:\n{logFilePath}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void Log(string message)
    {
        try
        {
            if (!string.IsNullOrEmpty(logFilePath))
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
                File.AppendAllText(logFilePath, logEntry);
            }
        }
        catch { }
    }

    private bool CheckVirtualizationStatus()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (var item in searcher.Get())
                {
                    var firmwareType = item["HypervisorPresent"];
                    if (firmwareType != null && (bool)firmwareType)
                        return true;
                }
            }

            // Alternative check via processor
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (var item in searcher.Get())
                {
                    var virtEnabled = item["VirtualizationFirmwareEnabled"];
                    if (virtEnabled != null && (bool)virtEnabled)
                        return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnableHyperVFeatures()
    {
        try
        {
            Log("Starting Hyper-V feature enablement...");
            
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All -NoRestart; Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Log($"Executing: {psi.FileName} {psi.Arguments}");
            
            var process = Process.Start(psi);
            if (process != null)
            {
                Log("PowerShell process started, waiting for completion...");
                
                // Read output asynchronously to prevent deadlocks
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                string output = await outputTask;
                string error = await errorTask;
                
                Log($"Hyper-V enablement exit code: {process.ExitCode}");
                if (!string.IsNullOrEmpty(output))
                    Log($"Hyper-V output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Log($"Hyper-V errors: {error}");
                    
                Log("Hyper-V feature enablement completed");
            }
            else
            {
                Log("ERROR: Failed to start PowerShell process for Hyper-V");
                throw new Exception("Failed to start Hyper-V enablement process");
            }
        }
        catch (Exception ex)
        {
            Log($"ERROR in EnableHyperVFeatures: {ex.Message}");
            throw new Exception($"Failed to enable Hyper-V features: {ex.Message}");
        }
    }

    private async Task DownloadDataArchive()
    {
        try
        {
            string downloadUrl = "https://github.com/talhanadeemreal/WSA-Cloudstream/releases/download/data.7z/data.7z";
            string downloadPath = Path.Combine(tempSetupFolder, "data.7z");

            Log($"Starting download from: {downloadUrl}");
            SendInstallLog("⬇ Starting download from GitHub...");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(30); // 30 min timeout for 630MB file

                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long? totalBytes = response.Content.Headers.ContentLength;
                    long totalMB = totalBytes.HasValue ? totalBytes.Value / 1024 / 1024 : 0;
                    
                    Log($"File size: {totalMB} MB");

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long downloadedBytes = 0;
                        int bytesRead;
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        long lastReportedBytes = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            // Report every 5MB downloaded
                            if (downloadedBytes - lastReportedBytes >= 5 * 1024 * 1024 || downloadedBytes == totalBytes)
                            {
                                double progressPercent = totalBytes.HasValue ? (double)downloadedBytes / totalBytes.Value * 100 : 0;
                                int progressForUI = (int)(progressPercent * 0.3); // Download is 0-30%
                                
                                long downloadedMB = downloadedBytes / 1024 / 1024;
                                double speedMBps = downloadedBytes / (stopwatch.Elapsed.TotalSeconds) / 1024 / 1024;

                                SendMessageToWebView($"Progress:{progressForUI}");
                                SendInstallLog($"⬇ Downloaded {downloadedMB} MB / {totalMB} MB ({speedMBps:F1} MB/s)");
                                
                                Log($"Downloaded: {downloadedMB} MB / {totalMB} MB ({progressPercent:F1}%)");
                                
                                lastReportedBytes = downloadedBytes;
                            }
                        }
                    }
                }
            }

            Log("Download complete");
            SendInstallLog("⬇ Download complete!");
        }
        catch (Exception ex)
        {
            Log($"ERROR in DownloadDataArchive: {ex.Message}");
            throw new Exception($"Failed to download data.7z: {ex.Message}");
        }
    }

    private async Task ExtractDataArchive()
    {
        try
        {
            Log("Creating installation directory...");
            // Create installation directory
            if (!Directory.Exists(installPath))
            {
                Directory.CreateDirectory(installPath);
                Log($"Created directory: {installPath}");
            }
            else
            {
                Log($"Directory already exists: {installPath}");
            }

            // Path to 7za.exe and data.7z
            string sevenZipPath = Path.Combine(tempSetupFolder, "7za.exe");
            string dataArchive = Path.Combine(tempSetupFolder, "data.7z");

            Log($"7za.exe path: {sevenZipPath}");
            Log($"7za.exe exists: {File.Exists(sevenZipPath)}");
            Log($"data.7z path: {dataArchive}");
            Log($"data.7z exists: {File.Exists(dataArchive)}");
            
            if (File.Exists(dataArchive))
            {
                FileInfo fi = new FileInfo(dataArchive);
                Log($"data.7z size: {fi.Length / 1024 / 1024} MB");
            }

            // Extract using 7za.exe
            Log("Starting 7-Zip extraction process...");
            var psi = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"x \"{dataArchive}\" -o\"{installPath}\" -y",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Log($"7za command: {psi.FileName} {psi.Arguments}");

            var process = Process.Start(psi);
            if (process != null)
            {
                Log("7-Zip process started successfully");
                Log("Waiting for extraction to complete...");

                // Send progress updates while waiting (no Task.Run to avoid threading issues)
                bool completed = false;
                int progressValue = 40;
                while (!completed)
                {
                    await Task.Delay(1000);
                    if (!process.HasExited)
                    {
                        if (progressValue <= 60)
                        {
                            Log($"Extraction in progress ({progressValue}%)...");
                            SendMessageToWebView($"Progress:{progressValue}");
                            progressValue += 10;
                        }
                    }
                    else
                    {
                        completed = true;
                    }
                }

                await process.WaitForExitAsync();
                Log($"7-Zip process exited with code: {process.ExitCode}");

                if (process.ExitCode != 0)
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    Log($"7-Zip stdout: {output}");
                    Log($"7-Zip stderr: {error}");
                    throw new Exception($"7-Zip extraction failed with exit code {process.ExitCode}: {error}");
                }
                
                Log("Extraction completed successfully");

                // List extracted contents to find WSA files
                Log("Listing extracted contents:");
                var dirs = Directory.GetDirectories(installPath);
                foreach (var dir in dirs)
                {
                    string dirName = Path.GetFileName(dir);
                    Log($"  Directory: {dirName}");
                    
                    // Check if this directory contains AppxManifest.xml
                    string manifestPath = Path.Combine(dir, "AppxManifest.xml");
                    if (File.Exists(manifestPath))
                    {
                        Log($"    *** Found AppxManifest.xml in {dirName} ***");
                    }
                }
                
                var files = Directory.GetFiles(installPath);
                Log($"Files in root: {files.Length}");
                foreach (var file in files.Take(10))
                {
                    Log($"  File: {Path.GetFileName(file)}");
                }
            }
            else
            {
                Log("ERROR: Failed to start 7-Zip process");
                throw new Exception("Failed to start 7-Zip process");
            }

            // Copy CloudLogo.ico to install directory
            Log("Copying CloudLogo.ico...");
            string iconSource = Path.Combine(tempSetupFolder, "CloudLogo.ico");
            string iconDest = Path.Combine(installPath, "CloudLogo.ico");
            System.IO.File.Copy(iconSource, iconDest, true);
            Log($"Icon copied to: {iconDest}");
        }
        catch (Exception ex)
        {
            Log($"ERROR in ExtractDataArchive: {ex.Message}");
            Log($"Stack trace: {ex.StackTrace}");
            throw new Exception($"Failed to extract data archive: {ex.Message}");
        }
    }

    private void CreateDesktopShortcut()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string destShortcutPath = Path.Combine(desktopPath, "CloudStream.lnk");
            
            // Source path: %AppData%\Microsoft\Windows\Start Menu\Programs\CloudStream.lnk
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string sourceShortcutPath = Path.Combine(appData, "Microsoft\\Windows\\Start Menu\\Programs\\CloudStream.lnk");

            Log($"Copying shortcut from: {sourceShortcutPath}");
            Log($"To: {destShortcutPath}");

            // Retry for a few seconds as the Start Menu shortcut might take a moment to appear
            int retries = 10;
            while (retries > 0 && !File.Exists(sourceShortcutPath))
            {
                Log($"Waiting for Start Menu shortcut... ({retries} retries left)");
                Thread.Sleep(500);
                retries--;
            }

            if (File.Exists(sourceShortcutPath))
            {
                File.Copy(sourceShortcutPath, destShortcutPath, true);
                Log("Shortcut copied successfully");
            }
            else
            {
                Log("WARNING: Start Menu shortcut not found. Desktop shortcut could not be created.");
                // Fallback? Or just log warning.
            }
        }
        catch (Exception ex)
        {
            Log($"ERROR in CreateDesktopShortcut: {ex.Message}");
        }
    }

    private async Task RunInstallScript()
    {
        try
        {
            // data.7z extracts to a "data" subfolder, so WSA files are in installPath\data\
            string dataFolder = Path.Combine(installPath, "data");
            string scriptPath = Path.Combine(dataFolder, "Install.ps1");
            
            Log($"Looking for Install.ps1 at: {scriptPath}");
            Log($"Install.ps1 exists: {File.Exists(scriptPath)}");

            if (!File.Exists(scriptPath))
            {
                Log("WARNING: Install.ps1 not found in data folder, copying from temp");
                string tempScript = Path.Combine(tempSetupFolder, "Install.ps1");
                if (File.Exists(tempScript))
                {
                    // Ensure data folder exists
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                        Log($"Created data folder: {dataFolder}");
                    }
                    
                    File.Copy(tempScript, scriptPath, true);
                    Log($"Copied Install.ps1 to: {scriptPath}");
                }
                else
                {
                    Log($"ERROR: Install.ps1 not found in temp folder either");
                    throw new Exception("Install.ps1 not found");
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \".\\Install.ps1\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = dataFolder, // Run from data subfolder where WSA files are
                Verb = "runas",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Log($"Running PowerShell script: {psi.FileName} {psi.Arguments}");
            Log($"Working Directory: {psi.WorkingDirectory}");

            var process = Process.Start(psi);
            if (process != null)
            {
                Log("PowerShell script started");
                await process.WaitForExitAsync();
                
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                
                Log($"PowerShell exit code: {process.ExitCode}");
                if (!string.IsNullOrEmpty(output))
                    Log($"PowerShell output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Log($"PowerShell errors: {error}");

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Installation script failed with exit code {process.ExitCode}: {error}");
                }
            }
            else
            {
                Log("ERROR: Failed to start PowerShell process");
                throw new Exception("Failed to start PowerShell process");
            }
        }
        catch (Exception ex)
        {
            Log($"ERROR in RunInstallScript: {ex.Message}");
            throw new Exception($"Failed to run install script: {ex.Message}");
        }
    }

    private void HandleCheckProcess(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            bool isRunning = processes.Length > 0;
            SendMessageToWebView($"ProcessFound:{processName}:{isRunning.ToString().ToLower()}");
        }
        catch
        {
            SendMessageToWebView($"ProcessFound:{processName}:false");
        }
    }

    private void HandleBrowseFolder()
    {
        Invoke(new Action(() =>
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select installation folder";
                folderDialog.SelectedPath = installPath;
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    installPath = folderDialog.SelectedPath;
                    SendMessageToWebView($"InstallPath:{installPath}");
                }
            }
        }));
    }

    private void SendMessageToWebView(string message)
    {
        try
        {
            if (webView?.CoreWebView2 != null && IsHandleCreated)
            {
                webView.CoreWebView2.PostWebMessageAsString(message);
            }
        }
        catch (Exception ex)
        {
            Log($"SendMessageToWebView error: {ex.Message}");
        }
    }

    private void SendInstallLog(string message)
    {
        SendMessageToWebView($"InstallLog:{message}");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Cleanup temp folder
        try
        {
            if (!string.IsNullOrEmpty(tempSetupFolder) && Directory.Exists(tempSetupFolder))
            {
                Directory.Delete(tempSetupFolder, true);
            }
        }
        catch { }

        // Cleanup log file
        try
        {
            if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
        }
        catch { }

        base.OnFormClosing(e);
    }
}
