using System.Reflection;
using GWxLauncher.Services;

namespace GWxLauncher.UI.TabControls
{
    internal partial class GlobalHelpTabContent : UserControl
    {
        public GlobalHelpTabContent()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // Try InformationalVersion first (includes git hash if present)
            var infoVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            
            string fullVersion = infoVersion ?? assembly.GetName().Version?.ToString(3) ?? "0.0.0";

            // Split version and build hash (format: "1.5.1+githash")
            var parts = fullVersion.Split('+');
            var versionNumber = parts[0]; // e.g., "1.5.1"
            var buildHash = parts.Length > 1 ? parts[1] : null;

            lblVersion.Text = $"Version: {versionNumber}";
            
            if (!string.IsNullOrEmpty(buildHash))
            {
                lblBuildInfo.Text = $"Build: {buildHash}";
                lblBuildInfo.Visible = true;
            }
            else
            {
                lblBuildInfo.Visible = false;
            }
        }

        private async void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            btnCheckForUpdates.Enabled = false;
            lblUpdateStatus.Text = "Checking for updates...";
            lblUpdateStatus.ForeColor = ThemeService.Palette.WindowFore;

            try
            {
                var updateChecker = new UpdateChecker();
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                if (updateInfo.UpdateAvailable)
                {
                    lblUpdateStatus.Text = $"Update available: v{updateInfo.LatestVersion} (Released: {updateInfo.PublishedAt?.ToLocalTime():MMM dd, yyyy})";
                    lblUpdateStatus.ForeColor = Color.Orange;
                    btnOpenReleases.Visible = true;
                    btnOpenReleases.Tag = updateInfo.ReleaseUrl;
                }
                else
                {
                    lblUpdateStatus.Text = "You're running the latest version!";
                    lblUpdateStatus.ForeColor = Color.LimeGreen;
                    btnOpenReleases.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblUpdateStatus.Text = $"Failed to check for updates: {ex.Message}";
                lblUpdateStatus.ForeColor = Color.Red;
                btnOpenReleases.Visible = false;
            }
            finally
            {
                btnCheckForUpdates.Enabled = true;
            }
        }

        private void btnOpenReleases_Click(object sender, EventArgs e)
        {
            if (btnOpenReleases.Tag is string url && !string.IsNullOrEmpty(url))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }

        private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Royel-Payne/GWxLauncher",
                UseShellExecute = true
            });
        }
    }
}
