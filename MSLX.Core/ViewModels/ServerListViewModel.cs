using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MSLX.Core.ViewModels;
using MSLX.Core.Utils;

namespace MSLX.Core.ViewModels
{
    public partial class ServerListViewModel : ViewModelBase
    {
        public string Title { get; } = "服务器列表";
        public string StatusText { get; } = "状态：";
        public string CoreText { get; } = "核心：";
        public string RunServerText { get; } = "开启服务器";
        public string OpenDirectory { get; } = "打开目录";

        // public string ServerSettings { get; } = "服务器设置";
        // public string DeleteServer { get; } = "删除服务器";

        public class LServerInfo
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public bool Status { get; set; }
            public string Core { get; set; }

            public LServerInfo(int id, string name, bool status, string core)
            {
                ID = id;
                Name = name;
                Status = status;
                Core = core;
            }
        }

        [ObservableProperty]
        private ObservableCollection<LServerInfo> _serverList = [];

        public ServerListViewModel()
        {
        }

        [ObservableProperty]
        private ObservableCollection<ServerViewModel> _serverViews = [];

#if DESKTOP
        private readonly Dictionary<int, SukiWindow> _openWindows = [];
#endif

        [RelayCommand]
        private void RunServer(object parameter)
        {
            if (parameter is int id)
            {
                if (ServerViews.Any(x => x.ID == id)) return;
                var serverView = new ServerViewModel(id);
                ServerViews.Add(serverView);

#if DESKTOP
                if (_openWindows.TryGetValue(id, out var existingWindow))
                {
                    existingWindow.Activate();
                    return;
                }
                var sukiWindow = new SukiWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = serverView
                };
                sukiWindow.Closed += (_, _) =>
                {
                    _openWindows.Remove(id);
                    ServerViews.Remove(serverView);
                };
                _openWindows.Add(id, sukiWindow);
                sukiWindow.Show();
#endif
            }
        }

        [RelayCommand]
        private void RefreshList()
        {
            var servers = Utility.ConfigService.GetServerList();
            ServerList.Clear();
            foreach (var server in servers)
            {
                ServerList.Add(new LServerInfo(server.ID, server.Name, false, server.Core));
            }
        }

        [RelayCommand]
        private void AddServer()
        {
            StackPanel stackPanel = new StackPanel()
            {
                Children =
                {
                    new TextBlock()
                    {
                        Text = "服务器名称"
                    },
                    new TextBox()
                    {
                        Name = "ServerName"
                    },
                    new TextBlock()
                    {
                        Text = "服务器运行目录"
                    },
                    new TextBox()
                    {
                        Name = "ServerBase"
                    },
                    new TextBlock()
                    {
                        Text = "核心"
                    },
                    new TextBox()
                    {
                        Name = "ServerCore"
                    },
                    new TextBlock()
                    {
                        Text = "Java"
                    },
                    new TextBox()
                    {
                        Name = "ServerJava"
                    }
                }
            };
            MainViewModel.DialogManager.CreateDialog().WithTitle("添加服务器").WithContent(stackPanel).WithActionButton("确定", _ =>
            {
                var serverName = stackPanel.Children.OfType<TextBox>().First(x => x.Name == "ServerName").Text;
                var serverBase = stackPanel.Children.OfType<TextBox>().First(x => x.Name == "ServerBase").Text;
                var serverCore = stackPanel.Children.OfType<TextBox>().First(x => x.Name == "ServerCore").Text;
                var serverJava = stackPanel.Children.OfType<TextBox>().First(x => x.Name == "ServerJava").Text;
                ServerList.Add(new LServerInfo(Utility.ConfigService.GenerateServerId(), serverName, false, serverCore));
                Utility.ConfigService.CreateServer(new Models.MCServerModel.ServerInfo(Utility.ConfigService.GenerateServerId(), serverName, serverBase, serverJava, serverCore, 0, 0, string.Empty));
                MainViewModel.DialogManager.DismissDialog();
            }).WithActionButton("关闭", _ => { }, true).TryShow();
        }

        [RelayCommand]
        private void DelServer(object parameter)
        {
            if (parameter is int id)
            {
                Utility.ConfigService.DeleteServer(id);
                refreshListCommand.Execute(null);
            }
        }
    }
}
