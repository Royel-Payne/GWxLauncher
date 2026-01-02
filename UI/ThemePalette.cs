using System.Drawing;

namespace GWxLauncher.UI
{
    internal abstract class ThemePalette
    {
        public abstract Color WindowBack { get; }
        public abstract Color WindowFore { get; }

        public abstract Color SurfaceBack { get; }
        public abstract Color SurfaceBorder { get; }

        public abstract Color HeaderBack { get; }
        public abstract Color Separator { get; }

        public abstract Color ButtonBack { get; }
        public abstract Color ButtonBorder { get; }
        public abstract Color ButtonFore { get; }

        public abstract Color PrimaryButtonBack { get; }
        public abstract Color PrimaryButtonBorder { get; }
        public abstract Color PrimaryButtonFore { get; }

        public abstract Color DangerButtonBack { get; }
        public abstract Color DangerButtonBorder { get; }
        public abstract Color DangerButtonFore { get; }

        public abstract Color InputBack { get; }
        public abstract Color InputFore { get; }
        public abstract Color InputBorder { get; }

        public abstract Color SubtleFore { get; }
        public abstract Color DisabledFore { get; }

        // Card palette equivalents
        public abstract Color CardBack { get; }
        public abstract Color CardSelectedBack { get; }
        public abstract Color CardBorder { get; }
        public abstract Color CardSelectedBorder { get; }
        public abstract Color CardNameFore { get; }
        public abstract Color CardSubFore { get; }

        public abstract Color BadgeBack { get; }
        public abstract Color BadgeBorder { get; }
        public abstract Color BadgeFore { get; }

        public abstract Color HoverBack { get; }
        public abstract Color HoverBorder { get; }

        public abstract Color Accent { get; }
        public abstract Color AccentHover { get; }
    }
}
