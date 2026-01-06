using GWxLauncher.Config;
using GWxLauncher.Domain;

namespace GWxLauncher.UI
{
    internal sealed partial class GlobalSettingsForm : Form
    {
        private readonly LauncherConfig _cfg;
        private readonly Services.ProfileManager _profileManager;
        private bool _restoredFromSavedPlacement;
        public event EventHandler? ImportCompleted;
        public event EventHandler? ProfilesBulkUpdated;

        public GlobalSettingsForm(Services.ProfileManager? profileManager = null)
        {
            InitializeComponent();

            _cfg = LauncherConfig.Load();
            _profileManager = profileManager ?? new Services.ProfileManager();

            // If we created it locally, make sure it’s loaded for Step 4 actions.
            // (MainForm’s instance is already loaded, so this is cheap/no-op for most cases.)
            if (_profileManager.Profiles.Count == 0)
                _profileManager.Load();

            TryRestoreSavedPlacement();
            Shown += GlobalSettingsForm_Shown;
            FormClosing += GlobalSettingsForm_FormClosing;

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LoadFromConfig();

            rbDark.CheckedChanged += ThemeRadio_CheckedChanged;
            rbLight.CheckedChanged += ThemeRadio_CheckedChanged;

            ThemeService.ApplyToForm(this);
        }

        private void GlobalSettingsForm_Load(object sender, EventArgs e)
        {
            // Intentionally empty (kept for designer hook)
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
        private void btnApplyGlobalFlags_Click(object sender, EventArgs e)
        {
            // Ensure we use the current on-screen values (not stale config).
            CommitUiToConfigAndSave();

            var pm = _profileManager;
            var targets = pm.Profiles.Where(p => p.GameType == GameType.GuildWars1).ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show(this, "No Guild Wars 1 profiles found.", "Nothing to do");
                return;
            }

            if (!ConfirmBulkApply("Apply global mod enable/disable state to all Guild Wars 1 profiles.", targets.Count))
                return;

            foreach (var p in targets)
            {
                p.Gw1ToolboxEnabled = _cfg.GlobalToolboxEnabled;
                p.Gw1Py4GwEnabled = _cfg.GlobalPy4GwEnabled;
                p.Gw1GModEnabled = _cfg.GlobalGModEnabled;
            }

            pm.Save();

            MessageBox.Show(this, $"Updated {targets.Count} profile(s).", "Done");
            ProfilesBulkUpdated?.Invoke(this, EventArgs.Empty);
        }
        private void btnApplyGlobalPaths_Click(object sender, EventArgs e)
        {
            // Ensure we use the current on-screen values (not stale config).
            CommitUiToConfigAndSave();

            var pm = _profileManager;
            var targets = pm.Profiles.Where(p => p.GameType == GameType.GuildWars1).ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show(this, "No Guild Wars 1 profiles found.", "Nothing to do");
                return;
            }

            if (!ConfirmBulkApply("Apply global DLL paths to all Guild Wars 1 profiles.", targets.Count))
                return;

            foreach (var p in targets)
            {
                if (!string.IsNullOrWhiteSpace(_cfg.LastToolboxPath))
                    p.Gw1ToolboxDllPath = _cfg.LastToolboxPath;

                if (!string.IsNullOrWhiteSpace(_cfg.LastPy4GWPath))
                    p.Gw1Py4GwDllPath = _cfg.LastPy4GWPath;

                if (!string.IsNullOrWhiteSpace(_cfg.LastGModPath))
                    p.Gw1GModDllPath = _cfg.LastGModPath;
            }

            pm.Save();

            MessageBox.Show(this, $"Updated {targets.Count} profile(s).", "Done");
            ProfilesBulkUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void LoadFromConfig()
        {
            // Theme
            var t = (_cfg.Theme ?? "Light").Trim();
            bool isDark = string.Equals(t, "Dark", StringComparison.OrdinalIgnoreCase);
            rbDark.Checked = isDark;
            rbLight.Checked = !isDark;

            // DLL paths (last known good)
            txtToolbox.Text = (_cfg.LastToolboxPath ?? "").Trim();
            txtGMod.Text = (_cfg.LastGModPath ?? "").Trim();
            txtPy4GW.Text = (_cfg.LastPy4GWPath ?? "").Trim();
            // Global mod kill-switches
            cbGlobalToolbox.Checked = _cfg.GlobalToolboxEnabled;
            cbGlobalPy4Gw.Checked = _cfg.GlobalPy4GwEnabled;
            cbGlobalGMod.Checked = _cfg.GlobalGModEnabled;

        }

