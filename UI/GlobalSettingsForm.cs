using System;
using System.IO;
using System.Windows.Forms;
using GWxLauncher.Config;

namespace GWxLauncher.UI
{
    internal sealed partial class GlobalSettingsForm : Form
    {
        private readonly LauncherConfig _cfg;
        private bool _restoredFromSavedPlacement;

        public GlobalSettingsForm()
        {
            InitializeComponent();

            _cfg = LauncherConfig.Load();
            TryRestoreSavedPlacement();
            Shown += GlobalSettingsForm_Shown;
            FormClosing += GlobalSettingsForm_FormClosing;

            // Modal dialog behavior
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LoadFromConfig();
            ThemeService.ApplyToForm(this);
        }

        private void GlobalSettingsForm_Load(object sender, EventArgs e)
        {
            // Intentionally empty (kept for designer hook)
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
        }

        private void SaveAndClose()
        {
            // Persist theme selection
            _cfg.Theme = rbDark.Checked ? "Dark" : "Light";

            // Persist DLL paths
            _cfg.LastToolboxPath = (txtToolbox.Text ?? "").Trim();
            _cfg.LastGModPath = (txtGMod.Text ?? "").Trim();
            _cfg.LastPy4GWPath = (txtPy4GW.Text ?? "").Trim();

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
            BrowseDllInto(txtPy4GW, "Select gMod DLL");
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
    }
}
