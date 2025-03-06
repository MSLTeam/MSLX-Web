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
}