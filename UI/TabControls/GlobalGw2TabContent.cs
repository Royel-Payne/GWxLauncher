using GWxLauncher.Config;
using GWxLauncher.Services;

namespace GWxLauncher.UI.TabControls
{
    public partial class GlobalGw2TabContent : UserControl
    {
        private LauncherConfig _cfg;

        public GlobalGw2TabContent()
        {
            InitializeComponent();
            ApplyTheme();

            // Ensure the control autosizes to its contents so the parent AutoScroll panel knows to scroll.
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            // Add padding at the bottom to ensure the last buttons aren't cut off by the scroll view
            this.Padding = new Padding(0, 0, 0, 64);

            cbGw2RenameWindowTitle.CheckedChanged += (s, e) => txtGw2TitleTemplate.Enabled = cbGw2RenameWindowTitle.Checked;
        }

        internal void BindConfig(LauncherConfig cfg)
        {
            _cfg = cfg;

            // Window Title
            cbGw2RenameWindowTitle.Checked = _cfg.Gw2WindowTitleEnabled;
            txtGw2TitleTemplate.Text = string.IsNullOrWhiteSpace(_cfg.Gw2WindowTitleTemplate)
                ? "GW2 • {ProfileName}"
                : _cfg.Gw2WindowTitleTemplate;
            txtGw2TitleTemplate.Enabled = cbGw2RenameWindowTitle.Checked;
        }

        internal void SaveConfig(LauncherConfig cfg)
        {
            cfg.Gw2WindowTitleEnabled = cbGw2RenameWindowTitle.Checked;

            var tpl = (txtGw2TitleTemplate.Text ?? "").Trim();
            cfg.Gw2WindowTitleTemplate = string.IsNullOrWhiteSpace(tpl) ? "GW2 • {ProfileName}" : tpl;
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
    }
}
