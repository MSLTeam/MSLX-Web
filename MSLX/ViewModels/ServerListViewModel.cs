using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MSLX.ViewModels
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

        public class ServerInfo
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public bool Status { get; set; }
            public string Core { get; set; }

            public ServerInfo(int id, string name, bool status, string core)
            {
                ID = id;
                Name = name;
                Status = status;
                Core = core;
            }
        }

        [ObservableProperty]
        private ObservableCollection<ServerInfo> _serverList = [];

        public ServerListViewModel()
        {
            ServerList.Add(new ServerInfo(1, "服务器1", true, "paper1.21.4.jar"));
            ServerList.Add(new ServerInfo(1, "服务器1", true, "paper1.21.4.jar"));
            ServerList.Add(new ServerInfo(1, "服务器1", true, "paper1.21.4.jar"));
            ServerList.Add(new ServerInfo(1, "服务器1", true, "paper1.21.4.jar"));
            ServerList.Add(new ServerInfo(1, "服务器1", true, "paper1.21.4.jar"));
            ServerList.Add(new ServerInfo(1, "服务器1", true, "paper1.21.4.jar"));
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
    }
}
