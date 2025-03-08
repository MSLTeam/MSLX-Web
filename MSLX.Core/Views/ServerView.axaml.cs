using Avalonia.Controls;
using Avalonia.Threading;

namespace MSLX.Core.Views;

public partial class ServerView : UserControl
{
    //private TextEditor? _textEditor;
    //private ServerViewModel? _viewModel;
    public ServerView()
    {
        InitializeComponent();
        //_textEditor = this.FindControl<TextEditor>("ServerLogsBox");
        //DataContextChanged += OnDataContextChanged;
    }

    /*
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        _viewModel = DataContext as ServerViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ServerViewModel.ServerLogs))
        {
            // Debug.WriteLine(_viewModel.ServerLogs);
            if (_textEditor == null) return;
            Dispatcher.UIThread.Post(() =>
            {
                _textEditor.Text = _viewModel?.ServerLogs;
            });
        }
    }

    private void ServerLogsBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                textBox.CaretIndex = textBox.Text.Length;
            }
        });
    }
    */
}