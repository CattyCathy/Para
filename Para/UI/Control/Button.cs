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
        private Canvas? _contentRoot;
        private Border? _animationLayer;
        private Viewbox? _viewbox;

        // Runtime values
        private bool _pressed = false;
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
            Loaded += Button_Loaded;
            MouseEnter += Button_MouseEnter;
            MouseLeave += Button_MouseLeave;
            MouseLeftButtonDown += Button_MouseLeftButtonDown;
            MouseLeftButtonUp += Button_MouseLeftButtonUp;
        }

        private void Button_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_animationLayer != null && _contentRoot != null)
            {
                _animationLayer.Height = _contentRoot.ActualHeight;
                _animationLayer.Width = _contentRoot.ActualWidth;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentRoot = GetTemplateChild("PART_ContentRoot") as Canvas;
            _animationLayer = GetTemplateChild("PART_AnimationLayer") as Border;
            _viewbox = GetTemplateChild("PART_Viewbox") as Viewbox;
            if (_contentRoot != null)
            {
                //_contentRoot.Height = ActualHeight;
                //_contentRoot.Width = ActualWidth;
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
            if (_animationLayer != null && _contentRoot!= null)
            {
                _scaleStartValue = (_contentRoot.ActualHeight - _animationLayer.Height) / (_contentRoot.ActualHeight * PressScaleMaximumDecraseRate);
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
            if (_animationLayer != null && _contentRoot != null)
            {
                _scaleStartValue = (_contentRoot.ActualHeight - _animationLayer.Height) / (_contentRoot.ActualHeight * PressScaleMaximumDecraseRate);
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
                if (_animationLayer != null && _contentRoot != null)
                {
                    double scaleValue = ease.Ease(elapsed / PressScaleProgressFinishTimeSpan);
                    _animationLayer.Height = _contentRoot.ActualHeight - (_contentRoot.ActualHeight * PressScaleMaximumDecraseRate * scaleValue);
                    _animationLayer.Width = _contentRoot.ActualWidth - (_contentRoot.ActualWidth * PressScaleMaximumDecraseRate * scaleValue);
                    Canvas.SetLeft(_animationLayer, _contentRoot.ActualWidth * 0.5 - _animationLayer.ActualWidth * 0.5);
                    Canvas.SetTop(_animationLayer, _contentRoot.ActualHeight * 0.5 - _animationLayer.ActualHeight * 0.5);
                }
            }
            else
            {
                ElasticEase ease = new ElasticEase { EasingMode = EasingMode.EaseOut, Springiness = 1, Oscillations = 3};
                if (_animationLayer != null && _contentRoot != null)
                {
                    double scaleValue = ease.Ease(elapsed / PressScaleProgressFinishTimeSpan);
                    _animationLayer.Height = _contentRoot.ActualHeight - (_contentRoot.ActualHeight * PressScaleMaximumDecraseRate * _scaleStartValue * (1- scaleValue));
                    _animationLayer.Width = _contentRoot.ActualWidth - (_contentRoot.ActualWidth * PressScaleMaximumDecraseRate * _scaleStartValue * (1 - scaleValue));
                    Canvas.SetLeft(_animationLayer, _contentRoot.ActualWidth * 0.5 - _animationLayer.ActualWidth * 0.5);
                    Canvas.SetTop(_animationLayer, _contentRoot.ActualHeight * 0.5 - _animationLayer.ActualHeight * 0.5);
                }
            }
        }
    }
}
