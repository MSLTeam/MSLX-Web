using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace MSLX.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        public string Announcement { get; } = "公告";

        [ObservableProperty]
        private string _announcementContent = "公告";
        /*
        public string AnnouncementContent
        {
            get => _announcementContent;
            set
            {
                if (SetProperty(ref _announcementContent, value))
                {
                    OnPropertyChanged(nameof(AnnouncementContent));
                }
            }
        }
        */
        public string StartServerText { get; } = "开启服务器";
        public string P2PBtn { get; } = "点对点联机";

        [RelayCommand]
        private void Loaded()
        {
            AnnouncementContent = "我是公告";
        }
        /*
        public RelayCommand LoadedCommand { get; }
        public HomeViewModel() { LoadedCommand = new RelayCommand(Loaded); }
        */

        private bool _isFullScreen = false;
        [RelayCommand]
        private void StartServer()
        {
            AnnouncementContent = "我是公告111";
            if(_isFullScreen)
            {
                WeakReferenceMessenger.Default.Send(new WindowStateMessage(WindowState.Normal));
                _isFullScreen = false;
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new WindowStateMessage(WindowState.FullScreen));
                _isFullScreen = true;
            }
        }
    }
    public class WindowStateMessage
    {
        public WindowState State { get; }
        public WindowStateMessage(WindowState state)
        {
            State = state;
        }
    }
}
