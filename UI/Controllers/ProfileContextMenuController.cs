using System;
using System.Windows.Forms;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileContextMenuController(
        IWin32Window owner,
        ProfileManager profiles,
        ProfileSelectionController selection,
        LaunchSessionPresenter launchSession,
        MainFormRefresher refresher,
        Func<bool> isShowCheckedOnly,
        Action<string> setStatus,
        Action<GameProfile, bool> launchProfile,
        Func<GameProfile, bool> trySelectProfileExecutable)
    {
        private readonly IWin32Window _owner = owner;
        private readonly ProfileManager _profiles = profiles;
        private readonly ProfileSelectionController _selection = selection;
        private readonly LaunchSessionPresenter _launchSession = launchSession;
        private readonly MainFormRefresher _refresher = refresher;

        private readonly Func<bool> _isShowCheckedOnly = isShowCheckedOnly;
        private readonly Action<string> _setStatus = setStatus;

        private readonly Action<GameProfile, bool> _launchProfile = launchProfile;
        private readonly Func<GameProfile, bool> _trySelectProfileExecutable = trySelectProfileExecutable;

        public readonly record struct ContextMenuState(
            bool HasSelectedProfile,
            bool CanShowLastLaunchDetails);

        private GameProfile? SelectedProfile()
            => _selection.GetSelectedProfile(_profiles.Profiles);

        public ContextMenuState GetContextMenuState()
        {
            bool hasProfile = SelectedProfile() != null;
            bool canShowLast = _launchSession.HasAnyReports;

            return new ContextMenuState(
                HasSelectedProfile: hasProfile,
                CanShowLastLaunchDetails: canShowLast);
        }

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
    }
}
