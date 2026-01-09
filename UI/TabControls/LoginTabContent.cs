using GWxLauncher.Domain;
using GWxLauncher.Services;
using System.Drawing;
using System.Windows.Forms;

namespace GWxLauncher.UI.TabControls
{
    public partial class LoginTabContent : UserControl
    {
        private GameProfile _profile;

        public LoginTabContent()
        {
            InitializeComponent();
            ApplyTheme();

            // Event wiring
            chkAutoLogin.CheckedChanged += (s, e) => UpdateUiState();
            chkAutoSelectChar.CheckedChanged += (s, e) => UpdateUiState();
        }

        public void BindProfile(GameProfile profile)
        {
            _profile = profile;
            
            bool isGw1 = profile.GameType == GameType.GuildWars1;
            bool isGw2 = profile.GameType == GameType.GuildWars2;

            // 1. Text & Config
            if (isGw1)
            {
                chkAutoLogin.Text = "Enable Auto-Login";
                chkAutoLogin.Checked = profile.Gw1AutoLoginEnabled;
                
                txtEmail.Text = profile.Gw1Email;
                txtPassword.Text = ""; // Never show password back
                
                lblPasswordSaved.Visible = !string.IsNullOrWhiteSpace(profile.Gw1PasswordProtected);
                
                chkAutoSelectChar.Checked = profile.Gw1AutoSelectCharacterEnabled;
                txtCharName.Text = profile.Gw1CharacterName;

                lblWarning.Text = "Warning: Passwords are encrypted (DPAPI) but stored locally so auto-login can work.\nUse at your own risk.";
                lblWarning.ForeColor = Color.Goldenrod;

                // Hidden/Shown controls
                lblLoginInfo.Visible = false;
                
                chkAutoSelectChar.Visible = true;
                lblCharName.Visible = true;
                txtCharName.Visible = true;
                
                chkAutoPlay.Visible = false;
            }
            else // GW2
            {
                chkAutoLogin.Text = "Enable Auto-Login";
                chkAutoLogin.Checked = profile.Gw2AutoLoginEnabled;

                lblLoginInfo.Text = "GW2 requires -autologin argument (managed automatically).";
                lblLoginInfo.Visible = true;
                lblLoginInfo.ForeColor = Color.Goldenrod;

                txtEmail.Text = profile.Gw2Email;
                txtPassword.Text = "";

                lblPasswordSaved.Visible = !string.IsNullOrWhiteSpace(profile.Gw2PasswordProtected);

                chkAutoPlay.Text = "Auto Play (press Enter on character selection)";
                chkAutoPlay.Checked = profile.Gw2AutoPlayEnabled;

                lblWarning.Text = "Warning: Storing passwords locally allows auto-login but carries risk.";
                lblWarning.ForeColor = Color.Red;

                // Hidden/Shown controls
                chkAutoSelectChar.Visible = false;
                lblCharName.Visible = false;
                txtCharName.Visible = false;
                
                chkAutoPlay.Visible = true;
            }

            UpdateUiState();
        }
        
        public void SaveProfile(GameProfile profile)
        {
            // If the passed profile is different, we should probably warn or just use mapped one.
            // Using logic from ProfileSettingsForm to save back.
            
            bool isGw1 = profile.GameType == GameType.GuildWars1;

            if (isGw1)
            {
                profile.Gw1AutoLoginEnabled = chkAutoLogin.Checked;
                profile.Gw1Email = txtEmail.Text.Trim();
                profile.Gw1AutoSelectCharacterEnabled = chkAutoSelectChar.Checked;
                profile.Gw1CharacterName = txtCharName.Text.Trim();

                var pw = txtPassword.Text;
                if (!string.IsNullOrWhiteSpace(pw))
                {
                    profile.Gw1PasswordProtected = DpapiProtector.ProtectToBase64(pw);
                }
            }
            else
            {
                profile.Gw2AutoLoginEnabled = chkAutoLogin.Checked;
                profile.Gw2Email = txtEmail.Text.Trim();
                profile.Gw2AutoPlayEnabled = chkAutoPlay.Checked;

                var pw = txtPassword.Text;
                if (!string.IsNullOrWhiteSpace(pw))
                {
                    profile.Gw2PasswordProtected = DpapiProtector.ProtectToBase64(pw);
                }
            }
        }

        private void UpdateUiState()
        {
            bool enabled = chkAutoLogin.Checked;

            lblEmail.Enabled = enabled;
            txtEmail.Enabled = enabled;
            lblPassword.Enabled = enabled;
            txtPassword.Enabled = enabled;
            lblPasswordSaved.Enabled = enabled;

            if (chkAutoSelectChar.Visible)
            {
                chkAutoSelectChar.Enabled = enabled;
                bool charEnabled = enabled && chkAutoSelectChar.Checked;
                lblCharName.Enabled = charEnabled;
                txtCharName.Enabled = charEnabled;
            }

            if (chkAutoPlay.Visible)
            {
                chkAutoPlay.Enabled = enabled;
            }
            
            // Warnings usually only relevant if enabled
            if (lblWarning != null) lblWarning.Visible = enabled;
            if (lblLoginInfo != null && lblLoginInfo.Visible) lblLoginInfo.Enabled = enabled;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
            
            // Fix Color for Labels/Specifics if not handled by generic applier
            // (Generic usually handles ForeColor/BackColor)
        }
    }
}
