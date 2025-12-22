using System.Drawing;

namespace GWxLauncher.UI
{
    internal sealed class DarkPalette : ThemePalette
    {
        public override Color WindowBack => Color.FromArgb(24, 24, 28);
        public override Color WindowFore => Color.Gainsboro;

        public override Color SurfaceBack => Color.FromArgb(30, 30, 36);
        public override Color SurfaceBorder => Color.FromArgb(60, 60, 70);

        public override Color HeaderBack => Color.FromArgb(30, 30, 36);
        public override Color Separator => Color.FromArgb(60, 60, 70);

        public override Color ButtonBack => Color.FromArgb(45, 45, 52);
        public override Color ButtonBorder => Color.FromArgb(80, 80, 90);
        public override Color ButtonFore => Color.White;

        public override Color PrimaryButtonBack => Color.FromArgb(0, 120, 215);
        public override Color PrimaryButtonBorder => Color.FromArgb(0, 100, 180);
        public override Color PrimaryButtonFore => Color.White;

        public override Color DangerButtonBack => Color.FromArgb(180, 60, 60);
        public override Color DangerButtonBorder => Color.FromArgb(150, 45, 45);
        public override Color DangerButtonFore => Color.White;

        public override Color InputBack => Color.FromArgb(18, 18, 22);
        public override Color InputFore => Color.Gainsboro;
        public override Color InputBorder => Color.FromArgb(80, 80, 90);

        public override Color SubtleFore => Color.FromArgb(180, 180, 190);
        public override Color DisabledFore => Color.FromArgb(140, 140, 150);

        public override Color CardBack => Color.FromArgb(35, 35, 40);
        public override Color CardSelectedBack => Color.FromArgb(45, 45, 52);

        public override Color CardBorder => Color.FromArgb(70, 70, 80);
        public override Color CardSelectedBorder => Color.FromArgb(0, 120, 215);

        public override Color CardNameFore => Color.White;
        public override Color CardSubFore => Color.Silver;

        public override Color BadgeBack => Color.FromArgb(60, 60, 70);
        public override Color BadgeBorder => Color.FromArgb(90, 90, 110);
        public override Color BadgeFore => Color.Gainsboro;

        public override Color HoverBack => Color.FromArgb(40, 40, 46);
        public override Color HoverBorder => Color.FromArgb(95, 95, 110);

        public override Color Accent => Color.FromArgb(0, 120, 215);
    }
}
