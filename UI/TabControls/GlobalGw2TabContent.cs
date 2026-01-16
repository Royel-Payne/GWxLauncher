using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI.Dialogs;

namespace GWxLauncher.UI.TabControls
{
    public partial class GlobalGw2TabContent : UserControl
    {
        private LauncherConfig _cfg = null!;  // Initialized in BindConfig()
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
                // Auto-generate copy plan and proceed immediately
                var copyPlan = GenerateCopyPlan(result);

                if (copyPlan.Count == 0)
                {
                    // No copies needed, just enable
                    _cfg.Gw2IsolationEnabled = newValue;
                    _cfg.Save();
                    return;
                }

                // Calculate total disk space needed
                long totalSpaceGB = 0;
                foreach (var (profile, _) in copyPlan)
                {
                    string? currentFolder = Path.GetDirectoryName(profile.ExecutablePath);
                    if (!string.IsNullOrWhiteSpace(currentFolder) && Directory.Exists(currentFolder))
                    {
                        long folderSizeBytes = GetFolderSize(currentFolder);
                        totalSpaceGB += folderSizeBytes / (1024 * 1024 * 1024);
                    }
                }

                // Show confirmation dialog
                using var confirmDialog = new Gw2IsolationConfirmDialog(
                    result.ProfilesWithDuplicateExePath,
                    copyPlan.Count,
                    totalSpaceGB);

                if (confirmDialog.ShowDialog(this) != DialogResult.OK)
                {
                    // User clicked No
                    _isUpdatingFromConfig = true;
                    chkGw2IsolationEnabled.Checked = false;
                    _isUpdatingFromConfig = false;
                    return;
                }

                // Process copies with auto-generated destinations
                foreach (var (profile, destFolder) in copyPlan)
                {

                    // Get source folder from profile's ExecutablePath
                    string? currentFolder = Path.GetDirectoryName(profile.ExecutablePath);
                    if (string.IsNullOrWhiteSpace(currentFolder) || !Directory.Exists(currentFolder))
                    {
                        MessageBox.Show(
                            $"Game folder for profile '{profile.Name}' does not exist:\n{currentFolder}",
                            "Invalid Folder",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        continue;
                    }

                    // Check disk space
                    if (!validator.CheckDiskSpace(currentFolder, destFolder))
                    {
                        var (sourceGB, freeGB, requiredGB) = validator.GetDiskSpaceInfo(currentFolder, destFolder);

                        MessageBox.Show(
                            $"Insufficient disk space for {profile.Name}!\n\n" +
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
                }

                // For profiles keeping the original folder, clear IsolationGameFolderPath
                // This tells the isolation service to use ExecutablePath instead
                var profilesWithCopies = copyPlan.Select(cp => cp.profile).ToList();
                
                foreach (var group in result.ExePathToProfiles.Values)
                {
                    if (group.Count > 0)
                    {
                        var keepOriginal = group[0]; // First profile in each group keeps original
                        if (!profilesWithCopies.Contains(keepOriginal))
                        {
                            keepOriginal.IsolationGameFolderPath = ""; // Empty = use original ExecutablePath
                        }
                    }
                }

                // Save all profile changes
                _profileManager.Save();
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

        /// <summary>
        /// Generate copy plan: which profiles get copies and where.
        /// Returns list of (profile, destinationFolder) tuples.
        /// First profile in each group keeps original (not included in list).
        /// </summary>
        private List<(GameProfile profile, string destFolder)> GenerateCopyPlan(Gw2IsolationValidationResult validationResult)
        {
            var copyPlan = new List<(GameProfile, string)>();

            foreach (var group in validationResult.ExePathToProfiles)
            {
                string originalExePath = group.Key;
                var profiles = group.Value;

                // Get original folder info
                string? originalFolder = Path.GetDirectoryName(originalExePath);
                if (string.IsNullOrEmpty(originalFolder))
                    continue;

                string originalFolderName = Path.GetFileName(originalFolder);
                string? parentFolder = Path.GetDirectoryName(originalFolder);
                if (string.IsNullOrEmpty(parentFolder))
                    parentFolder = originalFolder;

                // First profile keeps original (skip it)
                // profiles[0] will have IsolationGameFolderPath = "" (empty = use original)

                // Rest get copies with auto-generated names
                for (int i = 1; i < profiles.Count; i++)
                {
                    var profile = profiles[i];

                    // Generate destination: sibling folder with profile name appended
                    string cleanProfileName = SanitizeForFolderName(profile.Name);
                    string suggestedFolder = Path.Combine(parentFolder, $"{originalFolderName}_{cleanProfileName}");

                    copyPlan.Add((profile, suggestedFolder));
                }
            }

            return copyPlan;
        }

        private string SanitizeForFolderName(string input)
        {
            // Remove invalid filename characters and limit length
            var invalid = Path.GetInvalidFileNameChars();
            string clean = string.Concat(input.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            clean = clean.Replace(" ", "_").Trim();

            // Limit to 50 chars
            if (clean.Length > 50)
                clean = clean.Substring(0, 50);

            return clean;
        }

        private long GetFolderSize(string folderPath)
        {
            try
            {
                var dirInfo = new DirectoryInfo(folderPath);
                return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch
            {
                return 80L * 1024 * 1024 * 1024; // Default to 80GB if can't calculate
            }
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
