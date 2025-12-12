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
            chkToolbox = new CheckBox();
            txtToolboxDll = new TextBox();
            btnBrowseToolboxDll = new Button();
            chkPy4Gw = new CheckBox();
            txtPy4GwDll = new TextBox();
            btnBrowsePy4GwDll = new Button();
            chkGMod = new CheckBox();
            txtGModDll = new TextBox();
            btnBrowseGModDll = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            grpGw1Mods.SuspendLayout();
            SuspendLayout();
            // 
            // txtProfileName
            // 
            txtProfileName.Location = new Point(381, 69);
            txtProfileName.Name = "txtProfileName";
            txtProfileName.Size = new Size(183, 23);
            txtProfileName.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(284, 72);
            label1.Name = "label1";
            label1.Size = new Size(80, 15);
            label1.TabIndex = 1;
            label1.Text = "Display Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(284, 112);
            label2.Name = "label2";
            label2.Size = new Size(97, 15);
            label2.TabIndex = 2;
            label2.Text = "Game executable";
            // 
            // txtExecutablePath
            // 
            txtExecutablePath.Location = new Point(381, 109);
            txtExecutablePath.Name = "txtExecutablePath";
            txtExecutablePath.Size = new Size(183, 23);
            txtExecutablePath.TabIndex = 3;
            // 
            // btnBrowseExe
            // 
            btnBrowseExe.Location = new Point(590, 108);
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
            grpGw1Mods.Location = new Point(284, 153);
            grpGw1Mods.Name = "grpGw1Mods";
            grpGw1Mods.Size = new Size(400, 143);
            grpGw1Mods.TabIndex = 5;
            grpGw1Mods.TabStop = false;
            grpGw1Mods.Text = "GW1 Mods";
            // 
            // chkToolbox
            // 
            chkToolbox.AutoSize = true;
            chkToolbox.Location = new Point(19, 25);
            chkToolbox.Name = "chkToolbox";
            chkToolbox.Size = new Size(68, 19);
            chkToolbox.TabIndex = 0;
            chkToolbox.Text = "Toolbox";
            chkToolbox.UseVisualStyleBackColor = true;
            // 
            // txtToolboxDll
            // 
            txtToolboxDll.Location = new Point(97, 23);
            txtToolboxDll.Name = "txtToolboxDll";
            txtToolboxDll.Size = new Size(183, 23);
            txtToolboxDll.TabIndex = 1;
            // 
            // btnBrowseToolboxDll
            // 
            btnBrowseToolboxDll.Location = new Point(306, 23);
            btnBrowseToolboxDll.Name = "btnBrowseToolboxDll";
            btnBrowseToolboxDll.Size = new Size(75, 23);
            btnBrowseToolboxDll.TabIndex = 2;
            btnBrowseToolboxDll.Text = "Browse...";
            btnBrowseToolboxDll.UseVisualStyleBackColor = true;
            // 
            // chkPy4Gw
            // 
            chkPy4Gw.AutoSize = true;
            chkPy4Gw.Location = new Point(19, 62);
            chkPy4Gw.Name = "chkPy4Gw";
            chkPy4Gw.Size = new Size(64, 19);
            chkPy4Gw.TabIndex = 3;
            chkPy4Gw.Text = "Py4GW";
            chkPy4Gw.UseVisualStyleBackColor = true;
            // 
            // txtPy4GwDll
            // 
            txtPy4GwDll.Location = new Point(97, 60);
            txtPy4GwDll.Name = "txtPy4GwDll";
            txtPy4GwDll.Size = new Size(183, 23);
            txtPy4GwDll.TabIndex = 4;
            // 
            // btnBrowsePy4GwDll
            // 
            btnBrowsePy4GwDll.Location = new Point(306, 60);
            btnBrowsePy4GwDll.Name = "btnBrowsePy4GwDll";
            btnBrowsePy4GwDll.Size = new Size(75, 23);
            btnBrowsePy4GwDll.TabIndex = 5;
            btnBrowsePy4GwDll.Text = "Browse...";
            btnBrowsePy4GwDll.UseVisualStyleBackColor = true;
            // 
            // chkGMod
            // 
            chkGMod.AutoSize = true;
            chkGMod.Location = new Point(19, 99);
            chkGMod.Name = "chkGMod";
            chkGMod.Size = new Size(58, 19);
            chkGMod.TabIndex = 6;
            chkGMod.Text = "gMod";
            chkGMod.UseVisualStyleBackColor = true;
            // 
            // txtGModDll
            // 
            txtGModDll.Location = new Point(97, 97);
            txtGModDll.Name = "txtGModDll";
            txtGModDll.Size = new Size(183, 23);
            txtGModDll.TabIndex = 7;
            // 
            // btnBrowseGModDll
            // 
            btnBrowseGModDll.Location = new Point(306, 97);
            btnBrowseGModDll.Name = "btnBrowseGModDll";
            btnBrowseGModDll.Size = new Size(75, 23);
            btnBrowseGModDll.TabIndex = 8;
            btnBrowseGModDll.Text = "Browse...";
            btnBrowseGModDll.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(590, 338);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 6;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(489, 338);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // ProfileSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(721, 374);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(grpGw1Mods);
            Controls.Add(btnBrowseExe);
            Controls.Add(txtExecutablePath);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtProfileName);
            Name = "ProfileSettingsForm";
            Text = "ProfileSettingsForm";
            grpGw1Mods.ResumeLayout(false);
            grpGw1Mods.PerformLayout();
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
        private Button btnOk;
        private Button btnCancel;
    }
}