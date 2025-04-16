using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Models;
using MSLX.Core.Views.CreateServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels.CreateServer
{
    public partial class FastModeViewModel : ViewModelBase
    {
        // Design time data
        public FastModeViewModel()
        {
            ServerModel = new CreateServerModel { Name = "Design Time" };
            MainContent = new FastModeStep1ViewModel(ServerModel, this);
            Title += $" - {ServerModel.Name}";
        }

        private CreateServerModel ServerModel { get; set; }
        public FastModeViewModel(CreateServerModel createServerModel)
        {
            ServerModel = createServerModel;
            Title += $" - {ServerModel.Name}";
            MainContentList.Add(new FastModeStep1View { DataContext = new FastModeStep1ViewModel(ServerModel, this) });
            MainContentList.Add(new FastModeStep2View { DataContext = new FastModeStep2ViewModel(ServerModel, this) });
            MainContent = MainContentList[0];
        }

        public ObservableCollection<Control> MainContentList { get; set; } = new ObservableCollection<Control>();

        [ObservableProperty]
        private object? _mainContent;

        public string Title { get; set; } = "快速模式";

        public void SetMainContent(object content)
        {
            MainContent = content;
        }

        public void InstallServer()
        {
            Debug.WriteLine("InstallServer");
            Debug.WriteLine(ServerModel.Name);
        }
    }
}
