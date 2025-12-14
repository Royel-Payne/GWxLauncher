using System;
using System.Drawing;
using System.Windows.Forms;
using GWxLauncher.Services;

namespace GWxLauncher.UI
{
    internal sealed class LastLaunchDetailsForm : Form
    {
        public LastLaunchDetailsForm(LaunchReport report)
        {
            Text = "Last Launch Details";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(700, 500);
            MinimumSize = new Size(600, 400);

            ThemeService.ApplyToForm(this);

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10f),
                BackColor = ThemeService.Palette.InputBack,
                ForeColor = ThemeService.Palette.InputFore,
                Text = report.ToString()
            };

            Controls.Add(txt);
        }
    }
}
