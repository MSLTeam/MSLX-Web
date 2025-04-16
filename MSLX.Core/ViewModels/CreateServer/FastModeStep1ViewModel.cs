using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Models;
using MSLX.Core.Utils;
using MSLX.Core.Views.CreateServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels.CreateServer
{
    public partial class FastModeStep1ViewModel : ViewModelBase
    {
        public FastModeStep1ViewModel()
        {
            ServerModel = new CreateServerModel { Name = "Design Time" };
            FatherView = new FastModeViewModel();
        }

        private CreateServerModel ServerModel { get; set; }

        private FastModeViewModel FatherView { get; set; }

        public FastModeStep1ViewModel(CreateServerModel createServerModel,FastModeViewModel fastModeViewModel)
        {
            ServerModel = createServerModel;
            FatherView = fastModeViewModel;
        }

        public string Title { get; set; } = "步骤1 - 选择";
        public string CancelBtn { get; } = "取消";
        public string NextBtn { get; } = "下一步";

        [RelayCommand]
        private void Cancel()
        {
            MainViewSideMenu.NavigateTo<ServerListViewModel>();
            MainViewSideMenu.NavigateRemove<FastModeViewModel>();
        }

        [RelayCommand]
        private void Next()
        {
            FatherView.SetMainContent(FatherView.MainContentList[1]);
        }
    }
}
