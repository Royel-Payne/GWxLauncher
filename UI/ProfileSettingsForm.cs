using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Properties;
using System.Diagnostics;
using GWxLauncher.Services;
using GWxLauncher.UI;


namespace GWxLauncher.UI
{
    public partial class ProfileSettingsForm : Form
    {
        // -----------------------------
        // Fields / State
        // -----------------------------

        private readonly GameProfile _profile;
        private readonly LauncherConfig _cfg;
        private bool _loadingProfile;

        private bool _restoredFromSavedPlacement;
        private bool _gw1GmodPluginsInteractive = true;
        private bool _gw2RunAfterInteractive = true;

        // -----------------------------
        // Ctor / Form lifecycle
        // -----------------------------

        public ProfileSettingsForm(GameProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));

            InitializeComponent();
            ThemeService.ApplyToForm(this);

            InitGw2RunAfterContextMenu();

            // ListView theming + selection handlers
            lvGw2RunAfter.BackColor = ThemeService.Palette.InputBack;
            lvGw2RunAfter.ForeColor = ThemeService.Palette.InputFore;
            lvGw2RunAfter.SelectedIndexChanged += (s, e) => UpdateGw2RunAfterButtons();

            lvGw1GModPlugins.BackColor = ThemeService.Palette.InputBack;
            lvGw1GModPlugins.ForeColor = ThemeService.Palette.InputFore;
            lvGw1GModPlugins.SelectedIndexChanged += (s, e) => UpdateGw1GModPluginButtons();

            // When "disabled", prevent selection/checking so it *feels* disabled (without breaking theme bg)
            lvGw2RunAfter.ItemSelectionChanged += (s, e) =>
            {
                if (!_gw2RunAfterInteractive && e.IsSelected)
                {
                    if (e.Item != null)
                        e.Item.Selected = false;
                }
            };
            lvGw2RunAfter.ItemCheck += (s, e) =>
            {
                if (!_gw2RunAfterInteractive)
                    e.NewValue = e.CurrentValue;
            };

            // Explicit button handlers
            btnGw1AddPlugin.Click += btnGw1AddPlugin_Click;
            btnGw1RemovePlugin.Click += btnGw1RemovePlugin_Click;

            // Profile type visibility toggles
            bool isGw1 = _profile.GameType == GameType.GuildWars1;
            grpGw1Mods.Visible = isGw1;
            grpGw1Mods.Enabled = isGw1;
            grpGw1Login.Visible = isGw1;
            grpGw1Login.Enabled = isGw1;

            // Form icon per game
            Icon = _profile.GameType switch
            {
                GameType.GuildWars1 => Resources.Gw1Icon,
                GameType.GuildWars2 => Resources.Gw2Icon,
                _ => Icon
            };

            _cfg = LauncherConfig.Load();

            TryRestoreSavedPlacement();
            Shown += ProfileSettingsForm_Shown;
            FormClosing += ProfileSettingsForm_FormClosing;

            Text = $"Profile Settings – {profile.Name}";

            // Wire up button handlers (designer only sets DialogResult)
            btnOk.Click += btnOk_Click;
            btnBrowseExe.Click += btnBrowseExe_Click;
            btnBrowseToolboxDll.Click += btnBrowseToolboxDll_Click;
            btnBrowsePy4GwDll.Click += btnBrowsePy4GwDll_Click;
            btnBrowseGModDll.Click += btnBrowseGModDll_Click;
            btnCancel.Click += btnCancel_Click;

