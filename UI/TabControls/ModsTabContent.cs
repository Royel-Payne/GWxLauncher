using System.Diagnostics;
using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI.Helpers;

namespace GWxLauncher.UI.TabControls
{
    public partial class ModsTabContent : UserControl
    {
        private GameProfile _profile;
        private LauncherConfig _cfg;
        private bool _loadingProfile;
        
        // Interactive state
        private bool _gw1GmodPluginsInteractive = true;
        private bool _gw2RunAfterInteractive = true;

        private readonly ContextMenuStrip _gw2RunAfterMenu = new();
        private readonly ToolStripMenuItem _miPassMumble = new("Pass MumbleLink name") { CheckOnClick = true };

        public ModsTabContent()
        {
            InitializeComponent();
            ApplyTheme();
            
            _cfg = LauncherConfig.Load();

            InitGw2RunAfterContextMenu();
            WireUpEventHandlers();
        }

        public void BindProfile(GameProfile profile)
        {
            _profile = profile;
            _loadingProfile = true;

            UpdateForGameType(profile.GameType);

            if (profile.GameType == GameType.GuildWars1)
            {
                txtToolboxDll.Text = profile.Gw1ToolboxDllPath;
                chkToolbox.Checked = profile.Gw1ToolboxEnabled;

                txtPy4GwDll.Text = profile.Gw1Py4GwDllPath;
                chkPy4Gw.Checked = profile.Gw1Py4GwEnabled;

                txtGModDll.Text = profile.Gw1GModDllPath;
                chkGMod.Checked = profile.Gw1GModEnabled;

                RefreshGw1GModPluginList();
            }
            else // GW2
            {
                chkGw2RunAfterEnabled.Checked = profile.Gw2RunAfterEnabled;
                RefreshGw2RunAfterList();
            }

            UpdateGw1ModsUiState();
            UpdateGw2RunAfterUiState();
            
            _loadingProfile = false;
        }

        public void SaveProfile(GameProfile profile)
        {
            // Note: Lists are live-updated in the profile object as items are added/removed.
            // We just need to save the simple properties here.
            
            if (profile.GameType == GameType.GuildWars1)
            {
                profile.Gw1ToolboxEnabled = chkToolbox.Checked;
                profile.Gw1ToolboxDllPath = txtToolboxDll.Text.Trim();

                profile.Gw1Py4GwEnabled = chkPy4Gw.Checked;
                profile.Gw1Py4GwDllPath = txtPy4GwDll.Text.Trim();

                profile.Gw1GModEnabled = chkGMod.Checked;
                profile.Gw1GModDllPath = txtGModDll.Text.Trim();

                // Remember paths
                TryRememberLastToolPath(profile.Gw1ToolboxDllPath, () => _cfg.LastToolboxPath, v => _cfg.LastToolboxPath = v);
                TryRememberLastToolPath(profile.Gw1Py4GwDllPath, () => _cfg.LastPy4GWPath, v => _cfg.LastPy4GWPath = v);
                TryRememberLastToolPath(profile.Gw1GModDllPath, () => _cfg.LastGModPath, v => _cfg.LastGModPath = v);
            }
            else
            {
                profile.Gw2RunAfterEnabled = chkGw2RunAfterEnabled.Checked;
            }
            
            _cfg.Save();
        }

        public void UpdateForGameType(GameType gameType)
        {
            bool isGw1 = gameType == GameType.GuildWars1;
            tlpGw1.Visible = isGw1;
            tlpGw2.Visible = !isGw1;
        }

