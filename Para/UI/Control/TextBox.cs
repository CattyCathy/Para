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
                _caretIndex = Math.Max(0, Math.Min(value, Text.Length));
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && CaretIndex > 0 && Text.Length > 0)
            {
                Text = Text.Remove(CaretIndex - 1, 1);
                CaretIndex = Math.Max(0, CaretIndex - 1);
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

            var toRemove = _spriteTexts.Where((sc, idx) => idx >= Text.Length || sc.Char != Text[idx]).ToList();
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
            UpdateCaretPosition();
        }

        private void UpdateCaretPosition()
        {
            if (_charPanel == null || _animationLayer == null) return;

            var panelPoint = _charPanel.TransformToVisual(_animationLayer).Transform(new Point(0, 0));
            double caretX = panelPoint.X;
            double caretY = panelPoint.Y;

            if (_caretIndex > 0 && _caretIndex <= _spriteTexts.Count)
            {
                // Caret 在第N个字符的右侧
                var prevChar = _spriteTexts[_caretIndex - 1];
                var prevPoint = prevChar.TransformToVisual(_animationLayer).Transform(new Point(0, 0));
                caretX = prevPoint.X + prevChar.ActualWidth;
            }
            else if (_caretIndex > _spriteTexts.Count)
            {
                // 超出时，放在末尾
                if (_spriteTexts.Count > 0)
                {
                    var lastChar = _spriteTexts.Last();
                    var lastPoint = lastChar.TransformToVisual(_animationLayer).Transform(new Point(0, 0));
                    caretX = lastPoint.X + lastChar.ActualWidth;
                }
            }

            Canvas.SetLeft(_caret, caretX);
            Canvas.SetTop(_caret, caretY);
            Panel.SetZIndex(_caret, 200);
        }

    }
}
