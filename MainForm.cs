using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace GWxLauncher
{
    public partial class MainForm : Form
    {

        private LauncherConfig _config;

        public MainForm()
        {
            InitializeComponent();
            _config = LauncherConfig.Load();
        }

        private void btnLaunchGw1_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw1Path, "Guild Wars 1");
        }
        private void btnLaunchGw2_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw2Path, "Guild Wars 2");
        }

        private void LaunchGame(string exePath, string gameName)
        {
            try
            {
                // 🔹 New: handle missing/blank path from config
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    MessageBox.Show(
                        $"{gameName} path is not set.\n\n" +
                        "Please edit launcherConfig.json and set the correct EXE path.",
                        "Path Not Set",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Existing check: path is set, but file not found on disk
                if (!File.Exists(exePath))
                {
                    MessageBox.Show(
                        $"Could not find {gameName} executable.\n\nPath:\n{exePath}",
                        "Executable Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false
                };

                Process.Start(startInfo);

                if (lblStatus != null)
                    lblStatus.Text = $"{gameName} launched.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch {gameName}.\n\n{ex.Message}",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                    lblStatus.Text = $"Error launching {gameName}.";
            }
        }

        private void btnSetGw1Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 1 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                // If we already have a path, use its folder as the starting point
                if (!string.IsNullOrWhiteSpace(_config.Gw1Path))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(_config.Gw1Path);
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _config.Gw1Path = dialog.FileName;
                    _config.Save();
                    lblStatus.Text = "GW1 path updated.";
                }
            }
        }

        private void btnSetGw2Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 2 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(_config.Gw2Path))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(_config.Gw2Path);
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _config.Gw2Path = dialog.FileName;
                    _config.Save();
                    lblStatus.Text = "GW2 path updated.";
                }
            }
        }
    }
}

