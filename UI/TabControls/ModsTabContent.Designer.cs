using System.Windows.Forms;
using System.Drawing;

namespace GWxLauncher.UI.TabControls
{
    partial class ModsTabContent
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
            tlpGw1 = new TableLayoutPanel();
            chkToolbox = new CheckBox();
            lblToolboxPath = new Label();
            pnlToolboxPath = new Panel();
            txtToolboxDll = new TextBox();
            btnBrowseToolboxDll = new Button();
            chkPy4Gw = new CheckBox();
            lblPy4GwPath = new Label();
            pnlPy4GwPath = new Panel();
            txtPy4GwDll = new TextBox();
            btnBrowsePy4GwDll = new Button();
            chkGMod = new CheckBox();
            lblGModPath = new Label();
            pnlGModPath = new Panel();
            txtGModDll = new TextBox();
            btnBrowseGModDll = new Button();
            lblGw1GModPlugins = new Label();
            pnlGModPlugins = new Panel();
            lvGw1GModPlugins = new ListView();
            colGw1GModPlugin = new ColumnHeader();
            btnGw1AddPlugin = new Button();
            btnGw1RemovePlugin = new Button();
            tlpGw2 = new TableLayoutPanel();
            chkGw2RunAfterEnabled = new CheckBox();
            lblRunAfterPrograms = new Label();
            pnlGw2RunAfterList = new Panel();
            lvGw2RunAfter = new ListView();
            colGw2RunAfterMumble = new ColumnHeader();
            colGw2RunAfterName = new ColumnHeader();
            btnGw2AddProgram = new Button();
            btnGw2RemoveProgram = new Button();
            lblGw2RunAfterTip = new Label();
            tlpGw1.SuspendLayout();
            pnlToolboxPath.SuspendLayout();
            pnlPy4GwPath.SuspendLayout();
            pnlGModPath.SuspendLayout();
            pnlGModPlugins.SuspendLayout();
            tlpGw2.SuspendLayout();
            pnlGw2RunAfterList.SuspendLayout();
            SuspendLayout();
            // 
            // tlpGw1
            // 
            tlpGw1.AutoSize = true;
            tlpGw1.ColumnCount = 2;
            tlpGw1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tlpGw1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpGw1.Controls.Add(pnlGModPlugins, 0, 7);
            tlpGw1.Controls.Add(chkToolbox, 0, 0);
            tlpGw1.Controls.Add(lblToolboxPath, 0, 1);
            tlpGw1.Controls.Add(pnlToolboxPath, 1, 1);
            tlpGw1.Controls.Add(chkPy4Gw, 0, 2);
            tlpGw1.Controls.Add(lblPy4GwPath, 0, 3);
            tlpGw1.Controls.Add(pnlPy4GwPath, 1, 3);
            tlpGw1.Controls.Add(chkGMod, 0, 4);
            tlpGw1.Controls.Add(lblGModPath, 0, 5);
            tlpGw1.Controls.Add(pnlGModPath, 1, 5);
            tlpGw1.Controls.Add(lblGw1GModPlugins, 0, 6);
            tlpGw1.Dock = DockStyle.Top;
            tlpGw1.Location = new Point(15, 296);
            tlpGw1.Name = "tlpGw1";
            tlpGw1.RowCount = 8;
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.RowStyles.Add(new RowStyle());
            tlpGw1.Size = new Size(520, 331);
            tlpGw1.TabIndex = 0;
            tlpGw1.Visible = false;
            // 
            // chkToolbox
            // 
            chkToolbox.AutoSize = true;
            tlpGw1.SetColumnSpan(chkToolbox, 2);
            chkToolbox.Location = new Point(4, 4);
            chkToolbox.Margin = new Padding(4);
            chkToolbox.Name = "chkToolbox";
            chkToolbox.Size = new Size(106, 19);
            chkToolbox.TabIndex = 0;
            chkToolbox.Text = "Enable Toolbox";
            // 
            // lblToolboxPath
            // 
            lblToolboxPath.AutoSize = true;
            lblToolboxPath.Dock = DockStyle.Fill;
            lblToolboxPath.Location = new Point(4, 31);
            lblToolboxPath.Margin = new Padding(4);
            lblToolboxPath.Name = "lblToolboxPath";
            lblToolboxPath.Size = new Size(132, 15);
            lblToolboxPath.TabIndex = 1;
            lblToolboxPath.Text = "DLL Path:";
            lblToolboxPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlToolboxPath
            // 
            pnlToolboxPath.AutoSize = true;
            pnlToolboxPath.Controls.Add(txtToolboxDll);
            pnlToolboxPath.Controls.Add(btnBrowseToolboxDll);
            pnlToolboxPath.Dock = DockStyle.Fill;
            pnlToolboxPath.Location = new Point(140, 27);
            pnlToolboxPath.Margin = new Padding(0);
            pnlToolboxPath.Name = "pnlToolboxPath";
            pnlToolboxPath.Size = new Size(380, 23);
            pnlToolboxPath.TabIndex = 2;
            // 
            // txtToolboxDll
            // 
            txtToolboxDll.Dock = DockStyle.Fill;
            txtToolboxDll.Location = new Point(0, 0);
            txtToolboxDll.Margin = new Padding(4);
            txtToolboxDll.Name = "txtToolboxDll";
            txtToolboxDll.Size = new Size(340, 23);
            txtToolboxDll.TabIndex = 0;
            // 
            // btnBrowseToolboxDll
            // 
            btnBrowseToolboxDll.Dock = DockStyle.Right;
            btnBrowseToolboxDll.Location = new Point(340, 0);
            btnBrowseToolboxDll.Margin = new Padding(4);
            btnBrowseToolboxDll.Name = "btnBrowseToolboxDll";
            btnBrowseToolboxDll.Size = new Size(40, 23);
            btnBrowseToolboxDll.TabIndex = 1;
            btnBrowseToolboxDll.Text = "...";
            // 
            // chkPy4Gw
            // 
            chkPy4Gw.AutoSize = true;
            tlpGw1.SetColumnSpan(chkPy4Gw, 2);
            chkPy4Gw.Location = new Point(4, 54);
            chkPy4Gw.Margin = new Padding(4);
            chkPy4Gw.Name = "chkPy4Gw";
            chkPy4Gw.Size = new Size(100, 19);
            chkPy4Gw.TabIndex = 3;
            chkPy4Gw.Text = "Enable Py4Gw";
            // 
            // lblPy4GwPath
            // 
            lblPy4GwPath.AutoSize = true;
            lblPy4GwPath.Dock = DockStyle.Fill;
            lblPy4GwPath.Location = new Point(4, 81);
            lblPy4GwPath.Margin = new Padding(4);
            lblPy4GwPath.Name = "lblPy4GwPath";
            lblPy4GwPath.Size = new Size(132, 15);
            lblPy4GwPath.TabIndex = 4;
            lblPy4GwPath.Text = "DLL Path:";
            lblPy4GwPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlPy4GwPath
            // 
            pnlPy4GwPath.AutoSize = true;
            pnlPy4GwPath.Controls.Add(txtPy4GwDll);
            pnlPy4GwPath.Controls.Add(btnBrowsePy4GwDll);
            pnlPy4GwPath.Dock = DockStyle.Fill;
            pnlPy4GwPath.Location = new Point(140, 77);
            pnlPy4GwPath.Margin = new Padding(0);
            pnlPy4GwPath.Name = "pnlPy4GwPath";
            pnlPy4GwPath.Size = new Size(380, 23);
            pnlPy4GwPath.TabIndex = 5;
            // 
            // txtPy4GwDll
            // 
            txtPy4GwDll.Dock = DockStyle.Fill;
            txtPy4GwDll.Location = new Point(0, 0);
            txtPy4GwDll.Margin = new Padding(4);
            txtPy4GwDll.Name = "txtPy4GwDll";
            txtPy4GwDll.Size = new Size(340, 23);
            txtPy4GwDll.TabIndex = 0;
            // 
            // btnBrowsePy4GwDll
            // 
            btnBrowsePy4GwDll.Dock = DockStyle.Right;
            btnBrowsePy4GwDll.Location = new Point(340, 0);
            btnBrowsePy4GwDll.Margin = new Padding(4);
            btnBrowsePy4GwDll.Name = "btnBrowsePy4GwDll";
            btnBrowsePy4GwDll.Size = new Size(40, 23);
            btnBrowsePy4GwDll.TabIndex = 1;
            btnBrowsePy4GwDll.Text = "...";
            // 
            // chkGMod
            // 
            chkGMod.AutoSize = true;
            tlpGw1.SetColumnSpan(chkGMod, 2);
            chkGMod.Location = new Point(4, 104);
            chkGMod.Margin = new Padding(4);
            chkGMod.Name = "chkGMod";
            chkGMod.Size = new Size(97, 19);
            chkGMod.TabIndex = 6;
            chkGMod.Text = "Enable GMod";
            // 
            // lblGModPath
            // 
            lblGModPath.AutoSize = true;
            lblGModPath.Dock = DockStyle.Fill;
            lblGModPath.Location = new Point(4, 131);
            lblGModPath.Margin = new Padding(4);
            lblGModPath.Name = "lblGModPath";
            lblGModPath.Size = new Size(132, 15);
            lblGModPath.TabIndex = 7;
            lblGModPath.Text = "DLL Path:";
            lblGModPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlGModPath
            // 
            pnlGModPath.AutoSize = true;
            pnlGModPath.Controls.Add(txtGModDll);
            pnlGModPath.Controls.Add(btnBrowseGModDll);
            pnlGModPath.Dock = DockStyle.Fill;
            pnlGModPath.Location = new Point(140, 127);
            pnlGModPath.Margin = new Padding(0);
            pnlGModPath.Name = "pnlGModPath";
            pnlGModPath.Size = new Size(380, 23);
            pnlGModPath.TabIndex = 8;
            // 
            // txtGModDll
            // 
            txtGModDll.Dock = DockStyle.Fill;
            txtGModDll.Location = new Point(0, 0);
            txtGModDll.Margin = new Padding(4);
            txtGModDll.Name = "txtGModDll";
            txtGModDll.Size = new Size(340, 23);
            txtGModDll.TabIndex = 0;
            // 
            // btnBrowseGModDll
            // 
            btnBrowseGModDll.Dock = DockStyle.Right;
            btnBrowseGModDll.Location = new Point(340, 0);
            btnBrowseGModDll.Margin = new Padding(4);
            btnBrowseGModDll.Name = "btnBrowseGModDll";
            btnBrowseGModDll.Size = new Size(40, 23);
            btnBrowseGModDll.TabIndex = 1;
            btnBrowseGModDll.Text = "...";
            // 
            // lblGw1GModPlugins
            // 
            lblGw1GModPlugins.AutoSize = true;
            tlpGw1.SetColumnSpan(lblGw1GModPlugins, 2);
            lblGw1GModPlugins.Location = new Point(4, 154);
            lblGw1GModPlugins.Margin = new Padding(4);
            lblGw1GModPlugins.Name = "lblGw1GModPlugins";
            lblGw1GModPlugins.Size = new Size(85, 15);
            lblGw1GModPlugins.TabIndex = 9;
            lblGw1GModPlugins.Text = "GMod Plugins:";
            // 
            // pnlGModPlugins
            // 
            tlpGw1.SetColumnSpan(pnlGModPlugins, 2);
            pnlGModPlugins.Controls.Add(lvGw1GModPlugins);
            pnlGModPlugins.Controls.Add(btnGw1AddPlugin);
            pnlGModPlugins.Controls.Add(btnGw1RemovePlugin);
            pnlGModPlugins.Dock = DockStyle.Fill;
            pnlGModPlugins.Location = new Point(4, 177);
            pnlGModPlugins.Margin = new Padding(4);
            pnlGModPlugins.Name = "pnlGModPlugins";
            pnlGModPlugins.Size = new Size(512, 120);
            pnlGModPlugins.TabIndex = 10;
            // 
            // lvGw1GModPlugins
            // 
            lvGw1GModPlugins.Columns.AddRange(new ColumnHeader[] { colGw1GModPlugin });
            lvGw1GModPlugins.Dock = DockStyle.Left;
            lvGw1GModPlugins.HeaderStyle = ColumnHeaderStyle.None;
            lvGw1GModPlugins.Location = new Point(0, 0);
            lvGw1GModPlugins.Name = "lvGw1GModPlugins";
            lvGw1GModPlugins.Size = new Size(350, 150);
            lvGw1GModPlugins.TabIndex = 0;
            lvGw1GModPlugins.UseCompatibleStateImageBehavior = false;
            lvGw1GModPlugins.View = View.Details;
            // 
            // colGw1GModPlugin
            // 
            colGw1GModPlugin.Width = 330;
            // 
            // btnGw1AddPlugin
            // 
            btnGw1AddPlugin.Location = new Point(360, 0);
            btnGw1AddPlugin.Name = "btnGw1AddPlugin";
            btnGw1AddPlugin.Size = new Size(75, 23);
            btnGw1AddPlugin.TabIndex = 1;
            btnGw1AddPlugin.Text = "Add...";
            // 
            // btnGw1RemovePlugin
            // 
            btnGw1RemovePlugin.Location = new Point(360, 30);
            btnGw1RemovePlugin.Name = "btnGw1RemovePlugin";
            btnGw1RemovePlugin.Size = new Size(75, 23);
            btnGw1RemovePlugin.TabIndex = 2;
            btnGw1RemovePlugin.Text = "Remove";
            // 
            // tlpGw2
            // 
            tlpGw2.AutoSize = true;
            tlpGw2.ColumnCount = 2;
            tlpGw2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            tlpGw2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpGw2.Controls.Add(chkGw2RunAfterEnabled, 0, 0);
            tlpGw2.Controls.Add(lblRunAfterPrograms, 0, 1);
            tlpGw2.Controls.Add(pnlGw2RunAfterList, 0, 2);
            tlpGw2.Controls.Add(lblGw2RunAfterTip, 0, 3);
            tlpGw2.Dock = DockStyle.Top;
            tlpGw2.Location = new Point(15, 15);
            tlpGw2.Name = "tlpGw2";
            tlpGw2.RowCount = 5;
            tlpGw2.RowStyles.Add(new RowStyle());
            tlpGw2.RowStyles.Add(new RowStyle());
            tlpGw2.RowStyles.Add(new RowStyle());
            tlpGw2.RowStyles.Add(new RowStyle());
            tlpGw2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tlpGw2.Size = new Size(520, 281);
            tlpGw2.TabIndex = 0;
            tlpGw2.Visible = false;
            // 
            // chkGw2RunAfterEnabled
            // 
            chkGw2RunAfterEnabled.AutoSize = true;
            tlpGw2.SetColumnSpan(chkGw2RunAfterEnabled, 2);
            chkGw2RunAfterEnabled.Location = new Point(4, 4);
            chkGw2RunAfterEnabled.Margin = new Padding(4);
            chkGw2RunAfterEnabled.Name = "chkGw2RunAfterEnabled";
            chkGw2RunAfterEnabled.Size = new Size(181, 19);
            chkGw2RunAfterEnabled.TabIndex = 0;
            chkGw2RunAfterEnabled.Text = "Enable programs after launch";
            // 
            // lblRunAfterPrograms
            // 
            lblRunAfterPrograms.AutoSize = true;
            tlpGw2.SetColumnSpan(lblRunAfterPrograms, 2);
            lblRunAfterPrograms.Location = new Point(4, 31);
            lblRunAfterPrograms.Margin = new Padding(4);
            lblRunAfterPrograms.Name = "lblRunAfterPrograms";
            lblRunAfterPrograms.Size = new Size(114, 15);
            lblRunAfterPrograms.TabIndex = 1;
            lblRunAfterPrograms.Text = "Run After Programs:";
            // 
            // pnlGw2RunAfterList
            // 
            tlpGw2.SetColumnSpan(pnlGw2RunAfterList, 2);
            pnlGw2RunAfterList.Controls.Add(lvGw2RunAfter);
            pnlGw2RunAfterList.Controls.Add(btnGw2AddProgram);
            pnlGw2RunAfterList.Controls.Add(btnGw2RemoveProgram);
            pnlGw2RunAfterList.Dock = DockStyle.Fill;
            pnlGw2RunAfterList.Location = new Point(4, 54);
            pnlGw2RunAfterList.Margin = new Padding(4);
            pnlGw2RunAfterList.Name = "pnlGw2RunAfterList";
            pnlGw2RunAfterList.Size = new Size(512, 180);
            pnlGw2RunAfterList.TabIndex = 2;
            // 
            // lvGw2RunAfter
            // 
            lvGw2RunAfter.CheckBoxes = true;
            lvGw2RunAfter.Columns.AddRange(new ColumnHeader[] { colGw2RunAfterMumble, colGw2RunAfterName });
            lvGw2RunAfter.Dock = DockStyle.Left;
            lvGw2RunAfter.FullRowSelect = true;
            lvGw2RunAfter.HeaderStyle = ColumnHeaderStyle.None;
            lvGw2RunAfter.HideSelection = false;
            lvGw2RunAfter.Location = new Point(0, 0);
            lvGw2RunAfter.Name = "lvGw2RunAfter";
            lvGw2RunAfter.OwnerDraw = true;
            lvGw2RunAfter.Size = new Size(350, 180);
            lvGw2RunAfter.TabIndex = 0;
            lvGw2RunAfter.UseCompatibleStateImageBehavior = false;
            lvGw2RunAfter.View = View.Details;
            // 
            // colGw2RunAfterMumble
            // 
            colGw2RunAfterMumble.Width = 22;
            // 
            // colGw2RunAfterName
            // 
            colGw2RunAfterName.Width = 300;
            // 
            // btnGw2AddProgram
            // 
            btnGw2AddProgram.Location = new Point(360, 0);
            btnGw2AddProgram.Name = "btnGw2AddProgram";
            btnGw2AddProgram.Size = new Size(75, 23);
            btnGw2AddProgram.TabIndex = 1;
            btnGw2AddProgram.Text = "Add...";
            // 
            // btnGw2RemoveProgram
            // 
            btnGw2RemoveProgram.Location = new Point(360, 30);
            btnGw2RemoveProgram.Name = "btnGw2RemoveProgram";
            btnGw2RemoveProgram.Size = new Size(75, 23);
            btnGw2RemoveProgram.TabIndex = 2;
            btnGw2RemoveProgram.Text = "Remove";
            // 
            // lblGw2RunAfterTip
            // 
            lblGw2RunAfterTip.AutoSize = true;
            tlpGw2.SetColumnSpan(lblGw2RunAfterTip, 2);
            lblGw2RunAfterTip.ForeColor = Color.DarkGoldenrod;
            lblGw2RunAfterTip.Location = new Point(4, 242);
            lblGw2RunAfterTip.Margin = new Padding(4);
            lblGw2RunAfterTip.Name = "lblGw2RunAfterTip";
            lblGw2RunAfterTip.Size = new Size(510, 15);
            lblGw2RunAfterTip.TabIndex = 3;
            lblGw2RunAfterTip.Text = "Tip: Right-click a program to toggle MumbleLink pairing (badge “M”) for overlays (Blish, TacO).";
            // 
            // ModsTabContent
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(tlpGw1);
            Controls.Add(tlpGw2);
            Name = "ModsTabContent";
            Padding = new Padding(15);
            Size = new Size(550, 668);
            tlpGw1.ResumeLayout(false);
            tlpGw1.PerformLayout();
            pnlToolboxPath.ResumeLayout(false);
            pnlToolboxPath.PerformLayout();
            pnlPy4GwPath.ResumeLayout(false);
            pnlPy4GwPath.PerformLayout();
            pnlGModPath.ResumeLayout(false);
            pnlGModPath.PerformLayout();
            pnlGModPlugins.ResumeLayout(false);
            tlpGw2.ResumeLayout(false);
            tlpGw2.PerformLayout();
            pnlGw2RunAfterList.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpGw1;
        private System.Windows.Forms.CheckBox chkToolbox;
        private System.Windows.Forms.Label lblToolboxPath;
        private System.Windows.Forms.Panel pnlToolboxPath;
        private System.Windows.Forms.TextBox txtToolboxDll;
        private System.Windows.Forms.Button btnBrowseToolboxDll;
        
