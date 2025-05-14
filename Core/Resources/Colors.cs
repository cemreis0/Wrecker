using System.Drawing;

namespace Core.Resources
{
    public static class Colors
    {
        public static Color PrimaryLight => ColorTranslator.FromHtml("#4CAF50");
        public static Color PrimaryDark => ColorTranslator.FromHtml("#388E3C");
        public static Color SecondaryLight => ColorTranslator.FromHtml("#FFC107");
        public static Color SecondaryDark => ColorTranslator.FromHtml("#FFA000");

        public static Color BackgroundLight => ColorTranslator.FromHtml("#f0f0f0");
        public static Color BackgroundDark => ColorTranslator.FromHtml("#1e1e1e");

        public static Color TextLight => ColorTranslator.FromHtml("#212121");
        public static Color TextDark => ColorTranslator.FromHtml("#E0E0E0");
    }
}
