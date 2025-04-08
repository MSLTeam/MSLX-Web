using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using static MSLX.Core.Models.FrpService.MSLFrpModel;

namespace MSLX.Core.ViewModels.FrpService
{
    public partial class MSLFrpViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<Tunnel> _tunnels;
        
        [ObservableProperty]
        private string _username = "loading";
        
        [ObservableProperty]
        private string _usergroup = "loading";
        
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