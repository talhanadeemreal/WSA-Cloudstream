namespace CloudStreamInstaller;

static class Program
{
    public static SplashForm? SplashScreen { get; private set; }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Show splash screen immediately
        SplashScreen = new SplashForm();
        SplashScreen.Show();
        SplashScreen.UpdateProgress(0, "Loading");
        Application.DoEvents(); // Force splash to render

        // Give splash a moment to display
        System.Threading.Thread.Sleep(100);

        // Create main form (resource extraction happens here, takes 30-60s)
        var mainForm = new Form1();
        
        // Close splash and show main form
        if (SplashScreen != null && !SplashScreen.IsDisposed)
        {
            SplashScreen.Close();
            SplashScreen.Dispose();
        }

        Application.Run(mainForm);
    }

    public static void UpdateSplashProgress(int percentage, string message = "")
    {
        if (SplashScreen != null && !SplashScreen.IsDisposed)
        {
            SplashScreen.UpdateProgress(percentage, message);
            Application.DoEvents();
        }
    }
}
