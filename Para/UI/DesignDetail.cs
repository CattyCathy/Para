using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Para.UI
{
    public static class DesignDetail
    {
        public static class Control
        {
            public static class Caret
            {
                public static Brush CaretBrushHigh = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                public static Brush CaretBrushLow = new SolidColorBrush(Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF));
            }
        }
    }
}