            // Optional niceties
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LoadFromProfile();
        }
        private void lvGw2RunAfter_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }
        private void lvGw2RunAfter_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            // Column 0: let WinForms draw checkbox + default visuals
            if (e.ColumnIndex == 0)
            {
                e.DrawDefault = true;
                return;
            }

            // We only customize the Name column (column 1)
            if (e.ColumnIndex != 1)
            {
                e.DrawDefault = true;
                return;
            }

            // --- Paint background (selection-aware) ---
            Color back = lvGw2RunAfter.BackColor;
            Color fore = lvGw2RunAfter.ForeColor;

            if (e.Item != null && e.Item.Selected)
            {
                back = ThemeService.Palette.ButtonBack;
                fore = ThemeService.Palette.ButtonFore;
            }

            using (var bg = new SolidBrush(back))
                e.Graphics.FillRectangle(bg, e.Bounds);

            var p = e.Item?.Tag as RunAfterProgram;

            // --- Compute badge rect (right-aligned) ---
            bool showBadge = (p != null && p.PassMumbleLinkName);
            string badgeText = "M";

            // tighter ListView badge sizing (not the big card metrics)
            const int badgePadX = 10;
            const int badgePadY = 3;
            const int badgeRightPad = 8;

            using var badgeFont = new Font(ThemeService.Typography.BadgeFont.FontFamily, 7.5f, FontStyle.Bold);

            int badgeW = 0;
            int badgeH = 0;

            if (showBadge)
            {
                var sz = e.Graphics.MeasureString(badgeText, badgeFont);
                badgeW = (int)sz.Width + badgePadX;
                badgeH = (int)sz.Height + badgePadY;
            }

            // --- Text rect is the cell minus the badge area ---
            var textRect = e.Bounds;
            if (showBadge)
                textRect = Rectangle.FromLTRB(e.Bounds.Left, e.Bounds.Top, e.Bounds.Right - (badgeW + badgeRightPad), e.Bounds.Bottom);

            // Fix: Check for null before accessing e.SubItem.Text
            string subItemText = e.SubItem?.Text ?? string.Empty;

            TextRenderer.DrawText(
                e.Graphics,
                subItemText,
                lvGw2RunAfter.Font,
                textRect,
                fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // --- Draw badge pill (like MainForm badges) ---
            if (showBadge)
            {
                int x = e.Bounds.Right - badgeRightPad - badgeW;
                int y = e.Bounds.Top + (e.Bounds.Height - badgeH) / 2;
                var rect = new Rectangle(x, y, badgeW, badgeH);

                using var badgeBg = new SolidBrush(ThemeService.CardPalette.BadgeBack);
                using var badgePen = new Pen(ThemeService.CardPalette.BadgeBorder);

                e.Graphics.FillRectangle(badgeBg, rect);
                e.Graphics.DrawRectangle(badgePen, rect);

                TextRenderer.DrawText(
                    e.Graphics,
                    badgeText,
                    badgeFont,
                    rect,
                    ThemeService.CardPalette.BadgeFore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
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
                // No owner: fallback to a reasonable default
                StartPosition = FormStartPosition.CenterScreen;
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

        // -----------------------------
        // Load / Save
        // -----------------------------

        private void LoadFromProfile()
        {
            _loadingProfile = true;

            // ---- Load values first ----
            txtProfileName.Text = _profile.Name;
            txtExecutablePath.Text = _profile.ExecutablePath;

            chkGw1AutoLogin.Checked = _profile.Gw1AutoLoginEnabled;
            txtGw1Email.Text = _profile.Gw1Email;

            chkGw1AutoSelectCharacter.Checked = _profile.Gw1AutoSelectCharacterEnabled;
            txtGw1CharacterName.Text = _profile.Gw1CharacterName;

            // Never display stored password; show a hint instead.
            txtGw1Password.Text = "";
            lblGw1PasswordSaved.Visible = !string.IsNullOrWhiteSpace(_profile.Gw1PasswordProtected);
            lblGw1PasswordSaved.ForeColor = Color.Goldenrod;

            // Warning visible only when enabled
            lblGw1LoginWarning.Visible = chkGw1AutoLogin.Checked;
            lblGw1LoginWarning.ForeColor = Color.Goldenrod;

            // ---- Wire events once (current behavior) ----
            chkGw1AutoLogin.CheckedChanged += (s, e) =>
            {
                lblGw1LoginWarning.Visible = chkGw1AutoLogin.Checked;
                UpdateGw1LoginUiState();
            };

            chkGw2AutoLogin.CheckedChanged += (s, e) => UpdateGw2LoginUiState();
            chkGw2RunAfterEnabled.CheckedChanged += (s, e) => UpdateGw2RunAfterUiState();
            chkGw1AutoSelectCharacter.CheckedChanged += (s, e) => UpdateGw1LoginUiState();

            chkToolbox.CheckedChanged += (s, e) =>
            {
                if (!_loadingProfile && chkToolbox.Checked)
                    EnsureDllSelectedOrRevert(chkToolbox, txtToolboxDll, "GWToolboxdll.dll");
                UpdateGw1ModsUiState();
            };

            chkPy4Gw.CheckedChanged += (s, e) =>
            {
                if (!_loadingProfile && chkPy4Gw.Checked)
                    EnsureDllSelectedOrRevert(chkPy4Gw, txtPy4GwDll, "Py4GW DLL");
                UpdateGw1ModsUiState();
            };

            chkGMod.CheckedChanged += (s, e) =>
            {
                if (!_loadingProfile && chkGMod.Checked)
                    EnsureDllSelectedOrRevert(chkGMod, txtGModDll, "gMod.dll");
                UpdateGw1ModsUiState();
            };

            bool isGw1 = _profile.GameType == GameType.GuildWars1;
            bool isGw2 = _profile.GameType == GameType.GuildWars2;

            grpGw1Mods.Visible = isGw1;
            grpGw1Mods.Enabled = isGw1;

            grpGw1Login.Visible = isGw1;
            grpGw1Login.Enabled = isGw1;

            grpGw2Login.Visible = isGw2;
            grpGw2Login.Enabled = isGw2;

            if (isGw1)
            {
                chkGw1Multiclient.Visible = true;
                chkGw1Multiclient.Checked = _cfg.Gw1MulticlientEnabled;

                txtToolboxDll.Text = _profile.Gw1ToolboxDllPath;
                chkToolbox.Checked = _profile.Gw1ToolboxEnabled;

                txtPy4GwDll.Text = _profile.Gw1Py4GwDllPath;
                chkPy4Gw.Checked = _profile.Gw1Py4GwEnabled;

                txtGModDll.Text = _profile.Gw1GModDllPath;
                chkGMod.Checked = _profile.Gw1GModEnabled;

                RefreshGw1GModPluginList();
            }
            else if (isGw2)
            {
                grpGw2RunAfter.Visible = true;
                grpGw2RunAfter.Enabled = true;

                chkGw1Multiclient.Visible = true;
                chkGw1Multiclient.Checked = _cfg.Gw2MulticlientEnabled;

                chkGw2RunAfterEnabled.Checked = _profile.Gw2RunAfterEnabled;

                UpdateGw2RunAfterUiState();
                chkGw2AutoLogin.Checked = _profile.Gw2AutoLoginEnabled;
                txtGw2Email.Text = _profile.Gw2Email ?? "";
                txtGw2Password.Text = "";
                chkGw2AutoPlay.Checked = _profile.Gw2AutoPlayEnabled;

                lblGw2PasswordSaved.Visible = !string.IsNullOrWhiteSpace(_profile.Gw2PasswordProtected);
                lblGw2PasswordSaved.ForeColor = Color.Goldenrod;
                lblGw2LoginInfo.ForeColor = Color.Goldenrod;
                lblGw2Warning.ForeColor = Color.Red;
                chkGw2AutoLogin.CheckedChanged += (s, e) =>
                {
                    lblGw2LoginInfo.Visible = chkGw2AutoLogin.Checked;
                    lblGw2Warning.Visible = chkGw2AutoLogin.Checked;
                    UpdateGw2LoginUiState();
                };

                RefreshGw2RunAfterList();
            }
            else
            {
                chkGw1Multiclient.Visible = false;
            }

            UpdateGw1LoginUiState();
            UpdateGw1ModsUiState();
            UpdateGw2LoginUiState();
            _loadingProfile = false;
        }

        private void SaveToProfile()
        {
            _profile.Name = txtProfileName.Text.Trim();
            _profile.ExecutablePath = txtExecutablePath.Text.Trim();

            // GW1 login
            _profile.Gw1AutoLoginEnabled = chkGw1AutoLogin.Checked;
            _profile.Gw1Email = txtGw1Email.Text.Trim();
            _profile.Gw1AutoSelectCharacterEnabled = chkGw1AutoSelectCharacter.Checked;
            _profile.Gw1CharacterName = txtGw1CharacterName.Text.Trim();

            // Only update stored password if user typed a new one.
            var pw = txtGw1Password.Text;
            if (!string.IsNullOrWhiteSpace(pw))
            {
                _profile.Gw1PasswordProtected = Services.DpapiProtector.ProtectToBase64(pw);
            }

            if (_profile.GameType == GameType.GuildWars1)
            {
                _profile.Gw1ToolboxEnabled = chkToolbox.Checked;
                _profile.Gw1ToolboxDllPath = txtToolboxDll.Text.Trim();

                _profile.Gw1Py4GwEnabled = chkPy4Gw.Checked;
                _profile.Gw1Py4GwDllPath = txtPy4GwDll.Text.Trim();

                _profile.Gw1GModEnabled = chkGMod.Checked;
                _profile.Gw1GModDllPath = txtGModDll.Text.Trim();

                // Remember last-known good tool paths (silent; no UI)
                TryRememberLastToolPath(_profile.Gw1ToolboxDllPath, p => _cfg.LastToolboxPath = p);
                TryRememberLastToolPath(_profile.Gw1Py4GwDllPath, p => _cfg.LastPy4GWPath = p);
                TryRememberLastToolPath(_profile.Gw1GModDllPath, p => _cfg.LastGModPath = p);

                _cfg.Gw1MulticlientEnabled = chkGw1Multiclient.Checked;
                _cfg.Save();
            }

            if (_profile.GameType == GameType.GuildWars2)
            {
                _profile.Gw2RunAfterEnabled = chkGw2RunAfterEnabled.Checked;

                _profile.Gw2AutoLoginEnabled = chkGw2AutoLogin.Checked;
                _profile.Gw2Email = txtGw2Email.Text.Trim();
                _profile.Gw2AutoPlayEnabled = chkGw2AutoPlay.Checked;

                var pw2 = txtGw2Password.Text;
                if (!string.IsNullOrWhiteSpace(pw2))
                {
                    _profile.Gw2PasswordProtected = Services.DpapiProtector.ProtectToBase64(pw2);
                }

                _cfg.Gw2MulticlientEnabled = chkGw1Multiclient.Checked;
                _cfg.Save();
            }
        }

        // -----------------------------
        // UI state updates
        // -----------------------------

        private void UpdateGw1LoginUiState()
        {
            UpdateGw1AutoLoginUiState();
        }

        private void UpdateGw1AutoLoginUiState()
        {
            bool enabled = chkGw1AutoLogin.Checked;

            // Grey out the login fields when auto-login is off
            txtGw1Email.Enabled = enabled;
            lblGw1Email.Enabled = enabled;

            txtGw1Password.Enabled = enabled;
            lblGw1Password.Enabled = enabled;

            // Auto-select controls still exist, but are gated by Auto-Login first
            chkGw1AutoSelectCharacter.Enabled = enabled;

            // Character name remains gated by Auto-select, but only if auto-login is enabled
            bool charEnabled = enabled && chkGw1AutoSelectCharacter.Checked;
            txtGw1CharacterName.Enabled = charEnabled;
            lblGw1CharacterName.Enabled = charEnabled;

            // Status label stays visible if password is stored, but “greys out” when auto-login disabled
            lblGw1PasswordSaved.Enabled = enabled;
        }
        private void UpdateGw2RunAfterUiState()
        {
            // Only meaningful on GW2 profiles, but safe to call always
            bool enabled = chkGw2RunAfterEnabled.Checked;

            _gw2RunAfterInteractive = enabled;

            // Keep theme background ALWAYS; just grey out text and block actions.
            lvGw2RunAfter.Enabled = true;
            lvGw2RunAfter.BackColor = ThemeService.Palette.InputBack;
            lvGw2RunAfter.ForeColor = enabled ? ThemeService.Palette.InputFore : ThemeService.Palette.DisabledFore;

            btnGw2AddProgram.Enabled = enabled;
            btnGw2RemoveProgram.Enabled = enabled && (lvGw2RunAfter.SelectedItems.Count > 0);

            if (!enabled && lvGw2RunAfter.SelectedItems.Count > 0)
                lvGw2RunAfter.SelectedItems.Clear();
        }

        private void UpdateGw2LoginUiState()
        {
            // Only meaningful on GW2 profiles, but safe to call always
            bool enabled = chkGw2AutoLogin.Checked;

            txtGw2Email.Enabled = enabled;
            lblGw2Email.Enabled = enabled;

            txtGw2Password.Enabled = enabled;
            lblGw2Password.Enabled = enabled;

            chkGw2AutoPlay.Enabled = enabled;

            // “Password saved” label stays visible if stored, but greys out when disabled
            lblGw2PasswordSaved.Enabled = enabled;
        }

        private void UpdateGw1ModsUiState()
        {
            // Toolbox
            txtToolboxDll.Enabled = chkToolbox.Checked;
            btnBrowseToolboxDll.Enabled = chkToolbox.Checked;

            // Py4GW
            txtPy4GwDll.Enabled = chkPy4Gw.Checked;
            btnBrowsePy4GwDll.Enabled = chkPy4Gw.Checked;

            // gMod
            bool gmod = chkGMod.Checked;
            _gw1GmodPluginsInteractive = gmod;

            txtGModDll.Enabled = gmod;
            btnBrowseGModDll.Enabled = gmod;

            // gMod plugins:
            // Keep the dark background ALWAYS (do not disable the control),
            // just grey out the text and block actions.
            lvGw1GModPlugins.Enabled = true; // keep theme background
            lvGw1GModPlugins.BackColor = ThemeService.Palette.InputBack;
            lvGw1GModPlugins.ForeColor = gmod ? ThemeService.Palette.InputFore : ThemeService.Palette.DisabledFore;
            lblGw1GModPlugins.Enabled = gmod;

            btnGw1AddPlugin.Enabled = gmod;
            btnGw1RemovePlugin.Enabled = gmod && (lvGw1GModPlugins.SelectedItems.Count > 0);

            // If gMod is off, also clear any selection so it *feels* disabled.
            if (!gmod && lvGw1GModPlugins.SelectedItems.Count > 0)
                lvGw1GModPlugins.SelectedItems.Clear();
        }

        // -----------------------------
        // GW2 Run-After list
        // -----------------------------

        private readonly ContextMenuStrip _gw2RunAfterMenu = new();
        private readonly ToolStripMenuItem _miPassMumble = new("Pass MumbleLink name") { CheckOnClick = true };

        private void InitGw2RunAfterContextMenu()
        {
            _gw2RunAfterMenu.Items.Add(_miPassMumble);

            _gw2RunAfterMenu.Opening += (s, e) =>
            {
                var p = GetSelectedGw2RunAfterProgram();
                if (p == null)
                {
                    e.Cancel = true;
                    return;
                }

                _miPassMumble.Checked = p.PassMumbleLinkName;
            };

            _miPassMumble.Click += (s, e) =>
            {
                var p = GetSelectedGw2RunAfterProgram();
                if (p == null) return;

                p.PassMumbleLinkName = _miPassMumble.Checked;
                RefreshGw2RunAfterList(); // refresh badge + tooltip
            };
        }

        private RunAfterProgram? GetSelectedGw2RunAfterProgram()
        {
            if (lvGw2RunAfter.SelectedItems.Count == 0)
                return null;

            return lvGw2RunAfter.SelectedItems[0].Tag as RunAfterProgram;
        }

        private void lvGw2RunAfter_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var hit = lvGw2RunAfter.HitTest(e.Location);
            if (hit.Item == null)
                return;

            hit.Item.Selected = true;
            _gw2RunAfterMenu.Show(lvGw2RunAfter, e.Location);
        }

        private void RefreshGw2RunAfterList()
        {
            lvGw2RunAfter.Items.Clear();

            foreach (var p in _profile.Gw2RunAfterPrograms ?? new List<RunAfterProgram>())
            {
                var item = new ListViewItem("");     // Column 0: empty text (checkbox lives here)
                item.SubItems.Add(p.Name);           // Column 1: name (we will draw badge inside this cell)

                item.Checked = p.Enabled;
                item.Tag = p;

                // Put the path in the tooltip now that we hid the path column
                item.ToolTipText =
                    (p.PassMumbleLinkName
                        ? "M = This program receives the GW2 MumbleLink name.\nUsed to pair overlays (e.g. Blish HUD) with this GW2 instance."
                        : "This program launches normally.\nRight-click to enable MumbleLink pairing (M).")
                    + $"\n\nPath:\n{p.ExePath}";


                lvGw2RunAfter.Items.Add(item);
            }

            UpdateGw2RunAfterButtons();
        }

        private void UpdateGw2RunAfterButtons()
        {
            // Only meaningful on GW2 profiles, but safe to call always
            bool enabled = chkGw2RunAfterEnabled.Checked;
            btnGw2RemoveProgram.Enabled = enabled && (lvGw2RunAfter.SelectedItems.Count > 0);
        }

        private void lvGw2RunAfter_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is RunAfterProgram p)
                p.Enabled = e.Item.Checked;
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
                var vi = FileVersionInfo.GetVersionInfo(exe);
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
                Enabled = true,
                PassMumbleLinkName = ShouldDefaultPassMumble(exe)
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

        // -----------------------------
        // GW1 gMod plugins list
        // -----------------------------

        private void RefreshGw1GModPluginList()
        {
            lvGw1GModPlugins.Items.Clear();

            foreach (var path in _profile.Gw1GModPluginPaths ?? new List<string>())
            {
                // Display: filename without extension (your preference)
                string display = Path.GetFileNameWithoutExtension(path);

                var item = new ListViewItem(display)
                {
                    Tag = path
                };

                lvGw1GModPlugins.Items.Add(item);
            }

            UpdateGw1GModPluginButtons();
        }

        private void UpdateGw1GModPluginButtons()
        {
            bool gmod = chkGMod.Checked;
            btnGw1RemovePlugin.Enabled = gmod && (lvGw1GModPlugins.SelectedItems.Count > 0);
        }

        private void btnGw1AddPlugin_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select gMod plugin (.tpf)",
                Filter = "TPF files (*.tpf)|*.tpf|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            string path = dlg.FileName;

            // Ensure list exists
            _profile.Gw1GModPluginPaths ??= new List<string>();

            // Dedupe (case-insensitive, absolute path as stored value)
            if (!_profile.Gw1GModPluginPaths.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
                _profile.Gw1GModPluginPaths.Add(path);

            RefreshGw1GModPluginList();
        }

        private void btnGw1RemovePlugin_Click(object? sender, EventArgs e)
        {
            if (lvGw1GModPlugins.SelectedItems.Count == 0)
                return;

            var item = lvGw1GModPlugins.SelectedItems[0];
            if (item.Tag is string path)
            {
                _profile.Gw1GModPluginPaths.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
                RefreshGw1GModPluginList();
            }
        }

        // -----------------------------
        // Validation
        // -----------------------------

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

        // -----------------------------
        // Buttons / DialogResult
        // -----------------------------

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

                DialogResult = DialogResult.None;
                return;
            }

            if (!ValidateGw1ModSettings())
            {
                DialogResult = DialogResult.None;
                return;
            }

            // Push values into the profile once
            SaveToProfile();

            // If GW1 + gMod enabled, prepare %AppData% copy + modlist.txt now (so launch can't fail later)
            if (_profile.GameType == GameType.GuildWars1 && _profile.Gw1GModEnabled)
            {
                try
                {
                    Services.Gw1InjectionService.PreparePerProfileGModFolder(_profile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        $"gMod is enabled but setup failed:\n\n{ex.Message}",
                        "gMod setup failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    DialogResult = DialogResult.None; // keep the dialog open
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }


        private bool IsDuplicateGw1Executable(string selectedExePath)
        {
            if (_profile.GameType != GameType.GuildWars1)
                return false;

            string selectedFull = Path.GetFullPath(selectedExePath);

            var pm = new GWxLauncher.Services.ProfileManager();
            pm.Load();

            return pm.Profiles.Any(p =>
                p.Id != _profile.Id &&
                p.GameType == GameType.GuildWars1 &&
                !string.IsNullOrWhiteSpace(p.ExecutablePath) &&
                string.Equals(Path.GetFullPath(p.ExecutablePath), selectedFull, StringComparison.OrdinalIgnoreCase));
        }

        // -----------------------------
        // Browse helpers
        // -----------------------------
        private static bool ShouldDefaultPassMumble(string exePath)
        {
            string file = Path.GetFileName(exePath);

            return
                file.Contains("blish", StringComparison.OrdinalIgnoreCase) ||
                file.Contains("taco", StringComparison.OrdinalIgnoreCase);
        }

        private void btnBrowseExe_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select game executable"
            };

            var current = txtExecutablePath.Text?.Trim();

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
            else
            {
                var cfgPath = _profile.GameType == GameType.GuildWars1 ? _cfg.Gw1Path : _cfg.Gw2Path;
                cfgPath = cfgPath?.Trim();

                try
                {
                    if (!string.IsNullOrWhiteSpace(cfgPath) && File.Exists(cfgPath))
                    {
                        var dir = Path.GetDirectoryName(cfgPath);
                        if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                            dlg.InitialDirectory = dir;
                    }
                    else
                    {
                        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    }
                }
                catch
                {
                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
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

                if (ProtectedInstallPathPolicy.IsProtectedPath(dlg.FileName))
                {
                    bool cont = ProtectedInstallPathWarningDialog.ConfirmContinue(this, dlg.FileName);
                    if (!cont)
                        return;
                }

                txtExecutablePath.Text = dlg.FileName;
            }
        }
        private void EnsureDllSelectedOrRevert(CheckBox toggle, TextBox pathBox, string displayName)
        {
            var current = (pathBox.Text ?? "").Trim();

            // If we already have a valid path, we're good.
            if (!string.IsNullOrWhiteSpace(current))
                return;

            // Prompt: user is trying to enable tool with no path.
            var msg =
                $"{displayName} is enabled, but no DLL path is set.\n\n" +
                "Select the DLL now?";

            var result = MessageBox.Show(
                this,
                msg,
                "Missing DLL path",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                BrowseDllInto(pathBox);
                current = (pathBox.Text ?? "").Trim();

                // If still empty (user cancelled picker), revert toggle.
                if (string.IsNullOrWhiteSpace(current))
                    toggle.Checked = false;
            }
            else
            {
                toggle.Checked = false;
            }
        }

        private void TryRememberLastToolPath(string path, Action<string> assign)
        {
            path = (path ?? "").Trim();
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                if (File.Exists(path))
                    assign(path);
            }
            catch
            {
                // best-effort only
            }
        }

        private void BrowseDllInto(TextBox textBox)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                Title = "Select DLL"
            };

            var current = textBox.Text?.Trim();

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
            else
            {
                var exe = txtExecutablePath.Text?.Trim();

                try
                {
                    if (!string.IsNullOrWhiteSpace(exe) && File.Exists(exe))
                    {
                        var dir = Path.GetDirectoryName(exe);
                        if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                            dlg.InitialDirectory = dir;
                        else
                            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    }
                    else
                    {
                        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    }
                }
                catch
                {
                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                textBox.Text = dlg.FileName;
            }
        }

        private void btnBrowseToolboxDll_Click(object? sender, EventArgs e) => BrowseDllInto(txtToolboxDll);
        private void btnBrowsePy4GwDll_Click(object? sender, EventArgs e) => BrowseDllInto(txtPy4GwDll);
        private void btnBrowseGModDll_Click(object? sender, EventArgs e) => BrowseDllInto(txtGModDll);

        private void lblGw2Warning_Click(object sender, EventArgs e)
        {

        }
    }
}
