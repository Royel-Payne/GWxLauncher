namespace GWxLauncher
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblStatus = new Label();
            lstProfiles = new ListBox();
            ctxProfiles = new ContextMenuStrip(components);
            menuLaunchProfile = new ToolStripMenuItem();
            menuSetProfilePath = new ToolStripMenuItem();
            menuEditProfile = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            btnAddAccount = new Button();
            ctxProfiles.SuspendLayout();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(6, 301);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready";
            // 
            // lstProfiles
            // 
            lstProfiles.ContextMenuStrip = ctxProfiles;
            lstProfiles.FormattingEnabled = true;
            lstProfiles.ItemHeight = 15;
            lstProfiles.Location = new Point(5, 12);
            lstProfiles.Name = "lstProfiles";
            lstProfiles.Size = new Size(223, 94);
            lstProfiles.TabIndex = 5;
            lstProfiles.DoubleClick += lstProfiles_DoubleClick;
            lstProfiles.MouseDown += lstProfiles_MouseDown;
            // 
            // ctxProfiles
            // 
            ctxProfiles.Items.AddRange(new ToolStripItem[] { menuLaunchProfile, menuSetProfilePath, menuEditProfile, deleteToolStripMenuItem });
            ctxProfiles.Name = "ctxProfiles";
            ctxProfiles.Size = new Size(127, 92);
            // 
            // menuLaunchProfile
            // 
            menuLaunchProfile.Name = "menuLaunchProfile";
            menuLaunchProfile.Size = new Size(126, 22);
            menuLaunchProfile.Text = "Launch";
            menuLaunchProfile.Click += menuLaunchProfile_Click;
            // 
            // menuSetProfilePath
            // 
            menuSetProfilePath.Name = "menuSetProfilePath";
            menuSetProfilePath.Size = new Size(126, 22);
            menuSetProfilePath.Text = "Set Path...";
            menuSetProfilePath.Click += menuSetProfilePath_Click;
            // 
            // menuEditProfile
            // 
            menuEditProfile.Name = "menuEditProfile";
            menuEditProfile.Size = new Size(126, 22);
            menuEditProfile.Text = "Edit...";
            menuEditProfile.Click += menuEditProfile_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(126, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += menuDeleteProfile_Click;
            // 
            // btnAddAccount
            // 
            btnAddAccount.Location = new Point(6, 112);
            btnAddAccount.Name = "btnAddAccount";
            btnAddAccount.Size = new Size(100, 23);
            btnAddAccount.TabIndex = 6;
            btnAddAccount.Text = "Add Account...";
            btnAddAccount.UseVisualStyleBackColor = true;
            btnAddAccount.Click += btnAddAcount_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(244, 321);
            Controls.Add(btnAddAccount);
            Controls.Add(lstProfiles);
            Controls.Add(lblStatus);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            Text = "GWxLauncher";
            FormClosing += MainForm_FormClosing;
            ctxProfiles.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblStatus;
        private ListBox lstProfiles;
        private Button btnAddAccount;
        private ContextMenuStrip ctxProfiles;
        private ToolStripMenuItem menuLaunchProfile;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem menuSetProfilePath;
        private ToolStripMenuItem menuEditProfile;
    }
}
