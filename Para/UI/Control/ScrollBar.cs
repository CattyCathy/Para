using Para.UI.Container;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static Para.UI.DesignDetail.Control;

namespace Para.UI.Control
{
    public class ScrollBar : System.Windows.Controls.Primitives.ScrollBar
    {
        // Members
        private ScrollBarThumb? _thumb;
        private FrameworkElement? _track;

        // Local values for public properties
        private double _thumbWidth = DesignDetail.Control.ScrollBar.ThumbWidth;
        private double _trackWidth = DesignDetail.Control.ScrollBar.TrackWidth;

        // Event Properties
        public event EventHandler? PreviewThumbLeftButtonDown;



        // Public settable properties
        public double ThumbWidth
        {
            get => _thumbWidth;
            set
            {
                if (_thumb != null)
                {
                    if (Orientation == Orientation.Vertical)
                    {
                        _thumb.Width = value;
                    }
                    else
                    {
                        _thumb.Height = value;
                    }
                }
            }
        }

        public double TrackWidth
        {
            get => _trackWidth;
            set
            {
                if (this != null)
                {
                    if (Orientation == Orientation.Vertical)
                    {
                        Width = value;
                    }
                    else
                    {
                        Height = value;
                    }
                }
            }
        }

        static ScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(typeof(ScrollBar)));
        }

        public ScrollBar()
        {
            
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _thumb = GetTemplateChild("PART_Thumb") as ScrollBarThumb;
            _track = GetTemplateChild("PART_Track") as FrameworkElement;

            if (_thumb != null)
            {
                _thumb.DragDelta += Thumb_DragDelta;
                _thumb.PreviewMouseLeftButtonDown += _thumb_PreviewMouseLeftButtonDown;
                _thumb.Width = ThumbWidth;
                Width = TrackWidth;
            }

            UpdateThumb();
        }


        // Events

        // Events: Thumb drag
        /// <summary>
        /// Update thumb value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_thumb == null || _track == null) return;

            double trackLength = Orientation == Orientation.Vertical ? _track.ActualHeight : _track.ActualWidth;
            double thumbLength = Orientation == Orientation.Vertical ? _thumb.ActualHeight : _thumb.ActualWidth;
            double range = Maximum - Minimum;
            double maxOffset = trackLength - thumbLength;

            if (maxOffset <= 0) return;

            double offset = Orientation == Orientation.Vertical
                ? Canvas.GetTop(_thumb) + e.VerticalChange
                : Canvas.GetLeft(_thumb) + e.HorizontalChange;

            offset = Math.Max(0, Math.Min(maxOffset, offset));
            double newValue = Minimum + (offset / maxOffset) * range;
            Value = Math.Max(Minimum, Math.Min(Maximum, newValue));
        }


        // Methods

        // Mehtods: Visual
        /// <summary>
        /// Update the thumb position and size based on the current value, minimum, maximum, and viewport size.
        /// </summary>
        private void UpdateThumb()
        {
            if (_thumb == null || _track == null) return;

            double range = Maximum - Minimum;
            double viewport = ViewportSize;
            double trackLength = Orientation == Orientation.Vertical ? _track.ActualHeight : _track.ActualWidth;

            if (range <= 0 || trackLength <= 0 || viewport <= 0)
            {
                _thumb.Visibility = Visibility.Collapsed;
                return;
            }
            _thumb.Visibility = Visibility.Visible;

            double thumbLength = Math.Max( Math.Max(20, trackLength * viewport / (range + viewport)),DesignDetail.Control.ScrollBar.MinimumLength);
            if (Orientation == Orientation.Vertical)
            {
                _thumb.Height = thumbLength;
                double maxOffset = trackLength - thumbLength;
                double offset = maxOffset * (Value - Minimum) / (range == 0 ? 1 : range);
                Canvas.SetTop(_thumb, offset);
            }
            else
            {
                _thumb.Width = thumbLength;
                double maxOffset = trackLength - thumbLength;
                double offset = maxOffset * (Value - Minimum) / (range == 0 ? 1 : range);
                Canvas.SetLeft(_thumb, offset);
            }
        }

        // Methods: Mouse button
        public virtual void _thumb_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PreviewThumbLeftButtonDown?.Invoke(sender, e);
        }

        // Methods: Size changed
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateThumb();
        }

        // Methods: Value changed
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            UpdateThumb();
        }
    }
}