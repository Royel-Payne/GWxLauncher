using GWxLauncher.Config;
using GWxLauncher.Services;
using GWxLauncher.UI.Helpers;

namespace GWxLauncher.UI.TabControls
{
    public partial class GlobalGeneralTabContent : UserControl
    {
        private LauncherConfig _cfg;
        public event EventHandler? ImportCompleted;

        public GlobalGeneralTabContent()
        {
            InitializeComponent();
            ApplyTheme();
            
            rbDark.CheckedChanged += ThemeRadio_CheckedChanged;
            rbLight.CheckedChanged += ThemeRadio_CheckedChanged;
            btnImportAccountsJson.Click += btnImportAccountsJson_Click;
        }

        internal void BindConfig(LauncherConfig cfg)
        {
            _cfg = cfg;
            
            // Theme
            var t = (_cfg.Theme ?? "Light").Trim();
            bool isDark = string.Equals(t, "Dark", StringComparison.OrdinalIgnoreCase);
            rbDark.Checked = isDark;
            rbLight.Checked = !isDark;
        }

        internal void SaveConfig(LauncherConfig cfg)
        {
            // Theme is live-updated mostly, but we save the selection
            cfg.Theme = rbDark.Checked ? "Dark" : "Light";
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

        private void ThemeRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is RadioButton rb && !rb.Checked) return;

            var theme = rbDark.Checked ? AppTheme.Dark : AppTheme.Light;
            ThemeService.SetTheme(theme);

            foreach (Form f in Application.OpenForms)
            {
                ThemeService.ApplyToForm(f);
                f.Invalidate(true);
                f.Refresh();
            }
        }

        private void btnImportAccountsJson_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select accounts.json",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var importer = new AccountsJsonImportService();
                var result = importer.ImportFromFile(dlg.FileName, _cfg);

                if (result.ImportedCount == 0)
                {
                    MessageBox.Show(this, "No accounts were imported.\n\nThe file may be empty or missing required fields.", "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                bool needsAny = result.MissingToolboxPath || result.MissingGModPath || result.MissingPy4GwPath;

                if (needsAny)
                {
                    if (MessageBox.Show(this, "Imported settings enable tools but paths are missing. Select DLLs now?", "Missing DLL paths", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        if (result.MissingToolboxPath) PickGlobalDll(v => _cfg.LastToolboxPath = v, "Select Toolbox DLL");
                        if (result.MissingGModPath) PickGlobalDll(v => _cfg.LastGModPath = v, "Select gMod DLL");
                        if (result.MissingPy4GwPath) PickGlobalDll(v => _cfg.LastPy4GWPath = v, "Select Py4GW DLL");
                        
                        _cfg.Save();
                        importer.ApplyNewlySelectedDllPathsToImportedProfiles(result.ImportedProfileIds, result.ToolWantsByProfileId, _cfg);
                    }
                }
                
                ImportCompleted?.Invoke(this, EventArgs.Empty);
                MessageBox.Show(this, $"Imported {result.ImportedCount} profile(s).", "Import complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Import failed:\n\n{ex.Message}", "Import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PickGlobalDll(Action<string> setVal, string title)
        {
            // We can't easily reference the textboxes in other tabs, so we just use a temp control or just pick directly.
            // But wait, the previous code updated the UI textboxes too. 
            // In this refactor, GlobalGw1TabContent has the textboxes. 
            // We are updating the CONFIG directly here. The other tab will need to refresh from Config if it's open?
            // BindConfig on the other tab reads from Config.
            // If the user hasn't saved yet, Config might be stale? 
            // Actually _cfg is the shared object reference. So updating _cfg properties works.
            
            // To pick a file we need a control to anchor the dialog if using FilePickerHelper?
            // FilePickerHelper uses IWin32Window owner. 'this' works.
            // It also takes a TextBox. We don't have one here for these DLLs. 
            // We can overload FilePickerHelper or just use OpenFileDialog directly here since we don't have a textbox to update visually on THIS tab.
            
            using var dlg = new OpenFileDialog { Title = title, Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                setVal(dlg.FileName);
            }
        }
    }
}
