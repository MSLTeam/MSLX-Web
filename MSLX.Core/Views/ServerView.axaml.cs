using Avalonia.Controls;
using Avalonia.Threading;

namespace MSLX.Core.Views;

public partial class ServerView : UserControl
{
    public ServerView()
    {
        InitializeComponent();
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
}