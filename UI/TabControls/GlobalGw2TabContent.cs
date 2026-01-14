using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI.Dialogs;

namespace GWxLauncher.UI.TabControls
{
    public partial class GlobalGw2TabContent : UserControl
    {
        private LauncherConfig _cfg;
        private ProfileManager? _profileManager;
        private bool _isUpdatingFromConfig;

        public GlobalGw2TabContent()
        {
            InitializeComponent();
            ApplyTheme();

            // Ensure the control autosizes to its contents so the parent AutoScroll panel knows to scroll.
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // Add padding at the bottom to ensure the last buttons aren't cut off by the scroll view
            this.Padding = new Padding(0, 0, 0, 64);

            cbGw2RenameWindowTitle.CheckedChanged += (s, e) => txtGw2TitleTemplate.Enabled = cbGw2RenameWindowTitle.Checked;
            chkGw2IsolationEnabled.CheckedChanged += ChkGw2IsolationEnabled_CheckedChanged;
        }

        internal void SetProfileManager(ProfileManager profileManager)
        {
            _profileManager = profileManager;
        }

        internal void BindConfig(LauncherConfig cfg)
        {
            _cfg = cfg;
            _isUpdatingFromConfig = true;

            // Multiclient
            chkGw2Multiclient.Checked = _cfg.Gw2MulticlientEnabled;

            // Window Title
            cbGw2RenameWindowTitle.Checked = _cfg.Gw2WindowTitleEnabled;
            txtGw2TitleTemplate.Text = string.IsNullOrWhiteSpace(_cfg.Gw2WindowTitleTemplate)
                ? "GW2 • {ProfileName}"
                : _cfg.Gw2WindowTitleTemplate;
            txtGw2TitleTemplate.Enabled = cbGw2RenameWindowTitle.Checked;

            // Bulk Launch Delay
            numGw2BulkDelay.Value = Math.Clamp(_cfg.Gw2BulkLaunchDelaySeconds, 0, 90);

            // Isolation
            chkGw2IsolationEnabled.Checked = _cfg.Gw2IsolationEnabled;

            _isUpdatingFromConfig = false;
        }

        internal void SaveConfig(LauncherConfig cfg)
        {
            // Multiclient
            cfg.Gw2MulticlientEnabled = chkGw2Multiclient.Checked;

            cfg.Gw2WindowTitleEnabled = cbGw2RenameWindowTitle.Checked;

            var tpl = (txtGw2TitleTemplate.Text ?? "").Trim();
            cfg.Gw2WindowTitleTemplate = string.IsNullOrWhiteSpace(tpl) ? "GW2 • {ProfileName}" : tpl;

            // Bulk Launch Delay
            cfg.Gw2BulkLaunchDelaySeconds = (int)numGw2BulkDelay.Value;

            // Isolation
            cfg.Gw2IsolationEnabled = chkGw2IsolationEnabled.Checked;
        }

        private async void ChkGw2IsolationEnabled_CheckedChanged(object? sender, EventArgs e)
        {
            // Prevent recursive updates
            if (_isUpdatingFromConfig)
                return;

            bool newValue = chkGw2IsolationEnabled.Checked;

            // If disabling, just save and return
            if (!newValue)
            {
                _cfg.Gw2IsolationEnabled = false;
                _cfg.Save();
                return;
            }

            // If enabling, validate first
            if (_profileManager == null)
            {
                MessageBox.Show(
                    "ProfileManager not initialized. Cannot enable isolation.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                chkGw2IsolationEnabled.Checked = false;
                return;
            }

            var validator = new Gw2IsolationValidator();
            var result = validator.Validate(_profileManager.Profiles.ToList());

            if (!result.CanEnable)
            {
                // Show setup dialog
                using var setupDialog = new Gw2IsolationSetupDialog(result);
                if (setupDialog.ShowDialog(this) != DialogResult.OK)
                {
                    // User cancelled - revert checkbox
                    _isUpdatingFromConfig = true;
                    chkGw2IsolationEnabled.Checked = false;
                    _isUpdatingFromConfig = false;
                    return;
                }

                // User selected profiles to copy
                foreach (var profile in setupDialog.SelectedProfiles)
                {
                    // Get current game folder
                    string currentExePath = profile.ExecutablePath;
                    if (string.IsNullOrWhiteSpace(currentExePath))
                    {
                        MessageBox.Show(
                            $"Profile '{profile.Name}' does not have an executable path configured.",
                            "Invalid Profile",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        continue;
                    }

                    string? currentFolder = Path.GetDirectoryName(currentExePath);
                    if (string.IsNullOrWhiteSpace(currentFolder) || !Directory.Exists(currentFolder))
                    {
                        MessageBox.Show(
                            $"Game folder for profile '{profile.Name}' does not exist:\n{currentFolder}",
                            "Invalid Folder",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        continue;
                    }

                    // Show folder browser for destination
                    using var folderBrowser = new FolderBrowserDialog();
                    folderBrowser.Description = $"Select destination folder for copying profile: {profile.Name}";

                    // Suggest parent of current game folder
                    string parentFolder = Path.GetDirectoryName(currentFolder) ?? "";
                    string suggestedFolder = Path.Combine(
                        parentFolder,
                        $"{Path.GetFileName(currentFolder)}_Profile_{profile.Name.Replace(" ", "_")}");

                    folderBrowser.SelectedPath = suggestedFolder;

                    if (folderBrowser.ShowDialog(this) != DialogResult.OK)
                    {
                        _isUpdatingFromConfig = true;
                        chkGw2IsolationEnabled.Checked = false;
                        _isUpdatingFromConfig = false;
                        return;
                    }

                    string destFolder = folderBrowser.SelectedPath;

                    // Check disk space
                    if (!validator.CheckDiskSpace(currentFolder, destFolder))
                    {
                        var (sourceGB, freeGB, requiredGB) = validator.GetDiskSpaceInfo(currentFolder, destFolder);

                        MessageBox.Show(
                            $"Insufficient disk space!\n\n" +
                            $"Source size: {sourceGB:F1} GB\n" +
                            $"Free space: {freeGB:F1} GB\n" +
                            $"Required: {requiredGB:F1} GB (including 5GB safety margin)",
                            "Insufficient Space",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        _isUpdatingFromConfig = true;
                        chkGw2IsolationEnabled.Checked = false;
                        _isUpdatingFromConfig = false;
                        return;
                    }

                    // Show copy progress dialog
                    using var copyDialog = new Gw2FolderCopyProgressDialog(
                        profile, currentFolder, destFolder);

                    if (copyDialog.ShowDialog(this) != DialogResult.OK)
                    {
                        _isUpdatingFromConfig = true;
                        chkGw2IsolationEnabled.Checked = false;
                        _isUpdatingFromConfig = false;
                        return;
                    }

                    // Update profile with new game folder
                    profile.IsolationGameFolderPath = destFolder;
                    profile.ExecutablePath = Path.Combine(destFolder, "Gw2-64.exe");

                    // Save profile
                    _profileManager.Save();
                }
            }

            // Save setting
            _cfg.Gw2IsolationEnabled = newValue;
            _cfg.Save();

            MessageBox.Show(
                "GW2 Isolation enabled successfully!\n\n" +
                "Profiles will now use isolated AppData folders.\n" +
                "-shareArchive will be disabled when isolation is active.",
                "Isolation Enabled",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }
    }
}
