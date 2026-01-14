namespace GWxLauncher.UI.Dialogs
{
    partial class Gw2IsolationSetupDialog
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.checkedListBoxProfiles = new System.Windows.Forms.CheckedListBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.Location = new System.Drawing.Point(12, 9);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(560, 60);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Some profiles share the same game folder. Each profile needs a unique game folde" +
    "r for isolation.";
            // 
            // checkedListBoxProfiles
            // 
            this.checkedListBoxProfiles.CheckOnClick = true;
            this.checkedListBoxProfiles.FormattingEnabled = true;
            this.checkedListBoxProfiles.Location = new System.Drawing.Point(12, 72);
            this.checkedListBoxProfiles.Name = "checkedListBoxProfiles";
            this.checkedListBoxProfiles.Size = new System.Drawing.Size(560, 244);
            this.checkedListBoxProfiles.TabIndex = 1;
            this.checkedListBoxProfiles.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxProfiles_ItemCheck);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(416, 332);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(156, 30);
            this.btnCopy.TabIndex = 2;
            this.btnCopy.Text = "Copy Selected Folders...";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(335, 332);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // Gw2IsolationSetupDialog
            // 
            this.AcceptButton = this.btnCopy;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(584, 374);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.checkedListBoxProfiles);
            this.Controls.Add(this.lblMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Gw2IsolationSetupDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GW2 Isolation Setup";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.CheckedListBox checkedListBoxProfiles;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnCancel;
    }
}
