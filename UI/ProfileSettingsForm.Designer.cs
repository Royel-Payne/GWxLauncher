using static System.Net.Mime.MediaTypeNames;

namespace GWxLauncher.UI
{
    partial class ProfileSettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpGw1Mods = new GroupBox();
            btnBrowseGModDll = new Button();
            txtGModDll = new TextBox();
            chkGMod = new CheckBox();
            btnBrowsePy4GwDll = new Button();
            txtPy4GwDll = new TextBox();
            chkPy4Gw = new CheckBox();
            btnBrowseToolboxDll = new Button();
            txtToolboxDll = new TextBox();
            chkToolbox = new CheckBox();
            lblGw1GModPlugins = new Label();
            lvGw1GModPlugins = new ListView();
            colGw1GModPlugin = new ColumnHeader();
            btnGw1AddPlugin = new Button();
            btnGw1RemovePlugin = new Button();
            grpGw2RunAfter = new GroupBox();
            label3 = new Label();
            chkGw2RunAfterEnabled = new CheckBox();
            lvGw2RunAfter = new ListView();
            colGw2RunAfterMumble = new ColumnHeader();
            colGw2RunAfterName = new ColumnHeader();
            btnGw2AddProgram = new Button();
            btnGw2RemoveProgram = new Button();
            colGw2RunAfterPath = new ColumnHeader();
            btnOk = new Button();
            btnCancel = new Button();
            grpGw1Mods.SuspendLayout();
            grpGw2RunAfter.SuspendLayout();
            SuspendLayout();
            // 
            // grpGw1Mods
            // 
            grpGw1Mods.Controls.Add(btnBrowseGModDll);
            grpGw1Mods.Controls.Add(txtGModDll);
            grpGw1Mods.Controls.Add(chkGMod);
            grpGw1Mods.Controls.Add(btnBrowsePy4GwDll);
            grpGw1Mods.Controls.Add(txtPy4GwDll);
            grpGw1Mods.Controls.Add(chkPy4Gw);
            grpGw1Mods.Controls.Add(btnBrowseToolboxDll);
            grpGw1Mods.Controls.Add(txtToolboxDll);
            grpGw1Mods.Controls.Add(chkToolbox);
            grpGw1Mods.Controls.Add(lblGw1GModPlugins);
            grpGw1Mods.Controls.Add(lvGw1GModPlugins);
            grpGw1Mods.Controls.Add(btnGw1AddPlugin);
            grpGw1Mods.Controls.Add(btnGw1RemovePlugin);
            grpGw1Mods.Location = new Point(309, 105);
            grpGw1Mods.Name = "grpGw1Mods";
            grpGw1Mods.Size = new Size(400, 257);
            grpGw1Mods.TabIndex = 5;
            grpGw1Mods.TabStop = false;
            grpGw1Mods.Text = "GW1 Mods";
            // 
            // btnBrowseGModDll
            // 
            btnBrowseGModDll.Location = new Point(306, 85);
            btnBrowseGModDll.Name = "btnBrowseGModDll";
            btnBrowseGModDll.Size = new Size(75, 23);
            btnBrowseGModDll.TabIndex = 8;
            btnBrowseGModDll.Text = "Browse...";
            btnBrowseGModDll.UseVisualStyleBackColor = true;
            // 
            // txtGModDll
            // 
            txtGModDll.Location = new Point(87, 85);
            txtGModDll.Name = "txtGModDll";
            txtGModDll.Size = new Size(209, 23);
            txtGModDll.TabIndex = 7;
            // 
            // chkGMod
            // 
            chkGMod.AutoSize = true;
            chkGMod.Location = new Point(19, 87);
            chkGMod.Name = "chkGMod";
            chkGMod.Size = new Size(58, 19);
            chkGMod.TabIndex = 6;
            chkGMod.Text = "gMod";
            chkGMod.UseVisualStyleBackColor = true;
            // 
            // btnBrowsePy4GwDll
            // 
            btnBrowsePy4GwDll.Location = new Point(306, 53);
            btnBrowsePy4GwDll.Name = "btnBrowsePy4GwDll";
            btnBrowsePy4GwDll.Size = new Size(75, 23);
            btnBrowsePy4GwDll.TabIndex = 5;
            btnBrowsePy4GwDll.Text = "Browse...";
            btnBrowsePy4GwDll.UseVisualStyleBackColor = true;
            // 
            // txtPy4GwDll
            // 
            txtPy4GwDll.Location = new Point(87, 53);
            txtPy4GwDll.Name = "txtPy4GwDll";
            txtPy4GwDll.Size = new Size(209, 23);
            txtPy4GwDll.TabIndex = 4;
            // 
            // chkPy4Gw
            // 
            chkPy4Gw.AutoSize = true;
            chkPy4Gw.Location = new Point(19, 55);
            chkPy4Gw.Name = "chkPy4Gw";
            chkPy4Gw.Size = new Size(64, 19);
            chkPy4Gw.TabIndex = 3;
            chkPy4Gw.Text = "Py4GW";
            chkPy4Gw.UseVisualStyleBackColor = true;
            // 
            // btnBrowseToolboxDll
            // 
            btnBrowseToolboxDll.Location = new Point(306, 21);
            btnBrowseToolboxDll.Name = "btnBrowseToolboxDll";
            btnBrowseToolboxDll.Size = new Size(75, 23);
            btnBrowseToolboxDll.TabIndex = 2;
            btnBrowseToolboxDll.Text = "Browse...";
            btnBrowseToolboxDll.UseVisualStyleBackColor = true;
            // 
            // txtToolboxDll
            // 
            txtToolboxDll.Location = new Point(87, 21);
            txtToolboxDll.Name = "txtToolboxDll";
            txtToolboxDll.Size = new Size(209, 23);
            txtToolboxDll.TabIndex = 1;
            // 
            // chkToolbox
            // 
            chkToolbox.AutoSize = true;
            chkToolbox.Location = new Point(19, 23);
            chkToolbox.Name = "chkToolbox";
            chkToolbox.Size = new Size(68, 19);
            chkToolbox.TabIndex = 0;
            chkToolbox.Text = "Toolbox";
            chkToolbox.UseVisualStyleBackColor = true;
            // 
            // lblGw1GModPlugins
            // 
            lblGw1GModPlugins.AutoSize = true;
            lblGw1GModPlugins.Location = new Point(19, 120);
            lblGw1GModPlugins.Name = "lblGw1GModPlugins";
            lblGw1GModPlugins.Size = new Size(81, 15);
            lblGw1GModPlugins.TabIndex = 9;
            lblGw1GModPlugins.Text = "gMod plugins";
            // 
            // lvGw1GModPlugins
            // 
            lvGw1GModPlugins.Columns.AddRange(new ColumnHeader[] { colGw1GModPlugin });
            lvGw1GModPlugins.FullRowSelect = true;
            lvGw1GModPlugins.HeaderStyle = ColumnHeaderStyle.None;
            lvGw1GModPlugins.Location = new Point(19, 140);
            lvGw1GModPlugins.MultiSelect = false;
            lvGw1GModPlugins.Name = "lvGw1GModPlugins";
            lvGw1GModPlugins.Size = new Size(277, 101);
            lvGw1GModPlugins.TabIndex = 9;
            lvGw1GModPlugins.UseCompatibleStateImageBehavior = false;
            lvGw1GModPlugins.View = View.Details;
            // 
            // colGw1GModPlugin
            // 
            colGw1GModPlugin.Text = "Plugin";
            colGw1GModPlugin.Width = 260;
            // 
            // btnGw1AddPlugin
            // 
            btnGw1AddPlugin.Location = new Point(306, 140);
            btnGw1AddPlugin.Name = "btnGw1AddPlugin";
            btnGw1AddPlugin.Size = new Size(75, 23);
            btnGw1AddPlugin.TabIndex = 10;
            btnGw1AddPlugin.Text = "Add...";
            btnGw1AddPlugin.UseVisualStyleBackColor = true;
            // 
            // btnGw1RemovePlugin
            // 
            btnGw1RemovePlugin.Enabled = false;
            btnGw1RemovePlugin.Location = new Point(306, 169);
            btnGw1RemovePlugin.Name = "btnGw1RemovePlugin";
            btnGw1RemovePlugin.Size = new Size(75, 23);
            btnGw1RemovePlugin.TabIndex = 11;
            btnGw1RemovePlugin.Text = "Remove";
            btnGw1RemovePlugin.UseVisualStyleBackColor = true;
            // 
            // grpGw2RunAfter
            // 
            grpGw2RunAfter.Controls.Add(label3);
            grpGw2RunAfter.Controls.Add(chkGw2RunAfterEnabled);
            grpGw2RunAfter.Controls.Add(lvGw2RunAfter);
            grpGw2RunAfter.Controls.Add(btnGw2AddProgram);
            grpGw2RunAfter.Controls.Add(btnGw2RemoveProgram);
            grpGw2RunAfter.Location = new Point(309, 105);
            grpGw2RunAfter.Name = "grpGw2RunAfter";
            grpGw2RunAfter.Size = new Size(400, 257);
            grpGw2RunAfter.TabIndex = 5;
            grpGw2RunAfter.TabStop = false;
            grpGw2RunAfter.Text = "Run after launching";
            grpGw2RunAfter.Visible = false;
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Segoe UI", 9F);
            label3.ForeColor = Color.DarkGoldenrod;
            label3.Location = new Point(16, 218);
            label3.Name = "label3";
            label3.Size = new Size(307, 36);
            label3.TabIndex = 9;
            label3.Text = "Tip: Right-click a program to toggle MumbleLink pairing (badge “M”) for overlays (Blish, TacO).\r\n";
            // 
            // chkGw2RunAfterEnabled
            // 
            chkGw2RunAfterEnabled.AutoSize = true;
            chkGw2RunAfterEnabled.Location = new Point(16, 24);
            chkGw2RunAfterEnabled.Name = "chkGw2RunAfterEnabled";
            chkGw2RunAfterEnabled.Size = new Size(181, 19);
            chkGw2RunAfterEnabled.TabIndex = 0;
            chkGw2RunAfterEnabled.Text = "Enable programs after launch";
            chkGw2RunAfterEnabled.UseVisualStyleBackColor = true;
            // 
            // lvGw2RunAfter
            // 
            lvGw2RunAfter.CheckBoxes = true;
            lvGw2RunAfter.Columns.AddRange(new ColumnHeader[] { colGw2RunAfterMumble, colGw2RunAfterName });
            lvGw2RunAfter.FullRowSelect = true;
            lvGw2RunAfter.HeaderStyle = ColumnHeaderStyle.None;
            lvGw2RunAfter.Location = new Point(16, 52);
            lvGw2RunAfter.MultiSelect = false;
            lvGw2RunAfter.Name = "lvGw2RunAfter";
            lvGw2RunAfter.OwnerDraw = true;
            lvGw2RunAfter.ShowItemToolTips = true;
            lvGw2RunAfter.Size = new Size(280, 143);
            lvGw2RunAfter.TabIndex = 1;
            lvGw2RunAfter.UseCompatibleStateImageBehavior = false;
            lvGw2RunAfter.View = View.Details;
            lvGw2RunAfter.DrawColumnHeader += lvGw2RunAfter_DrawColumnHeader;
            lvGw2RunAfter.DrawSubItem += lvGw2RunAfter_DrawSubItem;
            lvGw2RunAfter.ItemChecked += lvGw2RunAfter_ItemChecked;
            lvGw2RunAfter.MouseUp += lvGw2RunAfter_MouseUp;
            // 
            // colGw2RunAfterMumble
            // 
            colGw2RunAfterMumble.Text = "";
            colGw2RunAfterMumble.TextAlign = HorizontalAlignment.Center;
            colGw2RunAfterMumble.Width = 22;
            // 
            // colGw2RunAfterName
            // 
            colGw2RunAfterName.Text = "Name";
            colGw2RunAfterName.Width = 248;
            // 
            // btnGw2AddProgram
            // 
            btnGw2AddProgram.Location = new Point(306, 52);
            btnGw2AddProgram.Name = "btnGw2AddProgram";
            btnGw2AddProgram.Size = new Size(75, 23);
            btnGw2AddProgram.TabIndex = 2;
            btnGw2AddProgram.Text = "Add...";
            btnGw2AddProgram.UseVisualStyleBackColor = true;
            btnGw2AddProgram.Click += btnGw2AddProgram_Click;
            // 
            // btnGw2RemoveProgram
            // 
            btnGw2RemoveProgram.Enabled = false;
            btnGw2RemoveProgram.Location = new Point(306, 81);
            btnGw2RemoveProgram.Name = "btnGw2RemoveProgram";
            btnGw2RemoveProgram.Size = new Size(75, 23);
            btnGw2RemoveProgram.TabIndex = 3;
            btnGw2RemoveProgram.Text = "Remove";
            btnGw2RemoveProgram.UseVisualStyleBackColor = true;
            btnGw2RemoveProgram.Click += btnGw2RemoveProgram_Click;
            // 
            // colGw2RunAfterPath
            // 
            colGw2RunAfterPath.Text = "Path";
            colGw2RunAfterPath.Width = 180;
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(530, 371);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 6;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(615, 371);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // ProfileSettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(721, 406);
            Controls.Add(grpGw2RunAfter);
            Controls.Add(grpGw1Mods);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Name = "ProfileSettingsForm";
            Text = "ProfileSettingsForm";
            grpGw1Mods.ResumeLayout(false);
            grpGw1Mods.PerformLayout();
            grpGw2RunAfter.ResumeLayout(false);
            grpGw2RunAfter.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpGw1Mods;
        private TextBox txtToolboxDll;
        private CheckBox chkToolbox;
        private Button btnBrowseGModDll;
        private TextBox txtGModDll;
        private CheckBox chkGMod;
        private Button btnBrowsePy4GwDll;
        private TextBox txtPy4GwDll;
        private CheckBox chkPy4Gw;
        private Button btnBrowseToolboxDll;
        private Button btnOk;
        private Button btnCancel;
        // GW2 "Run after launching"
        private GroupBox grpGw2RunAfter;
        private CheckBox chkGw2RunAfterEnabled;
        private ListView lvGw2RunAfter;
        private Button btnGw2AddProgram;
        private Button btnGw2RemoveProgram;
        private ColumnHeader colGw2RunAfterMumble;
        private ColumnHeader colGw2RunAfterName;
        private ColumnHeader colGw2RunAfterPath;
        // GW1 "gMod plugins UI"
        private ListView lvGw1GModPlugins;
        private ColumnHeader colGw1GModPlugin;
        private Button btnGw1AddPlugin;
        private Button btnGw1RemovePlugin;
        private Label lblGw1GModPlugins;
        private Label label3;
    }
}
