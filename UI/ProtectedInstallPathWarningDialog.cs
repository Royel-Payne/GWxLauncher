using GWxLauncher.Services;

namespace GWxLauncher.UI
{
    internal sealed class ProtectedInstallPathWarningDialog : Form
    {
        private readonly Button _btnContinue;
        private readonly Button _btnCancel;

        private ProtectedInstallPathWarningDialog(string exePath)
        {
            Text = "⚠️ Protected install path detected";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 260);

            ThemeService.ApplyToForm(this);

            var msg = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,

                WordWrap = true,          // allow wrapping within paragraphs
                ScrollBars = ScrollBars.None,

                BackColor = ThemeService.Palette.SurfaceBack,
                ForeColor = ThemeService.Palette.WindowFore,

                Font = Font,
                Location = new Point(16, 16),
                Size = new Size(ClientSize.Width - 32, 170),
                Text = ProtectedInstallPathPolicy.BuildWarningMessage(exePath)
            };

            _btnContinue = new Button
            {
                Text = "Continue anyway",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(140, 30),
                Location = new Point(ClientSize.Width - 16 - 140 - 110 - 10, ClientSize.Height - 16 - 30)
            };

            _btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Size = new Size(110, 30),
                Location = new Point(ClientSize.Width - 16 - 110, ClientSize.Height - 16 - 30)
            };

            Controls.Add(msg);
            Controls.Add(_btnContinue);
            Controls.Add(_btnCancel);

            AcceptButton = _btnContinue;
            CancelButton = _btnCancel;
        }

        public static bool ConfirmContinue(IWin32Window owner, string exePath)
        {
            using var dlg = new ProtectedInstallPathWarningDialog(exePath);
            return dlg.ShowDialog(owner) == DialogResult.OK;
        }
    }
}
