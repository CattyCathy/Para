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
    public class ScrollBarThumb : System.Windows.Controls.Primitives.Thumb
    {
        // Local values for public properties

        // Runtime values
        protected bool _isMouseOver = false;
        protected bool _isPressed = false;
        protected DateTime _lastAnimationStartTile = DateTime.Now;
        protected double _animationProgress;
        protected Color _startColor;
        protected Color _targetColor;


        // Public settable properties
        public Brush NormalColor = DesignDetail.Control.ScrollBarDraggableItem.BackgroundBrushNormal;
        public Brush HoverColor = DesignDetail.Control.ScrollBarDraggableItem.BackgroundBrushHover;
        public Brush PressedColor = DesignDetail.Control.ScrollBarDraggableItem.BackgroundBrushPressed;
        public double MinLength = DesignDetail.Control.ScrollBarDraggableItem.ScrollBarMinLength;
        public double Length
        {
            get
            {
                if (IsHorizontal)
                {
                    return base.Width;
                }
                else
                {
                    return base.Height;
                }
            }
            set
            {
                if (IsHorizontal)
                {
                    base.Width = Math.Max(MinLength, value);
                }
                else
                {
                    base.Height = Math.Max(MinLength, value);
                }
            }
        }
        public new double Width
        {
            get
            {
                if (IsHorizontal)
                {
                    return base.Height;
                }
                else
                {
                    return base.Width;
                }
            }
            set
            {
                if (IsHorizontal)
                {
                    base.Height = value;
                }
                else
                {
                    base.Width = value;
                }
            }
        }

        public static readonly DependencyProperty IsHorizontalProperty =
    DependencyProperty.Register(
        nameof(IsHorizontal),
        typeof(bool),
        typeof(ScrollBarThumb),
        new PropertyMetadata(false));

        public bool IsHorizontal
        {
            get => (bool)GetValue(IsHorizontalProperty);
            set => SetValue(IsHorizontalProperty, value);
        }


        static ScrollBarThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ScrollBarThumb),
                new FrameworkPropertyMetadata(typeof(ScrollBarThumb)));
        }

        public ScrollBarThumb()
        {
            Loaded += ScrollBar_Loaded;
            MouseEnter += ScrollBar_MouseEnter;
            MouseLeave += ScrollBar_MouseLeave;
            PreviewMouseLeftButtonDown += ScrollBar_PreviewMouseLeftButtonDown;
            PreviewMouseLeftButtonUp += ScrollBar_PreviewMouseLeftButtonUp;
        }

        protected void ScrollBar_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Background = NormalColor;
        }


        // Events

        // Events: Mouse
        protected void ScrollBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseOver = true;
            if (!_isPressed)
            {
                AnimateBackgroundTo(HoverColor);
            }
        }
        protected void ScrollBar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseOver = false;
            if (!_isPressed)
            {
                AnimateBackgroundTo(NormalColor);
            }
        }
        protected void ScrollBar_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isPressed = true;
            AnimateBackgroundTo(PressedColor);
        }
        protected void ScrollBar_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isPressed = false;
            AnimateBackgroundTo(_isMouseOver ? HoverColor : NormalColor);
        }



        // Methods

        // Methods: Animation
        protected void AnimateBackgroundTo(Brush targerBrush)
        {
            _lastAnimationStartTile = DateTime.Now;
            _startColor = BrushToColor(Background);
            _targetColor = BrushToColor(targerBrush);
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }
        // Methods： Helper
        protected static Color BrushToColor(Brush brush)
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
            _animationProgress = Math.Min(1.0, elapsed / DesignDetail.Control.ScrollBarDraggableItem.BackgroundBrushChangeInterval);
            Background = new SolidColorBrush(LerpColor(_startColor, _targetColor, _animationProgress));
            if (_animationProgress >= 1.0)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
        }
        protected static Color LerpColor(Color from, Color to, double t)
        {
            byte a = (byte)(from.A + (to.A - from.A) * t);
            byte r = (byte)(from.R + (to.R - from.R) * t);
            byte g = (byte)(from.G + (to.G - from.G) * t);
            byte b = (byte)(from.B + (to.B - from.B) * t);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
