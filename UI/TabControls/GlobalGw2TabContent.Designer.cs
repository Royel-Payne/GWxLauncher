namespace GWxLauncher.UI.TabControls
{
    partial class GlobalGw2TabContent
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            cbGw2RenameWindowTitle = new CheckBox();
            txtGw2TitleTemplate = new TextBox();
            lblGw2TitleHelp = new Label();
            grpWindow = new GroupBox();
            grpBulkLaunch = new GroupBox();
            chkGw2Multiclient = new CheckBox();
            lblGw2BulkDelay = new Label();
            numGw2BulkDelay = new NumericUpDown();
            lblGw2BulkDelayHelp = new Label();
            grpIsolation = new GroupBox();
            chkGw2IsolationEnabled = new CheckBox();
            lblIsolationHelp = new Label();
            grpWindow.SuspendLayout();
            grpBulkLaunch.SuspendLayout();
            grpIsolation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numGw2BulkDelay).BeginInit();
            SuspendLayout();
            // 
            // cbGw2RenameWindowTitle
            // 
            cbGw2RenameWindowTitle.AutoSize = true;
            cbGw2RenameWindowTitle.Location = new Point(20, 25);
            cbGw2RenameWindowTitle.Name = "cbGw2RenameWindowTitle";
            cbGw2RenameWindowTitle.Size = new Size(109, 19);
            cbGw2RenameWindowTitle.TabIndex = 0;
            cbGw2RenameWindowTitle.Text = "Set Titlebar Text";
            cbGw2RenameWindowTitle.UseVisualStyleBackColor = true;
            // 
            // txtGw2TitleTemplate
            // 
            txtGw2TitleTemplate.Location = new Point(20, 50);
            txtGw2TitleTemplate.Name = "txtGw2TitleTemplate";
            txtGw2TitleTemplate.Size = new Size(420, 23);
            txtGw2TitleTemplate.TabIndex = 1;
            // 
            // lblGw2TitleHelp
            // 
            lblGw2TitleHelp.AutoSize = true;
            lblGw2TitleHelp.ForeColor = Color.Goldenrod;
            lblGw2TitleHelp.Location = new Point(20, 78);
            lblGw2TitleHelp.Name = "lblGw2TitleHelp";
            lblGw2TitleHelp.Size = new Size(428, 15);
            lblGw2TitleHelp.TabIndex = 2;
            lblGw2TitleHelp.Text = "Default template if profile does not specify one. Example: \"GW2 • {ProfileName}\"";
            // 
            // grpWindow
            // 
            grpWindow.Controls.Add(cbGw2RenameWindowTitle);
            grpWindow.Controls.Add(txtGw2TitleTemplate);
            grpWindow.Controls.Add(lblGw2TitleHelp);
            grpWindow.Location = new Point(10, 10);
            grpWindow.Name = "grpWindow";
            grpWindow.Size = new Size(460, 100);
            grpWindow.TabIndex = 0;
            grpWindow.TabStop = false;
            grpWindow.Text = "Game Window Title";
            // 
            // grpBulkLaunch
            // 
            grpBulkLaunch.Controls.Add(chkGw2Multiclient);
            grpBulkLaunch.Controls.Add(lblGw2BulkDelay);
            grpBulkLaunch.Controls.Add(numGw2BulkDelay);
            grpBulkLaunch.Controls.Add(lblGw2BulkDelayHelp);
            grpBulkLaunch.Location = new Point(10, 120);
            grpBulkLaunch.Name = "grpBulkLaunch";
            grpBulkLaunch.Size = new Size(460, 135);
            grpBulkLaunch.TabIndex = 1;
            grpBulkLaunch.TabStop = false;
            grpBulkLaunch.Text = "Bulk Launch";
            // 
            // chkGw2Multiclient
            // 
            chkGw2Multiclient.AutoSize = true;
            chkGw2Multiclient.Location = new Point(20, 105);
            chkGw2Multiclient.Name = "chkGw2Multiclient";
            chkGw2Multiclient.Size = new Size(250, 19);
            chkGw2Multiclient.TabIndex = 3;
            chkGw2Multiclient.Text = "Enable Multiclient (allow multiple GW2 instances)";
            chkGw2Multiclient.UseVisualStyleBackColor = true;
            // 
            // lblGw2BulkDelay
            // 
            lblGw2BulkDelay.AutoSize = true;
            lblGw2BulkDelay.Location = new Point(20, 25);
            lblGw2BulkDelay.Name = "lblGw2BulkDelay";
            lblGw2BulkDelay.Size = new Size(160, 15);
            lblGw2BulkDelay.TabIndex = 0;
            lblGw2BulkDelay.Text = "Delay between launches (seconds):";
            // 
            // numGw2BulkDelay
            // 
            numGw2BulkDelay.Location = new Point(20, 45);
            numGw2BulkDelay.Maximum = 90;
            numGw2BulkDelay.Minimum = 0;
            numGw2BulkDelay.Name = "numGw2BulkDelay";
            numGw2BulkDelay.Size = new Size(80, 23);
            numGw2BulkDelay.TabIndex = 1;
            numGw2BulkDelay.Value = 15;
            // 
            // lblGw2BulkDelayHelp
            // 
            lblGw2BulkDelayHelp.AutoSize = true;
            lblGw2BulkDelayHelp.ForeColor = Color.Goldenrod;
            lblGw2BulkDelayHelp.Location = new Point(20, 73);
            lblGw2BulkDelayHelp.Name = "lblGw2BulkDelayHelp";
            lblGw2BulkDelayHelp.Size = new Size(400, 15);
            lblGw2BulkDelayHelp.TabIndex = 2;
            lblGw2BulkDelayHelp.Text = "Wait time between bulk launches. 0 = no wait. Range: 0-90 seconds.";
            // 
            // grpIsolation
            // 
            grpIsolation.Controls.Add(chkGw2IsolationEnabled);
            grpIsolation.Controls.Add(lblIsolationHelp);
            grpIsolation.Location = new Point(10, 265);
            grpIsolation.Name = "grpIsolation";
            grpIsolation.Size = new Size(460, 100);
            grpIsolation.TabIndex = 2;
            grpIsolation.TabStop = false;
            grpIsolation.Text = "Per-Profile Isolation (Advanced)";
            // 
            // chkGw2IsolationEnabled
            // 
            chkGw2IsolationEnabled.AutoSize = true;
            chkGw2IsolationEnabled.Location = new Point(20, 25);
            chkGw2IsolationEnabled.Name = "chkGw2IsolationEnabled";
            chkGw2IsolationEnabled.Size = new Size(280, 19);
            chkGw2IsolationEnabled.TabIndex = 0;
            chkGw2IsolationEnabled.Text = "Enable Per-Profile GW2 Settings (Isolated Mode)";
            chkGw2IsolationEnabled.UseVisualStyleBackColor = true;
            // 
            // lblIsolationHelp
            // 
            lblIsolationHelp.ForeColor = Color.Goldenrod;
            lblIsolationHelp.Location = new Point(20, 47);
            lblIsolationHelp.Name = "lblIsolationHelp";
            lblIsolationHelp.Size = new Size(420, 45);
            lblIsolationHelp.TabIndex = 1;
            lblIsolationHelp.Text = "Each profile gets isolated AppData via DLL injection. Requires unique game folder per profile. " +
                "Disables -shareArchive. Use for complete account/settings isolation.";
            // 
            // GlobalGw2TabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpIsolation);
            Controls.Add(grpBulkLaunch);
            Controls.Add(grpWindow);
            Name = "GlobalGw2TabContent";
            Size = new Size(500, 385);
            grpWindow.ResumeLayout(false);
            grpWindow.PerformLayout();
            grpBulkLaunch.ResumeLayout(false);
            grpBulkLaunch.PerformLayout();
            grpIsolation.ResumeLayout(false);
            grpIsolation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGw2BulkDelay).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox cbGw2RenameWindowTitle;
        private System.Windows.Forms.TextBox txtGw2TitleTemplate;
        private System.Windows.Forms.Label lblGw2TitleHelp;
        private System.Windows.Forms.GroupBox grpWindow;
        
        private System.Windows.Forms.GroupBox grpBulkLaunch;
        private System.Windows.Forms.CheckBox chkGw2Multiclient;
        private System.Windows.Forms.Label lblGw2BulkDelay;
        private System.Windows.Forms.NumericUpDown numGw2BulkDelay;
        private System.Windows.Forms.Label lblGw2BulkDelayHelp;
        
        private System.Windows.Forms.GroupBox grpIsolation;
        private System.Windows.Forms.CheckBox chkGw2IsolationEnabled;
        private System.Windows.Forms.Label lblIsolationHelp;
    }
}
