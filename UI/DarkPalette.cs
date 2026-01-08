namespace GWxLauncher.UI
{
    internal sealed class DarkPalette : ThemePalette
    {
        // Darker background creates depth behind the cards
        public override Color WindowBack => Color.FromArgb(18, 18, 20);
        public override Color WindowFore => Color.FromArgb(220, 220, 225);

        public override Color SurfaceBack => Color.FromArgb(28, 28, 32);
        public override Color SurfaceBorder => Color.FromArgb(45, 45, 50);

        public override Color HeaderBack => Color.FromArgb(28, 28, 32);
        public override Color Separator => Color.FromArgb(40, 40, 45);

        public override Color ButtonBack => Color.FromArgb(45, 45, 55);
        public override Color ButtonBorder => Color.FromArgb(70, 70, 85);
        public override Color ButtonFore => Color.White;

        public override Color PrimaryButtonBack => Color.FromArgb(0, 120, 215);
        public override Color PrimaryButtonBorder => Color.FromArgb(0, 100, 180);
        public override Color PrimaryButtonFore => Color.White;

        public override Color DangerButtonBack => Color.FromArgb(200, 50, 50);
        public override Color DangerButtonBorder => Color.FromArgb(170, 40, 40);
        public override Color DangerButtonFore => Color.White;

        public override Color InputBack => Color.FromArgb(12, 12, 15);
        public override Color InputFore => Color.White;
        public override Color InputBorder => Color.FromArgb(60, 60, 75);

        public override Color SubtleFore => Color.FromArgb(150, 150, 165);
        public override Color DisabledFore => Color.FromArgb(90, 90, 100);

        // Lighter cards on darker window = High Definition
        public override Color CardBack => Color.FromArgb(32, 32, 38);
        public override Color CardSelectedBack => Color.FromArgb(40, 40, 50);

        public override Color CardBorder => Color.FromArgb(55, 55, 65);
        public override Color CardSelectedBorder => Color.FromArgb(0, 120, 215);

        public override Color CardNameFore => Color.White;
        public override Color CardSubFore => Color.FromArgb(170, 170, 185);

        public override Color BadgeBack => Color.FromArgb(50, 50, 65);
        public override Color BadgeBorder => Color.FromArgb(75, 75, 95);
        public override Color BadgeFore => Color.FromArgb(210, 210, 220);

        public override Color HoverBack => Color.FromArgb(42, 42, 52);
        public override Color HoverBorder => Color.FromArgb(85, 85, 110);

        public override Color Accent => Color.FromArgb(0, 120, 215);
        public override Color AccentHover => Color.FromArgb(0, 120, 215, 150);
    }
}
