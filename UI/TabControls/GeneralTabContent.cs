using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;

namespace GWxLauncher.UI.TabControls
{
    /// <summary>
    /// UserControl for the General tab in ProfileSettingsForm.
    /// Contains profile name, executable path, launch arguments, and multiclient settings.
    /// </summary>
    public partial class GeneralTabContent : UserControl
    {
        private GameProfile _profile;
        private LauncherConfig _cfg;

        // Added to support ProfileSettingsForm initialization
        public GameType GameType
        {
            get => _profile?.GameType ?? GameType.GuildWars1;
            set
            {
                // Visibility logic is now handled in BindProfile. 
                // If needed, we could store it for initial state, but BindProfile is called shortly after.
            }
        }

        public GeneralTabContent()
        {
            InitializeComponent();
            ApplyTheme();

            this.btnBrowseExe.Click += btnBrowseExe_Click;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
        }

        public void BindProfile(GameProfile profile)
        {
            _profile = profile;
            _cfg = LauncherConfig.Load(); // Load config independently

            txtProfileName.Text = profile.Name;

            // Executable
            txtExecutable.Text = profile.ExecutablePath;

            // Args
            txtArgs.Text = profile.LaunchArguments;

            // Window Title
            bool isGw1 = profile.GameType == GameType.GuildWars1;
            bool isGw2 = profile.GameType == GameType.GuildWars2;

            // Show window title for both GW1 and GW2
            if (isGw1)
            {
                txtWindowTitle.Text = profile.Gw1WindowTitleLabel;
                lblWindowTitle.Visible = true;
                txtWindowTitle.Visible = true;

                chkMulticlient.Visible = true;
                chkMulticlient.Checked = _cfg.Gw1MulticlientEnabled;
            }
            else if (isGw2)
            {
                txtWindowTitle.Text = profile.Gw2WindowTitleLabel;
                lblWindowTitle.Visible = true;
                txtWindowTitle.Visible = true;

                chkMulticlient.Visible = true;
                chkMulticlient.Checked = _cfg.Gw2MulticlientEnabled;
            }
            else
            {
                lblWindowTitle.Visible = false;
                txtWindowTitle.Visible = false;
                chkMulticlient.Visible = false;
            }
        }

        public void SaveProfile(GameProfile profile)
        {
            profile.Name = txtProfileName.Text.Trim();
            profile.ExecutablePath = txtExecutable.Text.Trim();
            profile.LaunchArguments = txtArgs.Text.Trim();

            if (profile.GameType == GameType.GuildWars1)
            {
                string label = txtWindowTitle.Text.Trim();
                profile.Gw1WindowTitleLabel = string.IsNullOrWhiteSpace(label) ? null : label;

                // Save config multiclient
                _cfg.Gw1MulticlientEnabled = chkMulticlient.Checked;
            }
            else if (profile.GameType == GameType.GuildWars2)
            {
                string label = txtWindowTitle.Text.Trim();
                profile.Gw2WindowTitleLabel = string.IsNullOrWhiteSpace(label) ? null : label;

                _cfg.Gw2MulticlientEnabled = chkMulticlient.Checked;
            }

            _cfg.Save();
        }

        private void btnBrowseExe_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select game executable"
            };

            var current = txtExecutable.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(current) && File.Exists(current))
            {
                dlg.FileName = current;
                try
                {
                    var dir = Path.GetDirectoryName(current);
                    if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                        dlg.InitialDirectory = dir;
                }
                catch { }
            }
            else
            {
                // Fallback to config path or Program Files
                var cfgPath = _profile.GameType == GameType.GuildWars1 ? _cfg.Gw1Path : _cfg.Gw2Path;
                if (!string.IsNullOrWhiteSpace(cfgPath) && File.Exists(cfgPath))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(cfgPath);
                }
                else
                {
                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Duplicate check
                if (IsDuplicateGw1Executable(dlg.FileName))
                {
                    MessageBox.Show(
                        this,
                        "This Guild Wars executable is already used by another profile.\n\n" +
                        "Multiple GW1 instances require separate game folders.\n\n" +
                        "Please select a different Gw.exe.",
                        "Duplicate GW1 executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Protected path check
                if (ProtectedInstallPathPolicy.IsProtectedPath(dlg.FileName))
                {
                    // Assuming ProtectedInstallPathWarningDialog is available
                    if (!ProtectedInstallPathWarningDialog.ConfirmContinue(this.FindForm(), dlg.FileName))
                        return;
                }

                txtExecutable.Text = dlg.FileName;
            }
        }

        private bool IsDuplicateGw1Executable(string selectedExePath)
        {
            if (_profile == null || _profile.GameType != GameType.GuildWars1)
                return false;

            string selectedFull = Path.GetFullPath(selectedExePath);

            var pm = new ProfileManager();
            pm.Load();

            return pm.Profiles.Any(p =>
                p.Id != _profile.Id &&
                p.GameType == GameType.GuildWars1 &&
                !string.IsNullOrWhiteSpace(p.ExecutablePath) &&
                string.Equals(Path.GetFullPath(p.ExecutablePath), selectedFull, StringComparison.OrdinalIgnoreCase));
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }
    }
}
