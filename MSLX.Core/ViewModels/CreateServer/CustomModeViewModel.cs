using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Models;
using MSLX.Core.Utils;
using MSLX.Core.Views.CreateServer;

namespace MSLX.Core.ViewModels.CreateServer
{
    public partial class CustomModeViewModel : ViewModelBase
    {
        // Design time data
        public CustomModeViewModel()
        {
            ServerModel = new CreateServerModel { Name = "Design Time" };
            Title += $" - {ServerModel.Name}";
        }

        private CreateServerModel ServerModel { get; set; }
        public CustomModeViewModel(CreateServerModel createServerModel)
        {
            ServerModel = createServerModel;
            Title += $" - {ServerModel.Name}";
        }

        public string Title { get; set; } = "经典模式";
        public string CancelBtn { get; } = "取消";
        public string NextBtn { get; } = "完成";

        [ObservableProperty]
        private string _serverPath = string.Empty;

        [ObservableProperty]
        private string _serverJava = string.Empty;

        [ObservableProperty]
        private string _serverCore = string.Empty;

        [ObservableProperty]
        private int _serverMinMem = 0;

        [ObservableProperty]
        private int _serverMaxMem = 0;

        [ObservableProperty]
        private string _serverArgs = string.Empty;

        [RelayCommand]
        private void Cancel()
        {
            MainViewModel.NavigateTo<ServerListViewModel>();
            MainViewModel.NavigateRemove<CustomModeViewModel>();
        }

        [RelayCommand]
        private void Next()
        {
            ServerModel.ServerPath = ServerPath;
            ServerModel.JavaPath = ServerJava;
            ServerModel.CorePath = ServerCore;
            ServerModel.MinMemory = ServerMinMem;
            ServerModel.MaxMemory = ServerMaxMem;
            ServerModel.ServerArgs = ServerArgs;
            ConfigService.ServerList.CreateServer(new MCServerModel.ServerInfo
            {
                ID = ConfigService.ServerList.GenerateServerId(),
                Name = ServerModel.Name,
                Base = ServerModel.ServerPath,
                Java = ServerModel.JavaPath,
                Core = ServerModel.CorePath,
                MinM = ServerModel.MinMemory,
                MaxM = ServerModel.MaxMemory,
                Args = ServerModel.ServerArgs
            });
            CancelCommand.Execute(null);
        }
    }
}
