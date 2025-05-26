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
                Text = Text.Substring(0, Text.Length - 1);
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
            UpdateSpriteChars();
        }

        private void UpdateSpriteChars()
        {
            if (_charPanel == null) return;
            var oldSprites = _spriteTexts.ToList();
            _charPanel.Children.Clear();
            _spriteTexts.Clear();

            int i = 0;
            for (; i < _text.Length; i++)
            {
                SpriteChar spriteChar;
                if (i < oldSprites.Count && oldSprites[i].Char == _text[i])
                {
                    spriteChar = oldSprites[i];
                }
                else
                {
                    spriteChar = new SpriteChar
                    {
                        Char = _text[i],
                        FontSize = this.FontSize,
                        Foreground = this.Foreground,
                        HasShadow = DesignDetail.Text.SpriteText.HasShadow,
                        ShadowColor = DesignDetail.Text.SpriteText.ShadowColor,
                        ShadowOffset = DesignDetail.Text.SpriteText.ShadowOffset
                    };
                }
                _spriteTexts.Add(spriteChar);
                _charPanel.Children.Add(spriteChar);
            }

            for (; i < oldSprites.Count; i++)
            {
                var toDelete = oldSprites[i];
                _charPanel.Children.Add(toDelete);
                toDelete.PlayDeleteAnimation(() =>
                {
                    _charPanel.Children.Remove(toDelete);
                });
            }

            _charPanel.Children.Add(_caret);
        }


    }
}
