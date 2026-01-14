using System.Drawing;
using System.Windows.Forms;

namespace CloudStreamInstaller;

public class SplashForm : Form
{
    private PictureBox logoPictureBox;
    private Label versionLabel;
    private Label loadingLabel;
    private ProgressBar progressBar;

    public SplashForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // Form settings
        this.Size = new Size(500, 200);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(17, 17, 17); // #111111
        this.ShowInTaskbar = false;
        this.TopMost = true;

        // Load and display logo
        logoPictureBox = new PictureBox
        {
            Size = new Size(64, 64),
            Location = new Point(218, 30),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("CloudStreamInstaller.CloudLogo.ico"))
            {
                if (stream != null)
                {
                    var icon = new Icon(stream);
                    logoPictureBox.Image = icon.ToBitmap();
                }
            }
        }
        catch { }

        // Version label
        versionLabel = new Label
        {
            Text = "v1.2.0 | PC Version: Talha Nadeem",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(161, 161, 161), // #a1a1a1
            AutoSize = true,
            BackColor = Color.Transparent
        };
        versionLabel.Location = new Point((this.Width - versionLabel.PreferredWidth) / 2, 105);

        // Loading label
        loadingLabel = new Label
        {
            Text = "Loading installer... 0%",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
            ForeColor = Color.FromArgb(59, 130, 246), // #3b82f6
            AutoSize = true,
            BackColor = Color.Transparent
        };
        loadingLabel.Location = new Point((this.Width - loadingLabel.PreferredWidth) / 2, 135);

        // Progress bar
        progressBar = new ProgressBar
        {
            Size = new Size(400, 8),
            Location = new Point(50, 165),
            Style = ProgressBarStyle.Continuous,
            Value = 0
        };

        this.Controls.Add(logoPictureBox);
        this.Controls.Add(versionLabel);
        this.Controls.Add(loadingLabel);
        this.Controls.Add(progressBar);

        this.ResumeLayout(false);
    }

    public void UpdateProgress(int percentage, string message = "")
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateProgress(percentage, message)));
            return;
        }

        progressBar.Value = Math.Min(100, Math.Max(0, percentage));
        
        if (!string.IsNullOrEmpty(message))
        {
            loadingLabel.Text = $"{message} {percentage}%";
        }
        else
        {
            loadingLabel.Text = $"Loading installer... {percentage}%";
        }

        // Re-center label as text changes
        loadingLabel.Location = new Point((this.Width - loadingLabel.PreferredWidth) / 2, 135);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        // Draw border
        using (var pen = new Pen(Color.FromArgb(34, 34, 34), 1))
        {
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
