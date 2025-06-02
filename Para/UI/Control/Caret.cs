using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Para.UI.Control
{
    /// <summary>
    /// Rhythm powered caret!
    /// </summary>
    public class Caret : BeatSyncedControl
    {
        // Runtime values
        protected double _beatProgress;
        protected double _interval;
        protected DateTime _lastBeatTime = DateTime.Now;

        // Public settable properties
        public Brush CaretBrushHigh { get; set; } = DesignDetail.Control.Caret.CaretBrushHigh;
        public Brush CaretBrushLow { get; set; } = DesignDetail.Control.Caret.CaretBrushLow;

        static Caret()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Caret),
                new FrameworkPropertyMetadata(typeof(Caret)));
        }

        public Caret()
        {
            IsHitTestVisible = false;
            Focusable = false;
            Loaded += Caret_Loaded;
            Unloaded += (s, e) => CompositionTarget.Rendering -= OnRendering;
        }

        private void Caret_Loaded(object sender, RoutedEventArgs e)
        {
            Height = (double)((Parent as System.Windows.Controls.Control ?? this).FontSize)+2;
            Width = DesignDetail.Control.Caret.Width;
        }


        // Events

        // Events: Rendering
        private void OnRendering(object? sender, EventArgs e)  
        {
            double elapsed = (DateTime.Now - _lastBeatTime).TotalSeconds;
            _beatProgress = Math.Min(1.0, elapsed / _interval);

            if (CaretBrushHigh is SolidColorBrush high && CaretBrushLow is SolidColorBrush low)
            {
                var color = LerpColor(high.Color, low.Color, _beatProgress);
                this.Background = new SolidColorBrush(color);
            }

            if (_beatProgress >= 1.0)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
        }

        //Events: Beat
        public override void OnBeat(double interval)
        {
            // Run on UI thread, or it will be useless  
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnBeat(interval));
                return;
            }
            base.OnBeat(interval);

            _interval = interval;
            _beatProgress = 0.0;
            _lastBeatTime = DateTime.Now;
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }

        // Methods

        // Methods: Helper
        private static Color LerpColor(Color from, Color to, double t)
        {
            byte a = (byte)(from.A + (to.A - from.A) * t);
            byte r = (byte)(from.R + (to.R - from.R) * t);
            byte g = (byte)(from.G + (to.G - from.G) * t);
            byte b = (byte)(from.B + (to.B - from.B) * t);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
