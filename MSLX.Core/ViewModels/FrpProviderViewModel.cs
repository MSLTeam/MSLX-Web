using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.ViewModels.FrpService.MSLFrp;
using MSLX.Core.Views.FrpService.MSLFrp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels
{
    public partial class FrpProviderViewModel : ViewModelBase
    {
        [ObservableProperty] private int _selectedTab;
        [ObservableProperty] private object _mainContent;

        private readonly List<Control> _cachedViews = [];

        public FrpProviderViewModel()
        {
            var mslFrpView = new MSLFrpView { DataContext = new MSLFrpViewModel() };
            _cachedViews.Add(mslFrpView);

            MainContent = _cachedViews[0];
        }

        [RelayCommand]
        private void TabChanged()
        {
            if (SelectedTab >= _cachedViews.Count) return;
            MainContent = _cachedViews[SelectedTab];
        }
    }
}