using System.Runtime.InteropServices;
using GWxLauncher.Services;


namespace GWxLauncher.UI
{
    /// <summary>
    /// Single source of truth for WinForms look/feel.
    /// </summary>
    internal static class ThemeService
    {
        // --- Windows 10/11 title bar tweaks (best-effort; safe no-op on older OS) ---
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_DONOTROUND = 1;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize);

        private static AppTheme _currentTheme = AppTheme.Light;
        private static ThemePalette _currentPalette = new LightPalette();

        public static AppTheme CurrentTheme => _currentTheme;
        public static ThemePalette CurrentPalette => _currentPalette;

        public static void SetTheme(AppTheme theme)
        {
            _currentTheme = theme;
            _currentPalette = theme == AppTheme.Light
                ? new LightPalette()
                : new DarkPalette();
        }

        private static void ApplyWindowChrome(Form f)
        {
            try
            {
                if (!f.IsHandleCreated)
                    _ = f.Handle; // force handle creation

                // Dark title bar (Win10 1809+)
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    int useDark = (_currentTheme == AppTheme.Dark) ? 1 : 0;
                    DwmSetWindowAttribute(f.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                }

                // Disable rounded corners (Win11+)
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                {
                    int preference = DWMWCP_DONOTROUND;
                    DwmSetWindowAttribute(f.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
                }
            }
            catch
            {
                // best effort
            }
        }

        private static void ApplyAppIcon(Form f)
        {
            try
            {
                // Copy the main window icon to child dialogs (so they all get the .ico)
                var main = Application.OpenForms.Cast<Form>().FirstOrDefault();
                if (main?.Icon != null)
                    f.Icon = main.Icon;
            }
            catch
            {
                // best effort
            }
        }
        internal static class FontNames
        {
            public const string PreferredUi = "Segoe UI Variable";
        }

        internal static class Palette
        {
            public static Color WindowBack => CurrentPalette.WindowBack;
            public static Color WindowFore => CurrentPalette.WindowFore;

            // NEW: subtle surface layer for panels/containers
            public static Color SurfaceBack => CurrentPalette.SurfaceBack;
            public static Color SurfaceBorder => CurrentPalette.SurfaceBorder;

            public static Color HeaderBack => CurrentPalette.HeaderBack;
            public static Color Separator => CurrentPalette.Separator;

            public static Color ButtonBack => CurrentPalette.ButtonBack;
            public static Color ButtonBorder => CurrentPalette.ButtonBorder;
            public static Color ButtonFore => CurrentPalette.ButtonFore;

            public static Color PrimaryButtonBack => CurrentPalette.PrimaryButtonBack;
            public static Color PrimaryButtonBorder => CurrentPalette.PrimaryButtonBorder;
            public static Color PrimaryButtonFore => CurrentPalette.PrimaryButtonFore;

            public static Color DangerButtonBack => CurrentPalette.DangerButtonBack;
            public static Color DangerButtonBorder => CurrentPalette.DangerButtonBorder;
            public static Color DangerButtonFore => CurrentPalette.DangerButtonFore;

            public static Color InputBack => CurrentPalette.InputBack;
            public static Color InputFore => CurrentPalette.InputFore;
            public static Color InputBorder => CurrentPalette.InputBorder;

            public static Color SubtleFore => CurrentPalette.SubtleFore;
            public static Color DisabledFore => CurrentPalette.DisabledFore;
        }

        internal static class CardPalette
        {
            public static Color Back => CurrentPalette.CardBack;
            public static Color SelectedBack => CurrentPalette.CardSelectedBack;

            public static Color Border => CurrentPalette.CardBorder;
            public static Color SelectedBorder => CurrentPalette.CardSelectedBorder;

            public static Color NameFore => CurrentPalette.CardNameFore;
            public static Color SubFore => CurrentPalette.CardSubFore;

            public static Color BadgeBack => CurrentPalette.BadgeBack;
            public static Color BadgeBorder => CurrentPalette.BadgeBorder;
            public static Color BadgeFore => CurrentPalette.BadgeFore;

            public static Color HoverBack => CurrentPalette.HoverBack;
            public static Color HoverBorder => CurrentPalette.HoverBorder;

            public static Color Accent => CurrentPalette.Accent;
            public static Color AccentHover => CurrentPalette.AccentHover;
        }


        internal static class CardMetrics
        {
            // Card padding inside listbox item bounds
            public const int OuterPadding = 3;
            public const int RightGutter = 0;


            // Checkbox
            public const int CheckboxOffsetX = 8;
            public const int CheckboxOffsetY = 22;
            public const int CheckboxSize = 16;

            // Icon
            public const int IconOffsetX = 30;
            public const int IconOffsetY = 8;
            public const int IconSize = 32;

            // Text
            public const int TextOffsetX = 8;   // gap after icon
            public const int TextOffsetY = 10;

            // Badges
            public const int BadgeRightPadding = 10;
            public const int BadgeTopPadding = 10;
            public const int BadgeHorizontalPad = 12;
            public const int BadgeVerticalPad = 6;
            public const int BadgeSpacing = 6;
            public const int SubtitleGapY = 2;

            // Accent
            public const int AccentWidth = 4;
        }

        internal static class Typography
        {
            public static readonly Font BadgeFont = new Font("Segoe UI", 8f, FontStyle.Bold);
            public static readonly Padding BadgePadding = new Padding(6, 2, 6, 2);
        }

        public static void ApplyToForm(Form f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));

