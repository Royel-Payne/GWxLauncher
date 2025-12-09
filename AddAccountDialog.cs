using System;
using System.Windows.Forms;

namespace GWxLauncher
{
    public partial class AddAccountDialog : Form
    {
        public GameProfile? CreatedProfile { get; private set; }

        public AddAccountDialog()
        {
            InitializeComponent();

            // If you used a ComboBox:
            cboGameType.SelectedIndex = 0;
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

            // If you used a ComboBox:
            if (cboGameType.SelectedIndex == 0)
                gameType = GameType.GuildWars1;
            else
                gameType = GameType.GuildWars2;

            // If you used radio buttons:
            // if (rbGw1.Checked)
            //     gameType = GameType.GuildWars1;
            // else
            //     gameType = GameType.GuildWars2;

            CreatedProfile = new GameProfile
            {
                Name = name,
                GameType = gameType
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
