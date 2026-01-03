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
            flpProfiles = new FlowLayoutPanel();
            ctxProfiles = new ContextMenuStrip(components);
            menuLaunchProfile = new ToolStripMenuItem();
            menuEditProfile = new ToolStripMenuItem();
            menuCopyProfile = new ToolStripMenuItem();
            menuShowLastLaunchDetails = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            btnAddAccount = new Button();
            panelView = new Panel();
            btnNewView = new Button();
            btnLaunchAll = new Button();
            btnViewPrev = new Button();
            txtView = new TextBox();
            btnViewNext = new Button();
            btnSettings = new Button();
            chkArmBulk = new CheckBox();
            lblView = new Label();
            panelProfiles.SuspendLayout();
            ctxProfiles.SuspendLayout();
            panelView.SuspendLayout();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Location = new Point(0, 352);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(6);
            lblStatus.RightToLeft = RightToLeft.No;
            lblStatus.Size = new Size(265, 27);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelProfiles
            // 
            panelProfiles.BackColor = Color.FromArgb(24, 24, 28);
            panelProfiles.Controls.Add(flpProfiles);
            panelProfiles.Dock = DockStyle.Fill;
            panelProfiles.Location = new Point(0, 66);
            panelProfiles.Margin = new Padding(0);
            panelProfiles.Name = "panelProfiles";
            panelProfiles.Size = new Size(265, 286);
            panelProfiles.TabIndex = 5;
            // 
            // flpProfiles
            // 
            flpProfiles.AutoScroll = true;
            flpProfiles.Dock = DockStyle.Fill;
            flpProfiles.Location = new Point(0, 0);
            flpProfiles.Name = "flpProfiles";
            flpProfiles.Padding = new Padding(10, 5, 10, 5);
            flpProfiles.Size = new Size(265, 286);
            flpProfiles.TabIndex = 0;
            // 
            // ctxProfiles
            // 
            ctxProfiles.Items.AddRange(new ToolStripItem[] { menuLaunchProfile, menuEditProfile, menuCopyProfile, menuShowLastLaunchDetails, deleteToolStripMenuItem });
            ctxProfiles.Name = "ctxProfiles";
            ctxProfiles.Size = new Size(217, 114);
            // 
            // menuLaunchProfile
            // 
            menuLaunchProfile.Name = "menuLaunchProfile";
            menuLaunchProfile.Size = new Size(216, 22);
            menuLaunchProfile.Text = "Launch";
            menuLaunchProfile.Click += menuLaunchProfile_Click;
            // 
            // menuEditProfile
            // 
            menuEditProfile.Name = "menuEditProfile";
            menuEditProfile.Size = new Size(216, 22);
            menuEditProfile.Text = "Edit Account Info...";
            menuEditProfile.Click += menuEditProfile_Click;
            // 
            // menuCopyProfile
            // 
            menuCopyProfile.Name = "menuCopyProfile";
            menuCopyProfile.Size = new Size(216, 22);
            menuCopyProfile.Text = "Copy Profile";
            menuCopyProfile.Click += menuCopyProfile_Click;
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
            btnAddAccount.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnAddAccount.FlatStyle = FlatStyle.Flat;
            btnAddAccount.ForeColor = Color.White;
            btnAddAccount.Location = new Point(9, 35);
            btnAddAccount.Margin = new Padding(8);
            btnAddAccount.Name = "btnAddAccount";
            btnAddAccount.Size = new Size(32, 28);
            btnAddAccount.TabIndex = 0;
            btnAddAccount.Text = "👤";
            btnAddAccount.UseVisualStyleBackColor = false;
            btnAddAccount.Click += btnAddAcount_Click;
            // 
            // panelView
            // 
            panelView.BackColor = Color.FromArgb(24, 24, 28);
            panelView.Controls.Add(btnNewView);
            panelView.Controls.Add(btnLaunchAll);
            panelView.Controls.Add(btnViewPrev);
            panelView.Controls.Add(txtView);
            panelView.Controls.Add(btnViewNext);
            panelView.Controls.Add(btnSettings);
            panelView.Controls.Add(chkArmBulk);
            panelView.Controls.Add(btnAddAccount);
            panelView.Controls.Add(lblView);
            panelView.Dock = DockStyle.Top;
            panelView.Location = new Point(0, 0);
            panelView.Margin = new Padding(2);
            panelView.Name = "panelView";
            panelView.Size = new Size(265, 66);
            panelView.TabIndex = 10;
            // 
            // btnNewView
            // 
            btnNewView.BackColor = Color.FromArgb(45, 45, 52);
            btnNewView.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnNewView.FlatStyle = FlatStyle.Flat;
            btnNewView.ForeColor = Color.White;
            btnNewView.Location = new Point(9, 3);
            btnNewView.Margin = new Padding(8);
            btnNewView.Name = "btnNewView";
            btnNewView.Size = new Size(32, 28);
            btnNewView.TabIndex = 0;
            btnNewView.Text = "➕";
            btnNewView.UseVisualStyleBackColor = false;
            btnNewView.Click += btnNewView_Click;
            // 
            // btnLaunchAll
            // 
            btnLaunchAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLaunchAll.BackColor = Color.FromArgb(45, 45, 52);
            btnLaunchAll.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnLaunchAll.FlatStyle = FlatStyle.Flat;
            btnLaunchAll.ForeColor = Color.White;
            btnLaunchAll.Location = new Point(224, 35);
            btnLaunchAll.Name = "btnLaunchAll";
            btnLaunchAll.Size = new Size(32, 28);
            btnLaunchAll.TabIndex = 6;
            btnLaunchAll.Text = "▶";
            btnLaunchAll.UseVisualStyleBackColor = false;
            btnLaunchAll.Click += btnLaunchAll_Click;
            // 
            // btnViewPrev
            // 
            btnViewPrev.BackColor = Color.FromArgb(45, 45, 52);
            btnViewPrev.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnViewPrev.FlatStyle = FlatStyle.Flat;
            btnViewPrev.ForeColor = Color.White;
            btnViewPrev.Location = new Point(45, 5);
            btnViewPrev.Name = "btnViewPrev";
            btnViewPrev.Size = new Size(24, 24);
            btnViewPrev.TabIndex = 1;
            btnViewPrev.Text = "<";
            btnViewPrev.UseVisualStyleBackColor = false;
            btnViewPrev.Click += btnViewPrev_Click;
            // 
            // txtView
            // 
            txtView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtView.BackColor = Color.FromArgb(18, 18, 22);
            txtView.BorderStyle = BorderStyle.FixedSingle;
            txtView.ForeColor = Color.Gainsboro;
            txtView.Location = new Point(74, 6);
            txtView.Name = "txtView";
            txtView.Size = new Size(117, 23);
            txtView.TabIndex = 2;
            txtView.TextAlign = HorizontalAlignment.Center;
            txtView.TextChanged += txtView_TextChanged;
            txtView.Enter += txtView_Enter;
            txtView.KeyDown += txtView_KeyDown;
            txtView.Leave += txtView_Leave;
            // 
            // btnViewNext
            // 
            btnViewNext.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnViewNext.BackColor = Color.FromArgb(45, 45, 52);
            btnViewNext.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            btnViewNext.FlatStyle = FlatStyle.Flat;
            btnViewNext.ForeColor = Color.White;
            btnViewNext.Location = new Point(196, 5);
            btnViewNext.Name = "btnViewNext";
            btnViewNext.Size = new Size(24, 24);
            btnViewNext.TabIndex = 3;
            btnViewNext.Text = ">";
            btnViewNext.UseVisualStyleBackColor = false;
            btnViewNext.Click += btnViewNext_Click;
            // 
            // btnSettings
            // 
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.BackColor = Color.FromArgb(45, 45, 52);
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Location = new Point(224, 3);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(32, 28);
            btnSettings.TabIndex = 8;
            btnSettings.Text = "⚙";
            btnSettings.UseVisualStyleBackColor = false;
            // 
            // chkArmBulk
            // 
            chkArmBulk.AutoSize = true;
            chkArmBulk.ForeColor = Color.Gainsboro;
            chkArmBulk.Location = new Point(54, 43);
            chkArmBulk.Name = "chkArmBulk";
            chkArmBulk.Size = new Size(15, 14);
            chkArmBulk.TabIndex = 4;
            chkArmBulk.UseVisualStyleBackColor = true;
            chkArmBulk.CheckedChanged += chkArmBulk_CheckedChanged;
            // 
            // lblView
            // 
            lblView.Location = new Point(75, 35);
            lblView.Name = "lblView";
            lblView.Size = new Size(99, 28);
            lblView.TabIndex = 7;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(265, 379);
            Controls.Add(panelProfiles);
            Controls.Add(panelView);
            Controls.Add(lblStatus);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(281, 104);
            Name = "MainForm";
            Text = "GWxLauncher";
            FormClosing += MainForm_FormClosing;
            panelProfiles.ResumeLayout(false);
            ctxProfiles.ResumeLayout(false);
            panelView.ResumeLayout(false);
            panelView.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button btnSettings;
        private Label lblStatus;
        private Panel panelProfiles;
        private FlowLayoutPanel flpProfiles;
        private Button btnAddAccount;
        private ContextMenuStrip ctxProfiles;
        private ToolStripMenuItem menuLaunchProfile;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem menuEditProfile;
        private ToolStripMenuItem menuShowLastLaunchDetails;
        private ToolStripMenuItem menuCopyProfile;
        private Panel panelView;
        private Button btnViewPrev;
        private Button btnViewNext;
        private TextBox txtView;
        private CheckBox chkArmBulk;
        private Button btnNewView;
        private Button btnLaunchAll;
        private Label lblView;
    }
}
