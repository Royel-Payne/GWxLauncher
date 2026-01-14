using GWxLauncher.Domain;
using GWxLauncher.Services;

namespace GWxLauncher.UI.Dialogs
{
    /// <summary>
    /// Dialog shown when user tries to enable GW2 isolation but some profiles share game folders.
    /// Automatically determines which profiles need copies (N-1 for N profiles sharing a folder).
    /// Shows auto-generated destinations, allows user to confirm or customize.
    /// </summary>
    internal partial class Gw2IsolationSetupDialog : Form
    {
        private readonly Gw2IsolationValidationResult _validationResult;

        // Profiles that will get NEW folders (copies)
        public List<GameProfile> ProfilesToCopy { get; private set; } = new();
        
        // Map: Profile -> destination folder path
        public Dictionary<GameProfile, string> CopyDestinations { get; private set; } = new();

        public Gw2IsolationSetupDialog(Gw2IsolationValidationResult validationResult)
        {
            _validationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
            
            InitializeComponent();
            
            AutoGenerateCopyPlan();
            LoadProfiles();
        }

        private void AutoGenerateCopyPlan()
        {
            // For each group of profiles sharing a folder:
            // - First profile keeps original (no copy needed, IsolationGameFolderPath stays empty)
            // - Rest get auto-generated copy destinations
            foreach (var group in _validationResult.ExePathToProfiles)
            {
                string originalExePath = group.Key;
                var profiles = group.Value;

                // Get original folder info
                string? originalFolder = Path.GetDirectoryName(originalExePath);
                if (string.IsNullOrEmpty(originalFolder))
                    continue;

                string originalFolderName = Path.GetFileName(originalFolder);
                string? parentFolder = Path.GetDirectoryName(originalFolder);
                if (string.IsNullOrEmpty(parentFolder))
                    parentFolder = originalFolder;

                // First profile keeps original (skip it)
                // profiles[0] will have IsolationGameFolderPath = "" (empty = use original)

                // Rest get copies with auto-generated names
                for (int i = 1; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    
                    // Generate destination: sibling folder with profile name appended
                    // Example: C:\Games\Guild Wars 2 -> C:\Games\Guild Wars 2_ProfileName
                    string cleanProfileName = SanitizeForFolderName(profile.Name);
                    string suggestedFolder = Path.Combine(parentFolder, $"{originalFolderName}_{cleanProfileName}");

                    CopyDestinations[profile] = suggestedFolder;
                    ProfilesToCopy.Add(profile);
                }
            }
        }

        private string SanitizeForFolderName(string input)
        {
            // Remove invalid filename characters and limit length
            var invalid = Path.GetInvalidFileNameChars();
            string clean = string.Concat(input.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            clean = clean.Replace(" ", "_").Trim();
            
            // Limit to 50 chars
            if (clean.Length > 50)
                clean = clean.Substring(0, 50);
            
            return clean;
        }

        private void LoadProfiles()
        {
            checkedListBoxProfiles.Items.Clear();

            if (_validationResult.ProfilesWithDuplicateExePath.Count == 0)
            {
                lblMessage.Text = "All profiles have unique game folders. No action needed.";
                btnCopy.Enabled = false;
                btnCopy.Text = "Continue";
                return;
            }

            // Display the copy plan
            foreach (var group in _validationResult.ExePathToProfiles)
            {
                string originalExePath = group.Key;
                var profiles = group.Value;
                string? originalFolder = Path.GetDirectoryName(originalExePath);

                // Add header
                checkedListBoxProfiles.Items.Add($"== Folder: {originalFolder}");

                // First profile keeps original
                var keepOriginal = profiles[0];
                checkedListBoxProfiles.Items.Add($"   [KEEP] {keepOriginal.Name} (uses original folder)");

                // Rest get copies
                for (int i = 1; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    if (CopyDestinations.TryGetValue(profile, out string? destination))
                    {
                        checkedListBoxProfiles.Items.Add($"   [COPY] {profile.Name}");
                        checkedListBoxProfiles.Items.Add($"          -> {destination}");
                    }
                }

                checkedListBoxProfiles.Items.Add(""); // Spacer
            }

            UpdateMessage();
            btnCopy.Text = "Start Copying";
        }

        private void UpdateMessage()
        {
            int totalProfiles = _validationResult.ProfilesWithDuplicateExePath.Count;
            int copiesToCreate = ProfilesToCopy.Count;
            int profilesKeepingOriginal = totalProfiles - copiesToCreate;

            long estimatedSpaceGB = copiesToCreate * 75L; // Estimate 75GB per copy

            lblMessage.Text = $"Isolation Setup Summary:\n\n" +
                             $"- {totalProfiles} profiles share game folders\n" +
                             $"- {profilesKeepingOriginal} will keep original folder(s)\n" +
                             $"- {copiesToCreate} will get new copies\n\n" +
                             $"Estimated space needed: ~{estimatedSpaceGB} GB\n\n" +
                             $"Click 'Start Copying' to proceed.";
        }

        private void checkedListBoxProfiles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Disabled - this is now informational only
            e.NewValue = e.CurrentValue;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (ProfilesToCopy.Count == 0)
            {
                // No copies needed, just continue
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            // Validate destinations don't already exist
            var existingFolders = CopyDestinations.Values.Where(Directory.Exists).ToList();
            if (existingFolders.Any())
            {
                var result = MessageBox.Show(
                    $"The following destination folders already exist:\n\n" +
                    string.Join("\n", existingFolders.Take(5)) +
                    (existingFolders.Count > 5 ? $"\n... and {existingFolders.Count - 5} more" : "") +
                    "\n\nOverwrite?",
                    "Folders Exist",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
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

