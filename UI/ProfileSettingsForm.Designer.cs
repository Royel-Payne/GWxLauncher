using static System.Net.Mime.MediaTypeNames;

namespace GWxLauncher.UI
{
    partial class ProfileSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtProfileName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtExecutablePath = new TextBox();
            btnBrowseExe = new Button();
            grpGw1Mods = new GroupBox();
            btnBrowseGModDll = new Button();
            txtGModDll = new TextBox();
            chkGMod = new CheckBox();
            btnBrowsePy4GwDll = new Button();
            txtPy4GwDll = new TextBox();
            chkPy4Gw = new CheckBox();
            btnBrowseToolboxDll = new Button();
            txtToolboxDll = new TextBox();
            chkToolbox = new CheckBox();
            lblGw1GModPlugins = new Label();
            lvGw1GModPlugins = new ListView();
            colGw1GModPlugin = new ColumnHeader();
            btnGw1AddPlugin = new Button();
            btnGw1RemovePlugin = new Button();
            chkGw1Multiclient = new CheckBox();
            grpGw2RunAfter = new GroupBox();
            chkGw2RunAfterEnabled = new CheckBox();
            lvGw2RunAfter = new ListView();
            colGw2RunAfterName = new ColumnHeader();
            colGw2RunAfterPath = new ColumnHeader();
            btnGw2AddProgram = new Button();
            btnGw2RemoveProgram = new Button();
            grpGw1Login = new GroupBox();
            lblGw1LoginWarning = new Label();
            lblGw1PasswordSaved = new Label();
            txtGw1CharacterName = new TextBox();
            lblGw1CharacterName = new Label();
            chkGw1AutoSelectCharacter = new CheckBox();
            txtGw1Password = new TextBox();
            lblGw1Password = new Label();
            txtGw1Email = new TextBox();
            lblGw1Email = new Label();
            chkGw1AutoLogin = new CheckBox();
            btnOk = new Button();
            btnCancel = new Button();
            grpGw2Login = new GroupBox();
            chkGw2AutoLogin = new CheckBox();
            lblGw2LoginInfo = new Label();
            lblGw2Email = new Label();
            txtGw2Email = new TextBox();
            lblGw2Password = new Label();
            txtGw2Password = new TextBox();
            lblGw2PasswordSaved = new Label();
            chkGw2AutoPlay = new CheckBox();
            grpGw1Mods.SuspendLayout();
            grpGw2RunAfter.SuspendLayout();
            grpGw1Login.SuspendLayout();
            grpGw2Login.SuspendLayout();
            SuspendLayout();
            // 
            // txtProfileName
            // 
            txtProfileName.Location = new Point(98, 23);
            txtProfileName.Name = "txtProfileName";
            txtProfileName.Size = new Size(187, 23);
            txtProfileName.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 27);
            label1.Name = "label1";
            label1.Size = new Size(80, 15);
            label1.TabIndex = 1;
            label1.Text = "Display Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(309, 27);
            label2.Name = "label2";
            label2.Size = new Size(97, 15);
            label2.TabIndex = 2;
            label2.Text = "Game executable";
            // 
            // txtExecutablePath
            // 
            txtExecutablePath.Location = new Point(418, 23);
            txtExecutablePath.Name = "txtExecutablePath";
            txtExecutablePath.Size = new Size(187, 23);
            txtExecutablePath.TabIndex = 3;
            // 
            // btnBrowseExe
            // 
            btnBrowseExe.Location = new Point(615, 23);
            btnBrowseExe.Name = "btnBrowseExe";
            btnBrowseExe.Size = new Size(75, 23);
            btnBrowseExe.TabIndex = 4;
            btnBrowseExe.Text = "Browse...";
            btnBrowseExe.UseVisualStyleBackColor = true;
            // 
            // grpGw1Mods
            // 
            grpGw1Mods.Controls.Add(btnBrowseGModDll);
            grpGw1Mods.Controls.Add(txtGModDll);
            grpGw1Mods.Controls.Add(chkGMod);
            grpGw1Mods.Controls.Add(btnBrowsePy4GwDll);
            grpGw1Mods.Controls.Add(txtPy4GwDll);
            grpGw1Mods.Controls.Add(chkPy4Gw);
            grpGw1Mods.Controls.Add(btnBrowseToolboxDll);
            grpGw1Mods.Controls.Add(txtToolboxDll);
            grpGw1Mods.Controls.Add(chkToolbox);
            grpGw1Mods.Controls.Add(lblGw1GModPlugins);
            grpGw1Mods.Controls.Add(lvGw1GModPlugins);
            grpGw1Mods.Controls.Add(btnGw1AddPlugin);
            grpGw1Mods.Controls.Add(btnGw1RemovePlugin);
            grpGw1Mods.Location = new Point(309, 86);
            grpGw1Mods.Name = "grpGw1Mods";
            grpGw1Mods.Size = new Size(400, 257);
            grpGw1Mods.TabIndex = 5;
            grpGw1Mods.TabStop = false;
            grpGw1Mods.Text = "GW1 Mods";
            // 
            // btnBrowseGModDll
            // 
            btnBrowseGModDll.Location = new Point(306, 85);
            btnBrowseGModDll.Name = "btnBrowseGModDll";
            btnBrowseGModDll.Size = new Size(75, 23);
            btnBrowseGModDll.TabIndex = 8;
            btnBrowseGModDll.Text = "Browse...";
            btnBrowseGModDll.UseVisualStyleBackColor = true;
            // 
            // txtGModDll
            // 
            txtGModDll.Location = new Point(109, 85);
            txtGModDll.Name = "txtGModDll";
            txtGModDll.Size = new Size(187, 23);
            txtGModDll.TabIndex = 7;
            // 
            // chkGMod
            // 
            chkGMod.AutoSize = true;
            chkGMod.Location = new Point(19, 87);
            chkGMod.Name = "chkGMod";
            chkGMod.Size = new Size(58, 19);
            chkGMod.TabIndex = 6;
            chkGMod.Text = "gMod";
            chkGMod.UseVisualStyleBackColor = true;
            // 
            // btnBrowsePy4GwDll
            // 
            btnBrowsePy4GwDll.Location = new Point(306, 53);
            btnBrowsePy4GwDll.Name = "btnBrowsePy4GwDll";
            btnBrowsePy4GwDll.Size = new Size(75, 23);
            btnBrowsePy4GwDll.TabIndex = 5;
            btnBrowsePy4GwDll.Text = "Browse...";
            btnBrowsePy4GwDll.UseVisualStyleBackColor = true;
            // 
            // txtPy4GwDll
            // 
            txtPy4GwDll.Location = new Point(109, 53);
            txtPy4GwDll.Name = "txtPy4GwDll";
            txtPy4GwDll.Size = new Size(187, 23);
            txtPy4GwDll.TabIndex = 4;
            // 
            // chkPy4Gw
            // 
            chkPy4Gw.AutoSize = true;
            chkPy4Gw.Location = new Point(19, 55);
            chkPy4Gw.Name = "chkPy4Gw";
            chkPy4Gw.Size = new Size(64, 19);
            chkPy4Gw.TabIndex = 3;
            chkPy4Gw.Text = "Py4GW";
            chkPy4Gw.UseVisualStyleBackColor = true;
            // 
            // btnBrowseToolboxDll
            // 
            btnBrowseToolboxDll.Location = new Point(306, 21);
            btnBrowseToolboxDll.Name = "btnBrowseToolboxDll";
            btnBrowseToolboxDll.Size = new Size(75, 23);
            btnBrowseToolboxDll.TabIndex = 2;
            btnBrowseToolboxDll.Text = "Browse...";
            btnBrowseToolboxDll.UseVisualStyleBackColor = true;
            // 
            // txtToolboxDll
            // 
            txtToolboxDll.Location = new Point(109, 21);
            txtToolboxDll.Name = "txtToolboxDll";
            txtToolboxDll.Size = new Size(187, 23);
            txtToolboxDll.TabIndex = 1;
            // 
            // chkToolbox
            // 
            chkToolbox.AutoSize = true;
            chkToolbox.Location = new Point(19, 23);
            chkToolbox.Name = "chkToolbox";
            chkToolbox.Size = new Size(68, 19);
            chkToolbox.TabIndex = 0;
            chkToolbox.Text = "Toolbox";
            chkToolbox.UseVisualStyleBackColor = true;
            // 
            // lblGw1GModPlugins
            // 
            lblGw1GModPlugins.AutoSize = true;
            lblGw1GModPlugins.Location = new Point(19, 120);
            lblGw1GModPlugins.Name = "lblGw1GModPlugins";
            lblGw1GModPlugins.Size = new Size(81, 15);
            lblGw1GModPlugins.TabIndex = 9;
            lblGw1GModPlugins.Text = "gMod plugins";
            // 
            // lvGw1GModPlugins
            // 
            lvGw1GModPlugins.Columns.AddRange(new ColumnHeader[] { colGw1GModPlugin });
            lvGw1GModPlugins.FullRowSelect = true;
            lvGw1GModPlugins.HeaderStyle = ColumnHeaderStyle.None;
            lvGw1GModPlugins.Location = new Point(19, 140);
            lvGw1GModPlugins.MultiSelect = false;
            lvGw1GModPlugins.Name = "lvGw1GModPlugins";
            lvGw1GModPlugins.Size = new Size(277, 101);
            lvGw1GModPlugins.TabIndex = 9;
            lvGw1GModPlugins.UseCompatibleStateImageBehavior = false;
            lvGw1GModPlugins.View = View.Details;
            // 
            // colGw1GModPlugin
            // 
            colGw1GModPlugin.Text = "Plugin";
            colGw1GModPlugin.Width = 260;
            // 
            // btnGw1AddPlugin
            // 
            btnGw1AddPlugin.Location = new Point(306, 140);
            btnGw1AddPlugin.Name = "btnGw1AddPlugin";
            btnGw1AddPlugin.Size = new Size(75, 23);
            btnGw1AddPlugin.TabIndex = 10;
            btnGw1AddPlugin.Text = "Add...";
            btnGw1AddPlugin.UseVisualStyleBackColor = true;
            // 
            // btnGw1RemovePlugin
            // 
            btnGw1RemovePlugin.Enabled = false;
            btnGw1RemovePlugin.Location = new Point(306, 169);
            btnGw1RemovePlugin.Name = "btnGw1RemovePlugin";
            btnGw1RemovePlugin.Size = new Size(75, 23);
            btnGw1RemovePlugin.TabIndex = 11;
            btnGw1RemovePlugin.Text = "Remove";
            btnGw1RemovePlugin.UseVisualStyleBackColor = true;
            // 
            // chkGw1Multiclient
            // 
            chkGw1Multiclient.AutoSize = true;
            chkGw1Multiclient.Location = new Point(328, 48);
            chkGw1Multiclient.Name = "chkGw1Multiclient";
            chkGw1Multiclient.Size = new Size(83, 19);
            chkGw1Multiclient.TabIndex = 9;
            chkGw1Multiclient.Text = "Multiclient";
            chkGw1Multiclient.UseVisualStyleBackColor = true;
            // 
            // grpGw2RunAfter
            // 
            grpGw2RunAfter.Controls.Add(chkGw2RunAfterEnabled);
            grpGw2RunAfter.Controls.Add(lvGw2RunAfter);
            grpGw2RunAfter.Controls.Add(btnGw2AddProgram);
            grpGw2RunAfter.Controls.Add(btnGw2RemoveProgram);
            grpGw2RunAfter.Location = new Point(309, 86);
            grpGw2RunAfter.Name = "grpGw2RunAfter";
            grpGw2RunAfter.Size = new Size(400, 143);
            grpGw2RunAfter.TabIndex = 5;
            grpGw2RunAfter.TabStop = false;
            grpGw2RunAfter.Text = "Run after launching";
            grpGw2RunAfter.Visible = false;
            // 
            // chkGw2RunAfterEnabled
            // 
            chkGw2RunAfterEnabled.AutoSize = true;
            chkGw2RunAfterEnabled.Location = new Point(16, 24);
            chkGw2RunAfterEnabled.Name = "chkGw2RunAfterEnabled";
            chkGw2RunAfterEnabled.Size = new Size(181, 19);
            chkGw2RunAfterEnabled.TabIndex = 0;
            chkGw2RunAfterEnabled.Text = "Enable programs after launch";
            chkGw2RunAfterEnabled.UseVisualStyleBackColor = true;
            // 
            // lvGw2RunAfter
            // 
            lvGw2RunAfter.CheckBoxes = true;
            lvGw2RunAfter.Columns.AddRange(new ColumnHeader[] { colGw2RunAfterName, colGw2RunAfterPath });
            lvGw2RunAfter.FullRowSelect = true;
            lvGw2RunAfter.HeaderStyle = ColumnHeaderStyle.None;
            lvGw2RunAfter.Location = new Point(16, 52);
            lvGw2RunAfter.MultiSelect = false;
            lvGw2RunAfter.Name = "lvGw2RunAfter";
            lvGw2RunAfter.Size = new Size(280, 78);
            lvGw2RunAfter.TabIndex = 1;
            lvGw2RunAfter.UseCompatibleStateImageBehavior = false;
            lvGw2RunAfter.View = View.Details;
            lvGw2RunAfter.ItemChecked += lvGw2RunAfter_ItemChecked;
            // 
            // colGw2RunAfterName
            // 
            colGw2RunAfterName.Text = "Name";
            colGw2RunAfterName.Width = 90;
            // 
            // colGw2RunAfterPath
            // 
            colGw2RunAfterPath.Text = "Path";
            colGw2RunAfterPath.Width = 170;
            // 
            // btnGw2AddProgram
            // 
            btnGw2AddProgram.Location = new Point(306, 52);
            btnGw2AddProgram.Name = "btnGw2AddProgram";
            btnGw2AddProgram.Size = new Size(75, 23);
            btnGw2AddProgram.TabIndex = 2;
            btnGw2AddProgram.Text = "Add...";
            btnGw2AddProgram.UseVisualStyleBackColor = true;
            btnGw2AddProgram.Click += btnGw2AddProgram_Click;
            // 
            // btnGw2RemoveProgram
            // 
            btnGw2RemoveProgram.Enabled = false;
            btnGw2RemoveProgram.Location = new Point(306, 81);
            btnGw2RemoveProgram.Name = "btnGw2RemoveProgram";
            btnGw2RemoveProgram.Size = new Size(75, 23);
            btnGw2RemoveProgram.TabIndex = 3;
            btnGw2RemoveProgram.Text = "Remove";
            btnGw2RemoveProgram.UseVisualStyleBackColor = true;
            btnGw2RemoveProgram.Click += btnGw2RemoveProgram_Click;
            // 
            // grpGw1Login
            // 
            grpGw1Login.Controls.Add(lblGw1LoginWarning);
            grpGw1Login.Controls.Add(lblGw1PasswordSaved);
            grpGw1Login.Controls.Add(txtGw1CharacterName);
            grpGw1Login.Controls.Add(lblGw1CharacterName);
            grpGw1Login.Controls.Add(chkGw1AutoSelectCharacter);
            grpGw1Login.Controls.Add(txtGw1Password);
            grpGw1Login.Controls.Add(lblGw1Password);
            grpGw1Login.Controls.Add(txtGw1Email);
            grpGw1Login.Controls.Add(lblGw1Email);
            grpGw1Login.Controls.Add(chkGw1AutoLogin);
            grpGw1Login.Location = new Point(12, 86);
            grpGw1Login.Name = "grpGw1Login";
            grpGw1Login.Size = new Size(273, 257);
            grpGw1Login.TabIndex = 5;
            grpGw1Login.TabStop = false;
            grpGw1Login.Text = "GW1 Login";
            // 
            // lblGw1LoginWarning
            // 
            lblGw1LoginWarning.AutoSize = true;
            lblGw1LoginWarning.Location = new Point(16, 45);
            lblGw1LoginWarning.Name = "lblGw1LoginWarning";
            lblGw1LoginWarning.Size = new Size(205, 15);
            lblGw1LoginWarning.TabIndex = 9;
            lblGw1LoginWarning.Text = "Auto-login stores credentials (DPAPI).";
            lblGw1LoginWarning.Visible = false;
            // 
            // lblGw1PasswordSaved
            // 
            lblGw1PasswordSaved.AutoSize = true;
            lblGw1PasswordSaved.Location = new Point(16, 153);
            lblGw1PasswordSaved.Name = "lblGw1PasswordSaved";
            lblGw1PasswordSaved.Size = new Size(105, 15);
            lblGw1PasswordSaved.TabIndex = 5;
            lblGw1PasswordSaved.Text = "🔒 Password saved";
            lblGw1PasswordSaved.Visible = false;
            // 
            // txtGw1CharacterName
            // 
            txtGw1CharacterName.Location = new Point(16, 218);
            txtGw1CharacterName.Name = "txtGw1CharacterName";
            txtGw1CharacterName.Size = new Size(241, 23);
            txtGw1CharacterName.TabIndex = 8;
            // 
            // lblGw1CharacterName
            // 
            lblGw1CharacterName.AutoSize = true;
            lblGw1CharacterName.Location = new Point(16, 200);
            lblGw1CharacterName.Name = "lblGw1CharacterName";
            lblGw1CharacterName.Size = new Size(93, 15);
            lblGw1CharacterName.TabIndex = 7;
            lblGw1CharacterName.Text = "Character Name";
            // 
            // chkGw1AutoSelectCharacter
            // 
            chkGw1AutoSelectCharacter.AutoSize = true;
            chkGw1AutoSelectCharacter.Location = new Point(16, 176);
            chkGw1AutoSelectCharacter.Name = "chkGw1AutoSelectCharacter";
            chkGw1AutoSelectCharacter.Size = new Size(141, 19);
            chkGw1AutoSelectCharacter.TabIndex = 6;
            chkGw1AutoSelectCharacter.Text = "Auto-select Character";
            chkGw1AutoSelectCharacter.UseVisualStyleBackColor = true;
            // 
            // txtGw1Password
            // 
            txtGw1Password.Location = new Point(16, 127);
            txtGw1Password.Name = "txtGw1Password";
            txtGw1Password.Size = new Size(241, 23);
            txtGw1Password.TabIndex = 4;
            txtGw1Password.UseSystemPasswordChar = true;
            // 
            // lblGw1Password
            // 
            lblGw1Password.AutoSize = true;
            lblGw1Password.Location = new Point(16, 109);
            lblGw1Password.Name = "lblGw1Password";
            lblGw1Password.Size = new Size(57, 15);
            lblGw1Password.TabIndex = 3;
            lblGw1Password.Text = "Password";
            // 
            // txtGw1Email
            // 
            txtGw1Email.Location = new Point(16, 77);
            txtGw1Email.Name = "txtGw1Email";
            txtGw1Email.Size = new Size(241, 23);
            txtGw1Email.TabIndex = 2;
            // 
            // lblGw1Email
            // 
            lblGw1Email.AutoSize = true;
            lblGw1Email.Location = new Point(16, 59);
            lblGw1Email.Name = "lblGw1Email";
            lblGw1Email.Size = new Size(36, 15);
            lblGw1Email.TabIndex = 1;
            lblGw1Email.Text = "Email";
            // 
            // chkGw1AutoLogin
            // 
            chkGw1AutoLogin.AutoSize = true;
            chkGw1AutoLogin.Location = new Point(16, 25);
            chkGw1AutoLogin.Name = "chkGw1AutoLogin";
            chkGw1AutoLogin.Size = new Size(153, 19);
            chkGw1AutoLogin.TabIndex = 0;
            chkGw1AutoLogin.Text = "Enable GW1 Auto-Login";
            chkGw1AutoLogin.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(530, 371);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 6;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(615, 371);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpGw2Login
            // 
            grpGw2Login.Controls.Add(chkGw2AutoLogin);
            grpGw2Login.Controls.Add(lblGw2LoginInfo);
            grpGw2Login.Controls.Add(lblGw2Email);
            grpGw2Login.Controls.Add(txtGw2Email);
            grpGw2Login.Controls.Add(lblGw2Password);
            grpGw2Login.Controls.Add(txtGw2Password);
            grpGw2Login.Controls.Add(lblGw2PasswordSaved);
            grpGw2Login.Controls.Add(chkGw2AutoPlay);
            grpGw2Login.Location = new Point(12, 86);
            grpGw2Login.Name = "grpGw2Login";
            grpGw2Login.Size = new Size(273, 257);
            grpGw2Login.TabIndex = 5;
            grpGw2Login.TabStop = false;
            grpGw2Login.Text = "GW2 Login";
            grpGw2Login.Visible = false;
            // 
            // chkGw2AutoLogin
            // 
            chkGw2AutoLogin.AutoSize = true;
            chkGw2AutoLogin.Location = new Point(16, 25);
            chkGw2AutoLogin.Name = "chkGw2AutoLogin";
            chkGw2AutoLogin.Size = new Size(153, 19);
            chkGw2AutoLogin.TabIndex = 0;
            chkGw2AutoLogin.Text = "Enable GW2 Auto-Login";
            chkGw2AutoLogin.UseVisualStyleBackColor = true;
            // 
            // lblGw2LoginInfo
            // 
            lblGw2LoginInfo.AutoSize = true;
            lblGw2LoginInfo.Location = new Point(16, 45);
            lblGw2LoginInfo.Name = "lblGw2LoginInfo";
            lblGw2LoginInfo.Size = new Size(227, 15);
            lblGw2LoginInfo.TabIndex = 1;
            lblGw2LoginInfo.Text = "Uses launcher UI automation (best effort).";
            // 
            // lblGw2Email
            // 
            lblGw2Email.AutoSize = true;
            lblGw2Email.Location = new Point(16, 64);
            lblGw2Email.Name = "lblGw2Email";
            lblGw2Email.Size = new Size(36, 15);
            lblGw2Email.TabIndex = 2;
            lblGw2Email.Text = "Email";
            // 
            // txtGw2Email
            // 
            txtGw2Email.Location = new Point(16, 82);
            txtGw2Email.Name = "txtGw2Email";
            txtGw2Email.Size = new Size(241, 23);
            txtGw2Email.TabIndex = 3;
            // 
            // lblGw2Password
            // 
            lblGw2Password.AutoSize = true;
            lblGw2Password.Location = new Point(16, 112);
            lblGw2Password.Name = "lblGw2Password";
            lblGw2Password.Size = new Size(57, 15);
            lblGw2Password.TabIndex = 4;
            lblGw2Password.Text = "Password";
            // 
            // txtGw2Password
            // 
            txtGw2Password.Location = new Point(16, 130);
            txtGw2Password.Name = "txtGw2Password";
            txtGw2Password.Size = new Size(241, 23);
            txtGw2Password.TabIndex = 5;
            txtGw2Password.UseSystemPasswordChar = true;
            // 
            // lblGw2PasswordSaved
            // 
            lblGw2PasswordSaved.AutoSize = true;
            lblGw2PasswordSaved.Location = new Point(16, 156);
            lblGw2PasswordSaved.Name = "lblGw2PasswordSaved";
            lblGw2PasswordSaved.Size = new Size(105, 15);
            lblGw2PasswordSaved.TabIndex = 6;
            lblGw2PasswordSaved.Text = "🔒 Password saved";
            lblGw2PasswordSaved.Visible = false;
            // 
            // chkGw2AutoPlay
            // 
            chkGw2AutoPlay.AutoSize = true;
            chkGw2AutoPlay.Location = new Point(16, 182);
            chkGw2AutoPlay.Name = "chkGw2AutoPlay";
            chkGw2AutoPlay.Size = new Size(161, 19);
            chkGw2AutoPlay.TabIndex = 7;
            chkGw2AutoPlay.Text = "Auto-click Play (optional)";
            chkGw2AutoPlay.UseVisualStyleBackColor = true;
            // 
            // ProfileSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(721, 406);
            Controls.Add(grpGw2RunAfter);
            Controls.Add(grpGw1Mods);
            Controls.Add(chkGw1Multiclient);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(btnBrowseExe);
            Controls.Add(txtExecutablePath);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtProfileName);
            Controls.Add(grpGw1Login);
            Controls.Add(grpGw2Login);
            Name = "ProfileSettingsForm";
            Text = "ProfileSettingsForm";
            grpGw1Mods.ResumeLayout(false);
            grpGw1Mods.PerformLayout();
            grpGw2RunAfter.ResumeLayout(false);
            grpGw2RunAfter.PerformLayout();
            grpGw1Login.ResumeLayout(false);
            grpGw1Login.PerformLayout();
            grpGw2Login.ResumeLayout(false);
            grpGw2Login.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtProfileName;
        private Label label1;
        private Label label2;
        private TextBox txtExecutablePath;
        private Button btnBrowseExe;
        private GroupBox grpGw1Mods;
        private TextBox txtToolboxDll;
        private CheckBox chkToolbox;
        private Button btnBrowseGModDll;
        private TextBox txtGModDll;
        private CheckBox chkGMod;
        private Button btnBrowsePy4GwDll;
        private TextBox txtPy4GwDll;
        private CheckBox chkPy4Gw;
        private Button btnBrowseToolboxDll;
        private CheckBox chkGw1Multiclient;
        private Button btnOk;
        private Button btnCancel;
        // GW2 "Run after launching"
        private GroupBox grpGw2RunAfter;
        private CheckBox chkGw2RunAfterEnabled;
        private ListView lvGw2RunAfter;
        private Button btnGw2AddProgram;
        private Button btnGw2RemoveProgram;
        private ColumnHeader colGw2RunAfterName;
        private ColumnHeader colGw2RunAfterPath;
        private GroupBox grpGw2Login;
        private CheckBox chkGw2AutoLogin;
        private Label lblGw2LoginInfo;
        private Label lblGw2Email;
        private TextBox txtGw2Email;
        private Label lblGw2Password;
        private TextBox txtGw2Password;
        private Label lblGw2PasswordSaved;
        private CheckBox chkGw2AutoPlay;
        // GW1 "gMod plugins UI"
        private ListView lvGw1GModPlugins;
        private ColumnHeader colGw1GModPlugin;
        private Button btnGw1AddPlugin;
        private Button btnGw1RemovePlugin;
        private Label lblGw1GModPlugins;
        private GroupBox grpGw1Login;
        private CheckBox chkGw1AutoLogin;
        private Label lblGw1Email;
        private TextBox txtGw1Email;
        private Label lblGw1Password;
        private TextBox txtGw1Password;
        private Label lblGw1PasswordSaved;
        private CheckBox chkGw1AutoSelectCharacter;
        private Label lblGw1CharacterName;
        private TextBox txtGw1CharacterName;
        private Label lblGw1LoginWarning;


    }
}