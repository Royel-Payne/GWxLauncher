using GWxLauncher.Domain;

namespace GWxLauncher.UI.Dialogs
{
    /// <summary>
    /// Simple confirmation dialog for GW2 isolation setup.
    /// Shows which profiles share folders and confirms copy operation.
    /// </summary>
    internal partial class Gw2IsolationConfirmDialog : Form
    {
        public Gw2IsolationConfirmDialog(List<GameProfile> profilesWithDuplicates, int copyCount, long estimatedSpaceGB)
        {
            InitializeComponent();

            // Build profile list
            var profileNames = profilesWithDuplicates.Select(p => $"• {p.Name}").ToList();
            lblProfileList.Text = string.Join("\n", profileNames);

            // Update space note
            lblSpaceNote.Text = $"Note: Each copy requires ~{estimatedSpaceGB / copyCount}GB of disk space.";
        }
    }
}
