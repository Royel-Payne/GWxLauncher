namespace GWxLauncher.UI.TabControls
{
    /// <summary>
    /// UserControl for the Windowed Mode tab in ProfileSettingsForm.
    /// Placeholder for future windowed mode settings (borderless, custom resolution, etc.).
    /// </summary>
    public partial class WindowTabContent : UserControl
    {
        public WindowTabContent()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);

            if (lblPlaceholder != null)
            {
                lblPlaceholder.ForeColor = ThemeService.Palette.SubtleFore;
            }
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }
    }
}
