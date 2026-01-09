namespace GWxLauncher.UI.TabControls
{
    partial class LoginTabContent
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
            this.pnlGw1Login = new System.Windows.Forms.Panel();
            this.pnlGw2Login = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlGw1Login
            // 
            this.pnlGw1Login.AutoSize = true;
            this.pnlGw1Login.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlGw1Login.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGw1Login.Location = new System.Drawing.Point(10, 10);
            this.pnlGw1Login.Name = "pnlGw1Login";
            this.pnlGw1Login.Size = new System.Drawing.Size(530, 0);
            this.pnlGw1Login.TabIndex = 0;
            this.pnlGw1Login.Visible = false;
            // 
            // pnlGw2Login
            // 
            this.pnlGw2Login.AutoSize = true;
            this.pnlGw2Login.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlGw2Login.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGw2Login.Location = new System.Drawing.Point(10, 10);
            this.pnlGw2Login.Name = "pnlGw2Login";
            this.pnlGw2Login.Size = new System.Drawing.Size(530, 0);
            this.pnlGw2Login.TabIndex = 1;
            this.pnlGw2Login.Visible = false;
            // 
            // LoginTabContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.pnlGw2Login);
            this.Controls.Add(this.pnlGw1Login);
            this.Name = "LoginTabContent";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(550, 400);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlGw1Login;
        private System.Windows.Forms.Panel pnlGw2Login;
    }
}
