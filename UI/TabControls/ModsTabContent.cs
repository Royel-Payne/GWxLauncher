using GWxLauncher.Domain;

namespace GWxLauncher.UI.TabControls
{
    /// <summary>
    /// UserControl for the Mods/Plugins tab in ProfileSettingsForm.
    /// Adapts content based on GameType (GW1 mods vs GW2 RunAfter programs).
    /// </summary>
    public partial class ModsTabContent : UserControl
    {
        public ModsTabContent()
        {
            InitializeComponent();
            ApplyTheme();
        }

        public void ArrangeControls(Control[] gw1Controls, Control[] gw2Controls)
        {
            pnlGw1Mods.Controls.Clear();
            pnlGw2RunAfter.Controls.Clear();

            if (gw1Controls != null && gw1Controls.Length > 0)
                LayoutGw1Controls(gw1Controls);

            if (gw2Controls != null && gw2Controls.Length > 0)
                LayoutGw2Controls(gw2Controls);
        }

        private void LayoutGw1Controls(Control[] controls)
        {
            var chkToolbox = Array.Find(controls, c => c.Name == "chkToolbox");
            var txtToolboxDll = Array.Find(controls, c => c.Name == "txtToolboxDll");
            var btnBrowseToolboxDll = Array.Find(controls, c => c.Name == "btnBrowseToolboxDll");
            
            var chkPy4Gw = Array.Find(controls, c => c.Name == "chkPy4Gw");
            var txtPy4GwDll = Array.Find(controls, c => c.Name == "txtPy4GwDll");
            var btnBrowsePy4GwDll = Array.Find(controls, c => c.Name == "btnBrowsePy4GwDll");
            
            var chkGMod = Array.Find(controls, c => c.Name == "chkGMod");
            var txtGModDll = Array.Find(controls, c => c.Name == "txtGModDll");
            var btnBrowseGModDll = Array.Find(controls, c => c.Name == "btnBrowseGModDll");
            var lblGw1GModPlugins = Array.Find(controls, c => c.Name == "lblGw1GModPlugins");
            var lvGw1GModPlugins = Array.Find(controls, c => c.Name == "lvGw1GModPlugins");
            var btnGw1AddPlugin = Array.Find(controls, c => c.Name == "btnGw1AddPlugin");
            var btnGw1RemovePlugin = Array.Find(controls, c => c.Name == "btnGw1RemovePlugin");

            int y = 0;
            const int leftMargin = 10;  // Match General tab margin
            const int textboxLeft = 40;  // Indent textboxes under checkboxes
            const int textboxWidth = 320;
            const int buttonLeft = textboxLeft + textboxWidth + 10;  // Browse button position
            const int rowHeight = 35;

            // Toolbox
            if (chkToolbox != null)
            {
                chkToolbox.Location = new Point(leftMargin, y);
                chkToolbox.AutoSize = true;
                pnlGw1Mods.Controls.Add(chkToolbox);
                y += rowHeight;
            }

            if (txtToolboxDll != null && btnBrowseToolboxDll != null)
            {
                txtToolboxDll.Location = new Point(textboxLeft, y);
                txtToolboxDll.Width = textboxWidth;
                pnlGw1Mods.Controls.Add(txtToolboxDll);

                btnBrowseToolboxDll.Location = new Point(buttonLeft, y - 1);
                pnlGw1Mods.Controls.Add(btnBrowseToolboxDll);
                y += rowHeight + 5;
            }

            // Py4GW
            if (chkPy4Gw != null)
            {
                chkPy4Gw.Location = new Point(leftMargin, y);
                chkPy4Gw.AutoSize = true;
                pnlGw1Mods.Controls.Add(chkPy4Gw);
                y += rowHeight;
            }

            if (txtPy4GwDll != null && btnBrowsePy4GwDll != null)
            {
                txtPy4GwDll.Location = new Point(textboxLeft, y);
                txtPy4GwDll.Width = textboxWidth;
                pnlGw1Mods.Controls.Add(txtPy4GwDll);

                btnBrowsePy4GwDll.Location = new Point(buttonLeft, y - 1);
                pnlGw1Mods.Controls.Add(btnBrowsePy4GwDll);
                y += rowHeight + 5;
            }

            // gMod
            if (chkGMod != null)
            {
                chkGMod.Location = new Point(leftMargin, y);
                chkGMod.AutoSize = true;
                pnlGw1Mods.Controls.Add(chkGMod);
                y += rowHeight;
            }

            if (txtGModDll != null && btnBrowseGModDll != null)
            {
                txtGModDll.Location = new Point(textboxLeft, y);
                txtGModDll.Width = textboxWidth;
                pnlGw1Mods.Controls.Add(txtGModDll);

                btnBrowseGModDll.Location = new Point(buttonLeft, y - 1);
                pnlGw1Mods.Controls.Add(btnBrowseGModDll);
                y += rowHeight + 5;
            }

            // gMod plugins
            if (lblGw1GModPlugins != null)
            {
                lblGw1GModPlugins.Location = new Point(textboxLeft, y);
                lblGw1GModPlugins.AutoSize = true;
                pnlGw1Mods.Controls.Add(lblGw1GModPlugins);
                y += 25;
            }

            if (lvGw1GModPlugins != null)
            {
                // Make ListView same width as DLL textboxes
                lvGw1GModPlugins.Location = new Point(textboxLeft, y);
                lvGw1GModPlugins.Width = textboxWidth;
                lvGw1GModPlugins.Height = 120;
                pnlGw1Mods.Controls.Add(lvGw1GModPlugins);

                // Align buttons with Browse buttons above
                if (btnGw1AddPlugin != null)
                {
                    btnGw1AddPlugin.Location = new Point(buttonLeft, y);
                    pnlGw1Mods.Controls.Add(btnGw1AddPlugin);
                }

                if (btnGw1RemovePlugin != null)
                {
                    btnGw1RemovePlugin.Location = new Point(buttonLeft, y + 35);
                    pnlGw1Mods.Controls.Add(btnGw1RemovePlugin);
                }

                y += 130;
            }

            pnlGw1Mods.Height = y;
        }

