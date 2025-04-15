using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MSLX.Core.Utils;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Material.Icons.Avalonia;
using MSLX.Core.Models;
using SukiUI.Controls;

namespace MSLX.Core.ViewModels
{
    public partial class FrpListViewModel : ViewModelBase
    {
        private FrpcListModel.FrpConfig _selectedFrpConfig;

        [ObservableProperty]
        public ObservableCollection<FrpcListModel.FrpConfig> _frpConfigs ;

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
        public void OpenFrpConfig()
        {
            if (SelectedFrpConfig != null)
            {
                MainViewModel.NavigateToRunFrpc(SelectedFrpConfig.Id,SelectedFrpConfig.Name);
            }
        }

        [RelayCommand]
        public void AddFrpcConfig()
        {
            MainViewModel.NavigateRemove<FrpTunnelViewModel>();
            MainViewModel.NavigateTo(new SukiSideMenuItem
            {
                Header = "添加隧道",
                Icon = new MaterialIcon()
                {
                    Kind = MaterialIconKind.Add,
                },
                PageContent = new FrpTunnelViewModel(),
                IsContentMovable = false
            }, true, 2);
        }

        [RelayCommand]
        public void DeleteFrpcConfig()
        {
            if (SelectedFrpConfig != null)
            {
                ConfigService.FrpList.DeleteFrpConfig(SelectedFrpConfig.Id);
                LoadFrpConfigs();
            }
        }

        [RelayCommand]
        public void Loaded()
        {
            LoadFrpConfigs();
        }
    }
}