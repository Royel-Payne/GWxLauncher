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
            chkWindowedEnabled = new CheckBox();
            grpPosition = new GroupBox();
            lblH = new Label();
            numH = new NumericUpDown();
            lblW = new Label();
            numW = new NumericUpDown();
            lblY = new Label();
            numY = new NumericUpDown();
            lblX = new Label();
            numX = new NumericUpDown();
            grpBehavior = new GroupBox();
            chkBlockInputs = new CheckBox();
            chkLockWindow = new CheckBox();
            chkRememberChanges = new CheckBox();
            grpPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numH).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numW).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX).BeginInit();
            grpBehavior.SuspendLayout();
            SuspendLayout();
            // 
            // chkWindowedEnabled
            // 
            chkWindowedEnabled.AutoSize = true;
            chkWindowedEnabled.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkWindowedEnabled.Location = new Point(13, 13);
            chkWindowedEnabled.Name = "chkWindowedEnabled";
            chkWindowedEnabled.Size = new Size(160, 19);
            chkWindowedEnabled.TabIndex = 0;
            chkWindowedEnabled.Text = "Force Windowed Mode";
            chkWindowedEnabled.UseVisualStyleBackColor = true;
            // 
            // grpPosition
            // 
            grpPosition.Controls.Add(lblH);
            grpPosition.Controls.Add(numH);
            grpPosition.Controls.Add(lblW);
            grpPosition.Controls.Add(numW);
            grpPosition.Controls.Add(lblY);
            grpPosition.Controls.Add(numY);
            grpPosition.Controls.Add(lblX);
            grpPosition.Controls.Add(numX);
            grpPosition.Location = new Point(13, 50);
            grpPosition.Name = "grpPosition";
            grpPosition.Size = new Size(400, 100);
            grpPosition.TabIndex = 1;
            grpPosition.TabStop = false;
            grpPosition.Text = "Initial Position & Size";
            // 
            // lblH
            // 
            lblH.AutoSize = true;
            lblH.Location = new Point(160, 60);
            lblH.Name = "lblH";
            lblH.Size = new Size(46, 15);
            lblH.TabIndex = 7;
            lblH.Text = "Height:";
            // 
            // numH
            // 
            numH.Location = new Point(212, 58);
            numH.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numH.Name = "numH";
            numH.Size = new Size(80, 23);
            numH.TabIndex = 6;
            // 
            // lblW
            // 
            lblW.AutoSize = true;
            lblW.Location = new Point(20, 60);
            lblW.Name = "lblW";
            lblW.Size = new Size(42, 15);
            lblW.TabIndex = 5;
            lblW.Text = "Width:";
            // 
            // numW
            // 
            numW.Location = new Point(68, 58);
            numW.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numW.Name = "numW";
            numW.Size = new Size(80, 23);
            numW.TabIndex = 4;
            // 
            // lblY
            // 
            lblY.AutoSize = true;
            lblY.Location = new Point(189, 27);
            lblY.Name = "lblY";
            lblY.Size = new Size(17, 15);
            lblY.TabIndex = 3;
            lblY.Text = "Y:";
            // 
            // numY
            // 
            numY.Location = new Point(212, 25);
            numY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numY.Minimum = new decimal(new int[] { 10000, 0, 0, -2147483648 });
            numY.Name = "numY";
            numY.Size = new Size(80, 23);
            numY.TabIndex = 2;
            // 
            // lblX
            // 
            lblX.AutoSize = true;
            lblX.Location = new Point(45, 27);
            lblX.Name = "lblX";
            lblX.Size = new Size(17, 15);
            lblX.TabIndex = 1;
            lblX.Text = "X:";
            // 
            // numX
            // 
            numX.Location = new Point(68, 25);
            numX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numX.Minimum = new decimal(new int[] { 10000, 0, 0, -2147483648 });
            numX.Name = "numX";
            numX.Size = new Size(80, 23);
            numX.TabIndex = 0;
            // 
            // grpBehavior
            // 
            grpBehavior.Controls.Add(chkBlockInputs);
            grpBehavior.Controls.Add(chkLockWindow);
            grpBehavior.Controls.Add(chkRememberChanges);
            grpBehavior.Location = new Point(13, 170);
            grpBehavior.Name = "grpBehavior";
            grpBehavior.Size = new Size(400, 115);
            grpBehavior.TabIndex = 2;
            grpBehavior.TabStop = false;
            grpBehavior.Text = "Behavior";
            // 
            // chkBlockInputs
            // 
            chkBlockInputs.AutoSize = true;
            chkBlockInputs.Location = new Point(20, 80);
            chkBlockInputs.Name = "chkBlockInputs";
            chkBlockInputs.Size = new Size(195, 19);
            chkBlockInputs.TabIndex = 2;
            chkBlockInputs.Text = "Block minimize and close (X) button";
            chkBlockInputs.UseVisualStyleBackColor = true;
            // 
            // chkLockWindow
            // 
            chkLockWindow.AutoSize = true;
            chkLockWindow.Location = new Point(20, 52);
            chkLockWindow.Name = "chkLockWindow";
            chkLockWindow.Size = new Size(207, 19);
            chkLockWindow.TabIndex = 1;
            chkLockWindow.Text = "Prevent resizing or moving the window";
            chkLockWindow.UseVisualStyleBackColor = true;
            // 
            // chkRememberChanges
            // 
            chkRememberChanges.AutoSize = true;
            chkRememberChanges.Location = new Point(20, 25);
            chkRememberChanges.Name = "chkRememberChanges";
            chkRememberChanges.Size = new Size(300, 19);
            chkRememberChanges.TabIndex = 0;
            chkRememberChanges.Text = "Remember changes when resizing/moving the window";
            chkRememberChanges.UseVisualStyleBackColor = true;
            // 
            // WindowTabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpBehavior);
            Controls.Add(grpPosition);
            Controls.Add(chkWindowedEnabled);
            Name = "WindowTabContent";
            Padding = new Padding(10);
            Size = new Size(550, 400);
            grpPosition.ResumeLayout(false);
            grpPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numH).EndInit();
            ((System.ComponentModel.ISupportInitialize)numW).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX).EndInit();
            grpBehavior.ResumeLayout(false);
            grpBehavior.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private CheckBox chkWindowedEnabled;
        private GroupBox grpPosition;
        private Label lblH;
        private NumericUpDown numH;
        private Label lblW;
        private NumericUpDown numW;
        private Label lblY;
        private NumericUpDown numY;
        private Label lblX;
        private NumericUpDown numX;
        private GroupBox grpBehavior;
        private CheckBox chkBlockInputs;
        private CheckBox chkLockWindow;
        private CheckBox chkRememberChanges;
    }
}
#endregion
