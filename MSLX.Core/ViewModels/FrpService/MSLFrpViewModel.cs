using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static MSLX.Core.Models.FrpService.MSLFrpModel;

namespace MSLX.Core.ViewModels.FrpService
{
    public partial class MSLFrpViewModel : ViewModelBase
    {
        // 存储的隧道
        [ObservableProperty]
        private ObservableCollection<Tunnel> _tunnels;
        
        // 存储的节点
        [ObservableProperty]
        private ObservableCollection<Node> _nodes;
        
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
        
        // 选中的隧道
        [ObservableProperty]
        private Node _selectedNode;

        [RelayCommand]
        private async Task Loaded()
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
            
            Nodes = new ObservableCollection<Node>
            {
                new Node()
                {
                    Name = "节点1",
                    Bandwidth = 100,
                    MaxOpenPort = 100,
                    MinOpenPort = 100,
                    NeedRealName = 0,
                    UdpSupport = 1,
                    HttpSupport = 0,
                    KcpSupport = 1,
                    AllowUserGroup = 1,
                    Remarks = "节点1的备注信息",
                    Status = 1,
                }
            };
        }

        public MSLFrpViewModel()
        {
            
        }
    }
}