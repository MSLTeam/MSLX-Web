using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Utils;
using Newtonsoft.Json.Linq;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using static MSLX.Core.Models.FrpService.MSLFrpModel;

namespace MSLX.Core.ViewModels.FrpService.MSLFrp
{
    public partial class MSLFrpViewModel : ViewModelBase
    {
        public string UserToken { get; set; }
        

        [ObservableProperty]
        private object? _mainContent;
        

        [RelayCommand]
        private void Inited()
        {
            var _token = ConfigService.Config.ReadConfigKey("MSLUserToken")?.ToString();
            if (!string.IsNullOrEmpty(_token))
            {
                UserToken = _token;
                GetFrpInfo();
            }
            else
            {
                MainContent = new LoginViewModel(this);
            }
        }

        public void GetFrpInfo()
        {
            MainContent = new FrpMainViewModel()
            {
                UserToken = UserToken,
            };
            return;
        }

        public MSLFrpViewModel()
        {
            UserToken = string.Empty;
        }
    }
}