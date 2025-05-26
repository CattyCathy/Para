using Para.UI.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Para.UI.Control
{
    public class TextBox : BeatSyncedControl
    {
        public Caret _caret = new();
        protected string _text = string.Empty;
        protected List<SpriteText> _spriteTexts = [];

        public string Text
        {
            get => _text;
            set => _text = value;   
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
    }
}
