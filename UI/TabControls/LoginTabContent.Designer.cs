using System.Windows.Forms;
using System.Drawing;

namespace GWxLauncher.UI.TabControls
{
    partial class LoginTabContent
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.chkAutoLogin = new System.Windows.Forms.CheckBox();
            this.lblLoginInfo = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPasswordSaved = new System.Windows.Forms.Label();
            this.chkAutoSelectChar = new System.Windows.Forms.CheckBox();
            this.lblCharName = new System.Windows.Forms.Label();
            this.txtCharName = new System.Windows.Forms.TextBox();
            this.chkAutoPlay = new System.Windows.Forms.CheckBox();
            this.lblWarning = new System.Windows.Forms.Label();

            this.tlpMain.SuspendLayout();
            this.SuspendLayout();

            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.chkAutoLogin, 0, 0);
            this.tlpMain.Controls.Add(this.lblLoginInfo, 0, 1);
            this.tlpMain.Controls.Add(this.lblEmail, 0, 2);
            this.tlpMain.Controls.Add(this.txtEmail, 1, 2);
            this.tlpMain.Controls.Add(this.lblPassword, 0, 3);
            this.tlpMain.Controls.Add(this.txtPassword, 1, 3);
            this.tlpMain.Controls.Add(this.lblPasswordSaved, 1, 4);
            this.tlpMain.Controls.Add(this.chkAutoSelectChar, 0, 5);
            this.tlpMain.Controls.Add(this.lblCharName, 0, 6);
            this.tlpMain.Controls.Add(this.txtCharName, 1, 6);
            this.tlpMain.Controls.Add(this.chkAutoPlay, 0, 7);
            this.tlpMain.Controls.Add(this.lblWarning, 0, 8);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpMain.Location = new System.Drawing.Point(15, 15);
            this.tlpMain.AutoSize = true;
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 9;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // AutoLogin
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Info
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Email
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Password
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // PasswordSaved
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // AutoSelectChar
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // CharName
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // AutoPlay
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize)); // Warning
            this.tlpMain.TabIndex = 0;

            // Common Margin/Padding
            Padding controlMargin = new Padding(4);
            
            // 
            // chkAutoLogin
            // 
            this.tlpMain.SetColumnSpan(this.chkAutoLogin, 2);
            this.chkAutoLogin.AutoSize = true;
            this.chkAutoLogin.Name = "chkAutoLogin";
            this.chkAutoLogin.Text = "Enable auto-login";
            this.chkAutoLogin.UseVisualStyleBackColor = true;
            this.chkAutoLogin.Margin = controlMargin;

            // 
            // lblLoginInfo
            // 
            this.tlpMain.SetColumnSpan(this.lblLoginInfo, 2);
            this.lblLoginInfo.AutoSize = true;
            this.lblLoginInfo.Name = "lblLoginInfo";
            this.lblLoginInfo.Text = "Login Info";
            this.lblLoginInfo.Margin = controlMargin;
            this.lblLoginInfo.MaximumSize = new Size(500, 0);

            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Text = "Email / Account:";
            this.lblEmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblEmail.Dock = DockStyle.Fill;
            this.lblEmail.Margin = controlMargin;

            // 
            // txtEmail
            // 
            this.txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Margin = controlMargin;

            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Text = "Password:";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPassword.Dock = DockStyle.Fill;
            this.lblPassword.Margin = controlMargin;

            // 
            // txtPassword
            // 
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.Margin = controlMargin;

            // 
            // lblPasswordSaved
            // 
            this.lblPasswordSaved.AutoSize = true;
            this.lblPasswordSaved.Name = "lblPasswordSaved";
            this.lblPasswordSaved.Text = "Password stored securely.";
            this.lblPasswordSaved.Margin = controlMargin;
            this.lblPasswordSaved.Visible = false;

            // 
            // chkAutoSelectChar
            // 
            this.tlpMain.SetColumnSpan(this.chkAutoSelectChar, 2);
            this.chkAutoSelectChar.AutoSize = true;
            this.chkAutoSelectChar.Name = "chkAutoSelectChar";
            this.chkAutoSelectChar.Text = "Auto-select character";
            this.chkAutoSelectChar.UseVisualStyleBackColor = true;
            this.chkAutoSelectChar.Margin = controlMargin;

            // 
            // lblCharName
            // 
            this.lblCharName.AutoSize = true;
            this.lblCharName.Name = "lblCharName";
            this.lblCharName.Text = "Character Name:";
            this.lblCharName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCharName.Dock = DockStyle.Fill;
            this.lblCharName.Margin = controlMargin;

            // 
            // txtCharName
            // 
            this.txtCharName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCharName.Name = "txtCharName";
            this.txtCharName.Margin = controlMargin;

            // 
            // chkAutoPlay
            // 
            this.tlpMain.SetColumnSpan(this.chkAutoPlay, 2);
            this.chkAutoPlay.AutoSize = true;
            this.chkAutoPlay.Name = "chkAutoPlay";
            this.chkAutoPlay.Text = "Note: If \"autologin\" works but not \"autoplay\", use -autologin in args.";
            this.chkAutoPlay.UseVisualStyleBackColor = true;
            this.chkAutoPlay.Margin = controlMargin;

            // 
            // lblWarning
            // 
            this.tlpMain.SetColumnSpan(this.lblWarning, 2);
            this.lblWarning.AutoSize = true;
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Text = "Warning Text";
            this.lblWarning.Margin = controlMargin;
            this.lblWarning.MaximumSize = new Size(500, 0);

            // 
            // LoginTabContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tlpMain);
            this.Name = "LoginTabContent";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.Size = new System.Drawing.Size(550, 450);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.CheckBox chkAutoLogin;
        private System.Windows.Forms.Label lblLoginInfo;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPasswordSaved;
        private System.Windows.Forms.CheckBox chkAutoSelectChar;
        private System.Windows.Forms.Label lblCharName;
        private System.Windows.Forms.TextBox txtCharName;
        private System.Windows.Forms.CheckBox chkAutoPlay;
        private System.Windows.Forms.Label lblWarning;
    }
}
