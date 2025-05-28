using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Para.UI.Control
{
    public class ScrollBar : System.Windows.Controls.Control
    {
        // Public settable properties
        public Brush NormalColor = DesignDetail.Control.ScrollBar.BackgroundBrushNormal;
        public Brush HoverColor = DesignDetail.Control.ScrollBar.BackgroundBrushHover;
        public Brush PressedColor = DesignDetail.Control.ScrollBar.BackgroundBrushPressed;
        private bool _isMouseOver = false;
        private bool _isPressed = false;
        private DateTime _lastAnimationStartTile = DateTime.Now;
        private double _animationProgress;
        private Color _startColor;
        private Color _targetColor;

        static ScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ScrollBar),
                new FrameworkPropertyMetadata(typeof(ScrollBar)));
        }

        public ScrollBar()
        {
            Loaded += ScrollBar_Loaded;
            MouseEnter += ScrollBar_MouseEnter;
            MouseLeave += ScrollBar_MouseLeave;
            PreviewMouseLeftButtonDown += ScrollBar_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += ScrollBar_PreviewMouseLeftButtonUp;
        }


        private void ScrollBar_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Background = NormalColor;
        }
        private void ScrollBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseOver = true;
            if (!_isPressed)
            {
                AnimateBackgroundTo(HoverColor);
            }
        }
        private void ScrollBar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseOver = false;
            if (!_isPressed)
            {
                AnimateBackgroundTo(NormalColor);
            }
        }

        private void ScrollBar_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isPressed = true;
            AnimateBackgroundTo(PressedColor);
        }
        private void ScrollBar_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isPressed = false;
            AnimateBackgroundTo(_isMouseOver ? HoverColor : NormalColor);
        }

        protected Color BrushToColor(Brush brush)
        {
            if (brush == null)
            {
                return Colors.Transparent;
            }

            if (brush is SolidColorBrush solidBrush)
                return solidBrush.Color;

            if (brush is GradientBrush gradientBrush && gradientBrush.GradientStops.Count > 0)
                return gradientBrush.GradientStops[0].Color;

            return Colors.Red;
        }

        public void OnRendering(object? sender, EventArgs e)
        {
            var elapsed = (DateTime.Now - _lastAnimationStartTile).TotalSeconds;
            _animationProgress = Math.Min(1.0, elapsed / DesignDetail.Control.ScrollBar.BackgroundBrushChangeInterval);
            Background = new SolidColorBrush(LerpColor(_startColor, _targetColor, _animationProgress));
            if (_animationProgress >= 1.0)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
        }

        private void AnimateBackgroundTo(Brush targerBrush)
        {
            _lastAnimationStartTile = DateTime.Now;
            _startColor = BrushToColor(Background);
            _targetColor = BrushToColor(targerBrush);
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }

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
