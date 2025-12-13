using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Collections.Generic;
using System.Text.Json;

namespace GWxLauncher
{
    public partial class MainForm : Form
    {

        private LauncherConfig _config;
        private readonly ProfileManager _profileManager = new();

        private readonly Image _gw1Image = Properties.Resources.Gw1;
        private readonly Image _gw2Image = Properties.Resources.Gw2;

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_DONOTROUND = 1;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private LaunchReport? _lastLaunchReport;

        private readonly ViewStateStore _views = new();
        private bool _showCheckedOnly = false;
        private string _viewNameBeforeEdit = "";
        private bool _viewNameDirty = false;
        private bool _suppressViewTextEvents = false;
        private bool _suppressArmBulkEvents = false;



        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        private void RefreshProfileList()
        {
            lstProfiles.Items.Clear();

            var profiles = _profileManager.Profiles
                .OrderBy(p => p.GameType) // GW1 before GW2
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .AsEnumerable();

            // Visibility is controlled ONLY by Show Checked Accounts Only (view-scoped)
            if (_showCheckedOnly)
            {
                profiles = profiles.Where(p => _views.IsEligible(_views.ActiveViewName, p.Id));
            }

            foreach (var profile in profiles)
                lstProfiles.Items.Add(profile);

            UpdateBulkArmingUi();
        }


        private void UpdateBulkArmingUi()
        {
            // Always allow toggling this checkbox. It is a visibility control (UI-only).
            chkArmBulk.Enabled = true;

            bool anyEligible = _views.AnyEligibleInActiveView(_profileManager.Profiles);

            // Bulk armed (future) is a derived state: (any eligible) + (show checked only)
            bool armed = anyEligible && _showCheckedOnly;

            btnLaunchAll.Enabled = armed;

            if (armed)
                lblStatus.Text = $"Bulk launch armed · View: {_views.ActiveViewName}";
            else if (_showCheckedOnly && !anyEligible)
                lblStatus.Text = $"No checked profiles in view · View: {_views.ActiveViewName}";
        }
        private void chkArmBulk_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressArmBulkEvents)
                return;

            _showCheckedOnly = chkArmBulk.Checked;

            _views.SetShowCheckedOnly(_views.ActiveViewName, _showCheckedOnly);
            _views.Save();

