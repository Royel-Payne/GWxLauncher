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
            this.tlpGw1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkToolbox = new System.Windows.Forms.CheckBox();
            this.lblToolboxPath = new System.Windows.Forms.Label();
            this.pnlToolboxPath = new System.Windows.Forms.Panel();
            this.txtToolboxDll = new System.Windows.Forms.TextBox();
            this.btnBrowseToolboxDll = new System.Windows.Forms.Button();
            
            this.chkPy4Gw = new System.Windows.Forms.CheckBox();
            this.lblPy4GwPath = new System.Windows.Forms.Label();
            this.pnlPy4GwPath = new System.Windows.Forms.Panel();
            this.txtPy4GwDll = new System.Windows.Forms.TextBox();
            this.btnBrowsePy4GwDll = new System.Windows.Forms.Button();

            this.chkGMod = new System.Windows.Forms.CheckBox();
            this.lblGModPath = new System.Windows.Forms.Label();
            this.pnlGModPath = new System.Windows.Forms.Panel();
            this.txtGModDll = new System.Windows.Forms.TextBox();
            this.btnBrowseGModDll = new System.Windows.Forms.Button();

            this.lblGw1GModPlugins = new System.Windows.Forms.Label();
            this.pnlGModPlugins = new System.Windows.Forms.Panel();
            this.lvGw1GModPlugins = new System.Windows.Forms.ListView();
            this.colGw1GModPlugin = new System.Windows.Forms.ColumnHeader();
            this.btnGw1AddPlugin = new System.Windows.Forms.Button();
            this.btnGw1RemovePlugin = new System.Windows.Forms.Button();

            this.tlpGw2 = new System.Windows.Forms.TableLayoutPanel();
            this.chkGw2RunAfterEnabled = new System.Windows.Forms.CheckBox();
            this.lblRunAfterPrograms = new System.Windows.Forms.Label();
            this.pnlGw2RunAfterList = new System.Windows.Forms.Panel();
            this.lvGw2RunAfter = new System.Windows.Forms.ListView();
            this.colGw2RunAfterMumble = new System.Windows.Forms.ColumnHeader();
            this.colGw2RunAfterName = new System.Windows.Forms.ColumnHeader();
            this.btnGw2AddProgram = new System.Windows.Forms.Button();
            this.btnGw2RemoveProgram = new System.Windows.Forms.Button();
            this.lblGw2RunAfterTip = new System.Windows.Forms.Label();

            this.tlpGw1.SuspendLayout();
            this.pnlToolboxPath.SuspendLayout();
            this.pnlPy4GwPath.SuspendLayout();
            this.pnlGModPath.SuspendLayout();
            this.pnlGModPlugins.SuspendLayout();
            this.tlpGw2.SuspendLayout();
            this.pnlGw2RunAfterList.SuspendLayout();
            this.SuspendLayout();

            Padding controlMargin = new Padding(4);

            // 
            // tlpGw1
            // 
            this.tlpGw1.AutoSize = true;
            this.tlpGw1.ColumnCount = 2;
            this.tlpGw1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpGw1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            
            // Row 0: Toolbox Check
            this.tlpGw1.Controls.Add(this.chkToolbox, 0, 0);
            this.tlpGw1.SetColumnSpan(this.chkToolbox, 2);
            // Row 1: Toolbox Path
            this.tlpGw1.Controls.Add(this.lblToolboxPath, 0, 1);
            this.tlpGw1.Controls.Add(this.pnlToolboxPath, 1, 1);
            
            // Row 2: Py4Gw Check
            this.tlpGw1.Controls.Add(this.chkPy4Gw, 0, 2);
            this.tlpGw1.SetColumnSpan(this.chkPy4Gw, 2);
            // Row 3: Py4Gw Path
            this.tlpGw1.Controls.Add(this.lblPy4GwPath, 0, 3);
            this.tlpGw1.Controls.Add(this.pnlPy4GwPath, 1, 3);

            // Row 4: GMod Check
            this.tlpGw1.Controls.Add(this.chkGMod, 0, 4);
            this.tlpGw1.SetColumnSpan(this.chkGMod, 2);
            // Row 5: GMod Path
            this.tlpGw1.Controls.Add(this.lblGModPath, 0, 5);
            this.tlpGw1.Controls.Add(this.pnlGModPath, 1, 5);
            
            // Row 6: Plugins Header
            this.tlpGw1.Controls.Add(this.lblGw1GModPlugins, 0, 6);
            this.tlpGw1.SetColumnSpan(this.lblGw1GModPlugins, 2);
            // Row 7: Plugins List
            this.tlpGw1.Controls.Add(this.pnlGModPlugins, 0, 7);
            this.tlpGw1.SetColumnSpan(this.pnlGModPlugins, 2);

            this.tlpGw1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpGw1.Location = new System.Drawing.Point(15, 15);
            this.tlpGw1.Name = "tlpGw1";
            this.tlpGw1.RowCount = 8;
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw1.TabIndex = 0;
            this.tlpGw1.Visible = false;

            // --- Toolbox ---
            this.chkToolbox.AutoSize = true;
            this.chkToolbox.Text = "Enable Toolbox";
            this.chkToolbox.Margin = controlMargin;
            this.chkToolbox.Name = "chkToolbox";

            this.lblToolboxPath.AutoSize = true;
            this.lblToolboxPath.Text = "DLL Path:";
            this.lblToolboxPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblToolboxPath.Dock = DockStyle.Fill;
            this.lblToolboxPath.Margin = controlMargin;

            this.pnlToolboxPath.AutoSize = true;
            this.pnlToolboxPath.Dock = DockStyle.Fill;
            this.pnlToolboxPath.Margin = new Padding(0);
            this.pnlToolboxPath.Controls.Add(this.txtToolboxDll);
            this.pnlToolboxPath.Controls.Add(this.btnBrowseToolboxDll);

            this.txtToolboxDll.Dock = DockStyle.Fill;
            this.txtToolboxDll.Margin = controlMargin;
            this.txtToolboxDll.Name = "txtToolboxDll";

            this.btnBrowseToolboxDll.Dock = DockStyle.Right;
            this.btnBrowseToolboxDll.Width = 40;
            this.btnBrowseToolboxDll.Text = "...";
            this.btnBrowseToolboxDll.Margin = controlMargin;
            this.btnBrowseToolboxDll.Name = "btnBrowseToolboxDll";

            // --- Py4Gw ---
            this.chkPy4Gw.AutoSize = true;
            this.chkPy4Gw.Text = "Enable Py4Gw";
            this.chkPy4Gw.Margin = controlMargin;
            this.chkPy4Gw.Name = "chkPy4Gw";

            this.lblPy4GwPath.AutoSize = true;
            this.lblPy4GwPath.Text = "DLL Path:";
            this.lblPy4GwPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPy4GwPath.Dock = DockStyle.Fill;
            this.lblPy4GwPath.Margin = controlMargin;

            this.pnlPy4GwPath.AutoSize = true;
            this.pnlPy4GwPath.Dock = DockStyle.Fill;
            this.pnlPy4GwPath.Margin = new Padding(0);
            this.pnlPy4GwPath.Controls.Add(this.txtPy4GwDll);
            this.pnlPy4GwPath.Controls.Add(this.btnBrowsePy4GwDll);

            this.txtPy4GwDll.Dock = DockStyle.Fill;
            this.txtPy4GwDll.Margin = controlMargin;
            this.txtPy4GwDll.Name = "txtPy4GwDll";

            this.btnBrowsePy4GwDll.Dock = DockStyle.Right;
            this.btnBrowsePy4GwDll.Width = 40;
            this.btnBrowsePy4GwDll.Text = "...";
            this.btnBrowsePy4GwDll.Margin = controlMargin;
            this.btnBrowsePy4GwDll.Name = "btnBrowsePy4GwDll";

            // --- GMod ---
            this.chkGMod.AutoSize = true;
            this.chkGMod.Text = "Enable GMod";
            this.chkGMod.Margin = controlMargin;
            this.chkGMod.Name = "chkGMod";

            this.lblGModPath.AutoSize = true;
            this.lblGModPath.Text = "DLL Path:";
            this.lblGModPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblGModPath.Dock = DockStyle.Fill;
            this.lblGModPath.Margin = controlMargin;

            this.pnlGModPath.AutoSize = true;
            this.pnlGModPath.Dock = DockStyle.Fill;
            this.pnlGModPath.Margin = new Padding(0);
            this.pnlGModPath.Controls.Add(this.txtGModDll);
            this.pnlGModPath.Controls.Add(this.btnBrowseGModDll);

            this.txtGModDll.Dock = DockStyle.Fill;
            this.txtGModDll.Margin = controlMargin;
            this.txtGModDll.Name = "txtGModDll";

            this.btnBrowseGModDll.Dock = DockStyle.Right;
            this.btnBrowseGModDll.Width = 40;
            this.btnBrowseGModDll.Text = "...";
            this.btnBrowseGModDll.Margin = controlMargin;
            this.btnBrowseGModDll.Name = "btnBrowseGModDll";

            // --- Plugins ---
            this.lblGw1GModPlugins.AutoSize = true;
            this.lblGw1GModPlugins.Text = "GMod Plugins:";
            this.lblGw1GModPlugins.Margin = controlMargin;
            this.lblGw1GModPlugins.Name = "lblGw1GModPlugins";

            this.pnlGModPlugins.Dock = DockStyle.Fill;
            this.pnlGModPlugins.Height = 150;
            this.pnlGModPlugins.Margin = controlMargin;
            this.pnlGModPlugins.Controls.Add(this.lvGw1GModPlugins);
            this.pnlGModPlugins.Controls.Add(this.btnGw1AddPlugin);
            this.pnlGModPlugins.Controls.Add(this.btnGw1RemovePlugin);

            this.lvGw1GModPlugins.Dock = DockStyle.Left;
            this.lvGw1GModPlugins.Width = 350;
            this.lvGw1GModPlugins.View = View.Details;
            this.lvGw1GModPlugins.HeaderStyle = ColumnHeaderStyle.None;
            this.lvGw1GModPlugins.Columns.Add(this.colGw1GModPlugin);
            this.lvGw1GModPlugins.Name = "lvGw1GModPlugins";
            this.colGw1GModPlugin.Width = 330;

            this.btnGw1AddPlugin.Location = new Point(360, 0);
            this.btnGw1AddPlugin.Text = "Add...";
            this.btnGw1AddPlugin.Name = "btnGw1AddPlugin";

            this.btnGw1RemovePlugin.Location = new Point(360, 30);
            this.btnGw1RemovePlugin.Text = "Remove";
            this.btnGw1RemovePlugin.Name = "btnGw1RemovePlugin";


            // 
            // tlpGw2
            // 
            this.tlpGw2.AutoSize = true;
            this.tlpGw2.ColumnCount = 2;
            this.tlpGw2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlpGw2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));

            this.tlpGw2.Controls.Add(this.chkGw2RunAfterEnabled, 0, 0);
            this.tlpGw2.SetColumnSpan(this.chkGw2RunAfterEnabled, 2);

            this.tlpGw2.Controls.Add(this.lblRunAfterPrograms, 0, 1);
            this.tlpGw2.SetColumnSpan(this.lblRunAfterPrograms, 2);

            this.tlpGw2.Controls.Add(this.pnlGw2RunAfterList, 0, 2);
            this.tlpGw2.SetColumnSpan(this.pnlGw2RunAfterList, 2);
            
            this.tlpGw2.Controls.Add(this.lblGw2RunAfterTip, 0, 3);
            this.tlpGw2.SetColumnSpan(this.lblGw2RunAfterTip, 2);

            this.tlpGw2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpGw2.Location = new System.Drawing.Point(15, 15);
            this.tlpGw2.Name = "tlpGw2";
            this.tlpGw2.RowCount = 4;
            this.tlpGw2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tlpGw2.Visible = false;

            this.chkGw2RunAfterEnabled.AutoSize = true;
            this.chkGw2RunAfterEnabled.Text = "Enable programs after launch";
            this.chkGw2RunAfterEnabled.Margin = controlMargin;
            this.chkGw2RunAfterEnabled.Name = "chkGw2RunAfterEnabled";

            this.lblRunAfterPrograms.AutoSize = true;
            this.lblRunAfterPrograms.Text = "Run After Programs:";
            this.lblRunAfterPrograms.Margin = controlMargin;

            this.pnlGw2RunAfterList.Dock = DockStyle.Fill;
            this.pnlGw2RunAfterList.Height = 180;
            this.pnlGw2RunAfterList.Margin = controlMargin;
            this.pnlGw2RunAfterList.Controls.Add(this.lvGw2RunAfter);
            this.pnlGw2RunAfterList.Controls.Add(this.btnGw2AddProgram);
            this.pnlGw2RunAfterList.Controls.Add(this.btnGw2RemoveProgram);

            this.lvGw2RunAfter.Dock = DockStyle.Left;
            this.lvGw2RunAfter.Width = 350;
            this.lvGw2RunAfter.View = View.Details;
            this.lvGw2RunAfter.CheckBoxes = true;
            this.lvGw2RunAfter.OwnerDraw = true;
            this.lvGw2RunAfter.HeaderStyle = ColumnHeaderStyle.None;
            this.lvGw2RunAfter.Columns.AddRange(new ColumnHeader[] { this.colGw2RunAfterMumble, this.colGw2RunAfterName });
            this.lvGw2RunAfter.Name = "lvGw2RunAfter";
            
            this.colGw2RunAfterMumble.Width = 22;
            this.colGw2RunAfterName.Width = 300;

            this.btnGw2AddProgram.Location = new Point(360, 0);
            this.btnGw2AddProgram.Text = "Add...";
            this.btnGw2AddProgram.Name = "btnGw2AddProgram";

            this.btnGw2RemoveProgram.Location = new Point(360, 30);
            this.btnGw2RemoveProgram.Text = "Remove";
            this.btnGw2RemoveProgram.Name = "btnGw2RemoveProgram";

            this.lblGw2RunAfterTip.AutoSize = true;
            this.lblGw2RunAfterTip.Text = "Tip: Right-click a program to toggle MumbleLink pairing (badge “M”) for overlays (Blish, TacO).";
            this.lblGw2RunAfterTip.Margin = controlMargin;
            this.lblGw2RunAfterTip.ForeColor = Color.DarkGoldenrod;
            this.lblGw2RunAfterTip.Name = "lblGw2RunAfterTip";


            // 
            // ModsTabContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tlpGw2);
            this.Controls.Add(this.tlpGw1);
            this.Name = "ModsTabContent";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.Size = new System.Drawing.Size(550, 450);
            
            this.tlpGw1.ResumeLayout(false);
            this.tlpGw1.PerformLayout();
            this.pnlToolboxPath.ResumeLayout(false);
            this.pnlToolboxPath.PerformLayout();
            this.pnlPy4GwPath.ResumeLayout(false);
            this.pnlPy4GwPath.PerformLayout();
            this.pnlGModPath.ResumeLayout(false);
            this.pnlGModPath.PerformLayout();
            this.pnlGModPlugins.ResumeLayout(false);
            this.tlpGw2.ResumeLayout(false);
            this.tlpGw2.PerformLayout();
            this.pnlGw2RunAfterList.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
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
