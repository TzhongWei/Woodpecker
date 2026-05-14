using System.Drawing;

public static class AttributeColourUtil
{
    public static Color WhiteOverlay(Color original, double ratio)
        {
            Color white = Color.White;
            return Color.FromArgb(255,
                (int)(ratio * white.R + (1 - ratio) * original.R),
                (int)(ratio * white.G + (1 - ratio) * original.G),
                (int)(ratio * white.B + (1 - ratio) * original.B));
        }
        public static Color Overlay(Color original, Color overlay, double ratio)
        {
            return Color.FromArgb(255,
                (int)(ratio * overlay.R + (1 - ratio) * original.R),
                (int)(ratio * overlay.G + (1 - ratio) * original.G),
                (int)(ratio * overlay.B + (1 - ratio) * original.B));
        }
}