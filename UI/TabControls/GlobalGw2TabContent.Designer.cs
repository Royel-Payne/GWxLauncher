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
            grpWindow.SuspendLayout();
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
            // GlobalGw2TabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpWindow);
            Name = "GlobalGw2TabContent";
            Size = new Size(500, 150);
            grpWindow.ResumeLayout(false);
            grpWindow.PerformLayout();
            ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox cbGw2RenameWindowTitle;
        private System.Windows.Forms.TextBox txtGw2TitleTemplate;
        private System.Windows.Forms.Label lblGw2TitleHelp;
        private System.Windows.Forms.GroupBox grpWindow;
    }
}
