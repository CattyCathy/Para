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
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace Para.UI.Control
{
    /// <summary>
    /// Textbox with rhythm synced caret and animated characters.
    /// </summary>
    public class TextBox : BeatSyncedControl
    {
        // Members
        public Caret _caret = new();
        public StackPanel? _charPanel;
        public Canvas? _animationLayer;

        // Local values for public properties
        protected string _text = string.Empty;
        protected int _caretIndex = 0;

        // Runtime values
        protected List<SpriteChar> _spriteTexts = [];
        protected int? _lastRemovedCharIndex = null;
        protected HashSet<int>? _lastRemovedCharIndices = null;
        protected int _selectionStart = -1;
        protected int _selectionEnd = -1;

        // Temporary storage for original values
        public double CaretOriginalWidth;
        public Brush CaretOriginalHighColor;
        public Brush CaretOriginalLowColor;

        // Values for operation status
        protected int _SelectAnchor = -1;
        protected bool _isMouseSelecting = false;//Changing selecting zone using mouse
        protected bool _isKeyboardSelecting = false;//Changing selecting zone using keyboard
        
        // Generated properties
        public bool HasSelection => _selectionStart >= 0 && _selectionEnd > _selectionStart;

        // Public settable properties
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
            CaretOriginalHighColor = DesignDetail.Control.Caret.CaretBrushHigh;
            CaretOriginalLowColor = DesignDetail.Control.Caret.CaretBrushLow;
            Padding = DesignDetail.Control.TextBox.Padding;
            this.PreviewKeyDown += TextBox_PreviewKeyDown;
            this.PreviewKeyUp += TextBox_PreviewKeyUp;
            this.PreviewTextInput += TextBox_PreviewTextInput;
        }
        protected void TextBox_Loaded(object sender, RoutedEventArgs e)
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


        // Events

        // Events: Focus
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

        // Events: Keyboard
        protected void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if(e.Key == Key.Left || e.Key == Key.Right)
                {
                    if (!_isKeyboardSelecting && !HasSelection)
                    {
                        _SelectAnchor = CaretIndex;
                    }
                    _isKeyboardSelecting = true;
                    //Shift + Arrow keys to select characters
                    if (e.Key == Key.Left)
                    {
                        CaretIndex--;
                    }
                    else if (e.Key == Key.Right)
                    {
                        CaretIndex++;
                    }
                    
                    Select(_SelectAnchor, CaretIndex);
                    e.Handled = true;
                    return;
                }
            }
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.A)
                {
                    Select(0, Text.Length);
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.C)
                {
                    CopySelection();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.X)
                {
                    CutSelection();
                    e.Handled = true;
                    return;
                }
                if (e.Key == Key.V)
                {
                    if (Clipboard.ContainsText())
                    {
                        RestoreCaret();
                        ReplaceSelection(Clipboard.GetText());
                        e.Handled = true;
                        return;
                    }
                }
            }

            if (HasSelection)
            {
                //Backspace/Delete to delete selection
                if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    DeleteSelection();
                    e.Handled = true;
                    return;
                }
                //Arrow keys to move caret and cancel selection
                if (e.Key == Key.Left || e.Key == Key.Right)
                {
                    CaretIndex = e.Key == Key.Left ? _selectionStart : _selectionEnd;
                    CancelSelection();
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Back && CaretIndex > 0 && Text.Length > 0)
            {
                int removeIndex = CaretIndex - 1;
                if (removeIndex >= 0 && removeIndex < Text.Length)
                {
                    _lastRemovedCharIndex = removeIndex;
                    Text = Text.Remove(removeIndex, 1);
                    CaretIndex = removeIndex;
                }
                e.Handled = true;
            }
            else if(e.Key == Key.Delete && CaretIndex - 1 < Text.Length && Text.Length > 0)
            {
                int removeIndex = CaretIndex;
                if (removeIndex >= 0 && removeIndex < Text.Length)
                {
                    _lastRemovedCharIndex = removeIndex;
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
        protected void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                if (!HasSelection)
                {
                    _isKeyboardSelecting = false;
                }
            }
        }
        protected void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            RestoreCaret();
            ReplaceSelection(e.Text);
            e.Handled = true;
        }

        // Events: Mouse
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!HasSelection)
            {
                CaretOriginalWidth = _caret.Width;
                CaretOriginalHighColor = _caret.CaretBrushHigh;
                CaretOriginalLowColor = _caret.CaretBrushLow;
            }

            base.OnMouseDown(e);
            this.Focus();

            IInputElement inputElement = _animationLayer as IInputElement
                ?? _charPanel as IInputElement
                ?? this;
            Point mousePos = e.GetPosition(inputElement);

            if (e.ClickCount == 2)
            {
                // Double click: select all
                Select(0, Text.Length);
                _isMouseSelecting = false;
            }
            else
            {
                RestoreCaret();
                //Click and drag to select characters
                int caretIdx = GetCaretIndexFromPoint(mousePos);
                CaretIndex = caretIdx;
                _SelectAnchor = caretIdx;
                _isMouseSelecting = true;
                _selectionStart = caretIdx;
                _selectionEnd = caretIdx;
                CaptureMouse();
                _caret.Visibility = Visibility.Visible;
                UpdateCaretPosition();
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isMouseSelecting)
            {
                _isMouseSelecting = false;
                ReleaseMouseCapture();
                //Cancel selection if the start and end are the same
                if (_selectionStart == _selectionEnd)
                    CancelSelection();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isMouseSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                IInputElement inputElement = _animationLayer as IInputElement
                    ?? _charPanel as IInputElement
                    ?? this;
                Point mousePos = e.GetPosition(inputElement);
                int caretIdx = GetCaretIndexFromPoint(mousePos);
                CaretIndex = caretIdx;
                Select(_SelectAnchor, caretIdx);
                InvalidateVisual();
            }
        }


        // Methods

        // Methods: Visual
        public void UpdateSpriteChars()
        {
            if (_charPanel == null || _animationLayer == null) return;

            var oldChars = _spriteTexts.Select(sc => sc.Char).ToList();
            var newChars = Text.ToList();
            var lcs = LongestCommonSubsequence(oldChars, newChars);

            var toRemove = new List<SpriteChar>();
            if (_lastRemovedCharIndices != null)
            {
                foreach (var idx in _lastRemovedCharIndices.OrderByDescending(i => i))
                {
                    if (idx >= 0 && idx < _spriteTexts.Count)
                        toRemove.Add(_spriteTexts[idx]);
                }
                _lastRemovedCharIndices = null;
            }
            else if (_lastRemovedCharIndex.HasValue && _lastRemovedCharIndex.Value < _spriteTexts.Count)
            {
                toRemove.Add(_spriteTexts[_lastRemovedCharIndex.Value]);
                _lastRemovedCharIndex = null;
            }
            else
            {
                int oldIndex = 0, lcsIndex = 0;
                while (oldIndex < _spriteTexts.Count)
                {
                    if (lcsIndex < lcs.Count && _spriteTexts[oldIndex].Char == lcs[lcsIndex])
                    {
                        oldIndex++;
                        lcsIndex++;
                    }
                    else
                    {
                        toRemove.Add(_spriteTexts[oldIndex]);
                        oldIndex++;
                    }
                }
            }

            // Deletion animation
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
                sc.PlayDeleteAnimation(() => _animationLayer.Children.Remove(sc));
            }

            // Insert new char to avoid unexpected animation
            var newSpriteList = new List<SpriteChar>();
            int spriteIndex = 0, lcsIndex2 = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                if (lcsIndex2 < lcs.Count && Text[i] == lcs[lcsIndex2])
                {
                    // LCS using sprite char
                    newSpriteList.Add(_spriteTexts[spriteIndex]);
                    spriteIndex++;
                    lcsIndex2++;
                }
                else
                {
                    // Added char
                    var spriteChar = new SpriteChar
                    {
                        Char = Text[i],
                        FontSize = this.FontSize,
                        Foreground = this.Foreground,
                        HasShadow = DesignDetail.Text.SpriteChar.HasShadow,
                        ShadowColor = DesignDetail.Text.SpriteChar.ShadowColor,
                        ShadowOffset = DesignDetail.Text.SpriteChar.ShadowOffset
                    };
                    _charPanel.Children.Insert(i, spriteChar);
                    newSpriteList.Add(spriteChar);
                }
            }

            _spriteTexts = newSpriteList;

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

        // Methods: Helper
        protected static List<char> LongestCommonSubsequence(List<char> a, List<char> b)
        {
            int[,] dp = new int[a.Count + 1, b.Count + 1];
            for (int i = 0; i < a.Count; i++)
                for (int j = 0; j < b.Count; j++)
                    if (a[i] == b[j])
                        dp[i + 1, j + 1] = dp[i, j] + 1;
                    else
                        dp[i + 1, j + 1] = Math.Max(dp[i, j + 1], dp[i + 1, j]);

            var lcs = new List<char>();
            int x = a.Count, y = b.Count;
            while (x != 0 && y != 0)
            {
                if (a[x - 1] == b[y - 1])
                {
                    lcs.Insert(0, a[x - 1]);
                    x--; y--;
                }
                else if (dp[x - 1, y] > dp[x, y - 1])
                    x--;
                else
                    y--;
            }
            return lcs;
        }

        // Methods: Selection
        /// <summary>
        /// Set the selection range of the text box, caret will automatically cover the selected text.
        /// </summary>
        /// <param name="fromIndex">Index of the character that begins in the selection.</param>
        /// <param name="toIndex">Index of the character that ends in the selection.</param>
        public void Select(int fromIndex, int toIndex)
        {
            _selectionStart = Math.Max(0, Math.Min(fromIndex, toIndex));
            _selectionEnd = Math.Min(Text.Length, Math.Max(fromIndex, toIndex));

            if (HasSelection)
            {
                PlaySelectionCaretAnimation();
            }
            else
            {
                RestoreCaret();
            }
        }
        protected void CancelSelection()
        {
            _selectionStart = -1;
            _selectionEnd = -1;
            _isKeyboardSelecting = false;
            _isMouseSelecting = false;
            RestoreCaret();
            PlaySelectionCaretAnimation();
        }
        protected void DeleteSelection()
        {
            if (!HasSelection) return;
            int start = _selectionStart;
            int length = _selectionEnd - _selectionStart;
            if (length > 0)
            {
                _lastRemovedCharIndices = [];
                for (int i = 0; i < length; i++)
                    _lastRemovedCharIndices.Add(start + i);
            }
            Text = Text.Remove(start, length);
            RestoreCaret();
            CaretIndex = start;
            CancelSelection();
        }
        protected void ReplaceSelection(string newText)
        {
            if (!HasSelection)
            {
                Text = Text.Insert(CaretIndex, newText);
                CaretIndex += newText.Length;
            }
            else
            {
                int start = _selectionStart;
                int length = _selectionEnd - _selectionStart;
                Text = Text.Remove(start, length).Insert(start, newText);
                CaretIndex = start + newText.Length;
                CancelSelection();
            }
        }
        protected void CopySelection()
        {
            if (HasSelection)
            {
                Clipboard.SetText(Text[_selectionStart.._selectionEnd]);
            }
        }
        protected void CutSelection()
        {
            if (HasSelection)
            {
                CopySelection();
                DeleteSelection();
            }
        }

        // Methods: Caret
        public int GetCaretIndexFromPoint(Point mousePosition)
        {
            if (_charPanel == null || _spriteTexts.Count == 0)
                return 0;

            Point panelPoint = mousePosition;
            if (_animationLayer != null)
                panelPoint = _animationLayer.TransformToVisual(_charPanel).Transform(mousePosition);

            double x = panelPoint.X;
            int closestIndex = 0;
            double minDist = double.MaxValue;

            for (int i = 0; i <= _spriteTexts.Count; i++)
            {
                double charX = 0;
                if (i < _spriteTexts.Count)
                {
                    var charElem = _spriteTexts[i];
                    charX = charElem.TransformToAncestor(_charPanel).Transform(new Point(0, 0)).X;
                }
                else if (_spriteTexts.Count > 0)
                {
                    var lastChar = _spriteTexts[^1];
                    charX = lastChar.TransformToAncestor(_charPanel).Transform(new Point(0, 0)).X + lastChar.ActualWidth;
                }

                double dist = Math.Abs(x - charX);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }
        public void MoveCaret(Point mousePosition)
        {
            if (_charPanel == null || _spriteTexts.Count == 0)
            {
                CaretIndex = 0;
                return;
            }

            // Transform mouse position to _charPanel coordinates
            Point panelPoint = mousePosition;
            if (_animationLayer != null)
                panelPoint = _animationLayer.TransformToVisual(_charPanel).Transform(mousePosition);

            double x = panelPoint.X;
            int closestIndex = 0;
            double minDist = double.MaxValue;

            for (int i = 0; i <= _spriteTexts.Count; i++)
            {
                double charX = 0;
                if (i < _spriteTexts.Count)
                {
                    var charElem = _spriteTexts[i];
                    charX = charElem.TransformToAncestor(_charPanel).Transform(new Point(0, 0)).X;
                }
                else if (_spriteTexts.Count > 0)
                {
                    // After last char
                    var lastChar = _spriteTexts[^1];
                    charX = lastChar.TransformToAncestor(_charPanel).Transform(new Point(0, 0)).X + lastChar.ActualWidth;
                }

                double dist = Math.Abs(x - charX);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIndex = i;
                }
            }

            CaretIndex = closestIndex;
            RestoreCaret();
        }
        protected void UpdateCaretPosition()
        {
            if (_animationLayer == null) return;

            double caretX = 0;
            double caretY = 0;

            if (CaretIndex > 0 && !string.IsNullOrEmpty(Text))
            {
                string textBeforeCaret = Text[..CaretIndex];

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

            if (_animationLayer != null && _caret != null && !HasSelection)
            {
                DoubleAnimation? xAnimation = null;
                xAnimation = new DoubleAnimation(_caret.TransformToVisual(_animationLayer).Transform(new Point(0, 0)).X, caretX, new Duration(TimeSpan.FromSeconds(DesignDetail.Control.TextBox.CaretMovementInterval)))
                {
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                xAnimation.Completed += (s, e) =>
                {
                    Canvas.SetLeft(_caret, caretX);
                };
                _caret.BeginAnimation(Canvas.LeftProperty, null);
                _caret.BeginAnimation(Canvas.LeftProperty, xAnimation);
            }
            else
            {
                Canvas.SetLeft(_caret, caretX);
            }
            Canvas.SetTop(_caret, caretY);
            Panel.SetZIndex(_caret, 200);
        }
        protected void PlaySelectionCaretAnimation()
        {
            if (_animationLayer == null) return;

            //foreach (var rect in _selectionRects)
            //    _animationLayer.Children.Remove(rect);
            //_selectionRects.Clear();

            if (HasSelection && _charPanel != null && _spriteTexts.Count > 0)
            {
                int start = _selectionStart;
                int end = _selectionEnd;

                double selectionWidth = 0;
                double newStartX = 0;
                for (int i = start; i < end && i < _spriteTexts.Count; i++)
                {
                    var sprite = _spriteTexts[i];
                    if (i == start)
                    {
                        newStartX = sprite.TransformToVisual(_animationLayer).Transform(new Point(0, 0)).X;
                        _caret.CaretBrushHigh = DesignDetail.Control.TextBox.SelectionCaretBrush;
                        _caret.CaretBrushLow = DesignDetail.Control.TextBox.SelectionCaretBrush;
                    }
                    selectionWidth += sprite.ActualWidth;
                }

                DoubleAnimation? leftAnimation = null;
                leftAnimation = new DoubleAnimation(_caret.TransformToVisual(_animationLayer).Transform(new Point(0,0)).X, newStartX, new Duration(TimeSpan.FromSeconds(DesignDetail.Control.TextBox.CaretMovementInterval)))
                {
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                leftAnimation.Completed += (s, e) =>
                {
                    Canvas.SetLeft(_caret, newStartX);
                };
                DoubleAnimation? widthAnimation = null;
                widthAnimation = new DoubleAnimation(_caret.ActualWidth, selectionWidth, new Duration(TimeSpan.FromSeconds(DesignDetail.Control.TextBox.CaretMovementInterval)))
                {
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                widthAnimation.Completed += (s, e) =>
                {
                    _caret.Width = selectionWidth;
                };

                _caret.BeginAnimation(Canvas.LeftProperty, leftAnimation);
                _caret.BeginAnimation(Caret.WidthProperty, widthAnimation);

                //Apply rect to every single character(another selection method)
                //for (int i = start; i < end && i < _spriteTexts.Count; i++)
                //{
                //    var sprite = _spriteTexts[i];
                //    Point charPos = sprite.TransformToVisual(_animationLayer).Transform(new Point(0, 0));
                //    double width = sprite.ActualWidth;
                //    double height = sprite.ActualHeight;


                //    //var rect = new Rectangle
                //    //{
                //    //    Width = width,
                //    //    Height = height,
                //    //    Fill = _selectionHighlightBrush,
                //    //    IsHitTestVisible = false
                //    //};
                //    Canvas.SetLeft(rect, charPos.X);
                //    Canvas.SetTop(rect, charPos.Y);
                //    Panel.SetZIndex(rect, 50);
                //    //_animationLayer.Children.Add(rect);
                //    //_selectionRects.Add(rect);
                //}
            }
        }
        protected void RestoreCaret()
        {
            _caret.CaretBrushHigh = CaretOriginalHighColor ?? DesignDetail.Control.Caret.CaretBrushHigh;
            _caret.CaretBrushLow = CaretOriginalLowColor ?? DesignDetail.Control.Caret.CaretBrushLow;
            DoubleAnimation? widthAnimation = null;
            widthAnimation = new DoubleAnimation(_caret.ActualWidth, CaretOriginalWidth, new Duration(TimeSpan.FromSeconds(DesignDetail.Control.TextBox.CaretMovementInterval)))
            {
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            widthAnimation.Completed += (s, e) =>
            {
                _caret.Width = CaretOriginalWidth;
            };
            _caret.BeginAnimation(WidthProperty, widthAnimation);

            _caret.Visibility = Visibility.Visible;
            UpdateCaretPosition();
        }
    }
}
