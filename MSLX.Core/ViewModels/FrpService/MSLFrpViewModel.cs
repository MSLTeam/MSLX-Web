using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using static MSLX.Core.Models.FrpService.MSLFrpModel;

namespace MSLX.Core.ViewModels.FrpService
{
    public partial class MSLFrpViewModel : ViewModelBase
    {
        // 存储的隧道
        [ObservableProperty]
        private ObservableCollection<Tunnel> _tunnels;
        
        // 用户信息字段
        [ObservableProperty]
        private string _username = "loading";
        
        [ObservableProperty]
        private string _userGroup = "普通用户";
        
        [ObservableProperty]
        private string _userMaxTunnels = "3";
        
        [ObservableProperty]
        private string _userOutdated = "loading";

        // 选中的隧道
        [ObservableProperty]
        private Tunnel _selectedTunnel;

        public MSLFrpViewModel()
        {
            Tunnels = new ObservableCollection<Tunnel>
            {
                new Tunnel()
                {
                    Name = "Tunnel114514",
                    Status = "Running",
                    LocalPort = "8080",
                    RemotePort = "80",
                    Node = "Node1"
                }
            };
        }
    }
}