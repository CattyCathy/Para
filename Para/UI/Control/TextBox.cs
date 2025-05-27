using Para.UI.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Para.UI.Control
{
    /// <summary>
    /// Textbox with rhythm synced caret and animated characters.
    /// </summary>
    public class TextBox : BeatSyncedControl
    {
        public Caret _caret = new();
        protected string _text = string.Empty;
        protected List<SpriteChar> _spriteTexts = [];

        private StackPanel? _charPanel;
        private Canvas? _animationLayer;

        private int _caretIndex = 0;
        public int CaretIndex
        {
            get => _caretIndex;
            set
            {
                int newValue = Math.Max(0, Math.Min(value, Text.Length));
                if (_caretIndex != newValue)
                {
                    _caretIndex = newValue;
                    UpdateCaretPosition();
                }
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    UpdateSpriteChars();
                }
            }
        }

        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TextBox),
                new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        public TextBox()
        {
            Loaded += TextBox_Loaded;
            Focusable = true;
            FocusVisualStyle = null;
            Padding = DesignDetail.Control.TextBox.Padding;
            this.PreviewKeyDown += TextBox_PreviewKeyDown;
            this.PreviewTextInput += TextBox_PreviewTextInput;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _caret.Visibility = Visibility.Visible;
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            _caret.Visibility = Visibility.Collapsed;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && CaretIndex > 0 && Text.Length > 0)
            {
                int removeIndex = CaretIndex - 1;
                if (removeIndex >= 0 && removeIndex < Text.Length)
                {
                    Text = Text.Remove(removeIndex, 1);
                    CaretIndex = removeIndex;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                if (CaretIndex > 0)
                    CaretIndex--;
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                if (CaretIndex < Text.Length)
                    CaretIndex++;
                e.Handled = true;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int newCaretIndex = CaretIndex + e.Text.Length;
            Text = Text.Insert(CaretIndex, e.Text);
            CaretIndex = newCaretIndex;
            e.Handled = true;
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            //Set TextBox properties
            Background = DesignDetail.Control.TextBox.BackgroundBrush;
            Foreground = DesignDetail.Control.TextBox.ForegroundBrush;

            //Set caret properties
            _caret.CaretBrushHigh = DesignDetail.Control.Caret.CaretBrushHigh;
            _caret.CaretBrushLow = DesignDetail.Control.Caret.CaretBrushLow;
            _caret.Width = DesignDetail.Control.TextBox.CaretWidth;
            _caret.Height = (double)this.FontSize;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Get controls
            _charPanel = GetTemplateChild("PART_CharPanel") as StackPanel;
            _animationLayer = GetTemplateChild("PART_AnimationLayer") as Canvas;

            _animationLayer?.Children.Add(_caret);
            _caret.VerticalAlignment = VerticalAlignment.Center;
            _caret.Margin = new Thickness(0, DesignDetail.Control.TextBox.Padding.Top, 0, DesignDetail.Control.TextBox.Padding.Bottom);
            UpdateSpriteChars();
        }

        private void UpdateSpriteChars()
        {
            if (_charPanel == null || _animationLayer == null) return;

            var newCharCounts = Text.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

            var oldCharCounts = _spriteTexts.GroupBy(sc => sc.Char).ToDictionary(g => g.Key, g => g.Count());

            var toRemove = new List<SpriteChar>();
            var tempCounts = new Dictionary<char, int>(newCharCounts);

            foreach (var sc in _spriteTexts)
            {
                if (!tempCounts.TryGetValue(sc.Char, out int count) || count == 0)
                {
                    toRemove.Add(sc);
                }
                else
                {
                    tempCounts[sc.Char] = count - 1;
                }
            }
            var removePositions = new Dictionary<SpriteChar, Point>();
            foreach (var sc in toRemove)
            {
                var point = sc.TransformToVisual(_animationLayer).Transform(new Point(0, 0));
                removePositions[sc] = point;
            }

            foreach (var sc in toRemove)
            {
                _charPanel.Children.Remove(sc);
                _spriteTexts.Remove(sc);
            }
            foreach (var sc in toRemove)
            {
                _animationLayer.Children.Add(sc);
                var point = removePositions[sc];
                Canvas.SetLeft(sc, point.X);
                Canvas.SetTop(sc, point.Y);
                Panel.SetZIndex(sc, 100);

                sc.Width = sc.ActualWidth;
                sc.Height = sc.ActualHeight;

                sc.PlayDeleteAnimation(() =>
                {
                    _animationLayer.Children.Remove(sc);
                });
            }

            var toKeep = _spriteTexts;

            for (int i = toKeep.Count; i < Text.Length; i++)
            {
                var spriteChar = new SpriteChar
                {
                    Char = Text[i],
                    FontSize = this.FontSize,
                    Foreground = this.Foreground,
                    HasShadow = DesignDetail.Text.SpriteText.HasShadow,
                    ShadowColor = DesignDetail.Text.SpriteText.ShadowColor,
                    ShadowOffset = DesignDetail.Text.SpriteText.ShadowOffset
                };
                _charPanel.Children.Insert(i, spriteChar);
                _spriteTexts.Insert(i, spriteChar);
            }

            for (int i = 0; i < _spriteTexts.Count; i++)
            {
                if (_charPanel.Children.IndexOf(_spriteTexts[i]) != i)
                {
                    _charPanel.Children.Remove(_spriteTexts[i]);
                    _charPanel.Children.Insert(i, _spriteTexts[i]);
                }
            }
            if (CaretIndex > Text.Length)
                CaretIndex = Text.Length;
            UpdateCaretPosition();
        }

        private void UpdateCaretPosition()
        {
            if (_animationLayer == null) return;

            double caretX = 0;
            double caretY = 0;

            if (CaretIndex > 0 && !string.IsNullOrEmpty(Text))
            {
                string textBeforeCaret = Text.Substring(0, CaretIndex);

                var formattedText = new FormattedText(
                    textBeforeCaret,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                    this.FontSize,
                    this.Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );

                caretX = formattedText.WidthIncludingTrailingWhitespace;
            }

            if (_charPanel != null)
            {
                var panelPoint = _charPanel.TransformToVisual(_animationLayer).Transform(new Point(0, 0));
                caretX += panelPoint.X;
                caretY = panelPoint.Y;
            }

            Canvas.SetLeft(_caret, caretX);
            Canvas.SetTop(_caret, caretY);
            Panel.SetZIndex(_caret, 200);
        }

    }
}
