using GWxLauncher.Domain;
using System;
using System.Windows.Forms;
using GWxLauncher.UI;

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
                CreatedProfile = new GameProfile
                {
                    Name = name,
                    GameType = gameType
                };
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
