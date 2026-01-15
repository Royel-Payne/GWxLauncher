namespace GWxLauncher.UI.TabControls
{
    partial class GlobalHelpTabContent
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblVersion = new Label();
            lblBuildInfo = new Label();
            lblDescription = new Label();
            groupBox1 = new GroupBox();
            btnOpenReleases = new Button();
            lblUpdateStatus = new Label();
            btnCheckForUpdates = new Button();
            groupBox2 = new GroupBox();
            linkGitHub = new LinkLabel();
            lblAbout = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(142, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "GWxLauncher";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 10F);
            lblVersion.Location = new Point(23, 55);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(88, 19);
            lblVersion.TabIndex = 1;
            lblVersion.Text = "Version: 1.5.3";
            // 
            // lblBuildInfo
            // 
            lblBuildInfo.AutoSize = true;
            lblBuildInfo.Font = new Font("Segoe UI", 8F);
            lblBuildInfo.ForeColor = SystemColors.GrayText;
            lblBuildInfo.Location = new Point(23, 77);
            lblBuildInfo.Name = "lblBuildInfo";
            lblBuildInfo.Size = new Size(80, 13);
            lblBuildInfo.TabIndex = 2;
            lblBuildInfo.Text = "Build: (hash)";
            lblBuildInfo.Visible = false;
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(23, 100);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(209, 15);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "A Guild Wars 1 && 2 game launcher";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(btnOpenReleases);
            groupBox1.Controls.Add(lblUpdateStatus);
            groupBox1.Controls.Add(btnCheckForUpdates);
            groupBox1.Location = new Point(20, 130);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(10);
            groupBox1.Size = new Size(560, 120);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Updates";
            // 
            // btnOpenReleases
            // 
            btnOpenReleases.Location = new Point(13, 80);
            btnOpenReleases.Name = "btnOpenReleases";
            btnOpenReleases.Size = new Size(150, 28);
            btnOpenReleases.TabIndex = 2;
            btnOpenReleases.Text = "View Release Page";
            btnOpenReleases.UseVisualStyleBackColor = true;
            btnOpenReleases.Visible = false;
            btnOpenReleases.Click += btnOpenReleases_Click;
            // 
            // lblUpdateStatus
            // 
            lblUpdateStatus.AutoSize = true;
            lblUpdateStatus.Location = new Point(13, 55);
            lblUpdateStatus.Name = "lblUpdateStatus";
            lblUpdateStatus.Size = new Size(259, 15);
            lblUpdateStatus.TabIndex = 1;
            lblUpdateStatus.Text = "Click the button above to check for updates.";
            // 
            // btnCheckForUpdates
            // 
            btnCheckForUpdates.Location = new Point(13, 25);
            btnCheckForUpdates.Name = "btnCheckForUpdates";
            btnCheckForUpdates.Size = new Size(150, 28);
            btnCheckForUpdates.TabIndex = 0;
            btnCheckForUpdates.Text = "Check for Updates";
            btnCheckForUpdates.UseVisualStyleBackColor = true;
            btnCheckForUpdates.Click += btnCheckForUpdates_Click;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(linkGitHub);
            groupBox2.Controls.Add(lblAbout);
            groupBox2.Location = new Point(20, 270);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(10);
            groupBox2.Size = new Size(560, 100);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "About";
            // 
            // linkGitHub
            // 
            linkGitHub.AutoSize = true;
            linkGitHub.Location = new Point(13, 60);
            linkGitHub.Name = "linkGitHub";
            linkGitHub.Size = new Size(263, 15);
            linkGitHub.TabIndex = 1;
            linkGitHub.TabStop = true;
            linkGitHub.Text = "https://github.com/Royel-Payne/GWxLauncher";
            linkGitHub.LinkClicked += linkGitHub_LinkClicked;
            // 
            // lblAbout
            // 
            lblAbout.AutoSize = true;
            lblAbout.Location = new Point(13, 30);
            lblAbout.Name = "lblAbout";
            lblAbout.Size = new Size(447, 15);
            lblAbout.TabIndex = 0;
            lblAbout.Text = "GWxLauncher is an open-source game launcher for Guild Wars 1 and Guild Wars 2.";
            // 
            // GlobalHelpTabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(lblDescription);
            Controls.Add(lblBuildInfo);
            Controls.Add(lblVersion);
            Controls.Add(lblTitle);
            Name = "GlobalHelpTabContent";
            Size = new Size(600, 440);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Label lblVersion;
        private Label lblBuildInfo;
        private Label lblDescription;
        private GroupBox groupBox1;
        private Button btnCheckForUpdates;
        private Label lblUpdateStatus;
        private Button btnOpenReleases;
        private GroupBox groupBox2;
        private Label lblAbout;
        private LinkLabel linkGitHub;
    }
}
