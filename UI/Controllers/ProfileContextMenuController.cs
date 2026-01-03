using System;
using System.Windows.Forms;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileContextMenuController
    {
        private readonly IWin32Window _owner;
        private readonly ProfileManager _profiles;
        private readonly ProfileSelectionController _selection;
        private readonly LaunchSessionPresenter _launchSession;
        private readonly MainFormRefresher _refresher;

        private readonly Func<bool> _isShowCheckedOnly;
        private readonly Action<string> _setStatus;

        private readonly Action<GameProfile, bool> _launchProfile;
        private readonly Func<GameProfile, bool> _trySelectProfileExecutable;
        private readonly Func<GameProfile, bool> _trySelectGw1ToolboxDll;

        public ProfileContextMenuController(
            IWin32Window owner,
            ProfileManager profiles,
            ProfileSelectionController selection,
            LaunchSessionPresenter launchSession,
            MainFormRefresher refresher,
            Func<bool> isShowCheckedOnly,
            Action<string> setStatus,
            Action<GameProfile, bool> launchProfile,
            Func<GameProfile, bool> trySelectProfileExecutable,
            Func<GameProfile, bool> trySelectGw1ToolboxDll)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _launchSession = launchSession ?? throw new ArgumentNullException(nameof(launchSession));
            _refresher = refresher ?? throw new ArgumentNullException(nameof(refresher));

            _isShowCheckedOnly = isShowCheckedOnly ?? throw new ArgumentNullException(nameof(isShowCheckedOnly));
            _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));

            _launchProfile = launchProfile ?? throw new ArgumentNullException(nameof(launchProfile));
            _trySelectProfileExecutable = trySelectProfileExecutable ?? throw new ArgumentNullException(nameof(trySelectProfileExecutable));
            _trySelectGw1ToolboxDll = trySelectGw1ToolboxDll ?? throw new ArgumentNullException(nameof(trySelectGw1ToolboxDll));
        }

        private GameProfile? SelectedProfile()
            => _selection.GetSelectedProfile(_profiles.Profiles);

        public void ShowLastLaunchDetails()
        {
            if (!_launchSession.HasAnyReports)
                return;

            var dlg = new LastLaunchDetailsForm(_launchSession.AllReports);
            dlg.ShowDialog(_owner);
        }

        public void LaunchSelectedProfile()
        {
            var profile = SelectedProfile();
            if (profile == null)
                return;

            _launchProfile(profile, false);
        }

        public void SetSelectedProfilePath()
        {
            var profile = SelectedProfile();
            if (profile == null)
                return;

            _trySelectProfileExecutable(profile);
        }

        public void EditSelectedProfile()
        {
            var profile = SelectedProfile();
            if (profile == null)
                return;

            using var dlg = new ProfileSettingsForm(profile);
            if (dlg.ShowDialog(_owner) == DialogResult.OK)
            {
                _profiles.Save();
                _refresher.RequestRefresh(RefreshReason.ProfilesChanged);
            }
        }

        public void CopySelectedProfile()
        {
            var profile = SelectedProfile();
            if (profile == null)
                return;

            var copied = _profiles.CopyProfile(profile);

            // Intentionally unchecked in all views:
            // ViewStateStore returns false for unknown profile IDs, so no entry is created here.

            if (_isShowCheckedOnly())
            {
                MessageBox.Show(
                    _owner,
                    "Profile copied.\n\nIt starts unchecked in all views, so it may be hidden while \"Show Checked Accounts Only\" is enabled.\nDisable that option to see the new profile.",
                    "Copy Profile",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            _selection.Select(copied.Id);

            _refresher.RequestRefresh(RefreshReason.ProfilesChanged);
            _setStatus($"Copied profile: {profile.Name} → {copied.Name}");
        }

        public void DeleteSelectedProfile()
        {
            var profile = SelectedProfile();
            if (profile == null)
                return;

            var result = MessageBox.Show(
                _owner,
                $"Delete account \"{profile.Name}\"?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            _profiles.RemoveProfile(profile);
            _profiles.Save();

            _selection.EnsureSelectionValid(_profiles.Profiles);
            _refresher.RequestRefresh(RefreshReason.ProfilesChanged);

            _setStatus($"Deleted account: {profile.Name}.");
        }

        public void ToggleGw1Toolbox()
        {
            var profile = SelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
                return;

            // Assumes profile.Gw1ToolboxEnabled already reflects the toggled state.
            _profiles.Save();

            _setStatus(
                $"GW1 Toolbox injection " +
                (profile.Gw1ToolboxEnabled ? "enabled" : "disabled") +
                $" for {profile.Name}.");
        }

        public void SetGw1ToolboxPath()
        {
            var profile = SelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
                return;

            _trySelectGw1ToolboxDll(profile);
        }
    }
}
