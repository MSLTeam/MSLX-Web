using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MSLX.Core.Utils;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Models;

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

        private void LoadFrpConfigs()
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


    }
}