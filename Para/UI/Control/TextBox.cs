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
    public class TextBox : BeatSyncedControl
    {
        public Caret _caret = new();
        protected string _text = string.Empty;
        protected List<SpriteChar> _spriteTexts = [];

        private StackPanel? _charPanel;
        private Canvas? _animationLayer;

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
            this.PreviewKeyDown += TextBox_PreviewKeyDown;
            this.PreviewTextInput += TextBox_PreviewTextInput;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && Text.Length > 0)
            {
                Text = Text[..^1];
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
            Text += e.Text;
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
            _charPanel = GetTemplateChild("PART_CharPanel") as StackPanel;
            _animationLayer = GetTemplateChild("PART_AnimationLayer") as Canvas;
            UpdateSpriteChars();
        }

        private void UpdateSpriteChars()
        {
            if (_charPanel == null || _animationLayer == null) return;

            _charPanel.Children.Remove(_caret);
            _animationLayer.Children.Remove(_caret);

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

            _charPanel.Children.Remove(_caret);
            _charPanel.Children.Add(_caret);
        }
    }
}
