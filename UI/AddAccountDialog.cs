using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.UI;
using System.IO;

namespace GWxLauncher
{
    public partial class AddAccountDialog : Form
    {
        public GameProfile? CreatedProfile { get; private set; }

        private readonly GameProfile? _editingProfile;

        public AddAccountDialog()
        {
            InitializeComponent();
            ThemeService.ApplyToForm(this);

            // Make sure the combo is a fixed list
            cboGameType.DropDownStyle = ComboBoxStyle.DropDownList;

            if (cboGameType.Items.Count > 0)
            {
                cboGameType.SelectedIndex = 0;
            }
        }

        public AddAccountDialog(GameProfile existingProfile) : this()
        {
            _editingProfile = existingProfile;

            txtName.Text = existingProfile.Name;
            cboGameType.SelectedIndex = existingProfile.GameType == GameType.GuildWars1 ? 0 : 1;

            Text = "Edit Account";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(
                    "Please enter a display name for the account.",
                    "Missing name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            GameType gameType;

            if (cboGameType.SelectedIndex == 0)
                gameType = GameType.GuildWars1;
            else
                gameType = GameType.GuildWars2;

            if (_editingProfile != null)
            {
                // Edit existing profile in place
                _editingProfile.Name = name;
                _editingProfile.GameType = gameType;
                // keep ExecutablePath as-is

                CreatedProfile = _editingProfile;
            }
            else
            {
                // Create a new profile
                var p = new GameProfile
                {
                    Name = name,
                    GameType = gameType
                };

                // Auto-fill remembered tool paths (GW1 only) if profile paths are blank
                if (p.GameType == GameType.GuildWars1)
                {
                    var cfg = LauncherConfig.Load();

                    TryAutofillToolPath(p.Gw1ToolboxDllPath, cfg.LastToolboxPath, v => p.Gw1ToolboxDllPath = v);
                    TryAutofillToolPath(p.Gw1GModDllPath, cfg.LastGModPath, v => p.Gw1GModDllPath = v);
                    TryAutofillToolPath(p.Gw1Py4GwDllPath, cfg.LastPy4GWPath, v => p.Gw1Py4GwDllPath = v);
                }

                CreatedProfile = p;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        private static void TryAutofillToolPath(string currentValue, string candidatePath, Action<string> assign)
        {
            if (!string.IsNullOrWhiteSpace(currentValue))
                return;

            candidatePath = (candidatePath ?? "").Trim();
            if (string.IsNullOrWhiteSpace(candidatePath))
                return;

            try
            {
                if (File.Exists(candidatePath))
                    assign(candidatePath);
            }
            catch
            {
                // best-effort only
            }
        }
    }
}
