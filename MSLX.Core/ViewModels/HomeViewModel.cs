using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace MSLX.Core.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        public string Announcement { get; } = "公告";
        private string _noticeMd = "# 欢迎使用MSLX \nMSLTeam最新力作 - 跨平台开服器\n## \ud83d\udce2 MSL Frp 公告\n\n### \ud83d\udcc5 节点变更公告\n#### 2025年3月30日更新\n- \u200b**\u200b德阳节点\u200b**\u200b：变更为免费节点，需完成[实名认证](https://user.mslmc.net/user/profile)后使用\n- \u200b**\u200b认证特权\u200b**\u200b：高级用户可享免费实名通道\n\n#### 2025年4月1日更新\n- \u26a0\ufe0f \u200b**\u200b广州1节点\u200b**\u200b：永久下线，请迁移至新广州节点\n- \ud83d\ude80 \u200b**\u200b迁移建议\u200b**\u200b：推荐使用同区域负载均衡节点保障服务稳定性\n\n## \u26a0\ufe0f 重要提醒\n1. \u200b**\u200b认证要求\u200b**\u200b  \n   \ud83d\udccc 国内节点强制实名制（依据《网络安全法》第24条）  \n   \ud83d\udccc 认证路径：MSL用户中心 > 个人中心 > 实名认证\n\n2. \u200b**\u200b使用规范\u200b**\u200b  \n   \u274c 禁止搭建下载站/P2P服务  \n   \u274c 禁止持续占用>50Mbps带宽  \n   \u2705 合规用途：游戏联机等";
        public string NoticeMd
        {
            get => _noticeMd;
            set
            {
                if (_noticeMd != value)
                {
                    _noticeMd = value;
                    OnPropertyChanged(nameof(NoticeMd));
                }
            }
        }
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
