using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using MSLX.Core.ViewModels;
using SukiUI.Controls;
using SukiUI.Dialogs;
using System.Linq;

namespace MSLX.Core.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        /*
        WeakReferenceMessenger.Default.Register<WindowStateMessage>(this, (r, m) =>
        {
            WindowState = m.State;
        });
        */
    }

    private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SukiWindow_Closing(object sender, WindowClosingEventArgs e)
    {
        if (MainViewModel.ServerListView.ServerList.Any(x => x.Status == true))
        {
            e.Cancel = true;
            MainViewModel.DialogManager.CreateDialog().WithTitle("警告").WithContent("有服务器正在运行中，请先关闭所有服务器！").WithActionButton("确定", _ => { }, true).TryShow();
        }
    }
}