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
            menuShowLastLaunchDetails = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            btnAddAccount = new Button();
            panelView = new Panel();
            btnNewView = new Button();
            lblView = new Label();
            btnViewPrev = new Button();
            txtView = new TextBox();
            btnViewNext = new Button();
            chkArmBulk = new CheckBox();
            btnLaunchAll = new Button();
            panelProfiles.SuspendLayout();
            ctxProfiles.SuspendLayout();
            panelView.SuspendLayout();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Location = new Point(0, 374);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(6, 0, 0, 6);
            lblStatus.Size = new Size(45, 21);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready";
            // 
            // panelProfiles
            // 
            panelProfiles.BackColor = Color.FromArgb(24, 24, 28);
            panelProfiles.Controls.Add(lstProfiles);
            panelProfiles.Location = new Point(7, 102);
            panelProfiles.Name = "panelProfiles";
            panelProfiles.Size = new Size(355, 250);
            panelProfiles.TabIndex = 5;
            // 
            // lstProfiles
            // 
            lstProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lstProfiles.ContextMenuStrip = ctxProfiles;
            lstProfiles.DrawMode = DrawMode.OwnerDrawFixed;
            lstProfiles.FormattingEnabled = true;
            lstProfiles.ItemHeight = 60;
            lstProfiles.Location = new Point(3, 3);
            lstProfiles.Name = "lstProfiles";
            lstProfiles.Size = new Size(349, 244);
            lstProfiles.TabIndex = 6;
            lstProfiles.DrawItem += lstProfiles_DrawItem;
            lstProfiles.DoubleClick += lstProfiles_DoubleClick;
            lstProfiles.MouseDown += lstProfiles_MouseDown;
            // 
            // ctxProfiles
            // 
            ctxProfiles.Items.AddRange(new ToolStripItem[] { menuLaunchProfile, menuGw1ToolboxToggle, menuGw1ToolboxPath, menuSetProfilePath, menuEditProfile, menuShowLastLaunchDetails, deleteToolStripMenuItem });
            ctxProfiles.Name = "ctxProfiles";
            ctxProfiles.Size = new Size(217, 158);
            // 
            // menuLaunchProfile
            // 
            menuLaunchProfile.Name = "menuLaunchProfile";
            menuLaunchProfile.Size = new Size(216, 22);
            menuLaunchProfile.Text = "Launch";
            menuLaunchProfile.Click += menuLaunchProfile_Click;
            // 
            // menuGw1ToolboxToggle
            // 
            menuGw1ToolboxToggle.CheckOnClick = true;
            menuGw1ToolboxToggle.Name = "menuGw1ToolboxToggle";
            menuGw1ToolboxToggle.Size = new Size(216, 22);
            menuGw1ToolboxToggle.Text = "GW1 Toolbox (inject)";
            menuGw1ToolboxToggle.Click += menuGw1ToolboxToggle_Click;
            // 
            // menuGw1ToolboxPath
            // 
            menuGw1ToolboxPath.Name = "menuGw1ToolboxPath";
            menuGw1ToolboxPath.Size = new Size(216, 22);
            menuGw1ToolboxPath.Text = "Set GW1 Toolbox DLL…";
            menuGw1ToolboxPath.Click += menuGw1ToolboxPath_Click;
            // 
            // menuSetProfilePath
            // 
            menuSetProfilePath.Name = "menuSetProfilePath";
            menuSetProfilePath.Size = new Size(216, 22);
            menuSetProfilePath.Text = "Set Path...";
            menuSetProfilePath.Click += menuSetProfilePath_Click;
            // 
            // menuEditProfile
            // 
            menuEditProfile.Name = "menuEditProfile";
            menuEditProfile.Size = new Size(216, 22);
            menuEditProfile.Text = "Edit Profile...";
            menuEditProfile.Click += menuEditProfile_Click;
            // 
            // menuShowLastLaunchDetails
            // 
            menuShowLastLaunchDetails.Name = "menuShowLastLaunchDetails";
            menuShowLastLaunchDetails.Size = new Size(216, 22);
            menuShowLastLaunchDetails.Text = "Show Last Launch Details...";
            menuShowLastLaunchDetails.Click += menuShowLastLaunchDetails_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(216, 22);
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
            btnAddAccount.Size = new Size(411, 30);
            btnAddAccount.TabIndex = 0;
            btnAddAccount.Text = "ADD ACCOUNT…";
            btnAddAccount.UseVisualStyleBackColor = false;
            btnAddAccount.Click += btnAddAcount_Click;
            // 
            // panelView
            // 
            panelView.BackColor = Color.FromArgb(24, 24, 28);
            panelView.Controls.Add(btnNewView);
            panelView.Controls.Add(btnLaunchAll);
            panelView.Controls.Add(lblView);
            panelView.Controls.Add(btnViewPrev);
            panelView.Controls.Add(txtView);
            panelView.Controls.Add(btnViewNext);
            panelView.Controls.Add(chkArmBulk);
            panelView.Dock = DockStyle.Top;
            panelView.Location = new Point(0, 30);
            panelView.Name = "panelView";
            panelView.Size = new Size(411, 66);
            panelView.TabIndex = 10;
            // 
            // btnNewView
            // 
            btnNewView.Location = new Point(28, 5);
            btnNewView.Name = "btnNewView";
            btnNewView.Size = new Size(75, 23);
            btnNewView.TabIndex = 5;
            btnNewView.Text = "+";
            btnNewView.UseVisualStyleBackColor = true;
            btnNewView.Click += btnNewView_Click;
            // 
            // btnLaunchAll
            // 
            btnLaunchAll.BackColor = Color.FromArgb(45, 45, 52);
            btnLaunchAll.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnLaunchAll.FlatStyle = FlatStyle.Flat;
            btnLaunchAll.ForeColor = Color.White;
            btnLaunchAll.Location = new Point(316, 2);
            btnLaunchAll.Name = "btnLaunchAll";
            btnLaunchAll.Size = new Size(50, 24);
            btnLaunchAll.TabIndex = 6;
            btnLaunchAll.Text = "Launch";
            btnLaunchAll.UseVisualStyleBackColor = false;
            btnLaunchAll.Click += btnLaunchAll_Click;
            // 
            // lblView
            // 
            lblView.AutoSize = true;
            lblView.Location = new Point(121, 5);
            lblView.Name = "lblView";
            lblView.Size = new Size(35, 15);
            lblView.TabIndex = 0;
            lblView.Text = "View:";
            // 
            // btnViewPrev
            // 
            btnViewPrev.BackColor = Color.FromArgb(45, 45, 52);
            btnViewPrev.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnViewPrev.FlatStyle = FlatStyle.Flat;
            btnViewPrev.ForeColor = Color.White;
            btnViewPrev.Location = new Point(162, 2);
            btnViewPrev.Name = "btnViewPrev";
            btnViewPrev.Size = new Size(24, 24);
            btnViewPrev.TabIndex = 1;
            btnViewPrev.Text = "<";
            btnViewPrev.UseVisualStyleBackColor = false;
            btnViewPrev.Click += btnViewPrev_Click;
            // 
            // txtView
            // 
            txtView.BackColor = Color.FromArgb(18, 18, 22);
            txtView.BorderStyle = BorderStyle.FixedSingle;
            txtView.ForeColor = Color.Gainsboro;
            txtView.Location = new Point(190, 3);
            txtView.Name = "txtView";
            txtView.Size = new Size(92, 23);
            txtView.TabIndex = 2;
            txtView.TextChanged += txtView_TextChanged;
            txtView.Enter += txtView_Enter;
            txtView.KeyDown += txtView_KeyDown;
            txtView.Leave += txtView_Leave;
            // 
            // btnViewNext
            // 
            btnViewNext.BackColor = Color.FromArgb(45, 45, 52);
            btnViewNext.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnViewNext.FlatStyle = FlatStyle.Flat;
            btnViewNext.ForeColor = Color.White;
            btnViewNext.Location = new Point(286, 2);
            btnViewNext.Name = "btnViewNext";
            btnViewNext.Size = new Size(24, 24);
            btnViewNext.TabIndex = 3;
            btnViewNext.Text = ">";
            btnViewNext.UseVisualStyleBackColor = false;
            btnViewNext.Click += btnViewNext_Click;
            // 
            // chkArmBulk
            // 
            chkArmBulk.AutoSize = true;
            chkArmBulk.ForeColor = Color.Gainsboro;
            chkArmBulk.Location = new Point(30, 34);
            chkArmBulk.Name = "chkArmBulk";
            chkArmBulk.Size = new Size(15, 14);
            chkArmBulk.TabIndex = 4;
            chkArmBulk.UseVisualStyleBackColor = true;
            chkArmBulk.CheckedChanged += chkArmBulk_CheckedChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(411, 395);
            Controls.Add(panelProfiles);
            Controls.Add(lblStatus);
            Controls.Add(panelView);
            Controls.Add(btnAddAccount);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Text = "GWxLauncher";
            FormClosing += MainForm_FormClosing;
            panelProfiles.ResumeLayout(false);
            ctxProfiles.ResumeLayout(false);
            panelView.ResumeLayout(false);
            panelView.PerformLayout();
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
        private ToolStripMenuItem menuShowLastLaunchDetails;
        private Panel panelView;
        private Button btnViewPrev;
        private Button btnViewNext;
        private TextBox txtView;
        private CheckBox chkArmBulk;
        private Label lblView;
        private Button btnNewView;
        private Button btnLaunchAll;
        //private System.Windows.Forms.Button btnLaunchAll;

    }
}
