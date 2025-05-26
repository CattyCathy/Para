using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Para.UI.Text
{
    public class SpriteChar : BeatSyncedControl
    {
        public bool HasShadow;
        public System.Windows.Media.Color ShadowColor;
        public Vector2 ShadowOffset;
        private char _char = ' ';

        private double _interval;
        private DateTime _animationStartTime = DateTime.Now;

        public char Char
        {
            get => _char;
            set
            {
                if (_char != value)
                {
                    _char = value;
                    UpdateSize();
                    InvalidateVisual();
                }
            }
        }

        static SpriteChar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SpriteChar),
                new FrameworkPropertyMetadata(typeof(SpriteChar)));
        }

        public SpriteChar()
        {
            Loaded += SpriteText_Loaded;
            Unloaded += SpriteText_Unloaded;
            UpdateSize();
        }

        private void UpdateSize()
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var formattedText = new FormattedText(
                Char.ToString(),
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                FontSize,
                Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            Width = formattedText.WidthIncludingTrailingWhitespace;
            Height = formattedText.Height;
        }

        private void SpriteText_Loaded(object sender, RoutedEventArgs e)
        {
            _animationStartTime = DateTime.Now;
            _interval = DesignDetail.Text.SpriteText.CreateInterval;
            AnimateAppearance(true);
        }

        private void SpriteText_Unloaded(object sender, RoutedEventArgs e)
        {
            _animationStartTime = DateTime.Now;
            _interval = DesignDetail.Text.SpriteText.DestroyInterval;
            AnimateAppearance(false);
        }

        private void AnimateAppearance(bool appearing)
        {
            var fromOpacity = appearing ? 0.0 : 1.0;
            var toOpacity = appearing ? 1.0 : 0.0;
            var duration = new Duration(TimeSpan.FromSeconds(_interval));

            // 确保有 RenderTransform
            if (this.RenderTransform is not TranslateTransform translate)
            {
                translate = new TranslateTransform();
                this.RenderTransform = translate;
            }

            // Opacity 动画
            var opacityAnimation = new DoubleAnimation(fromOpacity, toOpacity, duration)
            {
                FillBehavior = FillBehavior.Stop
            };

            // Y 方向动画（仅删除时）
            DoubleAnimation? yAnimation = null;
            if (!appearing)
            {
                yAnimation = new DoubleAnimation(0, 30, duration) // 30 可调整为你想要的掉落距离
                {
                    FillBehavior = FillBehavior.Stop
                };
                yAnimation.Completed += (s, e) =>
                {
                    this.Visibility = Visibility.Collapsed;
                    translate.Y = 0; // 复位
                };
            }
            else
            {
                this.Visibility = Visibility.Visible;
                translate.Y = 0;
            }

            opacityAnimation.Completed += (s, e) =>
            {
                if (!appearing)
                    this.Opacity = 0;
                else
                    this.Opacity = 1;
            };

            this.BeginAnimation(OpacityProperty, opacityAnimation);
            if (yAnimation != null)
                translate.BeginAnimation(TranslateTransform.YProperty, yAnimation);
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var formattedText = new FormattedText(
                Char.ToString(),
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                FontSize,
                Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            
            //Render shadow if enabled
            if (HasShadow)
            {
                var shadowBrush = new SolidColorBrush(ShadowColor);
                drawingContext.DrawText(
                    new FormattedText(
                        Char.ToString(),
                        System.Globalization.CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        FontSize,
                        shadowBrush,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip),
                    new System.Windows.Point(ShadowOffset.X, ShadowOffset.Y));
            }
            //Render the character itself
            drawingContext.DrawText(formattedText, new System.Windows.Point(0, 0));
        }

        public void PlayDeleteAnimation(Action? onCompleted = null)
        {
            var interval = DesignDetail.Text.SpriteText.DestroyInterval;
            var fromOpacity = 1.0;
            var toOpacity = 0.0;
            var duration = new Duration(TimeSpan.FromSeconds(interval));

            if (this.RenderTransform is not TranslateTransform translate)
            {
                translate = new TranslateTransform();
                this.RenderTransform = translate;
            }

            var opacityAnimation = new DoubleAnimation(fromOpacity, toOpacity, duration)
            {
                FillBehavior = FillBehavior.Stop
            };
            var yAnimation = new DoubleAnimation(0, 10, duration)
            {
                FillBehavior = FillBehavior.Stop
            };

            int completedCount = 0;
            void OnAnyCompleted(object? s, EventArgs e)
            {
                completedCount++;
                if (completedCount == 2)
                {
                    this.Visibility = Visibility.Collapsed;
                    translate.Y = 0;
                    this.Opacity = 0;
                    onCompleted?.Invoke();
                }
            }

            opacityAnimation.Completed += OnAnyCompleted;
            yAnimation.Completed += OnAnyCompleted;

            this.BeginAnimation(OpacityProperty, opacityAnimation);
            translate.BeginAnimation(TranslateTransform.YProperty, yAnimation);
        }
    }
}
