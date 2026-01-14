using GWxLauncher.Domain;
using GWxLauncher.Services;

namespace GWxLauncher.UI.Dialogs
{
    /// <summary>
    /// Dialog shown when user tries to enable GW2 isolation but some profiles share game folders.
    /// Allows user to select which profiles need copying.
    /// </summary>
    internal partial class Gw2IsolationSetupDialog : Form
    {
        private readonly Gw2IsolationValidationResult _validationResult;

        public List<GameProfile> SelectedProfiles { get; private set; } = new();

        public Gw2IsolationSetupDialog(Gw2IsolationValidationResult validationResult)
        {
            _validationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
            
            InitializeComponent();
            
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            checkedListBoxProfiles.Items.Clear();

            if (_validationResult.ProfilesWithDuplicateExePath.Count == 0)
            {
                lblMessage.Text = "All profiles have unique game folders. No action needed.";
                btnCopy.Enabled = false;
                return;
            }

            // Group by exe path and display
            foreach (var group in _validationResult.ExePathToProfiles)
            {
                string exePath = group.Key;
                var profiles = group.Value;

                // Add header item (not selectable)
                int headerIndex = checkedListBoxProfiles.Items.Add($"?? Shared folder: {exePath}");
                
                // Add profile items (selectable)
                foreach (var profile in profiles)
                {
                    int itemIndex = checkedListBoxProfiles.Items.Add(profile);
                    // Auto-select all but one (user should leave at least one in original location)
                    if (profiles.IndexOf(profile) > 0)
                    {
                        checkedListBoxProfiles.SetItemChecked(itemIndex, true);
                    }
                }
            }

            UpdateMessage();
        }

        private void UpdateMessage()
        {
            int totalProfiles = _validationResult.ProfilesWithDuplicateExePath.Count;
            int selectedCount = checkedListBoxProfiles.CheckedItems.Count;

            lblMessage.Text = $"? {totalProfiles} profiles share game folders.\n\n" +
                             $"Selected {selectedCount} profile(s) for copying.\n" +
                             $"At least one profile per group should remain unchecked to keep the original folder.";
        }

        private void checkedListBoxProfiles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Update message after the check state changes
            BeginInvoke(new Action(UpdateMessage));
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            // Collect selected profiles (skip string headers)
            SelectedProfiles = checkedListBoxProfiles.CheckedItems
                .Cast<object>()
                .OfType<GameProfile>()
                .ToList();

            if (SelectedProfiles.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one profile to copy.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Validate at least one profile per group remains unchecked
            foreach (var group in _validationResult.ExePathToProfiles)
            {
                var groupProfiles = group.Value;
                var selectedInGroup = SelectedProfiles.Intersect(groupProfiles).ToList();

                if (selectedInGroup.Count == groupProfiles.Count)
                {
                    MessageBox.Show(
                        $"All profiles sharing the folder:\n{group.Key}\n\n" +
                        "are selected for copying. At least one profile must remain in the original location.\n\n" +
                        "Please uncheck one profile from this group.",
                        "Invalid Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