        private void LayoutGw2Controls(Control[] controls)
        {
            var chkGw2RunAfterEnabled = Array.Find(controls, c => c.Name == "chkGw2RunAfterEnabled");
            var lvGw2RunAfter = Array.Find(controls, c => c.Name == "lvGw2RunAfter");
            var btnGw2AddProgram = Array.Find(controls, c => c.Name == "btnGw2AddProgram");
            var btnGw2RemoveProgram = Array.Find(controls, c => c.Name == "btnGw2RemoveProgram");

            int y = 0;
            const int leftMargin = 10;  // Match General tab margin
            const int indentLeft = 40;  // Match GW1 mods textbox indent
            const int listWidth = 440;  // Adjusted width to match GW1 layout proportions
            const int buttonLeft = indentLeft + listWidth + 10;  // Buttons to the right

            if (chkGw2RunAfterEnabled != null)
            {
                chkGw2RunAfterEnabled.Location = new Point(leftMargin, y);
                chkGw2RunAfterEnabled.AutoSize = true;
                pnlGw2RunAfter.Controls.Add(chkGw2RunAfterEnabled);
                y += 40;
            }

            if (lvGw2RunAfter != null)
            {
                lvGw2RunAfter.Location = new Point(indentLeft, y);
                lvGw2RunAfter.Width = listWidth;
                lvGw2RunAfter.Height = 200;
                pnlGw2RunAfter.Controls.Add(lvGw2RunAfter);

                if (btnGw2AddProgram != null)
                {
                    btnGw2AddProgram.Location = new Point(buttonLeft, y);
                    pnlGw2RunAfter.Controls.Add(btnGw2AddProgram);
                }

                if (btnGw2RemoveProgram != null)
                {
                    btnGw2RemoveProgram.Location = new Point(buttonLeft, y + 35);
                    pnlGw2RunAfter.Controls.Add(btnGw2RemoveProgram);
                }

                y += 210;
            }

            pnlGw2RunAfter.Height = y;
        }

        public void UpdateForGameType(GameType gameType)
        {
            bool isGw1 = gameType == GameType.GuildWars1;
            pnlGw1Mods.Visible = isGw1;
            pnlGw2RunAfter.Visible = !isGw1;
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeService.Palette.WindowBack;
            pnlGw1Mods.BackColor = ThemeService.Palette.WindowBack;
            pnlGw2RunAfter.BackColor = ThemeService.Palette.WindowBack;
            ThemeService.ApplyToControlTree(this);
        }

        public void RefreshTheme()
        {
            ApplyTheme();
            this.Invalidate(true);
        }
    }
}
