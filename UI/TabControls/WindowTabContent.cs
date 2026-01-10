using GWxLauncher.Domain;

namespace GWxLauncher.UI.TabControls
{
    public partial class WindowTabContent : UserControl
    {
        public WindowTabContent()
        {
            InitializeComponent();

            // Wiring
            chkWindowedEnabled.CheckedChanged += (s, e) => UpdateUiState();
            chkRememberChanges.CheckedChanged += (s, e) => UpdateUiState();
            chkLockWindow.CheckedChanged += (s, e) => UpdateUiState();
        }

        public void BindProfile(GameProfile profile)
        {
            if (profile == null) return;

            chkWindowedEnabled.Checked = profile.WindowedModeEnabled;

            numX.Value = Clamp(profile.WindowX, numX.Minimum, numX.Maximum);
            numY.Value = Clamp(profile.WindowY, numY.Minimum, numY.Maximum);
            numW.Value = Clamp(profile.WindowWidth, numW.Minimum, numW.Maximum);
            numH.Value = Clamp(profile.WindowHeight, numH.Minimum, numH.Maximum);

            chkRememberChanges.Checked = profile.WindowRememberChanges;
            chkLockWindow.Checked = profile.WindowLockChanges;
            chkBlockInputs.Checked = profile.WindowBlockInputs;

            UpdateUiState();
        }

        public void SaveProfile(GameProfile profile)
        {
            if (profile == null) return;

            profile.WindowedModeEnabled = chkWindowedEnabled.Checked;

            profile.WindowX = (int)numX.Value;
            profile.WindowY = (int)numY.Value;
            profile.WindowWidth = (int)numW.Value;
            profile.WindowHeight = (int)numH.Value;

            profile.WindowRememberChanges = chkRememberChanges.Checked;
            profile.WindowLockChanges = chkLockWindow.Checked;
            profile.WindowBlockInputs = chkBlockInputs.Checked;
        }

        private void UpdateUiState()
        {
            bool windowed = chkWindowedEnabled.Checked;

            grpPosition.Enabled = windowed;
            grpBehavior.Enabled = windowed;

            // Behavior conflicts
            if (chkLockWindow.Checked)
            {
                if (chkRememberChanges.Checked)
                    chkRememberChanges.Checked = false; // Auto-disable

                chkRememberChanges.Enabled = false;
            }
            else
            {
                chkRememberChanges.Enabled = true;
            }
        }

        private decimal Clamp(decimal val, decimal min, decimal max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        private decimal Clamp(int val, decimal min, decimal max) => Clamp((decimal)val, min, max);
    }
}
