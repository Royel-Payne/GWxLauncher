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
            grpDll.Location = new Point(10, 120);
            grpDll.Name = "grpDll";
            grpDll.Size = new Size(460, 330);
            grpDll.TabIndex = 1;
            grpDll.TabStop = false;
            grpDll.Text = "DLL Injection (Globals)";
            // 
            // GlobalGw1TabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpDll);
            Controls.Add(grpWindow);
            Name = "GlobalGw1TabContent";
            Size = new Size(500, 500);
            grpWindow.ResumeLayout(false);
            grpWindow.PerformLayout();
            grpDll.ResumeLayout(false);
            grpDll.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox cbGw1RenameWindowTitle;
        private System.Windows.Forms.TextBox txtGw1TitleTemplate;
        private System.Windows.Forms.Label lblGw1TitleHelp;
        private System.Windows.Forms.GroupBox grpWindow;
        
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
