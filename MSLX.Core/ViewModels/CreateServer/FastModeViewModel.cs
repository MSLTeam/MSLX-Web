using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels.CreateServer
{
    public partial class FastModeViewModel : ViewModelBase
    {
        public string Title { get; } = "快速模式";
        public string CancelBtn { get; } = "取消";
        public string NextBtn { get; } = "下一步";


        [RelayCommand]
        private void Cancel()
        {
            MainViewModel.NavigateTo<ServerListViewModel>();
            MainViewModel.NavigateRemove(this);
        }

        [RelayCommand]
        private void Next()
        {

        }
    }
}
