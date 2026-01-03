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

        public void RefreshTheme()
        {
            // Force full repaint of custom-drawn cards after theme change
            _panel.SuspendLayout();

            foreach (Control c in _panel.Controls)
            {
                if (c is ProfileCardControl card)
                {
                    card.Invalidate();
                    card.Update(); // flush paint immediately to avoid ghosting
                }
            }

            _panel.ResumeLayout(true);
        }

        public void InitializePanel()
        {
            // Keep FlowLayoutPanel behaving consistently (moved from MainForm)
            _panel.WrapContents = true;
            _panel.FlowDirection = FlowDirection.LeftToRight;
            _panel.AutoScroll = true;

            // Ensure we start with the same outer gutter used by responsive layout.
            _panel.Padding = new Padding(CardOuterPad);

            // Relayout when width changes (defer until WinForms completes scroll calculations)
            _panel.ClientSizeChanged += (_, __) =>
            {
                if (_panel.IsHandleCreated)
                    _panel.BeginInvoke(new Action(() => ApplyResponsiveLayout(force: false)));
            };
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

        // Layout tuning knobs (moved from MainForm)
        private const int CardOuterPad = 6;
        private const int CardGap = 6;
        private const int CardMinWidth = 230;
        private const int CardMaxWidth = 520;
        private const int CardPreferredWidth = 340;

        private const int ScrollbarReserve = 10;

        private int _lastLayoutWidth = -1;

        public void ApplyResponsiveLayout(bool force = false)
        {
            if (!_panel.IsHandleCreated)
                return;

            int w = _panel.ClientSize.Width;
            if (w <= 0)
                return;

            // Only skip when not forced AND width unchanged.
            // Forced calls are used after rebuilds (new controls need margins/width applied).
            if (!force && w == _lastLayoutWidth)
                return;

            _lastLayoutWidth = w;

            var cards = _panel.Controls.OfType<ProfileCardControl>().ToList();
            if (cards.Count == 0)
                return;

            int sbW = SystemInformation.VerticalScrollBarWidth;

            int reserve = Math.Max(0, sbW - ScrollbarReserve);
            int availW = Math.Max(0, _panel.ClientSize.Width - (CardOuterPad * 2) - reserve);

            var (_, finalCardW) = ComputeGrid(availW);

            _panel.SuspendLayout();
            try
            {
                _panel.Padding = new Padding(CardOuterPad);

                int half = Math.Max(0, CardGap / 2);
                var margin = new Padding(half, 0, half, CardGap);

                foreach (var c in cards)
                {
                    c.Width = finalCardW;
                    c.Margin = margin;
                }
            }
            finally
            {
                _panel.ResumeLayout(true);
            }
        }

        private static (int columns, int cardWidth) ComputeGrid(int availableWidth)
        {
            if (availableWidth <= 0)
                return (1, CardMinWidth);

            // Start with as many columns as we can fit at minimum width
            int cols = Math.Max(1, (availableWidth + CardGap) / (CardMinWidth + CardGap));

            // Compute ideal width for that column count
            double ideal = (availableWidth - (CardGap * (cols - 1))) / (double)cols;

            // Add columns as soon as the next column still keeps cards at a "comfortable" width.
            // This makes columns appear earlier (not only when we hit max width).
            while (true)
            {
                int nextCols = cols + 1;
                double nextIdeal = (availableWidth - (CardGap * (nextCols - 1))) / (double)nextCols;

                // Stop if another column would make cards too narrow
                if (nextIdeal < CardMinWidth)
                    break;

                // Only add the column if cards would still be at least the preferred width
                if (nextIdeal < CardPreferredWidth)
                    break;

                cols = nextCols;
                ideal = nextIdeal;
            }

            int w = (int)Math.Floor(ideal);
            if (w < CardMinWidth) w = CardMinWidth;
            if (w > CardMaxWidth) w = CardMaxWidth;

            return (cols, w);
        }
    }
}
