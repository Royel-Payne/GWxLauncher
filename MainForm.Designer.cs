namespace GWxLauncher
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnLaunchGw1 = new Button();
            btnLaunchGw2 = new Button();
            lblStatus = new Label();
            btnSetGw1Path = new Button();
            btnSetGw2Path = new Button();
            SuspendLayout();
            // 
            // btnLaunchGw1
            // 
            btnLaunchGw1.Location = new Point(9, 32);
            btnLaunchGw1.Name = "btnLaunchGw1";
            btnLaunchGw1.Size = new Size(223, 23);
            btnLaunchGw1.TabIndex = 0;
            btnLaunchGw1.Text = "Launch Guild Wars 1";
            btnLaunchGw1.UseVisualStyleBackColor = true;
            btnLaunchGw1.Click += btnLaunchGw1_Click;
            // 
            // btnLaunchGw2
            // 
            btnLaunchGw2.Location = new Point(8, 172);
            btnLaunchGw2.Name = "btnLaunchGw2";
            btnLaunchGw2.Size = new Size(224, 23);
            btnLaunchGw2.TabIndex = 1;
            btnLaunchGw2.Text = "Launch Guild Wars 2";
            btnLaunchGw2.UseVisualStyleBackColor = true;
            btnLaunchGw2.Click += btnLaunchGw2_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(6, 301);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Ready";
            // 
            // btnSetGw1Path
            // 
            btnSetGw1Path.Location = new Point(12, 61);
            btnSetGw1Path.Name = "btnSetGw1Path";
            btnSetGw1Path.Size = new Size(101, 23);
            btnSetGw1Path.TabIndex = 3;
            btnSetGw1Path.Text = "Set GW1 Path";
            btnSetGw1Path.UseVisualStyleBackColor = true;
            btnSetGw1Path.Click += btnSetGw1Path_Click;
            // 
            // btnSetGw2Path
            // 
            btnSetGw2Path.Location = new Point(12, 199);
            btnSetGw2Path.Name = "btnSetGw2Path";
            btnSetGw2Path.Size = new Size(101, 23);
            btnSetGw2Path.TabIndex = 4;
            btnSetGw2Path.Text = "Set GW2 Path";
            btnSetGw2Path.UseVisualStyleBackColor = true;
            btnSetGw2Path.Click += btnSetGw2Path_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(244, 321);
            Controls.Add(btnSetGw2Path);
            Controls.Add(btnSetGw1Path);
            Controls.Add(lblStatus);
            Controls.Add(btnLaunchGw2);
            Controls.Add(btnLaunchGw1);
            Name = "MainForm";
            Text = "GWxLauncher";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnLaunchGw1;
        private Button btnLaunchGw2;
        private Label lblStatus;
        private Button btnSetGw1Path;
        private Button btnSetGw2Path;
    }
}
