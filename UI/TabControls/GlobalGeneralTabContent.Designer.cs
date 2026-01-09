namespace GWxLauncher.UI.TabControls
{
    partial class GlobalGeneralTabContent
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.rbLight = new System.Windows.Forms.RadioButton();
            this.rbDark = new System.Windows.Forms.RadioButton();
            this.grpTheme = new System.Windows.Forms.GroupBox();
            this.grpImport = new System.Windows.Forms.GroupBox();
            this.btnImportAccountsJson = new System.Windows.Forms.Button();
            this.lblImportInfo = new System.Windows.Forms.Label();

            this.grpTheme.SuspendLayout();
            this.grpImport.SuspendLayout();
            this.SuspendLayout();

            // 
            // grpTheme
            // 
            this.grpTheme.Controls.Add(this.rbLight);
            this.grpTheme.Controls.Add(this.rbDark);
            this.grpTheme.Location = new System.Drawing.Point(10, 10);
            this.grpTheme.Name = "grpTheme";
            this.grpTheme.Size = new System.Drawing.Size(400, 80);
            this.grpTheme.TabIndex = 0;
            this.grpTheme.TabStop = false;
            this.grpTheme.Text = "Appearance";

            // 
            // rbLight
            // 
            this.rbLight.AutoSize = true;
            this.rbLight.Location = new System.Drawing.Point(20, 30);
            this.rbLight.Name = "rbLight";
            this.rbLight.Size = new System.Drawing.Size(52, 19);
            this.rbLight.TabIndex = 0;
            this.rbLight.TabStop = true;
            this.rbLight.Text = "Light";
            this.rbLight.UseVisualStyleBackColor = true;

            // 
            // rbDark
            // 
            this.rbDark.AutoSize = true;
            this.rbDark.Location = new System.Drawing.Point(100, 30);
            this.rbDark.Name = "rbDark";
            this.rbDark.Size = new System.Drawing.Size(49, 19);
            this.rbDark.TabIndex = 1;
            this.rbDark.TabStop = true;
            this.rbDark.Text = "Dark";
            this.rbDark.UseVisualStyleBackColor = true;

            // 
            // grpImport
            // 
            this.grpImport.Controls.Add(this.lblImportInfo);
            this.grpImport.Controls.Add(this.btnImportAccountsJson);
            this.grpImport.Location = new System.Drawing.Point(10, 100);
            this.grpImport.Name = "grpImport";
            this.grpImport.Size = new System.Drawing.Size(400, 100);
            this.grpImport.TabIndex = 1;
            this.grpImport.TabStop = false;
            this.grpImport.Text = "Data";

            // 
            // lblImportInfo
            // 
            this.lblImportInfo.AutoSize = true;
            this.lblImportInfo.Location = new System.Drawing.Point(20, 30);
            this.lblImportInfo.Name = "lblImportInfo";
            this.lblImportInfo.Size = new System.Drawing.Size(250, 30);
            this.lblImportInfo.TabIndex = 0;
            this.lblImportInfo.Text = "Import profiles from Py4GW Launcher\r\n(accounts.json)";

            // 
            // btnImportAccountsJson
            // 
            this.btnImportAccountsJson.Location = new System.Drawing.Point(280, 30);
            this.btnImportAccountsJson.Name = "btnImportAccountsJson";
            this.btnImportAccountsJson.Size = new System.Drawing.Size(100, 30);
            this.btnImportAccountsJson.TabIndex = 1;
            this.btnImportAccountsJson.Text = "Import...";
            this.btnImportAccountsJson.UseVisualStyleBackColor = true;

            // 
            // GlobalGeneralTabContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpImport);
            this.Controls.Add(this.grpTheme);
            this.Name = "GlobalGeneralTabContent";
            this.Size = new System.Drawing.Size(500, 400);
            this.grpTheme.ResumeLayout(false);
            this.grpTheme.PerformLayout();
            this.grpImport.ResumeLayout(false);
            this.grpImport.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpTheme;
        private System.Windows.Forms.RadioButton rbLight;
        private System.Windows.Forms.RadioButton rbDark;
        private System.Windows.Forms.GroupBox grpImport;
        private System.Windows.Forms.Label lblImportInfo;
        private System.Windows.Forms.Button btnImportAccountsJson;
    }
}