            RefreshProfileList();
        }


        public MainForm()
        {
            InitializeComponent();
            chkArmBulk.BringToFront(); // ensure it's not obscured

            ctxProfiles.Opening += ctxProfiles_Opening;

            _config = LauncherConfig.Load();

            // Try to enable dark title bar (immersive dark mode)
            try
            {
                // 17763 = Windows 10 1809+, where this flag first appeared
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    int useDark = 1;
                    DwmSetWindowAttribute(
                        Handle,
                        DWMWA_USE_IMMERSIVE_DARK_MODE,
                        ref useDark,
                        sizeof(int));
                }

                // Already have: disable rounded corners on Win11+
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                {
                    int preference = DWMWCP_DONOTROUND;
                    DwmSetWindowAttribute(
                        Handle,
                        DWMWA_WINDOW_CORNER_PREFERENCE,
                        ref preference,
                        sizeof(int));
                }
            }
            catch
            {
                // If DWM isn't available for any reason, just fall back gracefully
            }

            // Basic dark mode – centralized so we can later swap palettes
            BackColor = Color.FromArgb(24, 24, 28);
            ForeColor = Color.Gainsboro;

            panelProfiles.BackColor = BackColor;
            panelProfiles.BorderStyle = BorderStyle.None;

            lstProfiles.BackColor = BackColor;
            lstProfiles.BorderStyle = BorderStyle.None;

            lblStatus.ForeColor = Color.Gainsboro;

            // Make the list wider than the panel so the scrollbar is clipped out of view
            if (lstProfiles.Parent == panelProfiles)
            {
                int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
                lstProfiles.Width = panelProfiles.Width + scrollbarWidth;
                lstProfiles.Height = panelProfiles.Height; // fill panel vertically
            }


            // Ensure Add Account header matches the theme
            btnAddAccount.BackColor = Color.FromArgb(45, 45, 52);
            btnAddAccount.ForeColor = Color.White;
            btnAddAccount.FlatStyle = FlatStyle.Flat;
            btnAddAccount.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnAddAccount.FlatAppearance.BorderSize = 1;

            // (your window-position restore logic)
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
            _views.Load();

            _suppressViewTextEvents = true;
            txtView.Text = _views.ActiveViewName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;



            RefreshProfileList();

        }

        private void lstProfiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= lstProfiles.Items.Count)
                return;

            var profile = lstProfiles.Items[e.Index] as GameProfile;

            if (profile == null)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Card bounds
            Rectangle card = e.Bounds;
            card.Inflate(-4, -4);

            Color backColor = selected
                // slightly lighter neutral when selected (no bright blue fog)
                ? Color.FromArgb(45, 45, 52)
                : Color.FromArgb(35, 35, 40);

            Color borderColor = selected
                // blue only on the outline to show selection
                ? Color.FromArgb(0, 120, 215)
                : Color.FromArgb(70, 70, 80);

            Color nameColor = Color.White;
            Color subColor = Color.Silver;

            using (var bgBrush = new SolidBrush(backColor))
            using (var borderPen = new Pen(borderColor))
            {
                g.FillRectangle(bgBrush, card);
                g.DrawRectangle(borderPen, card);
            }
            // Eligibility checkbox area (bulk launch eligibility; not selection)
            Rectangle cb = new Rectangle(card.Left + 8, card.Top + 22, 16, 16);
            bool eligible = _views.IsEligible(_views.ActiveViewName, profile.Id);
            CheckBoxState cbState = eligible ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
            CheckBoxRenderer.DrawCheckBox(g, cb.Location, cbState);

            // Icon area
            Rectangle iconRect = new Rectangle(card.Left + 30, card.Top + 8, 32, 32);
            Image? icon = profile.GameType == GameType.GuildWars1 ? _gw1Image : _gw2Image;
            if (icon != null)
            {
                g.DrawImage(icon, iconRect);
            }

            // Text area
            float textLeft = iconRect.Right + 8;
            float textTop = card.Top + 10;

            // base font: always non-null
            var baseFont = e.Font ?? this.Font;

            // Try a modern variable font, fall back if it doesn't exist
            Font nameFont;
            Font subFont;

            try
            {
                nameFont = new Font("Segoe UI Variable", baseFont.Size + 1, FontStyle.Bold);
                subFont = new Font("Segoe UI Variable", baseFont.Size - 1, FontStyle.Regular);
            }
            catch
            {
                nameFont = new Font(baseFont.FontFamily, baseFont.Size + 1, FontStyle.Bold);
                subFont = new Font(baseFont.FontFamily, baseFont.Size - 1, FontStyle.Regular);
            }

            // slightly lighter gray for the sublabel
            // Color subColor = Color.FromArgb(180, 180, 190);

            using (nameFont)
            using (var nameBrush = new SolidBrush(nameColor))
            using (subFont)
            using (var subBrush = new SolidBrush(subColor))
            {
                // primary line: display name
                g.DrawString(profile.Name, nameFont, nameBrush, textLeft, textTop);

                // secondary line: game label
                string gameLabel = profile.GameType == GameType.GuildWars1
                    ? "Guild Wars 1"
                    : "Guild Wars 2";

                g.DrawString(
                    gameLabel,
                    subFont,
                    subBrush,
                    textLeft,
                    textTop + nameFont.Height + 2);
            }



            if (selected)
            {
                e.DrawFocusRectangle();
            }
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
        private void ctxProfiles_Opening(object? sender, CancelEventArgs e)
        {
            var profile = GetSelectedProfile();
            bool isGw1 = profile != null && profile.GameType == GameType.GuildWars1;

            // Enable/disable GW1-specific items
            menuGw1ToolboxToggle.Enabled = isGw1;
            menuGw1ToolboxPath.Enabled = isGw1;

            // NEW: only enabled if we have a report this session
            menuShowLastLaunchDetails.Enabled = _lastLaunchReport != null;

            // Reflect current state in the checkbox
            if (isGw1 && profile != null)
            {
                menuGw1ToolboxToggle.Checked = profile.Gw1ToolboxEnabled;
            }
            else
            {
                menuGw1ToolboxToggle.Checked = false;
            }
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
        private bool TrySelectGw1ToolboxDll(GameProfile profile)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select GW1 Toolbox DLL for {profile.Name}";
                dialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(profile.Gw1ToolboxDllPath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(profile.Gw1ToolboxDllPath);
                    }
                    catch
                    {
                        // ignore bad paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    profile.Gw1ToolboxDllPath = dialog.FileName;
                    _profileManager.Save();
                    lblStatus.Text = $"Set GW1 Toolbox DLL for {profile.Name}.";
                    return true;
                }

                lblStatus.Text = $"No Toolbox DLL selected for {profile.Name}.";
                return false;
            }
        }

        private void LaunchProfile(GameProfile profile)
        {
            if (profile == null)
                return;

            // Resolve executable path: per-profile override first, then global config
            string exePath = profile.ExecutablePath;

            if (string.IsNullOrWhiteSpace(exePath))
            {
                exePath = profile.GameType == GameType.GuildWars1
                    ? _config.Gw1Path
                    : _config.Gw2Path;
            }

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show(
                    "No valid executable path is configured for this profile.\n\n" +
                    "Edit the profile or configure the game path in settings.",
                    "Missing executable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            var gameName = profile.GameType == GameType.GuildWars1
                ? "Guild Wars 1"
                : "Guild Wars 2";

            // If this is a GW1 profile, delegate launch + injection to Gw1InjectionService
            if (profile.GameType == GameType.GuildWars1)
            {
                var gw1Service = new Gw1InjectionService();

                if (gw1Service.TryLaunchGw1(profile, exePath, this, out var gw1Error, out var report))
                {
                    _lastLaunchReport = report;
                    lblStatus.Text = report.BuildSummary();
                }
                else
                {
                    _lastLaunchReport = report;

                    MessageBox.Show(
                        this,
                        gw1Error,
                        "Guild Wars 1 launch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    lblStatus.Text = report.BuildSummary();
                }

                return;
            }


            // Default: no injection (GW2, or GW1 with no DLLs enabled)
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
            int index = lstProfiles.IndexFromPoint(e.Location);
            if (index < 0) return;

            // Map click to item bounds
            Rectangle itemRect = lstProfiles.GetItemRectangle(index);
            var profile = lstProfiles.Items[index] as GameProfile;
            if (profile == null) return;

            // Checkbox rectangle must match drawing logic
            Rectangle card = itemRect;
            card.Inflate(-4, -4);
            Rectangle cb = new Rectangle(card.Left + 8, card.Top + 22, 16, 16);

            if (e.Button == MouseButtons.Left && cb.Contains(e.Location))
            {
                _views.ToggleEligible(_views.ActiveViewName, profile.Id);
                _views.Save();
                RefreshProfileList();
            }

            // Existing right-click selection behavior
            if (e.Button == MouseButtons.Right)
            {
                lstProfiles.SelectedIndex = index;
            }
        }
        private void StepView(int delta)
        {
            // Step through saved View names (Team1/Team2/Team3), not profiles
            var newName = _views.StepActiveView(delta);

            _suppressViewTextEvents = true;
            txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;
            
            // Switching view changes which profiles are checked (per-view),
            // but does not hide profiles by name.
            RefreshProfileList();
        }

        private void btnViewPrev_Click(object sender, EventArgs e)
        {
            StepView(-1);
        }

        private void btnViewNext_Click(object sender, EventArgs e)
        {
            StepView(+1);
        }
        private void ApplyViewScopedUiState()
        {
            _showCheckedOnly = _views.GetShowCheckedOnly(_views.ActiveViewName);

            _suppressArmBulkEvents = true;
            chkArmBulk.Checked = _showCheckedOnly;
            _suppressArmBulkEvents = false;
        }


        private void txtView_TextChanged(object sender, EventArgs e)
        {
            if (_suppressViewTextEvents)
                return;

            // Don’t mutate view store during typing.
            _viewNameDirty = true;
        }
        private void CommitViewRenameIfDirty()
        {
            if (!_viewNameDirty)
                return;

            _viewNameDirty = false;

            string newName = (txtView.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                // Revert to old name if user blanked it
                _suppressViewTextEvents = true;
                txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;
                return;
            }

            // Try rename. If it fails (duplicate name), revert & show a status hint.
            if (!_views.RenameActiveView(newName))
            {
                _suppressViewTextEvents = true;
                txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;

                lblStatus.Text = "View rename failed (name already exists).";
                return;
            }

            _views.Save();
            RefreshProfileList();
        }
        private void txtView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitViewRenameIfDirty();
            }
        }

        private void txtView_Leave(object sender, EventArgs e)
        {
            CommitViewRenameIfDirty();
        }

        private void txtView_Enter(object sender, EventArgs e)
        {
            _viewNameBeforeEdit = _views.ActiveViewName;
            _viewNameDirty = false;
        }
        private void btnNewView_Click(object sender, EventArgs e)
        {
            var newName = _views.CreateNewView("New View");
            _views.Save();

            _suppressViewTextEvents = true;
            txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            RefreshProfileList();
        }

        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e)
        {
            if (_lastLaunchReport == null)
                return;

            using var dlg = new LastLaunchDetailsForm(_lastLaunchReport);
            dlg.ShowDialog(this);
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
            var profile = lstProfiles.SelectedItem as GameProfile;
            if (profile == null)
                return;

            using var dlg = new ProfileSettingsForm(profile);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _profileManager.Save();
                RefreshProfileList();
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
        private void menuGw1ToolboxToggle_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
            {
                // Safety: if somehow clicked with no GW1 profile, just uncheck.
                menuGw1ToolboxToggle.Checked = false;
                return;
            }

            profile.Gw1ToolboxEnabled = menuGw1ToolboxToggle.Checked;
            _profileManager.Save();

            lblStatus.Text = $"GW1 Toolbox injection " +
                             (profile.Gw1ToolboxEnabled ? "enabled" : "disabled") +
                             $" for {profile.Name}.";
        }

        private void menuGw1ToolboxPath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
                return;

            TrySelectGw1ToolboxDll(profile);
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
        private void btnLaunchAll_Click(object sender, EventArgs e)
        {
            // Guard: bulk launch must be explicitly armed.
            bool anyEligible = _views.AnyEligibleInActiveView(_profileManager.Profiles);
            bool armed = anyEligible && _showCheckedOnly;

            if (!armed)
            {
                lblStatus.Text = $"Bulk launch not armed · View: {_views.ActiveViewName}";
                return;
            }

            var targets = _profileManager.Profiles
                .Where(p => _views.IsEligible(_views.ActiveViewName, p.Id))
                .OrderBy(p => p.GameType)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (targets.Count == 0)
            {
                lblStatus.Text = $"No checked profiles in view · View: {_views.ActiveViewName}";
                return;
            }

            foreach (var profile in targets)
                LaunchProfile(profile);
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
                    UpdateBulkArmingUi();
                    lblStatus.Text = $"Added account: {dialog.CreatedProfile.Name}";
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}

