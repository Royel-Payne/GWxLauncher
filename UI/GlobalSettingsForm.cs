using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI.Helpers;
using GWxLauncher.UI.TabControls;

namespace GWxLauncher.UI
{
    internal sealed partial class GlobalSettingsForm : Form
    {
        private readonly LauncherConfig _cfg;
        private readonly Services.ProfileManager _profileManager;
        private bool _restoredFromSavedPlacement;
        
        public event EventHandler? ImportCompleted;
        public event EventHandler? ProfilesBulkUpdated; // Bubbled up from tab

        // Tab infrastructure
        private Panel? _pnlButtonBar;
        private Panel? _pnlSidebar;
        private Panel? _pnlSplitter;
        private Panel? _pnlContentViewport;
        private ListBox? _lstTabs;

        // Tab content UserControls
        private GlobalGeneralTabContent? _generalTab;
        private GlobalGw1TabContent? _gw1Tab;

        public GlobalSettingsForm(Services.ProfileManager? profileManager = null)
        {
            _cfg = LauncherConfig.Load();
            _profileManager = profileManager ?? new Services.ProfileManager();

            if (_profileManager.Profiles.Count == 0)
                _profileManager.Load();

            // Step 1: Designer
            InitializeComponent();

            // Step 2: Create tab infrastructure
            CreateTabInfrastructure();

            // Step 3: Create UserControl instances
            CreateTabControls();

            // Step 4: Reparent buttons
            ReparentControlsToTabs();

            // Step 5: Wire up tab switching
            InitTabSidebar();

            // Step 6: Apply theme
            ThemeService.ApplyToForm(this);
            
            // Events
            TryRestoreSavedPlacement();
            Shown += GlobalSettingsForm_Shown;
            FormClosing += GlobalSettingsForm_FormClosing;

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LoadFromConfig();

            if (_lstTabs != null) _lstTabs.SelectedIndex = 0;
        }

        private void CreateTabInfrastructure()
        {
            // Button bar (bottom)
            _pnlButtonBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = ThemeService.Palette.WindowBack
            };
            this.Controls.Add(_pnlButtonBar);

            // Content viewport
            _pnlContentViewport = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ThemeService.Palette.WindowBack,
                Padding = new Padding(0)
            };
            this.Controls.Add(_pnlContentViewport);
            _pnlContentViewport.MouseEnter += (s, e) => _pnlContentViewport?.Focus();

            // Splitter
            _pnlSplitter = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = ThemeService.Palette.Separator
            };
            this.Controls.Add(_pnlSplitter);

            // Sidebar
            _pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = ThemeService.Palette.SurfaceBack
            };
            this.Controls.Add(_pnlSidebar);

            // Tab list
            _lstTabs = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 48,
                BorderStyle = BorderStyle.None,
                SelectionMode = SelectionMode.One,
                IntegralHeight = false,
                BackColor = ThemeService.Palette.SurfaceBack,
                ForeColor = ThemeService.Palette.WindowFore
            };
            _lstTabs.Items.AddRange(new object[] { "General", "Guild Wars 1" });
            _pnlSidebar.Controls.Add(_lstTabs);
        }

        private void CreateTabControls()
        {
            if (_pnlContentViewport == null) return;

            _generalTab = new GlobalGeneralTabContent
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            _generalTab.ImportCompleted += (s, e) => this.ImportCompleted?.Invoke(this, e);
            _generalTab.MouseEnter += (s, e) => _generalTab?.Focus();
            _pnlContentViewport.Controls.Add(_generalTab);

            _gw1Tab = new GlobalGw1TabContent
            {
                Dock = DockStyle.Top,
                Visible = false
            };
            _gw1Tab.ProfilesBulkUpdated += (s, e) => this.ProfilesBulkUpdated?.Invoke(this, e);
            _gw1Tab.MouseEnter += (s, e) => _gw1Tab?.Focus();
            _pnlContentViewport.Controls.Add(_gw1Tab);
        }

        private void ReparentControlsToTabs()
        {
            if (_pnlButtonBar == null) return;

            this.Controls.Remove(btnOk);
            this.Controls.Remove(btnCancel);

            _pnlButtonBar.Controls.Add(btnOk);
            _pnlButtonBar.Controls.Add(btnCancel);

            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Location = new Point(_pnlButtonBar.Width - 170, 15);
            btnCancel.Location = new Point(_pnlButtonBar.Width - 85, 15);
        }

        private void InitTabSidebar()
        {
            if (_lstTabs == null) return;
            _lstTabs.SelectedIndexChanged += lstTabs_SelectedIndexChanged;
            _lstTabs.DrawItem += lstTabs_DrawItem;
        }

        private void lstTabs_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_lstTabs == null || _lstTabs.SelectedIndex < 0) return;
            if (_generalTab == null || _gw1Tab == null) return;

            _generalTab.Visible = false;
            _gw1Tab.Visible = false;

            switch (_lstTabs.SelectedIndex)
            {
                case 0: // General
                    _generalTab.Visible = true;
                    _generalTab.Focus();
                    _generalTab.RefreshTheme(); // Ensure theme is fresh
                    break;
                case 1: // Guild Wars 1
                    _gw1Tab.Visible = true;
                    _gw1Tab.Focus();
                    _gw1Tab.RefreshTheme();
                    break;
            }

            if (_pnlContentViewport != null)
                _pnlContentViewport.AutoScrollPosition = new Point(0, 0);
        }
        
        private void lstTabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (_lstTabs == null || e.Index < 0) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = isSelected ? ThemeService.Palette.ButtonBack : ThemeService.Palette.SurfaceBack;

            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, e.Bounds);

            if (isSelected)
            {
                using (var accentBrush = new SolidBrush(ThemeService.CardPalette.Accent))
                {
                    var accentRect = new Rectangle(e.Bounds.Left, e.Bounds.Top, 4, e.Bounds.Height);
                    e.Graphics.FillRectangle(accentBrush, accentRect);
                }
            }

            string text = _lstTabs.Items[e.Index].ToString() ?? "";
            Color textColor = isSelected ? ThemeService.Palette.WindowFore : ThemeService.Palette.SubtleFore;

            TextRenderer.DrawText(
                e.Graphics, text, _lstTabs.Font,
                new Rectangle(e.Bounds.Left + 16, e.Bounds.Top, e.Bounds.Width - 16, e.Bounds.Height),
                textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private void LoadFromConfig()
        {
            _generalTab?.BindConfig(_cfg);
            _gw1Tab?.BindConfig(_cfg, _profileManager);
        }

        private void SaveAndClose()
        {
            _generalTab?.SaveConfig(_cfg);
            _gw1Tab?.SaveConfig(_cfg);
            _cfg.Save();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e) => SaveAndClose();
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void GlobalSettingsForm_Shown(object? sender, EventArgs e)
        {
            if (_restoredFromSavedPlacement)
                return;

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
    }
}
