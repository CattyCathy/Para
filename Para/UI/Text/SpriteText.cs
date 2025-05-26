using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Para.UI.Text
{
    public class SpriteText : BeatSyncedControl
    {
        public bool HasShadow;
        public System.Windows.Media.Color ShadowColor;
        public Vector2 ShadowOffset;
        public char Char = ' ';

        private double _beatProgress;
        private double _interval;
        private DateTime _animationStartTime = DateTime.Now;
        private Brush? _startBrush;
        private Brush? _endBrush;

        static SpriteText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SpriteText),
                new FrameworkPropertyMetadata(typeof(SpriteText)));
        }

        public SpriteText()
        {
            Loaded += SpriteText_Loaded;
            Unloaded += SpriteText_Unloaded;
        }

        private void SpriteText_Loaded(object sender, RoutedEventArgs e)
        {
            _animationStartTime = DateTime.Now;
            _interval = DesignDetail.Text.SpriteText.CreateInterval;
            _startBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            _endBrush = Foreground;
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SpriteText_Loaded(sender, e));
                return;
            }
        }

        private void SpriteText_Unloaded(object sender, RoutedEventArgs e)
        {
            _animationStartTime = DateTime.Now;
            _interval = DesignDetail.Text.SpriteText.DestroyInterval;
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SpriteText_Unloaded(sender, e));
                return;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var elapsed = (DateTime.Now - _animationStartTime).TotalSeconds;
            _beatProgress = Math.Min(1.0, elapsed / _interval);
            _startBrush = Foreground;
            _endBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
            if (_startBrush is SolidColorBrush start && _endBrush is SolidColorBrush end)
            {
                var color = LerpColor(start.Color, end.Color, _beatProgress);
                this.Background = new SolidColorBrush(color);
            }

            if (_beatProgress >= 1.0)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
        }

        private static System.Windows.Media.Color LerpColor(System.Windows.Media.Color from, System.Windows.Media.Color to, double t)
        {
            byte a = (byte)(from.A + (to.A - from.A) * t);
            byte r = (byte)(from.R + (to.R - from.R) * t);
            byte g = (byte)(from.G + (to.G - from.G) * t);
            byte b = (byte)(from.B + (to.B - from.B) * t);
            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }
    }
}
