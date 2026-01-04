using GWxLauncher.Domain;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileSelectionController
    {
        private Action<string?>? _setSelectedInGrid;

        public string? SelectedProfileId { get; private set; }

        public ProfileSelectionController(Action<string?>? setSelectedInGrid)
        {
            _setSelectedInGrid = setSelectedInGrid;
        }

        public void SetSelectedInGrid(Action<string?> setSelectedInGrid)
        {
            _setSelectedInGrid = setSelectedInGrid ?? throw new ArgumentNullException(nameof(setSelectedInGrid));
        }

        public void Select(string? profileId)
        {
            SelectedProfileId = profileId;
            _setSelectedInGrid?.Invoke(profileId);
        }

        public void ClearSelection()
        {
            Select(null);
        }

        public void EnsureSelectionValid(IEnumerable<GameProfile> profiles)
        {
            if (profiles == null)
                throw new ArgumentNullException(nameof(profiles));

            if (string.IsNullOrWhiteSpace(SelectedProfileId))
                return;

            bool exists = profiles.Any(p =>
                string.Equals(p.Id, SelectedProfileId, StringComparison.Ordinal));

            if (!exists)
                ClearSelection();
        }

        public GameProfile? GetSelectedProfile(IEnumerable<GameProfile> profiles)
        {
            if (profiles == null)
                throw new ArgumentNullException(nameof(profiles));

            if (string.IsNullOrWhiteSpace(SelectedProfileId))
                return null;

            return profiles.FirstOrDefault(p =>
                string.Equals(p.Id, SelectedProfileId, StringComparison.Ordinal));
        }
    }
}