        private void WireUpEventHandlers()
        {
            // --- GW1 Mods UI ---
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

            btnBrowseToolboxDll.Click += (s, e) => BrowseDllInto(txtToolboxDll);
            btnBrowsePy4GwDll.Click += (s, e) => BrowseDllInto(txtPy4GwDll);
            btnBrowseGModDll.Click += (s, e) => BrowseDllInto(txtGModDll);

            btnGw1AddPlugin.Click += btnGw1AddPlugin_Click;
            btnGw1RemovePlugin.Click += btnGw1RemovePlugin_Click;

            lvGw1GModPlugins.SelectedIndexChanged += (s, e) => UpdateGw1GModPluginButtons();

            // --- GW2 RunAfter UI ---
            chkGw2RunAfterEnabled.CheckedChanged += (s, e) => UpdateGw2RunAfterUiState();
            
            btnGw2AddProgram.Click += btnGw2AddProgram_Click;
            btnGw2RemoveProgram.Click += btnGw2RemoveProgram_Click;
            
            lvGw2RunAfter.SelectedIndexChanged += (s, e) => UpdateGw2RunAfterButtons();
            
            // Custom drawing / interaction for RunAfter list
            lvGw2RunAfter.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            lvGw2RunAfter.DrawSubItem += lvGw2RunAfter_DrawSubItem;
            lvGw2RunAfter.MouseUp += lvGw2RunAfter_MouseUp;
            lvGw2RunAfter.ItemChecked += lvGw2RunAfter_ItemChecked;
            
            // Interactive gating
            lvGw2RunAfter.ItemSelectionChanged += (s, e) =>
            {
                if (!_gw2RunAfterInteractive && e.IsSelected)
                    if (e.Item != null) e.Item.Selected = false;
            };
            lvGw2RunAfter.ItemCheck += (s, e) =>
            {
                if (!_gw2RunAfterInteractive)
                    e.NewValue = e.CurrentValue;
            };
        }

        // -----------------------------
        // UI State / Theming
        // -----------------------------

        private void UpdateGw1ModsUiState()
        {
            txtToolboxDll.Enabled = chkToolbox.Checked;
            btnBrowseToolboxDll.Enabled = chkToolbox.Checked;

            txtPy4GwDll.Enabled = chkPy4Gw.Checked;
            btnBrowsePy4GwDll.Enabled = chkPy4Gw.Checked;

            bool gmod = chkGMod.Checked;
            _gw1GmodPluginsInteractive = gmod;

            txtGModDll.Enabled = gmod;
            btnBrowseGModDll.Enabled = gmod;

            lvGw1GModPlugins.Enabled = true; 
            lvGw1GModPlugins.BackColor = ThemeService.Palette.InputBack;
            lvGw1GModPlugins.ForeColor = gmod ? ThemeService.Palette.InputFore : ThemeService.Palette.DisabledFore;
            
            btnGw1AddPlugin.Enabled = gmod;
            btnGw1RemovePlugin.Enabled = gmod && (lvGw1GModPlugins.SelectedItems.Count > 0);

            if (!gmod && lvGw1GModPlugins.SelectedItems.Count > 0)
                lvGw1GModPlugins.SelectedItems.Clear();
        }

        private void UpdateGw2RunAfterUiState()
        {
            bool enabled = chkGw2RunAfterEnabled.Checked;
            _gw2RunAfterInteractive = enabled;

            lvGw2RunAfter.Enabled = true;
            lvGw2RunAfter.BackColor = ThemeService.Palette.InputBack;
            lvGw2RunAfter.ForeColor = enabled ? ThemeService.Palette.InputFore : ThemeService.Palette.DisabledFore;

            btnGw2AddProgram.Enabled = enabled;
            btnGw2RemoveProgram.Enabled = enabled && (lvGw2RunAfter.SelectedItems.Count > 0);

            if (!enabled && lvGw2RunAfter.SelectedItems.Count > 0)
                lvGw2RunAfter.SelectedItems.Clear();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
            
            // ListViews need explicit color setting sometimes if OwnerDraw
            lvGw1GModPlugins.BackColor = ThemeService.Palette.InputBack;
            lvGw1GModPlugins.ForeColor = ThemeService.Palette.InputFore;
            
            lvGw2RunAfter.BackColor = ThemeService.Palette.InputBack;
            lvGw2RunAfter.ForeColor = ThemeService.Palette.InputFore;

            lblGw2RunAfterTip.ForeColor = Color.DarkGoldenrod;
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }

        // -----------------------------
        // GMod Logic
        // -----------------------------

