using System.Drawing;

namespace GWxLauncher.UI
{
    internal sealed class LightPalette : ThemePalette
    {
        public override Color WindowBack => Color.FromArgb(245, 245, 248);
        public override Color WindowFore => Color.FromArgb(28, 28, 32);

        public override Color SurfaceBack => Color.FromArgb(236, 236, 240);
        public override Color SurfaceBorder => Color.FromArgb(200, 200, 210);

        public override Color HeaderBack => SurfaceBack;
        public override Color Separator => SurfaceBorder;

        public override Color ButtonBack => Color.FromArgb(230, 230, 236);
        public override Color ButtonBorder => Color.FromArgb(190, 190, 200);
        public override Color ButtonFore => Color.FromArgb(28, 28, 32);

        public override Color PrimaryButtonBack => Color.FromArgb(0, 120, 215);
        public override Color PrimaryButtonBorder => Color.FromArgb(0, 100, 180);
        public override Color PrimaryButtonFore => Color.White;

        public override Color DangerButtonBack => Color.FromArgb(200, 60, 60);
        public override Color DangerButtonBorder => Color.FromArgb(170, 45, 45);
        public override Color DangerButtonFore => Color.White;

        public override Color InputBack => Color.White;
        public override Color InputFore => Color.FromArgb(28, 28, 32);
        public override Color InputBorder => Color.FromArgb(190, 190, 200);

        public override Color SubtleFore => Color.FromArgb(90, 90, 100);
        public override Color DisabledFore => Color.FromArgb(150, 150, 160);

        public override Color CardBack => SurfaceBack;
        public override Color CardSelectedBack => Color.FromArgb(225, 225, 232);

        public override Color CardBorder => SurfaceBorder;
        public override Color CardSelectedBorder => Color.FromArgb(0, 120, 215);

        public override Color CardNameFore => WindowFore;
        public override Color CardSubFore => Color.FromArgb(80, 80, 90);

        public override Color BadgeBack => Color.FromArgb(220, 220, 228);
        public override Color BadgeBorder => SurfaceBorder;
        public override Color BadgeFore => Color.FromArgb(70, 70, 80);

        public override Color HoverBack => Color.FromArgb(232, 232, 238);
        public override Color HoverBorder => Color.FromArgb(180, 180, 190);

        public override Color Accent => Color.FromArgb(0, 120, 215);
    }
}
