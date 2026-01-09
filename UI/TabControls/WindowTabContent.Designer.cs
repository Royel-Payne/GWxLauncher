namespace GWxLauncher.UI.TabControls
{
    partial class WindowTabContent
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
            lblPlaceholder = new Label();
            SuspendLayout();
            // 
            // lblPlaceholder
            // 
            lblPlaceholder.AutoSize = true;
            lblPlaceholder.Dock = DockStyle.Top;
            lblPlaceholder.Font = new Font("Segoe UI", 10F);
            lblPlaceholder.Location = new Point(10, 10);
            lblPlaceholder.Name = "lblPlaceholder";
            lblPlaceholder.Padding = new Padding(0, 10, 0, 10);
            lblPlaceholder.Size = new Size(268, 39);
            lblPlaceholder.TabIndex = 0;
            lblPlaceholder.Text = "Windowed mode settings will appear here.";
            // 
            // WindowTabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(lblPlaceholder);
            Name = "WindowTabContent";
            Padding = new Padding(10);
            Size = new Size(550, 400);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblPlaceholder;
    }
}
