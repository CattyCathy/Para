using Para.UI.Control;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Para.UI.Container
{
    public class ScrollView : ContentControl
    {
        // Members
        private ScrollBar? _verticalScrollBar;
        private ContentPresenter? _contentPresenter;
        private Border? _viewport;
        private Grid? _contentRoot;


        // Runtime values
        private Point? _dragStartPoint;
        private double _dragStartOffset;
        private bool _isDragging = false;
        private bool _isInertiaAnimating = false;
        private readonly Queue<(double value, DateTime time)> _velocitySamples = new();
        private double _inertiaVelocity = 0;

        // Public settable properties
        public double SmallChange { get; set; } = DesignDetail.Container.ScrollView.SmallChange; // px, the amount of change when the user clicks the track bar
        public double InertiaFriction { get; set; } = DesignDetail.Container.ScrollView.InertiaFriction; // Lower value for faster slowdown, should be between 0 and 1
        public double InertiaMinVelocity { get; set; } = DesignDetail.Container.ScrollView.InertiaMinVelocity; // px/s, will stop when the speed is less than it
        public int VelocitySampleCount { get; set; } = DesignDetail.Container.ScrollView.VelocitySampleCount;
        public double InertiaAnimationCancelInterval { get; set; } = DesignDetail.Container.ScrollView.InertiaAnimationCancelInterval;

        static ScrollView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollView), new FrameworkPropertyMetadata(typeof(ScrollView)));
        }

        public ScrollView()
        {
            SizeChanged += ScrollView_SizeChanged1;
        }

        private void ScrollView_SizeChanged1(object sender, SizeChangedEventArgs e)
        {
            if (_contentPresenter != null)
            {
                _contentPresenter.Width = e.NewSize.Width;
                _contentPresenter.Height = e.NewSize.Height;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _verticalScrollBar = GetTemplateChild("PART_VerticalScrollBar") as ScrollBar;
            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            _viewport = GetTemplateChild("PART_Viewport") as Border;
            _contentRoot = GetTemplateChild("PART_ContentRoot") as Grid;
                    
            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
                _verticalScrollBar.MouseLeftButtonDown += _verticalScrollBar_MouseLeftButtonDown;
                _verticalScrollBar.PreviewThumbLeftButtonDown += _verticalScrollBar_ThumbLeftButtonDown;
            }
            if (_contentPresenter != null)
                _contentPresenter.SizeChanged += ContentPresenter_SizeChanged;
            if (_contentRoot != null)
            {
                _contentRoot.PreviewMouseWheel += OnContentAreaMouseWheel;
                _contentRoot.PreviewMouseLeftButtonDown += OnContentAreaMouseLeftButtonDown;
                _contentRoot.PreviewMouseLeftButtonUp += OnContentAreaMouseLeftButtonUp;
                _contentRoot.PreviewMouseMove += OnContentAreaMouseMove;
            }
            this.SizeChanged += ScrollView_SizeChanged;

            UpdateScrollBar();
        }


        // Events

        // Events: Mouse button and wheel
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, null);
            }
        }
        private void _verticalScrollBar_ThumbLeftButtonDown(object? sender, EventArgs e)
        {
            if(_verticalScrollBar!= null)
                _verticalScrollBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, null);
        }
        private void _verticalScrollBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_verticalScrollBar != null)
                _verticalScrollBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, null);
        }
        private void OnContentAreaMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_verticalScrollBar != null && _verticalScrollBar.Visibility == Visibility.Visible)
            {
                _dragStartPoint = e.GetPosition(_contentRoot);
                _dragStartOffset = _verticalScrollBar.Value;
                _isDragging = true;
                _verticalScrollBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, null);// Stop animation
                _contentRoot?.CaptureMouse();
            }
        }
        private void OnContentAreaMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDragging && _verticalScrollBar != null)
            {
                _isDragging = false;
                _dragStartPoint = null;
                _contentRoot?.ReleaseMouseCapture();

                // Calculate average speed(px/s)
                double velocity = 0;
                DateTime lastSampleTime = DateTime.MinValue;
                if (_velocitySamples.Count >= 2)
                {
                    var samples = _velocitySamples.ToArray();
                    int frameCount = samples.Length - 1;
                    if (frameCount > 0)
                    {
                        velocity = (samples[^1].value - samples[0].value) / frameCount;
                    }

                    lastSampleTime = samples[^2].time;
                }
                _velocitySamples.Clear();

                if (DateTime.Now - lastSampleTime > TimeSpan.FromSeconds(InertiaAnimationCancelInterval))
                {
                    return;
                }

                _inertiaVelocity = velocity;
                if (System.Math.Abs(_inertiaVelocity) > InertiaMinVelocity)
                {
                    if (!_isInertiaAnimating)
                    {
                        _isInertiaAnimating = true;
                        CompositionTarget.Rendering += OnInertiaRendering;
                    }
                }
            }
        }
        private void ContentPresenter_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_verticalScrollBar != null && _verticalScrollBar.Visibility == Visibility.Visible)
            {
                _dragStartPoint = e.GetPosition(_contentPresenter);
                _dragStartOffset = _verticalScrollBar.Value;
                _contentPresenter?.CaptureMouse();
                e.Handled = true;
            }
        }
        private void ContentPresenter_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_dragStartPoint.HasValue && _verticalScrollBar != null && _verticalScrollBar.Visibility == Visibility.Visible)
            {
                var current = e.GetPosition(_contentPresenter);
                double delta = current.Y - _dragStartPoint.Value.Y;
                double newValue = _dragStartOffset - delta;
                _verticalScrollBar.Value = Math.Max(_verticalScrollBar.Minimum, Math.Min(_verticalScrollBar.Maximum, newValue));
                e.Handled = true;
            }
        }
        private void ContentPresenter_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_dragStartPoint.HasValue)
            {
                _dragStartPoint = null;
                _contentPresenter?.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
        private void OnContentAreaMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging && _dragStartPoint.HasValue && _verticalScrollBar != null && _verticalScrollBar.Visibility == Visibility.Visible)
            {
                var current = e.GetPosition(_contentRoot);
                double delta = current.Y - _dragStartPoint.Value.Y;
                double newValue = _dragStartOffset - delta;
                double clampedValue = Math.Max(_verticalScrollBar.Minimum, Math.Min(_verticalScrollBar.Maximum, newValue));
                _verticalScrollBar.Value = clampedValue;

                // Log value
                var now = DateTime.Now;
                _velocitySamples.Enqueue((clampedValue, now));
                while (_velocitySamples.Count > VelocitySampleCount)
                    _velocitySamples.Dequeue();
            }
        }
        private void OnContentAreaMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (_verticalScrollBar == null || _verticalScrollBar.Visibility != Visibility.Visible)
                return;

            double delta = e.Delta > 0 ? -_verticalScrollBar.SmallChange : _verticalScrollBar.SmallChange;
            double newValue = _verticalScrollBar.Value + delta;
            AnimateScrollTo(newValue, 200);
        }

        // Events: Size changed
        private void ScrollView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateScrollBar();
        }
        private void ContentPresenter_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateScrollBar();
        }

        // Events: Value changed
        private void VerticalScrollBar_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_contentPresenter?.Content is UIElement element)
            {
                element.RenderTransform = new TranslateTransform(0, -e.NewValue);
            }
        }


        // Methods

        // Methods: Visual
        private void AnimateScrollTo(double targetValue, int durationMs)
        {
            if (_verticalScrollBar == null) return;

            double to = Math.Max(_verticalScrollBar.Minimum, Math.Min(_verticalScrollBar.Maximum, targetValue));
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            animation.Completed += (s, e) =>
            {
                _verticalScrollBar.Value = to;
            };
            _verticalScrollBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);
        }
        private void UpdateScrollBar()
        {
            if (_verticalScrollBar == null || _contentPresenter == null || _viewport == null)
                return;

            if (_contentPresenter.Content is FrameworkElement content)
            {
                content.Measure(new Size(_viewport.ActualWidth, double.PositiveInfinity));
                double contentHeight = content.DesiredSize.Height;
                double viewportHeight = _viewport.ActualHeight;

                // Set the height ContentPresenter same with the content height
                _contentPresenter.Height = contentHeight;

                if (contentHeight > viewportHeight)
                {
                    _verticalScrollBar.Visibility = Visibility.Visible;
                    _verticalScrollBar.Minimum = 0;
                    _verticalScrollBar.Maximum = contentHeight - viewportHeight;
                    _verticalScrollBar.ViewportSize = viewportHeight;
                    _verticalScrollBar.SmallChange = SmallChange;
                    _verticalScrollBar.LargeChange = viewportHeight;
                }
                else
                {
                    _verticalScrollBar.Visibility = Visibility.Collapsed;
                    _verticalScrollBar.Value = 0;
                }
            }
            else
            {
                _verticalScrollBar.Visibility = Visibility.Collapsed;
                _verticalScrollBar.Value = 0;
                if (_contentPresenter != null)
                    _contentPresenter.Height = double.NaN;
            }
        }
        private void OnInertiaRendering(object? sender, System.EventArgs e)
        {
            if (_verticalScrollBar == null)
                return;

            double value = _verticalScrollBar.Value + _inertiaVelocity;
            if (value < _verticalScrollBar.Minimum)
            {
                value = _verticalScrollBar.Minimum;
                _inertiaVelocity = 0;
            }
            else if (value > _verticalScrollBar.Maximum)
            {
                value = _verticalScrollBar.Maximum;
                _inertiaVelocity = 0;
            }
            _verticalScrollBar.Value = value;

            _inertiaVelocity *= InertiaFriction;

            if (System.Math.Abs(_inertiaVelocity) < InertiaMinVelocity)
            {
                _isInertiaAnimating = false;
                CompositionTarget.Rendering -= OnInertiaRendering;
            }
        }
    }
}