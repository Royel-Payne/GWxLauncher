namespace GWxLauncher.UI.TabControls
{
    partial class ModsTabContent
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
            this.pnlGw1Mods = new System.Windows.Forms.Panel();
            this.pnlGw2RunAfter = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlGw1Mods
            // 
            this.pnlGw1Mods.AutoSize = true;
            this.pnlGw1Mods.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlGw1Mods.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGw1Mods.Location = new System.Drawing.Point(10, 10);
            this.pnlGw1Mods.Name = "pnlGw1Mods";
            this.pnlGw1Mods.Size = new System.Drawing.Size(530, 0);
            this.pnlGw1Mods.TabIndex = 0;
            this.pnlGw1Mods.Visible = false;
            // 
            // pnlGw2RunAfter
            // 
            this.pnlGw2RunAfter.AutoSize = true;
            this.pnlGw2RunAfter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlGw2RunAfter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGw2RunAfter.Location = new System.Drawing.Point(10, 10);
            this.pnlGw2RunAfter.Name = "pnlGw2RunAfter";
            this.pnlGw2RunAfter.Size = new System.Drawing.Size(530, 0);
            this.pnlGw2RunAfter.TabIndex = 1;
            this.pnlGw2RunAfter.Visible = false;
            // 
            // ModsTabContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.pnlGw2RunAfter);
            this.Controls.Add(this.pnlGw1Mods);
            this.Name = "ModsTabContent";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(550, 400);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlGw1Mods;
        private System.Windows.Forms.Panel pnlGw2RunAfter;
    }
}
