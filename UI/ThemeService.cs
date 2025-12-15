using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.InteropServices;

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

        private static void ApplyWindowChrome(Form f)
        {
            try
            {
                if (!f.IsHandleCreated)
                    _ = f.Handle; // force handle creation

                // Dark title bar (Win10 1809+)
                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
                {
                    int useDark = 1;
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
            // ─────────────────────────────────────
            // Window & global text
            // ─────────────────────────────────────
            public static readonly Color WindowBack = Color.FromArgb(24, 24, 28);
            public static readonly Color WindowFore = Color.Gainsboro;

            // ─────────────────────────────────────
            // Surfaces / containers
            // ─────────────────────────────────────
            public static readonly Color HeaderBack = Color.FromArgb(30, 30, 36);
            public static readonly Color Separator = Color.FromArgb(60, 60, 70);

            // ─────────────────────────────────────
            // Buttons
            // ─────────────────────────────────────
            public static readonly Color ButtonBack = Color.FromArgb(45, 45, 52);
            public static readonly Color ButtonBorder = Color.FromArgb(80, 80, 90);
            public static readonly Color ButtonFore = Color.White;

            // ─────────────────────────────────────
            // Inputs / fields
            // ─────────────────────────────────────
            public static readonly Color InputBack = Color.FromArgb(18, 18, 22);
            public static readonly Color InputFore = Color.Gainsboro;
            public static readonly Color InputBorder = Color.FromArgb(80, 80, 90);

            // ─────────────────────────────────────
            // Text variants
            // ─────────────────────────────────────
            public static readonly Color SubtleFore = Color.FromArgb(180, 180, 190);
            public static readonly Color DisabledFore = Color.FromArgb(140, 140, 150);
        }
        internal static class CardPalette
        {
            // Card background
            public static readonly Color Back = Color.FromArgb(35, 35, 40);
            public static readonly Color SelectedBack = Color.FromArgb(45, 45, 52);

            // Card border
            public static readonly Color Border = Color.FromArgb(70, 70, 80);
            public static readonly Color SelectedBorder = Color.FromArgb(0, 120, 215);

            // Card text
            public static readonly Color NameFore = Color.White;
            public static readonly Color SubFore = Color.Silver;

            // Badge pills
            public static readonly Color BadgeBack = Color.FromArgb(60, 60, 70);
            public static readonly Color BadgeBorder = Color.FromArgb(90, 90, 110);
            public static readonly Color BadgeFore = Color.Gainsboro;
            // Hover
            public static readonly Color HoverBack = Color.FromArgb(40, 40, 46);
            public static readonly Color HoverBorder = Color.FromArgb(95, 95, 110);

            // Accent (selection / hover indicator)
            public static readonly Color Accent = Color.FromArgb(0, 120, 215); // same as SelectedBorder (Windows blue)
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
                case Panel p:
                    // Panels usually act as “surfaces”
                    p.BackColor = Palette.WindowBack;
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
                    lb.BackColor = Palette.WindowBack;
                    lb.ForeColor = Palette.WindowFore;
                    lb.BorderStyle = BorderStyle.None;
                    break;

                case CheckBox chk:
                    chk.ForeColor = Palette.WindowFore;
                    break;

                case GroupBox gb:
                    gb.ForeColor = Palette.WindowFore;
                    gb.BackColor = Palette.WindowBack;
                    break;
            }
        }

        public static void StyleButton(Button b)
        {
            b.BackColor = Palette.ButtonBack;
            b.ForeColor = Palette.ButtonFore;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Palette.ButtonBorder;
            b.FlatAppearance.BorderSize = 1;
            b.UseVisualStyleBackColor = false;
        }
    }
}
