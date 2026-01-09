using GWxLauncher.Domain;

namespace GWxLauncher.UI.TabControls
{
    public partial class LoginTabContent : UserControl
    {
        public LoginTabContent()
        {
            InitializeComponent();
            ApplyTheme();
        }

        public void ArrangeControls(Control[] gw1Controls, Control[] gw2Controls)
        {
            pnlGw1Login.Controls.Clear();
            pnlGw2Login.Controls.Clear();

            if (gw1Controls != null && gw1Controls.Length > 0)
                LayoutGw1Controls(gw1Controls);

            if (gw2Controls != null && gw2Controls.Length > 0)
                LayoutGw2Controls(gw2Controls);
        }

        private void LayoutGw1Controls(Control[] controls)
        {
            var chkAutoLogin = Array.Find(controls, c => c.Name == "chkGw1AutoLogin");
            var lblEmail = Array.Find(controls, c => c.Name == "lblGw1Email") as Label;
            var txtEmail = Array.Find(controls, c => c.Name == "txtGw1Email");
            var lblPassword = Array.Find(controls, c => c.Name == "lblGw1Password") as Label;
            var txtPassword = Array.Find(controls, c => c.Name == "txtGw1Password");
            var lblPasswordSaved = Array.Find(controls, c => c.Name == "lblGw1PasswordSaved");
            var chkAutoSelectChar = Array.Find(controls, c => c.Name == "chkGw1AutoSelectCharacter");
            var lblCharName = Array.Find(controls, c => c.Name == "lblGw1CharacterName") as Label;
            var txtCharName = Array.Find(controls, c => c.Name == "txtGw1CharacterName");
            var lblWarning = Array.Find(controls, c => c.Name == "lblGw1LoginWarning");

            int y = 0;
            const int leftMargin = 10;  // Match General tab margin
            const int labelWidth = 120;
            const int fieldLeft = 130;
            const int rowHeight = 35;

            if (chkAutoLogin != null)
            {
                chkAutoLogin.Location = new Point(leftMargin, y);
                chkAutoLogin.AutoSize = true;
                pnlGw1Login.Controls.Add(chkAutoLogin);
                y += rowHeight;
            }

            if (lblEmail != null && txtEmail != null)
            {
                lblEmail.Location = new Point(leftMargin, y + 3);
                lblEmail.AutoSize = false;
                lblEmail.Size = new Size(labelWidth, 20);
                lblEmail.TextAlign = ContentAlignment.MiddleLeft;
                pnlGw1Login.Controls.Add(lblEmail);

                txtEmail.Location = new Point(fieldLeft, y);
                txtEmail.Width = 350;
                pnlGw1Login.Controls.Add(txtEmail);
                y += rowHeight;
            }

            if (lblPassword != null && txtPassword != null)
            {
                lblPassword.Location = new Point(leftMargin, y + 3);
                lblPassword.AutoSize = false;
                lblPassword.Size = new Size(labelWidth, 20);
                lblPassword.TextAlign = ContentAlignment.MiddleLeft;
                pnlGw1Login.Controls.Add(lblPassword);

                txtPassword.Location = new Point(fieldLeft, y);
                txtPassword.Width = 350;
                pnlGw1Login.Controls.Add(txtPassword);
                y += 25;
            }

            if (lblPasswordSaved != null)
            {
                lblPasswordSaved.Location = new Point(fieldLeft, y);
                lblPasswordSaved.AutoSize = true;
                pnlGw1Login.Controls.Add(lblPasswordSaved);
                y += 35;
            }

            if (chkAutoSelectChar != null)
            {
                chkAutoSelectChar.Location = new Point(leftMargin, y);
                chkAutoSelectChar.AutoSize = true;
                pnlGw1Login.Controls.Add(chkAutoSelectChar);
                y += rowHeight;
            }

            if (lblCharName != null && txtCharName != null)
            {
                lblCharName.Location = new Point(leftMargin, y + 3);
                lblCharName.AutoSize = false;
                lblCharName.Size = new Size(labelWidth, 20);
                lblCharName.TextAlign = ContentAlignment.MiddleLeft;
                pnlGw1Login.Controls.Add(lblCharName);

                txtCharName.Location = new Point(fieldLeft, y);
                txtCharName.Width = 350;
                pnlGw1Login.Controls.Add(txtCharName);
                y += rowHeight + 10;
            }

            if (lblWarning != null)
            {
                lblWarning.Location = new Point(leftMargin, y);
                lblWarning.MaximumSize = new Size(500, 0);
                lblWarning.AutoSize = true;
                pnlGw1Login.Controls.Add(lblWarning);
                y += lblWarning.Height + 10;
            }

            pnlGw1Login.Height = y;
        }

