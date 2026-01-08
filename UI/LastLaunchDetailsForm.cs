using System.Text;
using GWxLauncher.Services;

namespace GWxLauncher.UI
{
    internal sealed class LastLaunchDetailsForm : Form
    {
        public LastLaunchDetailsForm(IReadOnlyList<LaunchReport> reports)
        {
            Text = reports.Count <= 1 ? "Last Launch Details" : $"Last Launch Details ({reports.Count} attempts)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(800, 550);
            MinimumSize = new Size(650, 420);

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
                Text = BuildCombinedText(reports)
            };

            Controls.Add(txt);
        }

        private static string BuildCombinedText(IReadOnlyList<LaunchReport> reports)
        {
            if (reports.Count == 0)
                return "(No launch data.)";

            if (reports.Count == 1)
                return reports[0].ToString();

            var sb = new StringBuilder();
            for (int i = 0; i < reports.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 90));
                    sb.AppendLine();
                }

                sb.AppendLine($"Attempt {i + 1} of {reports.Count}");
                sb.AppendLine(reports[i].ToString());
            }

            return sb.ToString();
        }
    }
}
