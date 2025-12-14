using GWxLauncher.Domain;
using GWxLauncher.Properties;
using System;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace GWxLauncher.UI
{
    public partial class ProfileSettingsForm : Form
    {
        private readonly GameProfile _profile;

        public ProfileSettingsForm(GameProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));

            InitializeComponent();
            ThemeService.ApplyToForm(this);

            Icon = Icon.ExtractAssociatedIcon(_profile.ExecutablePath); // Use the game's icon if possible - fallback to default icon from ThemeService

            // Basic title – later we can add tabs/categories here
            Text = $"Profile Settings – {profile.Name}";

            // Wire up button handlers (designer only sets DialogResult)
            btnOk.Click += btnOk_Click;
            btnBrowseExe.Click += btnBrowseExe_Click;
            btnBrowseToolboxDll.Click += btnBrowseToolboxDll_Click;
            btnBrowsePy4GwDll.Click += btnBrowsePy4GwDll_Click;
            btnBrowseGModDll.Click += btnBrowseGModDll_Click;
            btnCancel.Click += btnCancel_Click;

            // Optional nicety
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LoadFromProfile();
        }

        // Theme remnants
        //private void ApplyTheme()
        //{
        //    BackColor = Color.FromArgb(24, 24, 28);
        //    ForeColor = Color.Gainsboro;

        //    foreach (Control c in Controls)
        //        ApplyThemeRecursive(c);
        //}

        private void ApplyThemeRecursive(Control c)
        {
            // Fix GW1 Mods group header specifically
            if (c is GroupBox gb)
            {
                gb.ForeColor = Color.Gainsboro;
                gb.BackColor = BackColor;
            }

            // Common controls
            if (c is Label lbl)
                lbl.ForeColor = Color.Gainsboro;

            if (c is TextBox tb)
            {
                tb.BackColor = Color.FromArgb(18, 18, 22);
                tb.ForeColor = Color.Gainsboro;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }

            if (c is Button btn)
            {
                btn.BackColor = Color.FromArgb(45, 45, 52);
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            }

            foreach (Control child in c.Controls)
                ApplyThemeRecursive(child);
        }

        private void LoadFromProfile()
        {
            txtProfileName.Text = _profile.Name;
            txtExecutablePath.Text = _profile.ExecutablePath;

            // Only show GW1 mod options if this is a GW1 profile
            bool isGw1 = _profile.GameType == GameType.GuildWars1;
            grpGw1Mods.Enabled = isGw1;

            if (isGw1)
            {
                chkToolbox.Checked = _profile.Gw1ToolboxEnabled;
                txtToolboxDll.Text = _profile.Gw1ToolboxDllPath;

                chkPy4Gw.Checked = _profile.Gw1Py4GwEnabled;
                txtPy4GwDll.Text = _profile.Gw1Py4GwDllPath;

                chkGMod.Checked = _profile.Gw1GModEnabled;
                txtGModDll.Text = _profile.Gw1GModDllPath;
            }
        }

        private void SaveToProfile()
        {
            _profile.Name = txtProfileName.Text.Trim();
            _profile.ExecutablePath = txtExecutablePath.Text.Trim();

            if (_profile.GameType == GameType.GuildWars1)
            {
                _profile.Gw1ToolboxEnabled = chkToolbox.Checked;
                _profile.Gw1ToolboxDllPath = txtToolboxDll.Text.Trim();

                _profile.Gw1Py4GwEnabled = chkPy4Gw.Checked;
                _profile.Gw1Py4GwDllPath = txtPy4GwDll.Text.Trim();

                _profile.Gw1GModEnabled = chkGMod.Checked;
                _profile.Gw1GModDllPath = txtGModDll.Text.Trim();
            }
        }
        private bool ValidateGw1ModSettings()
        {
            // Only relevant to GW1 profiles
            if (_profile.GameType != GameType.GuildWars1)
                return true;

            // Simple required-field checks; later we can add File.Exists if we want
            if (chkToolbox.Checked && string.IsNullOrWhiteSpace(txtToolboxDll.Text))
            {
                MessageBox.Show(
                    this,
                    "Toolbox is enabled, but no DLL path is set.\n\n" +
                    "Please browse to GWToolboxdll.dll or uncheck Toolbox.",
                    "Missing DLL path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (chkPy4Gw.Checked && string.IsNullOrWhiteSpace(txtPy4GwDll.Text))
            {
                MessageBox.Show(
                    this,
                    "Py4GW is enabled, but no DLL path is set.\n\n" +
                    "Please browse to the Py4GW DLL or uncheck Py4GW.",
                    "Missing DLL path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (chkGMod.Checked && string.IsNullOrWhiteSpace(txtGModDll.Text))
            {
                MessageBox.Show(
                    this,
                    "gMod is enabled, but no DLL path is set.\n\n" +
                    "Please browse to gMod.dll or uncheck gMod.",
                    "Missing DLL path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            return true;
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            var name = txtProfileName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(
                    this,
                    "Please enter a display name.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                // Prevent the dialog from closing
                DialogResult = DialogResult.None;
                return;
            }

            // Extra checks for GW1 mod settings
            if (!ValidateGw1ModSettings())
            {
                // Validation method already showed a message; keep the dialog open
                DialogResult = DialogResult.None;
                return;
            }

            // Push values into the profile and close as OK
            SaveToProfile();
            DialogResult = DialogResult.OK;
            Close();
        }


        private void btnBrowseExe_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select game executable"
            };

            if (File.Exists(txtExecutablePath.Text))
            {
                dlg.FileName = txtExecutablePath.Text;
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtExecutablePath.Text = dlg.FileName;
            }
        }

        private void BrowseDllInto(TextBox textBox)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = "Select DLL"
            };

            if (File.Exists(textBox.Text))
            {
                dlg.FileName = textBox.Text;
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                textBox.Text = dlg.FileName;
            }
        }


        private void btnBrowseToolboxDll_Click(object? sender, EventArgs e)
            => BrowseDllInto(txtToolboxDll);

        private void btnBrowsePy4GwDll_Click(object? sender, EventArgs e)
            => BrowseDllInto(txtPy4GwDll);

        private void btnBrowseGModDll_Click(object? sender, EventArgs e)
            => BrowseDllInto(txtGModDll);

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
