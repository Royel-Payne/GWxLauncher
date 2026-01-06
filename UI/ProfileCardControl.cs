using GWxLauncher.Domain;
using System.Windows.Forms.VisualStyles;

namespace GWxLauncher.UI
{
    internal sealed class ProfileCardControl : UserControl
    {
        public GameProfile Profile { get; }
        public bool IsSelected { get; private set; }
        private bool _isHot;

        public event EventHandler? Clicked;
        public event EventHandler? DoubleClicked;
        public event MouseEventHandler? RightClicked;

        private readonly Image _gw1Image;
        private readonly Image _gw2Image;
        private readonly Font _nameFont;
        private readonly Font _subFont;

        // Provided by MainForm (so this control stays dumb + reusable)
        public Func<string, bool>? IsEligible { get; set; }
        public Action<string>? ToggleEligible { get; set; }

        // Provided by MainForm (runtime-only). True if this profile is currently running.
        public Func<string, bool>? IsRunning { get; set; }

        public ProfileCardControl(
            GameProfile profile,
            Image gw1Image,
            Image gw2Image,
            Font nameFont,
            Font subFont)
        {
            Profile = profile;
            _gw1Image = gw1Image;
            _gw2Image = gw2Image;
            _nameFont = nameFont;
            _subFont = subFont;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Cursor = Cursors.Hand;
            BackColor = ThemeService.Palette.WindowBack;

            // Prevent WinForms focus/ScrollControlIntoView behavior from fighting scrolling.
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;

            // This size is your “card” size. Wrapping is handled by FlowLayoutPanel.
            Size = new Size(320, 64);
            Margin = new Padding(0);

            MouseUp += OnMouseUp;
            MouseDoubleClick += (_, __) => DoubleClicked?.Invoke(this, EventArgs.Empty);
            Click += (_, __) => Clicked?.Invoke(this, EventArgs.Empty);
            MouseEnter += (_, __) => { _isHot = true; Invalidate(); };
            MouseLeave += (_, __) => { _isHot = false; Invalidate(); };
        }