            ApplyWindowChrome(f);
            ApplyAppIcon(f);

            f.BackColor = Palette.WindowBack;
            f.ForeColor = Palette.WindowFore;

            ApplyToControlTree(f);
        }

        public static void ApplyToControlTree(Control root)
        {
            if (root == null) return;

            // Apply to the root first
            ApplyControl(root);

            // Then recurse
            foreach (Control child in root.Controls)
                ApplyToControlTree(child);
        }

        private static void ApplyControl(Control c)
        {
            switch (c)
            {
                case FlowLayoutPanel flp:
                    // Profiles surface / scrolling surface
                    flp.BackColor = Palette.WindowBack;
                    flp.ForeColor = Palette.WindowFore;
                    break;

                case UserControl uc:
                    // Your ProfileCardControl falls here
                    uc.BackColor = Palette.WindowBack;
                    uc.ForeColor = Palette.WindowFore;
                    break;

                case Panel p:
                    // Panels act as a subtle “surface” layer
                    p.BackColor = Palette.SurfaceBack;
                    p.ForeColor = Palette.WindowFore;
                    break;

                case Label l:
                    l.ForeColor = Palette.WindowFore;
                    break;

                case Button b:
                    StyleButton(b);
                    break;

                case TextBox tb:
                    tb.BackColor = Palette.InputBack;
                    tb.ForeColor = Palette.InputFore;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ComboBox cb:
                    cb.BackColor = Palette.InputBack;
                    cb.ForeColor = Palette.InputFore;
                    cb.FlatStyle = FlatStyle.Flat;
                    break;

                case ListBox lb:
                    lb.BackColor = Palette.SurfaceBack;
                    lb.ForeColor = Palette.WindowFore;
                    lb.BorderStyle = BorderStyle.None;
                    break;

                case CheckBox chk:
                    chk.ForeColor = Palette.WindowFore;
                    break;

                case GroupBox gb:
                    gb.ForeColor = Palette.WindowFore;
                    gb.BackColor = Palette.SurfaceBack;
                    break;
            }
        }

        public static void StyleButton(Button b)
        {
            StyleButton(b, ActionRole.Default);
        }

        public static void StyleButton(Button b, ActionRole role)
        {
            if (b == null) return;

            Color back;
            Color fore;
            Color border;

            switch (role)
            {
                case ActionRole.Primary:
                    back = Palette.PrimaryButtonBack;
                    fore = Palette.PrimaryButtonFore;
                    border = Palette.PrimaryButtonBorder;
                    break;

                case ActionRole.Destructive:
                    back = Palette.DangerButtonBack;
                    fore = Palette.DangerButtonFore;
                    border = Palette.DangerButtonBorder;
                    break;

                default:
                    back = Palette.ButtonBack;
                    fore = Palette.ButtonFore;
                    border = Palette.ButtonBorder;
                    break;
            }

            b.BackColor = back;
            b.ForeColor = fore;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = border;

            // Specifically for the gear button to remove the white border and prevent "pop"
            if (b.Name == "btnSettings")
            {
                b.FlatAppearance.BorderSize = 0; // Strip border entirely
                b.FlatAppearance.MouseOverBackColor = Palette.ButtonBack;
            }
            else
            {
                b.FlatAppearance.BorderSize = 0; // Remove border for other buttons 
            }
        }
    }
}
