using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI.Helpers;

namespace GWxLauncher.UI.TabControls
{
    public partial class GlobalGw1TabContent : UserControl
    {
        private LauncherConfig _cfg;
        private ProfileManager _profileManager;
        
        public event EventHandler ProfilesBulkUpdated;

        public GlobalGw1TabContent()
        {
            InitializeComponent();
            ApplyTheme();
            
            // Ensure the control autosizes to its contents so the parent AutoScroll panel knows to scroll.
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // Add padding at the bottom to ensure the last buttons aren't cut off by the scroll view
            this.Padding = new Padding(0, 0, 0, 64);

            cbGw1RenameWindowTitle.CheckedChanged += (s, e) => txtGw1TitleTemplate.Enabled = cbGw1RenameWindowTitle.Checked;
            
            btnBrowseToolbox.Click += (s, e) => BrowseDllInto(txtToolbox, "Select Toolbox DLL");
            btnBrowsePy4GW.Click += (s, e) => BrowseDllInto(txtPy4GW, "Select Py4GW DLL");
            btnBrowseGMod.Click += (s, e) => BrowseDllInto(txtGMod, "Select gMod DLL");

            btnApplyGlobalFlags.Click += btnApplyGlobalFlags_Click;
            btnApplyGlobalPaths.Click += btnApplyGlobalPaths_Click;
        }

        internal void BindConfig(LauncherConfig cfg, ProfileManager profileManager)
        {
            _cfg = cfg;
            _profileManager = profileManager;

            // Window Title
            cbGw1RenameWindowTitle.Checked = _cfg.Gw1WindowTitleEnabled;
            txtGw1TitleTemplate.Text = string.IsNullOrWhiteSpace(_cfg.Gw1WindowTitleTemplate) 
                ? "GW1 · {ProfileName}" 
                : _cfg.Gw1WindowTitleTemplate;
            txtGw1TitleTemplate.Enabled = cbGw1RenameWindowTitle.Checked;

            // DLLs
            string tbPath = (_cfg.LastToolboxPath ?? "").Trim();
            if (string.IsNullOrEmpty(tbPath))
            {
                var p = _profileManager.Profiles.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Gw1ToolboxDllPath));
                if (p != null) tbPath = p.Gw1ToolboxDllPath.Trim();
            }
            txtToolbox.Text = tbPath;

            string pyPath = (_cfg.LastPy4GWPath ?? "").Trim();
            if (string.IsNullOrEmpty(pyPath))
            {
                var p = _profileManager.Profiles.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Gw1Py4GwDllPath));
                if (p != null) pyPath = p.Gw1Py4GwDllPath.Trim();
            }
            txtPy4GW.Text = pyPath;

            string gmPath = (_cfg.LastGModPath ?? "").Trim();
            if (string.IsNullOrEmpty(gmPath))
            {
                var p = _profileManager.Profiles.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Gw1GModDllPath));
                if (p != null) gmPath = p.Gw1GModDllPath.Trim();
            }
            txtGMod.Text = gmPath;

            // Kill-switches
            cbGlobalToolbox.Checked = _cfg.GlobalToolboxEnabled;
            cbGlobalPy4Gw.Checked = _cfg.GlobalPy4GwEnabled;
            cbGlobalGMod.Checked = _cfg.GlobalGModEnabled;
        }

        internal void SaveConfig(LauncherConfig cfg)
        {
            cfg.Gw1WindowTitleEnabled = cbGw1RenameWindowTitle.Checked;
            
            var tpl = (txtGw1TitleTemplate.Text ?? "").Trim();
            cfg.Gw1WindowTitleTemplate = string.IsNullOrWhiteSpace(tpl) ? "GW1 · {ProfileName}" : tpl;

            cfg.LastToolboxPath = (txtToolbox.Text ?? "").Trim();
            cfg.LastPy4GWPath = (txtPy4GW.Text ?? "").Trim();
            cfg.LastGModPath = (txtGMod.Text ?? "").Trim();

            cfg.GlobalToolboxEnabled = cbGlobalToolbox.Checked;
            cfg.GlobalPy4GwEnabled = cbGlobalPy4Gw.Checked;
            cfg.GlobalGModEnabled = cbGlobalGMod.Checked;
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

        private void BrowseDllInto(TextBox target, string title)
        {
            _ = FilePickerHelper.TryPickDll(this.FindForm(), target, title);
        }

        private bool ConfirmBulkApply(string action, int count)
        {
            return MessageBox.Show(
                this,
                $"This will update {count} profile(s).\n\n{action}\n\nContinue?",
                "Confirm bulk update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void btnApplyGlobalFlags_Click(object? sender, EventArgs e)
        {
            // The textboxes/checkboxes on screen might have changed, but maybe not saved to _cfg yet.
            // BindConfig/SaveConfig cycle is usually triggered by parent.
            // We should push current UI state to config or just use UI state directly.
            // But for bulk updating PROFILES, we are taking the "Global Enable State" from THIS UI and sticking it into the PROFILES.
            
            var targets = _profileManager.Profiles.Where(p => p.GameType == GameType.GuildWars1).ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show(this, "No Guild Wars 1 profiles found.", "Nothing to do");
                return;
            }

            if (!ConfirmBulkApply("Apply these Enable/Disable states to all Guild Wars 1 profiles?", targets.Count))
                return;

            foreach (var p in targets)
            {
                p.Gw1ToolboxEnabled = cbGlobalToolbox.Checked;
                p.Gw1Py4GwEnabled = cbGlobalPy4Gw.Checked;
                p.Gw1GModEnabled = cbGlobalGMod.Checked;
            }

            _profileManager.Save();
            ProfilesBulkUpdated?.Invoke(this, EventArgs.Empty);
            MessageBox.Show(this, $"Updated {targets.Count} profile(s).", "Done");
        }

        private void btnApplyGlobalPaths_Click(object? sender, EventArgs e)
        {
            var targets = _profileManager.Profiles.Where(p => p.GameType == GameType.GuildWars1).ToList();
            if (targets.Count == 0)
            {
                MessageBox.Show(this, "No Guild Wars 1 profiles found.", "Nothing to do");
                return;
            }

            if (!ConfirmBulkApply("Apply these DLL paths to all Guild Wars 1 profiles?", targets.Count))
                return;

            string tb = (txtToolbox.Text ?? "").Trim();
            string py = (txtPy4GW.Text ?? "").Trim();
            string gm = (txtGMod.Text ?? "").Trim();

            foreach (var p in targets)
            {
                if (!string.IsNullOrWhiteSpace(tb)) p.Gw1ToolboxDllPath = tb;
                if (!string.IsNullOrWhiteSpace(py)) p.Gw1Py4GwDllPath = py;
                if (!string.IsNullOrWhiteSpace(gm)) p.Gw1GModDllPath = gm;
            }

            _profileManager.Save();
            ProfilesBulkUpdated?.Invoke(this, EventArgs.Empty);
            MessageBox.Show(this, $"Updated {targets.Count} profile(s).", "Done");
        }
    }
}
