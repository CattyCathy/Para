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
                //Brush
                public static readonly Brush CaretBrushHigh = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                public static readonly Brush CaretBrushLow = new SolidColorBrush(Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF));

                //Size
                public static readonly double Width = 3;
            }
            public static class TextBox
            {
                //Brush
                public static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
                public static readonly Brush ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                public static readonly Brush SelectionCaretBrush = new SolidColorBrush(Color.FromArgb(0x88, 0x33, 0x99, 0xFF));

                //Size
                public static readonly double CaretWidth = 4;
                public static readonly Thickness Padding = new(8, 4, 8, 4);

                //Animation
                public static readonly double CaretMovementInterval = 0.1;
            }
            public static class ScrollBar
            {
                //Brush
                public static readonly Brush BackgroundBrushNormal = new SolidColorBrush(Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF));
                public static readonly Brush BackgroundBrushHover = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                public static readonly Brush BackgroundBrushPressed = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));

                //Size
                /// <summary>
                /// Attention: when scroll bar is a horizontal bar, the width is the height of the bar.
                /// </summary>
                public static readonly double ScrollBarWidth = 8;
                public static readonly double ScrollBarMinLength = 16;

                //Animation
                public static readonly double BackgroundBrushChangeInterval = 0.3;
            }
            public static class ScrollBarcontainer
            {

            }
        }

        public static class Text
        {
            public static readonly FontFamily DefaultFontFamily = new("Segoe UI");
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
