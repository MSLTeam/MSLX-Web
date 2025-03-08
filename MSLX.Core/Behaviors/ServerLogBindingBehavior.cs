using Avalonia.Xaml.Interactivity;
using Avalonia;
using AvaloniaEdit;
using System;
using AvaloniaEdit.Utils;

namespace MSLX.Core.Behaviors
{
    public class ServerLogBindingBehavior : Behavior<TextEditor>
    {
        private TextEditor? _textEditor;

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<ServerLogBindingBehavior, string>(nameof(Text));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is TextEditor textEditor)
            {
                _textEditor = textEditor;
                _textEditor.TextChanged += TextChanged;
                _textEditor.Options.AllowScrollBelowDocument=false;
                this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_textEditor != null)
            {
                _textEditor.TextChanged -= TextChanged;
            }
        }

        private void TextChanged(object? sender, EventArgs eventArgs)
        {
            if (_textEditor != null && _textEditor.Document != null)
            {
                Text = _textEditor.Document.Text;
            }
        }

        private void TextPropertyChanged(string text)
        {
            if (_textEditor != null && _textEditor.Document != null && text != null)
            {
                //var caretOffset = _textEditor.CaretOffset;
                _textEditor.Document.Text = text;
                _textEditor.ScrollToEnd();
                //_textEditor.CaretOffset = caretOffset;
            }
        }
    }
}