        private void SaveAndClose()
        {
            // Persist theme selection
            _cfg.Theme = rbDark.Checked ? "Dark" : "Light";

            // Persist DLL paths
            _cfg.LastToolboxPath = (txtToolbox.Text ?? "").Trim();
            _cfg.LastGModPath = (txtGMod.Text ?? "").Trim();
            _cfg.LastPy4GWPath = (txtPy4GW.Text ?? "").Trim();
            // Persist global mod kill-switches (runtime gating only; does not modify profiles)
            _cfg.GlobalToolboxEnabled = cbGlobalToolbox.Checked;
            _cfg.GlobalPy4GwEnabled = cbGlobalPy4Gw.Checked;
            _cfg.GlobalGModEnabled = cbGlobalGMod.Checked;

            _cfg.Save();

            // Apply theme immediately
            ThemeService.SetTheme(rbDark.Checked ? AppTheme.Dark : AppTheme.Light);

            foreach (Form f in Application.OpenForms)
            {
                ThemeService.ApplyToForm(f);
                f.Invalidate(true);
                f.Refresh();
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void TryRestoreSavedPlacement()
        {
            if (_cfg.GlobalSettingsX >= 0 && _cfg.GlobalSettingsY >= 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_cfg.GlobalSettingsX, _cfg.GlobalSettingsY);
                _restoredFromSavedPlacement = true;
            }

            if (_cfg.GlobalSettingsWidth > 0 && _cfg.GlobalSettingsHeight > 0)
            {
                Size = new Size(_cfg.GlobalSettingsWidth, _cfg.GlobalSettingsHeight);
            }
        }

        private void GlobalSettingsForm_Shown(object? sender, EventArgs e)
        {
            // If we restored from saved placement, don't override it.
            if (_restoredFromSavedPlacement)
                return;

            // Match ProfileSettingsForm behavior: anchor near Owner when possible.
            if (Owner != null)
            {
                var ownerBounds = Owner.Bounds;
                var wa = Screen.FromControl(Owner).WorkingArea;

                int gap = 12;
                int xRight = ownerBounds.Right + gap;
                int xLeft = ownerBounds.Left - gap - Width;

                int x =
                    (xRight + Width <= wa.Right) ? xRight :
                    (xLeft >= wa.Left) ? xLeft :
                    Math.Max(wa.Left, Math.Min(xRight, wa.Right - Width));

                int y = Math.Max(wa.Top, Math.Min(ownerBounds.Top, wa.Bottom - Height));

                StartPosition = FormStartPosition.Manual;
                Location = new Point(x, y);
            }
            else
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void GlobalSettingsForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _cfg.GlobalSettingsX = Left;
                _cfg.GlobalSettingsY = Top;
                _cfg.GlobalSettingsWidth = Width;
                _cfg.GlobalSettingsHeight = Height;
                _cfg.Save();
            }
            else
            {
                var b = RestoreBounds;
                _cfg.GlobalSettingsX = b.Left;
                _cfg.GlobalSettingsY = b.Top;
                _cfg.GlobalSettingsWidth = b.Width;
                _cfg.GlobalSettingsHeight = b.Height;
                _cfg.Save();
            }
        }

        private void btnImportAccountsJson_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select accounts.json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var importer = new Services.AccountsJsonImportService();
                var result = importer.ImportFromFile(dlg.FileName, _cfg);

