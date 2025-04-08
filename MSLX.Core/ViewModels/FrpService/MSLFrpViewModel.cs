using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MSLX.Core.ViewModels.FrpService
{
    public class Tunnel : INotifyPropertyChanged
    {
        private string _name;
        private string _status;
        private string _localPort;
        private string _remotePort;
        private string _node;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string LocalPort
        {
            get => _localPort;
            set { _localPort = value; OnPropertyChanged(); }
        }

        public string RemotePort
        {
            get => _remotePort;
            set { _remotePort = value; OnPropertyChanged(); }
        }
        
        public string Node
        {
            get => _node;
            set { _node = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MSLFrpViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<Tunnel> _tunnels;
        
        [ObservableProperty]
        private string _username = "loading";
        
        public MSLFrpViewModel()
        {
            Tunnels = new ObservableCollection<Tunnel>
            {
                new Tunnel()
                {
                    Name = "Tunnel1",
                    Status = "Running",
                    LocalPort = "8080",
                    RemotePort = "80",
                    Node = "Node1"
                }
            };
        }
    }
}