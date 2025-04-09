using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels.CreateServer
{
    public partial class FastModeStep2ViewModel : ViewModelBase
    {
        public FastModeStep2ViewModel()
        {
            ServerModel = new CreateServerModel { Name = "Design Time" };
            FatherView = new FastModeViewModel();
        }

        private CreateServerModel ServerModel { get; set; }
        private FastModeViewModel FatherView { get; set; }

        public FastModeStep2ViewModel(CreateServerModel createServerModel, FastModeViewModel fastModeViewModel)
        {
            ServerModel = createServerModel;
            FatherView = fastModeViewModel;
        }

        public string Title { get; set; } = "步骤2 - 确认服务器信息";
        public string CancelBtn { get; } = "上一步";
        public string NextBtn { get; } = "完成";

        [RelayCommand]
        private void Return()
        {
            FatherView.SetMainContent(FatherView.MainContentList[0]);
        }

        [RelayCommand]
        private void Next()
        {
            Debug.WriteLine("Next");
            Debug.WriteLine(ServerModel.Name);
            ServerModel.Name= "MCServer123";
            FatherView.InstallServer();
        }
    }
}
