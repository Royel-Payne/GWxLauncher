namespace GWxLauncher.UI.Dialogs
{
    partial class Gw2IsolationConfirmDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pictureBoxWarning = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblExplanation = new System.Windows.Forms.Label();
            this.lblProfileList = new System.Windows.Forms.Label();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.lblSpaceNote = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxWarning
            // 
            this.pictureBoxWarning.Location = new System.Drawing.Point(15, 20);
            this.pictureBoxWarning.Name = "pictureBoxWarning";
            this.pictureBoxWarning.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxWarning.TabIndex = 0;
            this.pictureBoxWarning.TabStop = false;
            this.pictureBoxWarning.Image = System.Drawing.SystemIcons.Warning.ToBitmap();
            this.pictureBoxWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(60, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(280, 15);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Profile Isolation requires each GW2 profile to have its";
            // 
            // lblExplanation
            // 
            this.lblExplanation.AutoSize = true;
            this.lblExplanation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblExplanation.Location = new System.Drawing.Point(60, 37);
            this.lblExplanation.Name = "lblExplanation";
            this.lblExplanation.Size = new System.Drawing.Size(75, 15);
            this.lblExplanation.TabIndex = 2;
            this.lblExplanation.Text = "own game folder.";
            // 
            // lblProfileList
            // 
            this.lblProfileList.Location = new System.Drawing.Point(60, 90);
            this.lblProfileList.Name = "lblProfileList";
            this.lblProfileList.Size = new System.Drawing.Size(310, 80);
            this.lblProfileList.TabIndex = 4;
            this.lblProfileList.Text = "• Profile 1\n• Profile 2";
            // 
            // lblQuestion
            // 
            this.lblQuestion.Location = new System.Drawing.Point(60, 180);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(310, 30);
            this.lblQuestion.TabIndex = 5;
            this.lblQuestion.Text = "Would you like to copy game folders for profiles that share paths?";
            // 
            // lblSpaceNote
            // 
            this.lblSpaceNote.AutoSize = true;
            this.lblSpaceNote.ForeColor = System.Drawing.Color.DarkRed;
            this.lblSpaceNote.Location = new System.Drawing.Point(60, 220);
            this.lblSpaceNote.Name = "lblSpaceNote";
            this.lblSpaceNote.Size = new System.Drawing.Size(250, 15);
            this.lblSpaceNote.TabIndex = 6;
            this.lblSpaceNote.Text = "Note: Each copy requires ~80GB of disk space.";
            // 
            // btnYes
            // 
            this.btnYes.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnYes.Location = new System.Drawing.Point(215, 255);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(75, 28);
            this.btnYes.TabIndex = 7;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            // 
            // btnNo
            // 
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnNo.Location = new System.Drawing.Point(296, 255);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(75, 28);
            this.btnNo.TabIndex = 8;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            // 
            // Gw2IsolationConfirmDialog
            // 
            this.AcceptButton = this.btnYes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnNo;
            this.ClientSize = new System.Drawing.Size(384, 295);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.lblSpaceNote);
            this.Controls.Add(this.lblQuestion);
            this.Controls.Add(this.lblProfileList);
            this.Controls.Add(this.lblExplanation);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pictureBoxWarning);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Gw2IsolationConfirmDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Duplicate Game Folders Detected";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxWarning;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblExplanation;
        private System.Windows.Forms.Label lblProfileList;
        private System.Windows.Forms.Label lblQuestion;
        private System.Windows.Forms.Label lblSpaceNote;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
    }
}
