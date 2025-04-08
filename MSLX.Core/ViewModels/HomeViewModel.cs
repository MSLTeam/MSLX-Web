using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MSLX.Core.Utils;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MSLX.Core.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _announcement = "Loading...";
        /*
        public string Announcement
        {
            get => _announcement
            set
            {
                if (SetProperty(ref _announcement, value))
                {
                    OnPropertyChanged(nameof(Announcement));
                }
            }
        }
        */
        public string StartServerText { get; } = "开启服务器";

        [RelayCommand]
        private async Task Loaded()
        {
            var (Success, Data, Message) = await MSLApi.GetDataAsync("/query/notice", queryParameters: new Dictionary<string, string> { { "query", "noticeMd" } });
            if (Data == null || Message == null)
            {
                Announcement = "暂无公告";
                return;
            }
            if (Success)
            {
                Announcement = ((JObject)Data)["noticeMd"]?.ToString() ?? "暂无公告";
            }
            else
            {
                Announcement = Message;
            }
        }
        /*
        public RelayCommand LoadedCommand { get; }
        public HomeViewModel() { LoadedCommand = new RelayCommand(Loaded); }
        */

        private bool _isFullScreen = false;
        [RelayCommand]
        private void StartServer()
        {
            
        }

        [RelayCommand]
        private void Test()
        {
            if (_isFullScreen)
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
