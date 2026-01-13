using System.Windows.Forms;
using System.Drawing;

namespace GWxLauncher.UI.TabControls
{
    partial class GeneralTabContent
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            tlpMain = new TableLayoutPanel();
            lblProfileName = new Label();
            txtProfileName = new TextBox();
            lblExecutable = new Label();
            pnlExeInput = new Panel();
            txtExecutable = new TextBox();
            btnBrowseExe = new Button();
            lblArgs = new Label();
            txtArgs = new TextBox();
            lblWindowTitle = new Label();
            txtWindowTitle = new TextBox();
            tlpMain.SuspendLayout();
            pnlExeInput.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.AutoSize = true;
            tlpMain.ColumnCount = 2;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(lblProfileName, 0, 0);
            tlpMain.Controls.Add(txtProfileName, 1, 0);
            tlpMain.Controls.Add(lblExecutable, 0, 1);
            tlpMain.Controls.Add(pnlExeInput, 1, 1);
            tlpMain.Controls.Add(lblArgs, 0, 2);
            tlpMain.Controls.Add(txtArgs, 1, 2);
            tlpMain.Controls.Add(lblWindowTitle, 0, 3);
            tlpMain.Controls.Add(txtWindowTitle, 1, 3);
            tlpMain.Dock = DockStyle.Top;
            tlpMain.Location = new Point(15, 15);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 4;
            tlpMain.RowStyles.Add(new RowStyle());
            tlpMain.RowStyles.Add(new RowStyle());
            tlpMain.RowStyles.Add(new RowStyle());
            tlpMain.RowStyles.Add(new RowStyle());
            tlpMain.Size = new Size(520, 116);
            tlpMain.TabIndex = 0;
            // 
            // lblProfileName
            // 
            lblProfileName.AutoSize = true;
            lblProfileName.Dock = DockStyle.Fill;
            lblProfileName.Location = new Point(4, 4);
            lblProfileName.Margin = new Padding(4);
            lblProfileName.Name = "lblProfileName";
            lblProfileName.Size = new Size(132, 23);
            lblProfileName.TabIndex = 0;
            lblProfileName.Text = "Display Name:";
            lblProfileName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtProfileName
            // 
            txtProfileName.Dock = DockStyle.Fill;
            txtProfileName.Location = new Point(144, 4);
            txtProfileName.Margin = new Padding(4);
            txtProfileName.Name = "txtProfileName";
            txtProfileName.Size = new Size(372, 23);
            txtProfileName.TabIndex = 1;
            // 
            // lblExecutable
            // 
            lblExecutable.AutoSize = true;
            lblExecutable.Dock = DockStyle.Fill;
            lblExecutable.Location = new Point(4, 35);
            lblExecutable.Margin = new Padding(4);
            lblExecutable.Name = "lblExecutable";
            lblExecutable.Size = new Size(132, 15);
            lblExecutable.TabIndex = 2;
            lblExecutable.Text = "Game Executable:";
            lblExecutable.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlExeInput
            // 
            pnlExeInput.AutoSize = true;
            pnlExeInput.Controls.Add(txtExecutable);
            pnlExeInput.Controls.Add(btnBrowseExe);
            pnlExeInput.Dock = DockStyle.Fill;
            pnlExeInput.Location = new Point(140, 31);
            pnlExeInput.Margin = new Padding(0);
            pnlExeInput.Name = "pnlExeInput";
            pnlExeInput.Size = new Size(380, 23);
            pnlExeInput.TabIndex = 3;
            // 
            // txtExecutable
            // 
            txtExecutable.Dock = DockStyle.Fill;
            txtExecutable.Location = new Point(0, 0);
            txtExecutable.Margin = new Padding(4);
            txtExecutable.Name = "txtExecutable";
            txtExecutable.Size = new Size(340, 23);
            txtExecutable.TabIndex = 0;
            // 
            // btnBrowseExe
            // 
            btnBrowseExe.Dock = DockStyle.Right;
            btnBrowseExe.Location = new Point(340, 0);
            btnBrowseExe.Margin = new Padding(4);
            btnBrowseExe.Name = "btnBrowseExe";
            btnBrowseExe.Size = new Size(40, 23);
            btnBrowseExe.TabIndex = 1;
            btnBrowseExe.Text = "...";
            // 
            // lblArgs
            // 
            lblArgs.AutoSize = true;
            lblArgs.Dock = DockStyle.Fill;
            lblArgs.Location = new Point(4, 58);
            lblArgs.Margin = new Padding(4);
            lblArgs.Name = "lblArgs";
            lblArgs.Size = new Size(132, 23);
            lblArgs.TabIndex = 4;
            lblArgs.Text = "Launch Arguments:";
            lblArgs.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtArgs
            // 
            txtArgs.Dock = DockStyle.Fill;
            txtArgs.Location = new Point(144, 58);
            txtArgs.Margin = new Padding(4);
            txtArgs.Name = "txtArgs";
            txtArgs.Size = new Size(372, 23);
            txtArgs.TabIndex = 5;
            // 
            // lblWindowTitle
            // 
            lblWindowTitle.AutoSize = true;
            lblWindowTitle.Dock = DockStyle.Fill;
            lblWindowTitle.Location = new Point(4, 89);
            lblWindowTitle.Margin = new Padding(4);
            lblWindowTitle.Name = "lblWindowTitle";
            lblWindowTitle.Size = new Size(132, 23);
            lblWindowTitle.TabIndex = 6;
            lblWindowTitle.Text = "Window Title Label:";
            lblWindowTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtWindowTitle
            // 
            txtWindowTitle.Dock = DockStyle.Fill;
            txtWindowTitle.Location = new Point(144, 89);
            txtWindowTitle.Margin = new Padding(4);
            txtWindowTitle.Name = "txtWindowTitle";
            txtWindowTitle.Size = new Size(372, 23);
            txtWindowTitle.TabIndex = 7;
            // 
            // GeneralTabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(tlpMain);
            Name = "GeneralTabContent";
            Padding = new Padding(15);
            Size = new Size(550, 450);
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
            pnlExeInput.ResumeLayout(false);
            pnlExeInput.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblProfileName;
        private System.Windows.Forms.TextBox txtProfileName;
        private System.Windows.Forms.Label lblExecutable;
        private System.Windows.Forms.Panel pnlExeInput;
        private System.Windows.Forms.TextBox txtExecutable;
        private System.Windows.Forms.Button btnBrowseExe;
        private System.Windows.Forms.Label lblArgs;
        private System.Windows.Forms.TextBox txtArgs;
        private System.Windows.Forms.Label lblWindowTitle;
        private System.Windows.Forms.TextBox txtWindowTitle;
    }
}