        private System.Windows.Forms.CheckBox chkPy4Gw;
        private System.Windows.Forms.Label lblPy4GwPath;
        private System.Windows.Forms.Panel pnlPy4GwPath;
        private System.Windows.Forms.TextBox txtPy4GwDll;
        private System.Windows.Forms.Button btnBrowsePy4GwDll;

        private System.Windows.Forms.CheckBox chkGMod;
        private System.Windows.Forms.Label lblGModPath;
        private System.Windows.Forms.Panel pnlGModPath;
        private System.Windows.Forms.TextBox txtGModDll;
        private System.Windows.Forms.Button btnBrowseGModDll;

        private System.Windows.Forms.Label lblGw1GModPlugins;
        private System.Windows.Forms.Panel pnlGModPlugins;
        private System.Windows.Forms.ListView lvGw1GModPlugins;
        private System.Windows.Forms.ColumnHeader colGw1GModPlugin;
        private System.Windows.Forms.Button btnGw1AddPlugin;
        private System.Windows.Forms.Button btnGw1RemovePlugin;

        private System.Windows.Forms.TableLayoutPanel tlpGw2;
        private System.Windows.Forms.CheckBox chkGw2RunAfterEnabled;
        private System.Windows.Forms.Label lblRunAfterPrograms;
        private System.Windows.Forms.Panel pnlGw2RunAfterList;
        private System.Windows.Forms.ListView lvGw2RunAfter;
        private System.Windows.Forms.ColumnHeader colGw2RunAfterMumble;
        private System.Windows.Forms.ColumnHeader colGw2RunAfterName;
        private System.Windows.Forms.Button btnGw2AddProgram;
        private System.Windows.Forms.Button btnGw2RemoveProgram;
        private System.Windows.Forms.Label lblGw2RunAfterTip;
    }
}