        private void RefreshGw1GModPluginList()
        {
            lvGw1GModPlugins.Items.Clear();
            foreach (var path in _profile.Gw1GModPluginPaths ?? new List<string>())
            {
                string display = Path.GetFileNameWithoutExtension(path);
                var item = new ListViewItem(display) { Tag = path };
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

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string path = dlg.FileName;
            _profile.Gw1GModPluginPaths ??= new List<string>();
            
            if (!_profile.Gw1GModPluginPaths.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
                _profile.Gw1GModPluginPaths.Add(path);

            RefreshGw1GModPluginList();
        }

        private void btnGw1RemovePlugin_Click(object? sender, EventArgs e)
        {
            if (lvGw1GModPlugins.SelectedItems.Count == 0) return;
            var item = lvGw1GModPlugins.SelectedItems[0];
            if (item.Tag is string path)
            {
                _profile.Gw1GModPluginPaths.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
                RefreshGw1GModPluginList();
            }
        }

        // -----------------------------
        // GW2 RunAfter Logic
        // -----------------------------
        
        private void InitGw2RunAfterContextMenu()
        {
            _gw2RunAfterMenu.Items.Add(_miPassMumble);
            _gw2RunAfterMenu.Opening += (s, e) =>
            {
                var p = GetSelectedGw2RunAfterProgram();
                if (p == null) { e.Cancel = true; return; }
                _miPassMumble.Checked = p.PassMumbleLinkName;
            };
            _miPassMumble.Click += (s, e) =>
            {
                var p = GetSelectedGw2RunAfterProgram();
                if (p == null) return;
                p.PassMumbleLinkName = _miPassMumble.Checked;
                RefreshGw2RunAfterList();
            };
        }

        private RunAfterProgram? GetSelectedGw2RunAfterProgram()
        {
            if (lvGw2RunAfter.SelectedItems.Count == 0) return null;
            return lvGw2RunAfter.SelectedItems[0].Tag as RunAfterProgram;
        }

        private void RefreshGw2RunAfterList()
        {
            lvGw2RunAfter.Items.Clear();
            foreach (var p in _profile.Gw2RunAfterPrograms ?? new List<RunAfterProgram>())
            {
                var item = new ListViewItem(""); 
                item.SubItems.Add(p.Name);
                item.Checked = p.Enabled;
                item.Tag = p;
                item.ToolTipText = (p.PassMumbleLinkName ? "M = Paired via MumbleLink.\n" : "Normal launch.\n") + $"Path:\n{p.ExePath}";
                lvGw2RunAfter.Items.Add(item);
            }
            UpdateGw2RunAfterButtons();
        }

        private void UpdateGw2RunAfterButtons()
        {
            bool enabled = chkGw2RunAfterEnabled.Checked;
            btnGw2RemoveProgram.Enabled = enabled && (lvGw2RunAfter.SelectedItems.Count > 0);
        }

        private void btnGw2AddProgram_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select program to run after launching",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var exe = dlg.FileName;
            string name = Path.GetFileNameWithoutExtension(exe);
            try
            {
                var vi = FileVersionInfo.GetVersionInfo(exe);
                if (!string.IsNullOrWhiteSpace(vi.FileDescription)) name = vi.FileDescription.Trim();
            }
            catch {}

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
            if (lvGw2RunAfter.SelectedItems.Count == 0) return;
            var item = lvGw2RunAfter.SelectedItems[0];
            if (item.Tag is RunAfterProgram p)
            {
                _profile.Gw2RunAfterPrograms.Remove(p);
                RefreshGw2RunAfterList();
            }
        }
        
        private void lvGw2RunAfter_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = lvGw2RunAfter.HitTest(e.Location);
            if (hit.Item == null) return;
            hit.Item.Selected = true;
            _gw2RunAfterMenu.Show(lvGw2RunAfter, e.Location);
        }

