using static System.Net.Mime.MediaTypeNames;

namespace GWxLauncher.UI
{
    partial class GlobalSettingsForm
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

        private void InitializeComponent()
        {
            btnApplyGlobalFlags = new Button();
            btnApplyGlobalPaths = new Button();
            cbGlobalToolbox = new CheckBox();
            cbGlobalPy4Gw = new CheckBox();
            cbGlobalGMod = new CheckBox();
            grpDll = new GroupBox();
            label5 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            btnBrowseGMod = new Button();
            txtGMod = new TextBox();
            btnBrowsePy4Gw = new Button();
            txtPy4GW = new TextBox();
            btnBrowseToolbox = new Button();
            txtToolbox = new TextBox();
            grpGeneral = new GroupBox();
            rbLight = new RadioButton();
            rbDark = new RadioButton();
            label4 = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            grpImport = new GroupBox();
            label6 = new Label();
            btnImportAccountsJson = new Button();
            grpDll.SuspendLayout();
            grpGeneral.SuspendLayout();
            grpImport.SuspendLayout();
            SuspendLayout();
            // 
            // btnApplyGlobalFlags
            // 
            btnApplyGlobalFlags.Location = new Point(61, 206);
            btnApplyGlobalFlags.Name = "btnApplyGlobalFlags";
            btnApplyGlobalFlags.Size = new Size(280, 23);
            btnApplyGlobalFlags.TabIndex = 16;
            btnApplyGlobalFlags.Text = "Apply global mod settings to all profiles";
            btnApplyGlobalFlags.UseVisualStyleBackColor = true;
            btnApplyGlobalFlags.Click += btnApplyGlobalFlags_Click;
            // 
            // btnApplyGlobalPaths
            // 
            btnApplyGlobalPaths.Location = new Point(61, 235);
            btnApplyGlobalPaths.Name = "btnApplyGlobalPaths";
            btnApplyGlobalPaths.Size = new Size(280, 23);
            btnApplyGlobalPaths.TabIndex = 17;
            btnApplyGlobalPaths.Text = "Apply global paths to all profiles";
            btnApplyGlobalPaths.UseVisualStyleBackColor = true;
            btnApplyGlobalPaths.Click += btnApplyGlobalPaths_Click;
            // 
            // cbGlobalToolbox
            // 
            cbGlobalToolbox.AutoSize = true;
            cbGlobalToolbox.Location = new Point(61, 131);
            cbGlobalToolbox.Name = "cbGlobalToolbox";
            cbGlobalToolbox.Size = new Size(151, 19);
            cbGlobalToolbox.TabIndex = 13;
            cbGlobalToolbox.Text = "Enable Toolbox globally";
            cbGlobalToolbox.UseVisualStyleBackColor = true;
            // 
            // cbGlobalPy4Gw
            // 
            cbGlobalPy4Gw.AutoSize = true;
            cbGlobalPy4Gw.Location = new Point(61, 156);
            cbGlobalPy4Gw.Name = "cbGlobalPy4Gw";
            cbGlobalPy4Gw.Size = new Size(147, 19);
            cbGlobalPy4Gw.TabIndex = 14;
            cbGlobalPy4Gw.Text = "Enable Py4GW globally";
            cbGlobalPy4Gw.UseVisualStyleBackColor = true;
            // 
            // cbGlobalGMod
            // 
            cbGlobalGMod.AutoSize = true;
            cbGlobalGMod.Location = new Point(61, 181);
            cbGlobalGMod.Name = "cbGlobalGMod";
            cbGlobalGMod.Size = new Size(141, 19);
            cbGlobalGMod.TabIndex = 15;
            cbGlobalGMod.Text = "Enable gMod globally";
            cbGlobalGMod.UseVisualStyleBackColor = true;
            // 
            // grpDll
            // 
            grpDll.Controls.Add(btnApplyGlobalPaths);
            grpDll.Controls.Add(btnApplyGlobalFlags);
            grpDll.Controls.Add(label5);
            grpDll.Controls.Add(label3);
            grpDll.Controls.Add(label2);
            grpDll.Controls.Add(label1);
            grpDll.Controls.Add(btnBrowseGMod);
            grpDll.Controls.Add(txtGMod);
            grpDll.Controls.Add(btnBrowsePy4Gw);
            grpDll.Controls.Add(txtPy4GW);
            grpDll.Controls.Add(btnBrowseToolbox);
            grpDll.Controls.Add(txtToolbox);
            grpDll.Controls.Add(cbGlobalGMod);
            grpDll.Controls.Add(cbGlobalPy4Gw);
            grpDll.Controls.Add(cbGlobalToolbox);
            grpDll.Location = new Point(309, 12);
            grpDll.Name = "grpDll";
            grpDll.Size = new Size(400, 331);
            grpDll.TabIndex = 6;
            grpDll.TabStop = false;
            grpDll.Text = "DLL Injection (Default Paths)";
            // 
            // label5
            // 
            label5.ForeColor = Color.Goldenrod;
            label5.Location = new Point(16, 292);
            label5.Name = "label5";
            label5.Size = new Size(365, 36);
            label5.TabIndex = 12;
            label5.Text = "\"These paths are automatically reused for new profiles and imports when available.\"\r\n";
            label5.Click += label5_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 62);
            label3.Name = "label3";
            label3.Size = new Size(45, 15);
            label3.TabIndex = 11;
            label3.Text = "Py4GW";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 94);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 10;
            label2.Text = "gMod";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 30);
            label1.Name = "label1";
            label1.Size = new Size(49, 15);
            label1.TabIndex = 9;
            label1.Text = "Toolbox";
            // 
            // btnBrowseGMod
            // 
            btnBrowseGMod.Location = new Point(306, 90);
            btnBrowseGMod.Name = "btnBrowseGMod";
            btnBrowseGMod.Size = new Size(75, 23);
            btnBrowseGMod.TabIndex = 8;
            btnBrowseGMod.Text = "Browse...";
            btnBrowseGMod.UseVisualStyleBackColor = true;
            btnBrowseGMod.Click += btnBrowseGMod_Click;
            // 
            // txtGMod
            // 
            txtGMod.Location = new Point(61, 90);
            txtGMod.Name = "txtGMod";
            txtGMod.Size = new Size(235, 23);
            txtGMod.TabIndex = 7;
            // 
            // btnBrowsePy4Gw
            // 
            btnBrowsePy4Gw.Location = new Point(306, 58);
            btnBrowsePy4Gw.Name = "btnBrowsePy4Gw";
            btnBrowsePy4Gw.Size = new Size(75, 23);
            btnBrowsePy4Gw.TabIndex = 5;
            btnBrowsePy4Gw.Text = "Browse...";
            btnBrowsePy4Gw.UseVisualStyleBackColor = true;
            btnBrowsePy4Gw.Click += btnBrowsePy4GW_Click;
            // 
            // txtPy4GW
            // 
            txtPy4GW.Location = new Point(61, 58);
            txtPy4GW.Name = "txtPy4GW";
            txtPy4GW.Size = new Size(235, 23);
            txtPy4GW.TabIndex = 4;
            // 
            // btnBrowseToolbox
            // 
            btnBrowseToolbox.Location = new Point(306, 26);
            btnBrowseToolbox.Name = "btnBrowseToolbox";
            btnBrowseToolbox.Size = new Size(75, 23);
            btnBrowseToolbox.TabIndex = 2;
            btnBrowseToolbox.Text = "Browse...";
            btnBrowseToolbox.UseVisualStyleBackColor = true;
            btnBrowseToolbox.Click += btnBrowseToolbox_Click;
            // 
            // txtToolbox
            // 
            txtToolbox.Location = new Point(61, 26);
            txtToolbox.Name = "txtToolbox";
            txtToolbox.Size = new Size(235, 23);
            txtToolbox.TabIndex = 1;
            // 
            // grpGeneral
            // 
            grpGeneral.Controls.Add(rbLight);
            grpGeneral.Controls.Add(rbDark);
            grpGeneral.Controls.Add(label4);
            grpGeneral.Location = new Point(12, 13);
            grpGeneral.Name = "grpGeneral";
            grpGeneral.Size = new Size(273, 113);
            grpGeneral.TabIndex = 7;
            grpGeneral.TabStop = false;
            grpGeneral.Text = "General";
            // 
            // rbLight
            // 
            rbLight.AutoSize = true;
            rbLight.Location = new Point(141, 32);
            rbLight.Name = "rbLight";
            rbLight.Size = new Size(52, 19);
            rbLight.TabIndex = 2;
            rbLight.TabStop = true;
            rbLight.Text = "Light";
            rbLight.UseVisualStyleBackColor = true;
            // 
            // rbDark
            // 
            rbDark.AutoSize = true;
            rbDark.Location = new Point(86, 32);
            rbDark.Name = "rbDark";
            rbDark.Size = new Size(49, 19);
            rbDark.TabIndex = 1;
            rbDark.TabStop = true;
            rbDark.Text = "Dark";
            rbDark.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(17, 34);
            label4.Name = "label4";
            label4.Size = new Size(44, 15);
            label4.TabIndex = 0;
            label4.Text = "Theme";
            // 
            // btnOk
            // 
            btnOk.Location = new Point(530, 369);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 8;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(615, 369);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // grpImport
            // 
            grpImport.Controls.Add(label6);
            grpImport.Controls.Add(btnImportAccountsJson);
            grpImport.Location = new Point(12, 153);
            grpImport.Name = "grpImport";
            grpImport.Size = new Size(273, 117);
            grpImport.TabIndex = 10;
            grpImport.TabStop = false;
            grpImport.Text = "Import";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 29);
            label6.Name = "label6";
            label6.Size = new Size(187, 30);
            label6.TabIndex = 1;
            label6.Text = "Import Py4GW Launcher accounts\r\n (accounts.json)";
            label6.Click += label6_Click;
            // 
            // btnImportAccountsJson
            // 
            btnImportAccountsJson.Location = new Point(183, 78);
            btnImportAccountsJson.Name = "btnImportAccountsJson";
            btnImportAccountsJson.Size = new Size(75, 23);
            btnImportAccountsJson.TabIndex = 0;
            btnImportAccountsJson.Text = "Import...";
            btnImportAccountsJson.UseVisualStyleBackColor = true;
            btnImportAccountsJson.Click += btnImportAccountsJson_Click;
            // 
            // GlobalSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(721, 406);
            Controls.Add(grpImport);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(grpGeneral);
            Controls.Add(grpDll);
            Name = "GlobalSettingsForm";
            Load += GlobalSettingsForm_Load_1;
            grpDll.ResumeLayout(false);
            grpDll.PerformLayout();
            grpGeneral.ResumeLayout(false);
            grpGeneral.PerformLayout();
            grpImport.ResumeLayout(false);
            grpImport.PerformLayout();
            ResumeLayout(false);

        }

        private Button btnApplyGlobalFlags;
        private Button btnApplyGlobalPaths;
        private CheckBox cbGlobalToolbox;
        private CheckBox cbGlobalPy4Gw;
        private CheckBox cbGlobalGMod;
        private GroupBox grpDll;
        private Button btnBrowseGMod;
        private TextBox txtGMod;
        private Button btnBrowsePy4Gw;
        private TextBox txtPy4GW;
        private Button btnBrowseToolbox;
        private TextBox txtToolbox;
        private Label label3;
        private Label label2;
        private Label label1;
        private GroupBox grpGeneral;
        private Label label4;
        private RadioButton rbDark;
        private RadioButton rbLight;
        private Button btnOk;
        private Button btnCancel;
        private Label label5;
        private GroupBox grpImport;
        private Label label6;
        private Button btnImportAccountsJson;
    }
}