        public void SetSelected(bool selected)
        {
            if (IsSelected == selected) return;
            IsSelected = selected;
            Invalidate();
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            // Left-click on the checkbox toggles eligibility (if wired)
            if (e.Button == MouseButtons.Left && ToggleEligible != null)
            {
                var cbRect = new Rectangle(
                    ThemeService.CardMetrics.CheckboxOffsetX,
                    ThemeService.CardMetrics.CheckboxOffsetY,
                    ThemeService.CardMetrics.CheckboxSize,
                    ThemeService.CardMetrics.CheckboxSize);

                if (cbRect.Contains(e.Location))
                {
                    ToggleEligible(Profile.Id);
                    Invalidate();
                    return;
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                RightClicked?.Invoke(this, e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Background
            var back = IsSelected
                ? ThemeService.CardPalette.SelectedBack
                : (_isHot ? ThemeService.CardPalette.HoverBack : ThemeService.CardPalette.Back);

            using (var bg = new SolidBrush(back))
                g.FillRectangle(bg, ClientRectangle);

            // Accent stripe (left)
            if (IsSelected || _isHot)
            {
                var stripe = new Rectangle(0, 0, ThemeService.CardMetrics.AccentWidth, Height);

                var stripeColor = IsSelected
                    ? ThemeService.CardPalette.Accent
                    : ThemeService.CardPalette.AccentHover;

                using (var stripeBrush = new SolidBrush(stripeColor))
                    g.FillRectangle(stripeBrush, stripe);
            }

            // Eligibility checkbox
            bool eligible = false;
            if (IsEligible != null)
            {
                try { eligible = IsEligible(Profile.Id); } catch { eligible = false; }
            }

            var cbRect = new Rectangle(
                ThemeService.CardMetrics.CheckboxOffsetX,
                ThemeService.CardMetrics.CheckboxOffsetY,
                ThemeService.CardMetrics.CheckboxSize,
                ThemeService.CardMetrics.CheckboxSize);

            CheckBoxRenderer.DrawCheckBox(
                g,
                cbRect.Location,
                eligible ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);

            // Icon
            var icon = Profile.GameType == GameType.GuildWars1 ? _gw1Image : _gw2Image;

            var iconRect = new Rectangle(
                ThemeService.CardMetrics.IconOffsetX,
                ThemeService.CardMetrics.IconOffsetY,
                ThemeService.CardMetrics.IconSize,
                ThemeService.CardMetrics.IconSize);

            g.DrawImage(icon, iconRect);

            // --- Responsive Text Layout ---
            int textLeft = iconRect.Right + ThemeService.CardMetrics.TextOffsetX;

            // ADJUSTMENT: textRight now accounts for the fixed 40px badge column width 
            // plus a small buffer to prevent the name from touching the pills.
            int textRight = Width - ThemeService.CardMetrics.BadgeRightPadding - 48;

            var nameRect = new Rectangle(textLeft, ThemeService.CardMetrics.TextOffsetY, textRight - textLeft, 22);
            var subRect = new Rectangle(textLeft, nameRect.Bottom - 2, textRight - textLeft, 18);

            // DRAW NAME: Using StringFormat to ensure long names truncate gracefully with "..."
            using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                using (var nameBrush = new SolidBrush(ThemeService.CardPalette.NameFore))
                    g.DrawString(Profile.Name, _nameFont, nameBrush, nameRect, sf);
            }

            var subText = Profile.GameType == GameType.GuildWars1 ? "Guild Wars 1" : "Guild Wars 2";

            bool isRunning = false;
            try
            {
                if (Profile.GameType == GameType.GuildWars1 && IsRunning != null)
                    isRunning = IsRunning(Profile.Id);
            }
            catch { /* best-effort */ }

            if (isRunning)
            {
                // No dot. Just a subtle color shift.
                using (var subBrush = new SolidBrush(ThemeService.CardPalette.Accent))
                    g.DrawString(subText, _subFont, subBrush, subRect);
            }
            else
            {
                using (var subBrush = new SolidBrush(ThemeService.CardPalette.SubFore))
                    g.DrawString(subText, _subFont, subBrush, subRect);
            }


            // --- Badges (Vertical Column) ---
            var badges = BuildBadges(Profile);
            DrawBadges(g, badges);

            // Bottom separator line
            using (var pen = new Pen(ThemeService.Palette.Separator))
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }

        // builds the badge list
        private static List<string> BuildBadges(GameProfile profile)
        {
            // GW1: TB, gMod, Py4
            if (profile.GameType == GameType.GuildWars1)
            {
                var badges = new List<string>(3);
                if (profile.Gw1ToolboxEnabled) badges.Add("TB");
                if (profile.Gw1GModEnabled) badges.Add("gMod");
                if (profile.Gw1Py4GwEnabled) badges.Add("Py4GW");
                return badges;
            }

            // GW2: Blish (when RunAfter enabled and any enabled program looks like Blish)
            if (profile.GameType == GameType.GuildWars2)
            {
                var progs = profile.Gw2RunAfterPrograms ?? new List<RunAfterProgram>();
                bool anyEnabled = profile.Gw2RunAfterEnabled && progs.Any(x => x.Enabled);

                if (anyEnabled)
                {
                    bool blish = progs.Any(x =>
                        x.Enabled &&
                        (
                            (x.Name?.IndexOf("Blish", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                            (x.ExePath?.IndexOf("Blish", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                        ));

                    if (blish)
                        return new List<string>(1) { "Blish" };
                }
            }

            return new List<string>(0);
        }

        private void DrawBadges(Graphics g, IReadOnlyList<string> badges)
        {
            if (badges.Count == 0) return;

            // Standardized dimensions for a tight, professional column
            const int fixedWidth = 40;
            const int fixedHeight = 16;
            const int spacing = 4;

            // Use a slightly smaller font for the pills to increase internal margins
            using var tinyFont = new Font(ThemeService.Typography.BadgeFont.FontFamily,
                                         ThemeService.Typography.BadgeFont.Size - 1f,
                                         FontStyle.Regular);

            // Calculate total height of the stack to center it vertically
            int totalStackHeight = (badges.Count * fixedHeight) + ((badges.Count - 1) * spacing);

            // Vertical center calculation based on control height (64px)
            int startY = (Height - totalStackHeight) / 2;
            int badgeRight = Width - ThemeService.CardMetrics.BadgeRightPadding;

            using var badgeBg = new SolidBrush(ThemeService.CardPalette.BadgeBack);
            using var badgePen = new Pen(ThemeService.CardPalette.BadgeBorder);

            for (int i = 0; i < badges.Count; i++)
            {
                var rect = new Rectangle(badgeRight - fixedWidth, startY + (i * (fixedHeight + spacing)), fixedWidth, fixedHeight);

                g.FillRectangle(badgeBg, rect);
                g.DrawRectangle(badgePen, rect);

                // Center text perfectly inside the uniform pill
                TextRenderer.DrawText(
                    g,
                    badges[i],
                    tinyFont,
                    rect,
                    ThemeService.CardPalette.BadgeFore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
