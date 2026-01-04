namespace GWxLauncher.UI
{
    internal sealed class LightPalette : ThemePalette
    {
        public override Color WindowBack => Color.FromArgb(235, 235, 240);
        public override Color WindowFore => Color.FromArgb(30, 30, 35);

        public override Color SurfaceBack => Color.FromArgb(245, 245, 250);
        public override Color SurfaceBorder => Color.FromArgb(200, 200, 215);

        public override Color HeaderBack => Color.FromArgb(245, 245, 250);
        public override Color Separator => Color.FromArgb(190, 190, 205);

        public override Color ButtonBack => Color.FromArgb(225, 225, 235);
        public override Color ButtonBorder => Color.FromArgb(180, 180, 200);
        public override Color ButtonFore => Color.FromArgb(45, 45, 55);

        public override Color PrimaryButtonBack => Color.FromArgb(0, 105, 210);
        public override Color PrimaryButtonBorder => Color.FromArgb(0, 85, 180);
        public override Color PrimaryButtonFore => Color.White;

        public override Color DangerButtonBack => Color.FromArgb(210, 45, 45);
        public override Color DangerButtonBorder => Color.FromArgb(180, 35, 35);
        public override Color DangerButtonFore => Color.White;

        public override Color InputBack => Color.White;
        public override Color InputFore => Color.Black;
        public override Color InputBorder => Color.FromArgb(170, 170, 190);

        public override Color SubtleFore => Color.FromArgb(90, 95, 115);
        public override Color DisabledFore => Color.FromArgb(160, 160, 175);

        // White cards on soft gray window = High Contrast
        public override Color CardBack => Color.White;
        public override Color CardSelectedBack => Color.FromArgb(235, 242, 255);

        public override Color CardBorder => Color.FromArgb(185, 185, 205);
        public override Color CardSelectedBorder => Color.FromArgb(0, 105, 210);

        public override Color CardNameFore => Color.Black;
        public override Color CardSubFore => Color.FromArgb(85, 85, 105);

        public override Color BadgeBack => Color.FromArgb(230, 235, 245);
        public override Color BadgeBorder => Color.FromArgb(200, 205, 220);
        public override Color BadgeFore => Color.FromArgb(60, 65, 85);

        public override Color HoverBack => Color.FromArgb(248, 250, 255);
        public override Color HoverBorder => Color.FromArgb(160, 175, 210);

        public override Color Accent => Color.FromArgb(0, 105, 210);
        public override Color AccentHover => Color.FromArgb(0, 105, 210, 120);
    }
}