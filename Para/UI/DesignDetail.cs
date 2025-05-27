using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Para.UI
{
    /// <summary>
    /// Default Color, Mesurement and Animation details for UI controls.
    /// </summary>
    public static class DesignDetail
    {
        public static class Control
        {
            public static class Caret
            {
                public static readonly Brush CaretBrushHigh = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                public static readonly Brush CaretBrushLow = new SolidColorBrush(Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF));
                public static readonly double Width = 3;
            }
            public static class TextBox
            {
                public static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
                public static readonly Brush ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                public static readonly double CaretWidth = 4;
                public static readonly Thickness Padding = new Thickness(8, 4, 8, 4);
            }
        }

        public static class Text
        {
            public static readonly FontFamily DefaultFontFamily = new FontFamily("Segoe UI");
            public static class SpriteText
            {
                public static readonly bool HasShadow = true;
                public static readonly Color ShadowColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
                public static readonly Vector2 ShadowOffset = new(1, 1);
                public static readonly Color Foreground = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                public static readonly double CreateInterval = 0.3;
                public static readonly double DestroyInterval = 0.3;
                public static readonly double DropEnd = 48;
            }
        }
    }
}
