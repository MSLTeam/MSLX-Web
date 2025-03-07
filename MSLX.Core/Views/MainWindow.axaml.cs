using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using MSLX.ViewModels;
using SukiUI.Controls;

namespace MSLX.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<WindowStateMessage>(this, (r, m) =>
        {
            WindowState = m.State;
        });
    }

    private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SukiWindow_Closing(object sender, WindowClosingEventArgs e)
    {
    }
}