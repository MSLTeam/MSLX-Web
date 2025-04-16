using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using MSLX.Core.Models;
using MSLX.Core.Utils;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels.CreateServer
{
    public partial class GuideViewModel : ViewModelBase
    {
        public string Title { get; } = "创建服务器向导";
        public string FastMode { get; } = "快速模式（小白模式）";
        public string CustomMode { get; } = "经典模式（自定义模式）";
        public string ImportMode { get; } = "导入模式（下载/导入整合包）";
        public string CancelBtn { get; } = "取消";
        public string NextBtn { get; } = "下一步";

        [ObservableProperty]
        private bool _isFastMode = true;
        [ObservableProperty]
        private bool _isCustomMode = false;
        [ObservableProperty]
        private bool _isImportMode = false;

        [ObservableProperty]
        private string _nameText = "MCServer";

        [RelayCommand]
        private void Cancel()
        {
            MainViewSideMenu.NavigateTo<ServerListViewModel>();
            MainViewSideMenu.NavigateRemove(this);
        }

        [RelayCommand]
        private void Next()
        {
            CreateServerModel createServerModel = new CreateServerModel
            {
                Name = NameText,
            };
            if (IsFastMode)
            {
                // Navigate to FastModeViewModel
                MainViewSideMenu.NavigateTo(new SukiSideMenuItem
                {
                    Header = "快速模式",
                    Icon = new MaterialIcon()
                    {
                        Kind = MaterialIconKind.Add,
                    },
                    PageContent = new FastModeViewModel(createServerModel),
                    IsContentMovable = false
                }, true, 2);
                MainViewSideMenu.NavigateRemove(this);
            }
            else if (IsCustomMode)
            {
                // Navigate to CustomModeViewModel
                MainViewSideMenu.NavigateTo(new SukiSideMenuItem
                {
                    Header = "经典模式",
                    Icon = new MaterialIcon()
                    {
                        Kind = MaterialIconKind.Add,
                    },
                    PageContent = new CustomModeViewModel(createServerModel),
                    IsContentMovable = false
                }, true, 2);
                MainViewSideMenu.NavigateRemove(this);
            }
            else if (IsImportMode)
            {
                // Navigate to ImportModeViewModel
                MainViewSideMenu.NavigateTo<HomeViewModel>();
            }
        }
    }
}
