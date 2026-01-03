// Marker - Refactor Identifier: 01-03-26 01:18:00
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GWxLauncher.Domain;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ProfileGridController
    {
        private readonly Func<string, bool> _isEligible;
        private readonly Action<string> _toggleEligible;

        private readonly Action<string> _onSelected;
        private readonly Action<string> _onDoubleClicked;
        private readonly Action<string, Point> _onRightClicked;
        private readonly FlowLayoutPanel _panel;


        private readonly List<ProfileCardControl> _cards = new();

        public ProfileGridController(
            FlowLayoutPanel panel,
            Func<string, bool> isEligible,
            Action<string> toggleEligible,
            Action<string> onSelected,
            Action<string> onDoubleClicked,
            Action<string, Point> onRightClicked)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _isEligible = isEligible ?? throw new ArgumentNullException(nameof(isEligible));
            _toggleEligible = toggleEligible ?? throw new ArgumentNullException(nameof(toggleEligible));

            _onSelected = onSelected ?? throw new ArgumentNullException(nameof(onSelected));
            _onDoubleClicked = onDoubleClicked ?? throw new ArgumentNullException(nameof(onDoubleClicked));
            _onRightClicked = onRightClicked ?? throw new ArgumentNullException(nameof(onRightClicked));
        }

        public void Rebuild(
            IEnumerable<GameProfile> profiles,
            string? selectedProfileId,
            Image gw1Image,
            Image gw2Image,
            Font nameFont,
            Font subFont)
        {
            _panel.SuspendLayout();
            try
            {
                _panel.Controls.Clear();
                _cards.Clear();

                foreach (var profile in profiles)
                {
                    var card = new ProfileCardControl(
                        profile,
                        gw1Image,
                        gw2Image,
                        nameFont,
                        subFont)
                    {
                        IsEligible = id => _isEligible(id),
                        ToggleEligible = id => _toggleEligible(id)
                    };

                    card.Clicked += (_, __) => _onSelected(profile.Id);
                    card.DoubleClicked += (_, __) => _onDoubleClicked(profile.Id);
                    card.RightClicked += (_, e) =>
                        _onRightClicked(profile.Id, card.PointToScreen(e.Location));

                    card.SetSelected(string.Equals(profile.Id, selectedProfileId, StringComparison.Ordinal));

                    _cards.Add(card);
                    _panel.Controls.Add(card);
                }
            }
            finally
            {
                _panel.ResumeLayout();
            }
        }

        public void SetSelectedProfile(string? id)
        {
            foreach (var card in _cards)
                card.SetSelected(string.Equals(card.Profile.Id, id, StringComparison.Ordinal));
        }

        public void ApplyResponsiveLayout()
        {
            // Bug workaround:
        }
    }
}
