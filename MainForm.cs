using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Drawing;


namespace GWxLauncher
{
    public partial class MainForm : Form
    {

        private LauncherConfig _config;
        private readonly ProfileManager _profileManager = new();

        private void RefreshProfileList()
        {
            lstProfiles.Items.Clear();

            foreach (var profile in _profileManager.Profiles)
            {
                // ToString() on GameProfile handles "[GW1] Name" etc.
                lstProfiles.Items.Add(profile);
            }
        }

        public MainForm()
        {
            InitializeComponent();
            _config = LauncherConfig.Load();

            // 🔹 Restore window placement if we have saved values
            if (_config.WindowX >= 0 && _config.WindowY >= 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_config.WindowX, _config.WindowY);
            }

            if (_config.WindowWidth > 0 && _config.WindowHeight > 0)
            {
                Size = new Size(_config.WindowWidth, _config.WindowHeight);
            }

            if (_config.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            _profileManager.Load();
            RefreshProfileList();
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _config.WindowX = Left;
                _config.WindowY = Top;
                _config.WindowWidth = Width;
                _config.WindowHeight = Height;
                _config.WindowMaximized = false;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                _config.WindowMaximized = true;

                // Remember where the window would be in normal state
                var bounds = RestoreBounds;
                _config.WindowX = bounds.Left;
                _config.WindowY = bounds.Top;
                _config.WindowWidth = bounds.Width;
                _config.WindowHeight = bounds.Height;
            }

            _config.Save();
        }

        private void btnLaunchGw1_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw1Path, "Guild Wars 1");
        }
        private void btnLaunchGw2_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw2Path, "Guild Wars 2");
        }
        private GameProfile? GetSelectedProfile()
        {
            return lstProfiles.SelectedItem as GameProfile;
        }
        // Lets the user pick an executable for a profile, then saves & refreshes the list.
        // Returns true if a path was chosen, false if the user cancelled.
        private bool TrySelectProfileExecutable(GameProfile profile)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select executable for {profile.Name}";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                // Start from the profile's own path if we have one…
                string initialPath = profile.ExecutablePath;

                // …otherwise fall back to the legacy global config just for convenience
                if (string.IsNullOrWhiteSpace(initialPath))
                {
                    initialPath = profile.GameType == GameType.GuildWars1
                        ? _config.Gw1Path
                        : _config.Gw2Path;
                }

                if (!string.IsNullOrWhiteSpace(initialPath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(initialPath);
                    }
                    catch
                    {
                        // ignore bad paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    profile.ExecutablePath = dialog.FileName;
                    _profileManager.Save();
                    RefreshProfileList();
                    lblStatus.Text = $"Updated path for {profile.Name}.";

                    return true;
                }

                lblStatus.Text = $"No path selected for {profile.Name}.";
                return false;
            }
        }

        private void LaunchProfile(GameProfile profile)
        {
            string exePath = profile.ExecutablePath;

            // If the profile doesn't have a path yet, prompt the user to set one
            if (string.IsNullOrWhiteSpace(exePath))
            {
                var result = MessageBox.Show(
                    $"No executable path is set for \"{profile.Name}\".\n\n" +
                    "Would you like to select it now?",
                    "Executable not set",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Open the file picker; if they cancel, we don't launch
                    if (!TrySelectProfileExecutable(profile))
                    {
                        return;
                    }

                    exePath = profile.ExecutablePath;
                }
                else
                {
                    lblStatus.Text = $"Launch canceled for {profile.Name}.";
                    return;
                }
            }

            string gameName = profile.GameType == GameType.GuildWars1
                ? "Guild Wars 1"
                : "Guild Wars 2";

            LaunchGame(exePath, gameName);
        }

        private void lstProfiles_DoubleClick(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile);
            }
        }
        private void lstProfiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = lstProfiles.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    lstProfiles.SelectedIndex = index;
                }
            }
        }
        private void menuLaunchProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile);
            }
        }

        private void menuSetProfilePath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            TrySelectProfileExecutable(profile);
        }


        private void menuEditProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            using (var dialog = new AddAccountDialog(profile))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.CreatedProfile != null)
                {
                    _profileManager.Save();
                    RefreshProfileList();
                    lblStatus.Text = $"Updated account: {dialog.CreatedProfile.Name}.";
                }
            }
        }

        private void menuDeleteProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            var result = MessageBox.Show(
                $"Delete account \"{profile.Name}\"?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _profileManager.RemoveProfile(profile);
                _profileManager.Save();
                RefreshProfileList();
                lblStatus.Text = $"Deleted account: {profile.Name}.";
            }
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
                        "Please set the correct EXE path.",
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

        private void btnAddAcount_Click(object sender, EventArgs e)
        {
            using (var dialog = new AddAccountDialog())
            {
                var result = dialog.ShowDialog(this);
                if (result == DialogResult.OK && dialog.CreatedProfile != null)
                {
                    _profileManager.AddProfile(dialog.CreatedProfile);
                    _profileManager.Save();     // 🔹 persist to profiles.json

                    RefreshProfileList();
                    lblStatus.Text = $"Added account: {dialog.CreatedProfile.Name}";
                }
            }
        }
    }
}