        private void LayoutGw2Controls(Control[] controls)
        {
            var chkAutoLogin = Array.Find(controls, c => c.Name == "chkGw2AutoLogin");
            var lblLoginInfo = Array.Find(controls, c => c.Name == "lblGw2LoginInfo");
            var lblEmail = Array.Find(controls, c => c.Name == "lblGw2Email") as Label;
            var txtEmail = Array.Find(controls, c => c.Name == "txtGw2Email");
            var lblPassword = Array.Find(controls, c => c.Name == "lblGw2Password") as Label;
            var txtPassword = Array.Find(controls, c => c.Name == "txtGw2Password");
            var lblPasswordSaved = Array.Find(controls, c => c.Name == "lblGw2PasswordSaved");
            var chkAutoPlay = Array.Find(controls, c => c.Name == "chkGw2AutoPlay");
            var lblWarning = Array.Find(controls, c => c.Name == "lblGw2Warning");

            int y = 0;
            const int leftMargin = 10;  // Match General tab margin
            const int labelWidth = 120;
            const int fieldLeft = 130;
            const int rowHeight = 35;

            if (chkAutoLogin != null)
            {
                chkAutoLogin.Location = new Point(leftMargin, y);
                chkAutoLogin.AutoSize = true;
                pnlGw2Login.Controls.Add(chkAutoLogin);
                y += rowHeight;
            }

            if (lblLoginInfo != null)
            {
                lblLoginInfo.Location = new Point(leftMargin, y);
                lblLoginInfo.MaximumSize = new Size(500, 0);
                lblLoginInfo.AutoSize = true;
                pnlGw2Login.Controls.Add(lblLoginInfo);
                y += lblLoginInfo.Height + 10;
            }

            if (lblEmail != null && txtEmail != null)
            {
                lblEmail.Location = new Point(leftMargin, y + 3);
                lblEmail.AutoSize = false;
                lblEmail.Size = new Size(labelWidth, 20);
                lblEmail.TextAlign = ContentAlignment.MiddleLeft;
                pnlGw2Login.Controls.Add(lblEmail);

                txtEmail.Location = new Point(fieldLeft, y);
                txtEmail.Width = 350;
                pnlGw2Login.Controls.Add(txtEmail);
                y += rowHeight;
            }

            if (lblPassword != null && txtPassword != null)
            {
                lblPassword.Location = new Point(leftMargin, y + 3);
                lblPassword.AutoSize = false;
                lblPassword.Size = new Size(labelWidth, 20);
                lblPassword.TextAlign = ContentAlignment.MiddleLeft;
                pnlGw2Login.Controls.Add(lblPassword);

                txtPassword.Location = new Point(fieldLeft, y);
                txtPassword.Width = 350;
                pnlGw2Login.Controls.Add(txtPassword);
                y += 25;
            }

            if (lblPasswordSaved != null)
            {
                lblPasswordSaved.Location = new Point(fieldLeft, y);
                lblPasswordSaved.AutoSize = true;
                pnlGw2Login.Controls.Add(lblPasswordSaved);
                y += 35;
            }

            if (chkAutoPlay != null)
            {
                chkAutoPlay.Location = new Point(leftMargin, y);
                chkAutoPlay.AutoSize = true;
                pnlGw2Login.Controls.Add(chkAutoPlay);
                y += rowHeight + 10;
            }

            if (lblWarning != null)
            {
                lblWarning.Location = new Point(leftMargin, y);
                lblWarning.MaximumSize = new Size(500, 0);
                lblWarning.AutoSize = true;
                pnlGw2Login.Controls.Add(lblWarning);
                y += lblWarning.Height + 10;
            }

            pnlGw2Login.Height = y;
        }

        public void UpdateForGameType(GameType gameType)
        {
            bool isGw1 = gameType == GameType.GuildWars1;
            pnlGw1Login.Visible = isGw1;
            pnlGw2Login.Visible = !isGw1;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            pnlGw1Login.BackColor = ThemeService.Palette.WindowBack;
            pnlGw2Login.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }
    }
}
