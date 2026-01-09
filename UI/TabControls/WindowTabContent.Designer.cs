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
            this.lblPlaceholder = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPlaceholder
            // 
            this.lblPlaceholder.AutoSize = true;
            this.lblPlaceholder.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPlaceholder.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPlaceholder.Location = new System.Drawing.Point(10, 10);
            this.lblPlaceholder.Name = "lblPlaceholder";
            this.lblPlaceholder.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.lblPlaceholder.Size = new System.Drawing.Size(285, 39);
            this.lblPlaceholder.TabIndex = 0;
            this.lblPlaceholder.Text = "Windowed mode settings will appear here.";
            // 
            // WindowTabContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.lblPlaceholder);
            this.Name = "WindowTabContent";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(550, 400);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblPlaceholder;
    }
}
