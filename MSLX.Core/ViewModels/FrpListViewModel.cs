using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MSLX.Core.Utils;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using MSLX.Core.Models;
using SukiUI.Controls;
using System.Linq;

namespace MSLX.Core.ViewModels
{
    public partial class FrpListViewModel : ViewModelBase
    {
        private FrpcListModel.FrpConfig _selectedFrpConfig;

        [ObservableProperty]
        private ObservableCollection<FrpcListModel.FrpConfig> _frpConfigs ;

        public FrpcListModel.FrpConfig SelectedFrpConfig
        {
            get => _selectedFrpConfig;
            set
            {
                _selectedFrpConfig = value;
                OnPropertyChanged();
            }
        }

        public FrpListViewModel()
        {
            LoadFrpConfigs();
        }

        public void LoadFrpConfigs()
        {
            var frpList = ConfigService.FrpList.GetFrpList();
            FrpConfigs = new ObservableCollection<FrpcListModel.FrpConfig>();
            FrpConfigs.Clear();
            foreach (var config in frpList)
            {
                FrpConfigs.Add(new FrpcListModel.FrpConfig
                {
                    Id = (int)config["ID"],
                    Name = (string)config["Name"],
                    Service = (string)config["Service"],
                    ConfigType = (string)config["ConfigType"]
                });
            }
        }

        [RelayCommand]
        private void OpenFrpConfig()
        {
            if (SelectedFrpConfig != null)
            {
                var existingPage = App.MainView.MainPages.FirstOrDefault(p => p.PageContent is FrpcRunViewModel runFrpcViewModel && runFrpcViewModel.FrpcId == SelectedFrpConfig.Id);
                if (existingPage != null)
                {
                    App.MainView.ActivePage = existingPage;
                    return;
                }

                var newPage = new SukiSideMenuItem
                {
                    PageContent = new FrpcRunViewModel(SelectedFrpConfig.Id),
                    IsVisible = false, // 隐藏菜单项
                };

                App.MainView.MainPages.Add(newPage);
                App.MainView.ActivePage = newPage;
            }
        }

        [RelayCommand]
        private void AddFrpcConfig()
        {
            MainViewSideMenu.NavigateRemove<FrpProviderViewModel>();

            MainViewSideMenu.NavigateTo(new SukiSideMenuItem
            {
                Header = "添加隧道",
                Icon = new MaterialIcon()
                {
                    Kind = MaterialIconKind.Add,
                },
                PageContent = new FrpProviderViewModel(),
                IsContentMovable = false
            }, true, MainViewSideMenu.GetActivePageIndex() + 1);
        }

        [RelayCommand]
        private void DeleteFrpcConfig()
        {
            if (SelectedFrpConfig != null)
            {
                ConfigService.FrpList.DeleteFrpConfig(SelectedFrpConfig.Id);
                LoadFrpConfigs();
            }
        }

        [RelayCommand]
        private void Loaded()
        {
            LoadFrpConfigs();
        }
    }
}