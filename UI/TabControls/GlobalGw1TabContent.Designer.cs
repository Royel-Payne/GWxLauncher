namespace GWxLauncher.UI.TabControls
{
    partial class GlobalGw1TabContent
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            cbGw1RenameWindowTitle = new CheckBox();
            txtGw1TitleTemplate = new TextBox();
            lblGw1TitleHelp = new Label();
            grpWindow = new GroupBox();
            grpBulkLaunch = new GroupBox();
            lblGw1BulkDelay = new Label();
            numGw1BulkDelay = new NumericUpDown();
            lblGw1BulkDelayHelp = new Label();
            cbGlobalToolbox = new CheckBox();
            txtToolbox = new TextBox();
            btnBrowseToolbox = new Button();
            cbGlobalPy4Gw = new CheckBox();
            txtPy4GW = new TextBox();
            btnBrowsePy4GW = new Button();
            cbGlobalGMod = new CheckBox();
            txtGMod = new TextBox();
            btnBrowseGMod = new Button();
            btnApplyGlobalFlags = new Button();
            btnApplyGlobalPaths = new Button();
            lblDllHelp = new Label();
            grpDll = new GroupBox();
            grpWindow.SuspendLayout();
            grpBulkLaunch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numGw1BulkDelay).BeginInit();
            grpDll.SuspendLayout();
            SuspendLayout();
            // 
            // cbGw1RenameWindowTitle
            // 
            cbGw1RenameWindowTitle.AutoSize = true;
            cbGw1RenameWindowTitle.Location = new Point(20, 25);
            cbGw1RenameWindowTitle.Name = "cbGw1RenameWindowTitle";
            cbGw1RenameWindowTitle.Size = new Size(109, 19);
            cbGw1RenameWindowTitle.TabIndex = 0;
            cbGw1RenameWindowTitle.Text = "Set Titlebar Text";
            cbGw1RenameWindowTitle.UseVisualStyleBackColor = true;
            // 
            // txtGw1TitleTemplate
            // 
            txtGw1TitleTemplate.Location = new Point(20, 50);
            txtGw1TitleTemplate.Name = "txtGw1TitleTemplate";
            txtGw1TitleTemplate.Size = new Size(420, 23);
            txtGw1TitleTemplate.TabIndex = 1;
            // 
            // lblGw1TitleHelp
            // 
            lblGw1TitleHelp.AutoSize = true;
            lblGw1TitleHelp.ForeColor = Color.Goldenrod;
            lblGw1TitleHelp.Location = new Point(20, 78);
            lblGw1TitleHelp.Name = "lblGw1TitleHelp";
            lblGw1TitleHelp.Size = new Size(428, 15);
            lblGw1TitleHelp.TabIndex = 2;
            lblGw1TitleHelp.Text = "Default template if profile does not specify one. Example: \"GW1 · {ProfileName}\"";
            // 
            // grpWindow
            // 
            grpWindow.Controls.Add(cbGw1RenameWindowTitle);
            grpWindow.Controls.Add(txtGw1TitleTemplate);
            grpWindow.Controls.Add(lblGw1TitleHelp);
            grpWindow.Location = new Point(10, 10);
            grpWindow.Name = "grpWindow";
            grpWindow.Size = new Size(460, 100);
            grpWindow.TabIndex = 0;
            grpWindow.TabStop = false;
            grpWindow.Text = "Game Window Title";
            // 
            // grpBulkLaunch
            // 
            grpBulkLaunch.Controls.Add(lblGw1BulkDelay);
            grpBulkLaunch.Controls.Add(numGw1BulkDelay);
            grpBulkLaunch.Controls.Add(lblGw1BulkDelayHelp);
            grpBulkLaunch.Location = new Point(10, 120);
            grpBulkLaunch.Name = "grpBulkLaunch";
            grpBulkLaunch.Size = new Size(460, 110);
            grpBulkLaunch.TabIndex = 1;
            grpBulkLaunch.TabStop = false;
            grpBulkLaunch.Text = "Bulk Launch";
            // 
            // lblGw1BulkDelay
            // 
            lblGw1BulkDelay.AutoSize = true;
            lblGw1BulkDelay.Location = new Point(20, 25);
            lblGw1BulkDelay.Name = "lblGw1BulkDelay";
            lblGw1BulkDelay.Size = new Size(160, 15);
            lblGw1BulkDelay.TabIndex = 0;
            lblGw1BulkDelay.Text = "Delay between launches (seconds):";
            // 
            // numGw1BulkDelay
            // 
            numGw1BulkDelay.Location = new Point(20, 45);
            numGw1BulkDelay.Maximum = 90;
            numGw1BulkDelay.Minimum = 0;
            numGw1BulkDelay.Name = "numGw1BulkDelay";
            numGw1BulkDelay.Size = new Size(80, 23);
            numGw1BulkDelay.TabIndex = 1;
            numGw1BulkDelay.Value = 15;
            // 
            // lblGw1BulkDelayHelp
            // 
            lblGw1BulkDelayHelp.AutoSize = true;
            lblGw1BulkDelayHelp.ForeColor = Color.Goldenrod;
            lblGw1BulkDelayHelp.Location = new Point(20, 73);
            lblGw1BulkDelayHelp.Name = "lblGw1BulkDelayHelp";
            lblGw1BulkDelayHelp.Size = new Size(400, 15);
            lblGw1BulkDelayHelp.TabIndex = 2;
            lblGw1BulkDelayHelp.Text = "Wait time between bulk launches. 0 = no wait. Range: 0-90 seconds.";
            // 
            // cbGlobalToolbox
            // 
            cbGlobalToolbox.AutoSize = true;
            cbGlobalToolbox.Location = new Point(20, 25);
            cbGlobalToolbox.Name = "cbGlobalToolbox";
            cbGlobalToolbox.Size = new Size(151, 19);
            cbGlobalToolbox.TabIndex = 0;
            cbGlobalToolbox.Text = "Enable Toolbox globally";
            // 
            // txtToolbox
            // 
            txtToolbox.Location = new Point(20, 50);
            txtToolbox.Name = "txtToolbox";
            txtToolbox.Size = new Size(370, 23);
            txtToolbox.TabIndex = 1;
            // 
            // btnBrowseToolbox
            // 
            btnBrowseToolbox.Location = new Point(400, 49);
            btnBrowseToolbox.Name = "btnBrowseToolbox";
            btnBrowseToolbox.Size = new Size(40, 25);
            btnBrowseToolbox.TabIndex = 2;
            btnBrowseToolbox.Text = "...";
            // 
            // cbGlobalPy4Gw
            // 
            cbGlobalPy4Gw.AutoSize = true;
            cbGlobalPy4Gw.Location = new Point(20, 90);
            cbGlobalPy4Gw.Name = "cbGlobalPy4Gw";
            cbGlobalPy4Gw.Size = new Size(147, 19);
            cbGlobalPy4Gw.TabIndex = 3;
            cbGlobalPy4Gw.Text = "Enable Py4GW globally";
            // 
            // txtPy4GW
            // 
            txtPy4GW.Location = new Point(20, 115);
            txtPy4GW.Name = "txtPy4GW";
            txtPy4GW.Size = new Size(370, 23);
            txtPy4GW.TabIndex = 4;
            // 
            // btnBrowsePy4GW
            // 
            btnBrowsePy4GW.Location = new Point(400, 114);
            btnBrowsePy4GW.Name = "btnBrowsePy4GW";
            btnBrowsePy4GW.Size = new Size(40, 25);
            btnBrowsePy4GW.TabIndex = 5;
            btnBrowsePy4GW.Text = "...";
            // 
            // cbGlobalGMod
            // 
            cbGlobalGMod.AutoSize = true;
            cbGlobalGMod.Location = new Point(20, 155);
            cbGlobalGMod.Name = "cbGlobalGMod";
            cbGlobalGMod.Size = new Size(141, 19);
            cbGlobalGMod.TabIndex = 6;
            cbGlobalGMod.Text = "Enable gMod globally";
            // 
            // txtGMod
            // 
            txtGMod.Location = new Point(20, 180);
            txtGMod.Name = "txtGMod";
            txtGMod.Size = new Size(370, 23);
            txtGMod.TabIndex = 7;
            // 
            // btnBrowseGMod
            // 
            btnBrowseGMod.Location = new Point(400, 179);
            btnBrowseGMod.Name = "btnBrowseGMod";
            btnBrowseGMod.Size = new Size(40, 25);
            btnBrowseGMod.TabIndex = 8;
            btnBrowseGMod.Text = "...";
            // 
            // btnApplyGlobalFlags
            // 
            btnApplyGlobalFlags.Location = new Point(20, 255);
            btnApplyGlobalFlags.Name = "btnApplyGlobalFlags";
            btnApplyGlobalFlags.Size = new Size(420, 25);
            btnApplyGlobalFlags.TabIndex = 10;
            btnApplyGlobalFlags.Text = "Apply enable/disable states to all profiles";
            // 
            // btnApplyGlobalPaths
            // 
            btnApplyGlobalPaths.Location = new Point(20, 285);
            btnApplyGlobalPaths.Name = "btnApplyGlobalPaths";
            btnApplyGlobalPaths.Size = new Size(420, 25);
            btnApplyGlobalPaths.TabIndex = 11;
            btnApplyGlobalPaths.Text = "Apply paths to all profiles";
            // 
            // lblDllHelp
            // 
            lblDllHelp.ForeColor = Color.Goldenrod;
            lblDllHelp.Location = new Point(20, 215);
            lblDllHelp.Name = "lblDllHelp";
            lblDllHelp.Size = new Size(420, 30);
            lblDllHelp.TabIndex = 9;
            lblDllHelp.Text = "These paths are defaults for new profiles. The checkboxes above act as global kill-switches (gate) at launch time.";
            // 
            // grpDll
            // 
            grpDll.Controls.Add(cbGlobalToolbox);
            grpDll.Controls.Add(txtToolbox);
            grpDll.Controls.Add(btnBrowseToolbox);
            grpDll.Controls.Add(cbGlobalPy4Gw);
            grpDll.Controls.Add(txtPy4GW);
            grpDll.Controls.Add(btnBrowsePy4GW);
            grpDll.Controls.Add(cbGlobalGMod);
            grpDll.Controls.Add(txtGMod);
            grpDll.Controls.Add(btnBrowseGMod);
            grpDll.Controls.Add(lblDllHelp);
            grpDll.Controls.Add(btnApplyGlobalFlags);
            grpDll.Controls.Add(btnApplyGlobalPaths);
            grpDll.Location = new Point(10, 240);
            grpDll.Name = "grpDll";
            grpDll.Size = new Size(460, 330);
            grpDll.TabIndex = 2;
            grpDll.TabStop = false;
            grpDll.Text = "DLL Injection (Globals)";
            // 
            // GlobalGw1TabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpDll);
            Controls.Add(grpBulkLaunch);
            Controls.Add(grpWindow);
            Name = "GlobalGw1TabContent";
            Size = new Size(500, 600);
            grpWindow.ResumeLayout(false);
            grpWindow.PerformLayout();
            grpBulkLaunch.ResumeLayout(false);
            grpBulkLaunch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGw1BulkDelay).EndInit();
            grpDll.ResumeLayout(false);
            grpDll.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox cbGw1RenameWindowTitle;
        private System.Windows.Forms.TextBox txtGw1TitleTemplate;
        private System.Windows.Forms.Label lblGw1TitleHelp;
        private System.Windows.Forms.GroupBox grpWindow;
        
        private System.Windows.Forms.GroupBox grpBulkLaunch;
        private System.Windows.Forms.Label lblGw1BulkDelay;
        private System.Windows.Forms.NumericUpDown numGw1BulkDelay;
        private System.Windows.Forms.Label lblGw1BulkDelayHelp;
        
        private System.Windows.Forms.CheckBox cbGlobalToolbox;
        private System.Windows.Forms.TextBox txtToolbox;
        private System.Windows.Forms.Button btnBrowseToolbox;

        private System.Windows.Forms.CheckBox cbGlobalPy4Gw;
        private System.Windows.Forms.TextBox txtPy4GW;
        private System.Windows.Forms.Button btnBrowsePy4GW;

        private System.Windows.Forms.CheckBox cbGlobalGMod;
        private System.Windows.Forms.TextBox txtGMod;
        private System.Windows.Forms.Button btnBrowseGMod;
        
        private System.Windows.Forms.Button btnApplyGlobalFlags;
        private System.Windows.Forms.Button btnApplyGlobalPaths;
        private System.Windows.Forms.Label lblDllHelp;
        private System.Windows.Forms.GroupBox grpDll;
    }
}