        private void lvGw2RunAfter_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is RunAfterProgram p)
                p.Enabled = e.Item.Checked;
        }

        private void lvGw2RunAfter_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex == 0) { e.DrawDefault = true; return; }
            if (e.ColumnIndex != 1) { e.DrawDefault = true; return; }

            Color back = lvGw2RunAfter.BackColor;
            Color fore = lvGw2RunAfter.ForeColor;

            if (e.Item != null && e.Item.Selected)
            {
                back = ThemeService.Palette.ButtonBack;
                fore = ThemeService.Palette.ButtonFore;
            }

            using (var bg = new SolidBrush(back)) e.Graphics.FillRectangle(bg, e.Bounds);

            var p = e.Item?.Tag as RunAfterProgram;
            bool showBadge = (p != null && p.PassMumbleLinkName);
            string badgeText = "M";

            const int badgePadX = 10;
            const int badgePadY = 3;
            const int badgeRightPad = 8;

            using var badgeFont = new Font(ThemeService.Typography.BadgeFont.FontFamily, 7.5f, FontStyle.Bold);
            int badgeW = 0, badgeH = 0;

            if (showBadge)
            {
                var sz = e.Graphics.MeasureString(badgeText, badgeFont);
                badgeW = (int)sz.Width + badgePadX;
                badgeH = (int)sz.Height + badgePadY;
            }

            var textRect = e.Bounds;
            if (showBadge)
                textRect = Rectangle.FromLTRB(e.Bounds.Left, e.Bounds.Top, e.Bounds.Right - (badgeW + badgeRightPad), e.Bounds.Bottom);

            TextRenderer.DrawText(e.Graphics, e.SubItem?.Text ?? "", lvGw2RunAfter.Font, textRect, fore, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (showBadge)
            {
                int x = e.Bounds.Right - badgeRightPad - badgeW;
                int y = e.Bounds.Top + (e.Bounds.Height - badgeH) / 2;
                var rect = new Rectangle(x, y, badgeW, badgeH);

                using var badgeBg = new SolidBrush(ThemeService.CardPalette.BadgeBack);
                using var badgePen = new Pen(ThemeService.CardPalette.BadgeBorder);

                e.Graphics.FillRectangle(badgeBg, rect);
                e.Graphics.DrawRectangle(badgePen, rect);

                TextRenderer.DrawText(e.Graphics, badgeText, badgeFont, rect, ThemeService.CardPalette.BadgeFore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        // -----------------------------
        // Helpers
        // -----------------------------

        private static bool ShouldDefaultPassMumble(string exePath)
        {
            string file = Path.GetFileName(exePath);
            return file.Contains("blish", StringComparison.OrdinalIgnoreCase) ||
                   file.Contains("taco", StringComparison.OrdinalIgnoreCase);
        }

        private void BrowseDllInto(TextBox textBox)
        {
            var exe = _profile.ExecutablePath; 
            string? fallbackDir = null;
            try {
                if (!string.IsNullOrWhiteSpace(exe) && File.Exists(exe)) {
                    var dir = Path.GetDirectoryName(exe);
                    if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir)) fallbackDir = dir;
                }
                fallbackDir ??= Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            } catch { fallbackDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); }

            _ = FilePickerHelper.TryPickDll(this.FindForm(), textBox, "Select DLL", fallbackDir);
        }

        private void EnsureDllSelectedOrRevert(CheckBox toggle, TextBox pathBox, string displayName)
        {
            var current = (pathBox.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(current)) return;

            if (MessageBox.Show(this, $"{displayName} is enabled, but no DLL path is set.\n\nSelect the DLL now?", "Missing DLL path", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                BrowseDllInto(pathBox);
                if (string.IsNullOrWhiteSpace((pathBox.Text ?? "").Trim())) toggle.Checked = false;
            }
            else
            {
                toggle.Checked = false;
            }
        }

        private void TryRememberLastToolPath(string path, Func<string> getCurrent, Action<string> setValue)
        {
            path = (path ?? "").Trim();
            if (string.IsNullOrWhiteSpace(path)) return;
            try {
                if (!File.Exists(path)) return;
                var current = (getCurrent?.Invoke() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(current)) return;
                setValue?.Invoke(path);
            } catch {}
        }
    }
}
