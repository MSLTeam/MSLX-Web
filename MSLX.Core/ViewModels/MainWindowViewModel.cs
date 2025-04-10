using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Toasts;

namespace MSLX.Core.ViewModels
{
    
    public partial class  MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        public ISukiToastManager _toastManager = new SukiToastManager();
    }
}