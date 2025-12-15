using GWxLauncher.Domain;
using GWxLauncher.Properties;
using System;
using System.Resources;
using System.Windows.Forms;
using GWxLauncher.Config;
using System.Drawing;

namespace GWxLauncher.UI
{
    public partial class ProfileSettingsForm : Form
    {
        private readonly GameProfile _profile;
        private readonly LauncherConfig _cfg;
        
        private bool _restoredFromSavedPlacement;

        public ProfileSettingsForm(GameProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));

            InitializeComponent();
            ThemeService.ApplyToForm(this);

            bool isGw1 = _profile.GameType == GameType.GuildWars1;
            grpGw1Mods.Visible = isGw1;
            grpGw1Mods.Enabled = isGw1;

            Icon = _profile.GameType switch
            {
                GameType.GuildWars1 => Resources.Gw1Icon,
                GameType.GuildWars2 => Resources.Gw2Icon,
                _ => Icon // fallback to whatever is already set
            };

            _cfg = LauncherConfig.Load();

            TryRestoreSavedPlacement();
            Shown += ProfileSettingsForm_Shown;
            FormClosing += ProfileSettingsForm_FormClosing;


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
        private void LoadFromProfile()
        {
            txtProfileName.Text = _profile.Name;
            txtExecutablePath.Text = _profile.ExecutablePath;

            // Only show GW1 mod options if this is a GW1 profile
            bool isGw1 = _profile.GameType == GameType.GuildWars1;
            // Only show GW2 mod options if this is a GW2 profile
            bool isGw2 = _profile.GameType == GameType.GuildWars2;

            grpGw1Mods.Visible = isGw1;
            grpGw1Mods.Enabled = isGw1;
            grpGw2RunAfter.Visible = isGw2;
            grpGw2RunAfter.Enabled = isGw2;

            chkGw1Multiclient.Checked = _cfg.Gw1MulticlientEnabled;


            if (isGw1)
            {
                chkToolbox.Checked = _profile.Gw1ToolboxEnabled;
                txtToolboxDll.Text = _profile.Gw1ToolboxDllPath;

                chkPy4Gw.Checked = _profile.Gw1Py4GwEnabled;
                txtPy4GwDll.Text = _profile.Gw1Py4GwDllPath;

                chkGMod.Checked = _profile.Gw1GModEnabled;
                txtGModDll.Text = _profile.Gw1GModDllPath;
            }
            if (isGw2)
            {
                chkGw2RunAfterEnabled.Checked = _profile.Gw2RunAfterEnabled;
                RefreshGw2RunAfterList();
            }
        }
        private void RefreshGw2RunAfterList()
        {
            lvGw2RunAfter.Items.Clear();

            foreach (var p in _profile.Gw2RunAfterPrograms ?? new List<RunAfterProgram>())
            {
                var item = new ListViewItem(p.Name);
                item.SubItems.Add(p.ExePath);
                item.Checked = p.Enabled;
                item.Tag = p;
                lvGw2RunAfter.Items.Add(item);
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

                _cfg.Gw1MulticlientEnabled = chkGw1Multiclient.Checked;
                _cfg.Save();
            }
            if (_profile.GameType == GameType.GuildWars2)
            {
                _profile.Gw2RunAfterEnabled = chkGw2RunAfterEnabled.Checked;
            }
        }
        private void ProfileSettingsForm_Shown(object? sender, EventArgs e)
        {
            // If we restored from saved placement, don't override it.
            if (_restoredFromSavedPlacement)
                return;

            // If we have an owner (MainForm shows this dialog with ShowDialog(this)), anchor near it.
            if (Owner != null)
            {
                var ownerBounds = Owner.Bounds;

                // Prefer to the right of the owner; if no space, place to the left.
                var wa = Screen.FromControl(Owner).WorkingArea;

                int gap = 12;
                int xRight = ownerBounds.Right + gap;
                int xLeft = ownerBounds.Left - gap - Width;

                int x = (xRight + Width <= wa.Right) ? xRight :
                        (xLeft >= wa.Left) ? xLeft :
                        Math.Max(wa.Left, Math.Min(xRight, wa.Right - Width));

                int y = Math.Max(wa.Top, Math.Min(ownerBounds.Top, wa.Bottom - Height));

                StartPosition = FormStartPosition.Manual;
                Location = new Point(x, y);
            }
            else
            {
                // No owner: fallback to a reasonable default
                StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void TryRestoreSavedPlacement()
        {
            if (_cfg.ProfileSettingsX >= 0 && _cfg.ProfileSettingsY >= 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_cfg.ProfileSettingsX, _cfg.ProfileSettingsY);
                _restoredFromSavedPlacement = true;
            }

            if (_cfg.ProfileSettingsWidth > 0 && _cfg.ProfileSettingsHeight > 0)
            {
                Size = new Size(_cfg.ProfileSettingsWidth, _cfg.ProfileSettingsHeight);
            }
        }

        private void ProfileSettingsForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _cfg.ProfileSettingsX = Left;
                _cfg.ProfileSettingsY = Top;
                _cfg.ProfileSettingsWidth = Width;
                _cfg.ProfileSettingsHeight = Height;
                _cfg.Save();
            }
            else
            {
                // If minimized/maximized, save RestoreBounds instead (so we persist something sane)
                var b = RestoreBounds;
                _cfg.ProfileSettingsX = b.Left;
                _cfg.ProfileSettingsY = b.Top;
                _cfg.ProfileSettingsWidth = b.Width;
                _cfg.ProfileSettingsHeight = b.Height;
                _cfg.Save();
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
        private void btnGw2AddProgram_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select program to run after launching",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            var exe = dlg.FileName;

            // Friendly name: file description if available, else file name
            string name;
            try
            {
                var vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
                name = string.IsNullOrWhiteSpace(vi.FileDescription)
                    ? Path.GetFileNameWithoutExtension(exe)
                    : vi.FileDescription.Trim();
            }
            catch
            {
                name = Path.GetFileNameWithoutExtension(exe);
            }

            _profile.Gw2RunAfterPrograms.Add(new RunAfterProgram
            {
                Name = name,
                ExePath = exe,
                Enabled = true
            });

            RefreshGw2RunAfterList();
        }

        private void btnGw2RemoveProgram_Click(object sender, EventArgs e)
        {
            if (lvGw2RunAfter.SelectedItems.Count == 0)
                return;

            var item = lvGw2RunAfter.SelectedItems[0];
            if (item.Tag is RunAfterProgram p)
            {
                _profile.Gw2RunAfterPrograms.Remove(p);
                RefreshGw2RunAfterList();
            }
        }
        private void lvGw2RunAfter_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is RunAfterProgram p)
                p.Enabled = e.Item.Checked;
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
