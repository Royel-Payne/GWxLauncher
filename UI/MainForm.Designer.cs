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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lblStatus = new Label();
            panelProfiles = new Panel();
            lstProfiles = new ListBox();
            ctxProfiles = new ContextMenuStrip(components);
            menuLaunchProfile = new ToolStripMenuItem();
            menuGw1ToolboxToggle = new ToolStripMenuItem();
            menuGw1ToolboxPath = new ToolStripMenuItem();
            menuSetProfilePath = new ToolStripMenuItem();
            menuEditProfile = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            btnAddAccount = new Button();
            panelProfiles.SuspendLayout();
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
            // panelProfiles
            // 
            panelProfiles.BackColor = Color.FromArgb(24, 24, 28);
            panelProfiles.Controls.Add(lstProfiles);
            panelProfiles.Location = new Point(6, 42);
            panelProfiles.Name = "panelProfiles";
            panelProfiles.Size = new Size(232, 244);
            panelProfiles.TabIndex = 5;
            // 
            // lstProfiles
            // 
            lstProfiles.ContextMenuStrip = ctxProfiles;
            lstProfiles.DrawMode = DrawMode.OwnerDrawFixed;
            lstProfiles.FormattingEnabled = true;
            lstProfiles.ItemHeight = 60;
            lstProfiles.Location = new Point(0, 0);
            lstProfiles.Name = "lstProfiles";
            lstProfiles.Size = new Size(120, 64);
            lstProfiles.TabIndex = 6;
            lstProfiles.DrawItem += lstProfiles_DrawItem;
            lstProfiles.DoubleClick += lstProfiles_DoubleClick;
            lstProfiles.MouseDown += lstProfiles_MouseDown;
            // 
            // ctxProfiles
            // 
            ctxProfiles.Items.AddRange(new ToolStripItem[] { menuLaunchProfile, menuGw1ToolboxToggle, menuGw1ToolboxPath, menuSetProfilePath, menuEditProfile, deleteToolStripMenuItem });
            ctxProfiles.Name = "ctxProfiles";
            ctxProfiles.Size = new Size(196, 158);
            // 
            // menuLaunchProfile
            // 
            menuLaunchProfile.Name = "menuLaunchProfile";
            menuLaunchProfile.Size = new Size(195, 22);
            menuLaunchProfile.Text = "Launch";
            menuLaunchProfile.Click += menuLaunchProfile_Click;
            // 
            // menuGw1ToolboxToggle
            // 
            menuGw1ToolboxToggle.CheckOnClick = true;
            menuGw1ToolboxToggle.Name = "menuGw1ToolboxToggle";
            menuGw1ToolboxToggle.Size = new Size(195, 22);
            menuGw1ToolboxToggle.Text = "GW1 Toolbox (inject)";
            menuGw1ToolboxToggle.Click += menuGw1ToolboxToggle_Click;
            // 
            // menuGw1ToolboxPath
            // 
            menuGw1ToolboxPath.Name = "menuGw1ToolboxPath";
            menuGw1ToolboxPath.Size = new Size(195, 22);
            menuGw1ToolboxPath.Text = "Set GW1 Toolbox DLL…";
            menuGw1ToolboxPath.Click += menuGw1ToolboxPath_Click;
            // 
            // menuSetProfilePath
            // 
            menuSetProfilePath.Name = "menuSetProfilePath";
            menuSetProfilePath.Size = new Size(195, 22);
            menuSetProfilePath.Text = "Set Path...";
            menuSetProfilePath.Click += menuSetProfilePath_Click;
            // 
            // menuEditProfile
            // 
            menuEditProfile.Name = "menuEditProfile";
            menuEditProfile.Size = new Size(195, 22);
            menuEditProfile.Text = "Edit Profile...";
            menuEditProfile.Click += menuEditProfile_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(195, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += menuDeleteProfile_Click;
            // 
            // btnAddAccount
            // 
            btnAddAccount.BackColor = Color.FromArgb(45, 45, 52);
            btnAddAccount.Dock = DockStyle.Top;
            btnAddAccount.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnAddAccount.FlatStyle = FlatStyle.Flat;
            btnAddAccount.ForeColor = Color.White;
            btnAddAccount.Location = new Point(0, 0);
            btnAddAccount.Margin = new Padding(8);
            btnAddAccount.Name = "btnAddAccount";
            btnAddAccount.Size = new Size(248, 30);
            btnAddAccount.TabIndex = 0;
            btnAddAccount.Text = "ADD ACCOUNT…";
            btnAddAccount.UseVisualStyleBackColor = false;
            btnAddAccount.Click += btnAddAcount_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(248, 321);
            Controls.Add(btnAddAccount);
            Controls.Add(panelProfiles);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Text = "GWxLauncher";
            FormClosing += MainForm_FormClosing;
            panelProfiles.ResumeLayout(false);
            ctxProfiles.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblStatus;
        private Panel panelProfiles;
        private ListBox lstProfiles;
        private Button btnAddAccount;
        private ContextMenuStrip ctxProfiles;
        private ToolStripMenuItem menuLaunchProfile;
        private ToolStripMenuItem menuGw1ToolboxToggle;
        private ToolStripMenuItem menuGw1ToolboxPath;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem menuSetProfilePath;
        private ToolStripMenuItem menuEditProfile;
    }
}
