using GWxLauncher.Domain;

namespace GWxLauncher.UI.TabControls
{
    /// <summary>
    /// UserControl for the General tab in ProfileSettingsForm.
    /// Contains profile name, executable path, launch arguments, and multiclient settings.
    /// </summary>
    public partial class GeneralTabContent : UserControl
    {
        private GameType _gameType;

        public GeneralTabContent()
        {
            InitializeComponent();
            ApplyTheme();
        }

        // Public properties for data binding
        public GameType GameType
        {
            get => _gameType;
            set
            {
                _gameType = value;
                UpdateVisibilityForGameType();
            }
        }

        // Events
        public event EventHandler? BrowseExecutableRequested;

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
        }

        private void UpdateVisibilityForGameType()
        {
            // GW1-specific controls visibility will be set when controls are reparented
            // This method is called after reparenting to adjust visibility
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }
    }
}
