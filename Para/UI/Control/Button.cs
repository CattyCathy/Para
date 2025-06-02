using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Para.UI.Control
{
    public class Button : System.Windows.Controls.ContentControl
    {
        // Members
        private Grid? _contentRoot;
        private ContentPresenter? _contentPresenter;
        private Viewbox? _viewbox;

        // Runtime values
        private bool _pressed = false;
        private double _currentPressScaleProgress = 0;
        private double _viewboxWidth;
        private double _viewboxHeight;
        private double _scaledRate = 0;
        private double _scaleStartValue;
        private DateTime _animationStartTime = DateTime.Now;


        // Public settable values
        public ClickMode ClickMode { get; set; } = ClickMode.Release;
        public double PressScaleProgressFinishTimeSpan = 1;
        public double PressScaleMaximumDecraseRate = 0.2;

        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new System.Windows.FrameworkPropertyMetadata(typeof(Button)));
        }

        public Button()
        {
            MouseEnter += Button_MouseEnter;
            MouseLeave += Button_MouseLeave;
            MouseLeftButtonDown += Button_MouseLeftButtonDown;
            MouseLeftButtonUp += Button_MouseLeftButtonUp;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentRoot = GetTemplateChild("PART_ContentRoot") as Grid;
            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
            _viewbox = GetTemplateChild("PART_Viewbox") as Viewbox;
            if (_contentRoot != null)
            {
                _contentRoot.Height = ActualHeight;
                _contentRoot.Width = ActualWidth;
            }
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ClickMode == ClickMode.Hover)
            {
                OnClick(this, e);
            }
        }
        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void Button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Button_MouseLeftButtonDown(sender, e));
                return;
            }
            _pressed = true;
            _animationStartTime = DateTime.Now;
            if (_viewbox != null)
            {
                _scaleStartValue = (Height - _viewbox.Height) / (Height * PressScaleMaximumDecraseRate);
            }
            if (ClickMode == ClickMode.Press)
            {
                OnClick(this, e);
            }
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }

        private void Button_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Button_MouseLeftButtonDown(sender, e));
                return;
            }
            _pressed = false;
            _animationStartTime = DateTime.Now;
            if (_viewbox != null)
            {
                _scaleStartValue = (Height - _viewbox.Height) / (Height * PressScaleMaximumDecraseRate);
            }
            if (ClickMode == ClickMode.Release && IsMouseCaptured && _pressed)
            {
                OnClick(this, e);
            }
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }


        public virtual void OnClick(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void OnRendering(object? sender, System.EventArgs eventArgs)
        {
            double elapsed = (DateTime.Now - _animationStartTime).TotalSeconds;
            if(elapsed > PressScaleProgressFinishTimeSpan)
            {
                elapsed = PressScaleProgressFinishTimeSpan;
            }
            if (_pressed)
            {
                QuarticEase ease = new QuarticEase { EasingMode = EasingMode.EaseOut };
                if (_viewbox != null)
                {
                    double scaleValue = ease.Ease(elapsed / PressScaleProgressFinishTimeSpan);
                    _viewbox.Height = Height - (Height * PressScaleMaximumDecraseRate * scaleValue);
                    _viewbox.Width = Width - (Width * PressScaleMaximumDecraseRate * scaleValue);
                }
            }
            else
            {
                ElasticEase ease = new ElasticEase { EasingMode = EasingMode.EaseOut };
                if (_viewbox != null)
                {
                    double scaleValue = ease.Ease(elapsed / PressScaleProgressFinishTimeSpan);
                    _viewbox.Height = Height - (Height * PressScaleMaximumDecraseRate * _scaleStartValue * (1- scaleValue));
                    _viewbox.Width = Width - (Width * PressScaleMaximumDecraseRate * _scaleStartValue * (1 - scaleValue));
                }
            }

            if (_viewbox != null)
            {
                _scaledRate = (Height - _viewbox.Height) / (Height * PressScaleMaximumDecraseRate);
            }
        }
    }
}