                if (result.ImportedCount == 0)
                {
                    MessageBox.Show(
                        this,
                        "No accounts were imported.\n\nThe file may be empty or missing required fields like gw_path.",
                        "Import complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Post-import prompt: only if source wanted tools enabled but we don't know the DLLs yet.
                bool needsAny =
                    result.MissingToolboxPath || result.MissingGModPath || result.MissingPy4GwPath;

                if (needsAny)
                {
                    var msg =
                        "This accounts file indicates one or more injection tools are enabled, " +
                        "but GWxLauncher doesn’t know where those DLLs are yet.\n\n" +
                        "Select them now?";

                    var pickNow = MessageBox.Show(
                        this,
                        msg,
                        "Missing DLL paths",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes;

                    if (pickNow)
                    {
                        // Let user pick missing DLLs, update config + UI immediately
                        if (result.MissingToolboxPath)
                            PickDllIntoGlobalSetting(txtToolbox, "Select Toolbox DLL", v => _cfg.LastToolboxPath = v);

                        if (result.MissingGModPath)
                            PickDllIntoGlobalSetting(txtGMod, "Select gMod DLL", v => _cfg.LastGModPath = v);

                        if (result.MissingPy4GwPath)
                            PickDllIntoGlobalSetting(txtPy4GW, "Select Py4GW DLL", v => _cfg.LastPy4GWPath = v);

                        _cfg.Save();

                        // Re-apply tool enables for the just-imported profiles now that paths exist
                        importer.ApplyNewlySelectedDllPathsToImportedProfiles(
                            result.ImportedProfileIds,
                            result.ToolWantsByProfileId,
                            _cfg);
                    }
                }
                ImportCompleted?.Invoke(this, EventArgs.Empty);
                MessageBox.Show(
                    this,
                    $"Imported {result.ImportedCount} profile(s).\n\n" +
                    "Imported profiles start unchecked in all views.",
                    "Import complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Import failed:\n\n{ex.Message}",
                    "Import error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void PickDllIntoGlobalSetting(TextBox target, string title, Action<string> setConfigValue)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = title
            };

            // best-effort starting folder
            var current = (target.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(current) && File.Exists(current))
            {
                dlg.FileName = current;
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                target.Text = dlg.FileName;
                setConfigValue(dlg.FileName);
            }
        }
                private void CommitUiToConfigAndSave()
        {
            // Persist theme selection (in config) — saving is needed so bulk-apply uses current values.
            _cfg.Theme = rbDark.Checked ? "Dark" : "Light";

            // Persist DLL paths
            _cfg.LastToolboxPath = (txtToolbox.Text ?? "").Trim();
            _cfg.LastGModPath = (txtGMod.Text ?? "").Trim();
            _cfg.LastPy4GWPath = (txtPy4GW.Text ?? "").Trim();

            // Persist global mod kill-switches
            _cfg.GlobalToolboxEnabled = cbGlobalToolbox.Checked;
            _cfg.GlobalPy4GwEnabled = cbGlobalPy4Gw.Checked;
            _cfg.GlobalGModEnabled = cbGlobalGMod.Checked;

            _cfg.Save();
        }
        private void ThemeRadio_CheckedChanged(object? sender, EventArgs e)
        {
            // Only act when the radio is becoming checked (avoid double-firing on uncheck)
            if (sender is RadioButton rb && !rb.Checked)
                return;

            var theme = rbDark.Checked ? AppTheme.Dark : AppTheme.Light;

            // Live preview (do NOT save here; OK will persist)
            ThemeService.SetTheme(theme);

            foreach (Form f in Application.OpenForms)
            {
                ThemeService.ApplyToForm(f);
                f.Invalidate(true);
                f.Refresh();
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SaveAndClose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnBrowseToolbox_Click(object sender, EventArgs e)
        {
            BrowseDllInto(txtToolbox, "Select Toolbox DLL");
        }

        private void btnBrowseGMod_Click(object sender, EventArgs e)
        {
            BrowseDllInto(txtGMod, "Select gMod DLL");
        }

        private void btnBrowsePy4GW_Click(object sender, EventArgs e)
        {
            BrowseDllInto(txtPy4GW, "Select Py4GW DLL");
        }

        private void BrowseDllInto(TextBox target, string title)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = title
            };

            var current = (target.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(current) && File.Exists(current))
            {
                dlg.FileName = current;
                try
                {
                    var dir = Path.GetDirectoryName(current);
                    if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                        dlg.InitialDirectory = dir;
                }
                catch { /* best-effort */ }
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
                target.Text = dlg.FileName;
        }

        private void GlobalSettingsForm_Load_1(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
