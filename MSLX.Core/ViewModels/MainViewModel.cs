using Material.Icons.Avalonia;
using Material.Icons;
using SukiUI.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace MSLX.Core.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public static ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    public static ISukiToastManager ToastManager { get; } = new SukiToastManager();
    public static ServerListViewModel ServerListView { get; } = new ServerListViewModel();
    
    private readonly ObservableCollection<SukiSideMenuItem> _mainPages = new()
    {
        new SukiSideMenuItem
        {
            Header = "主页",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.Home,
            },
            PageContent = new HomeViewModel(),
        },
        new SukiSideMenuItem
        {
            Header = "服务器列表",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.ViewList,
            },
            PageContent = ServerListView,
        },
        new SukiSideMenuItem
        {
            Header = "内网映射",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.NavigationVariant,
            },
            PageContent = new FrpListViewModel(),
        },
        new SukiSideMenuItem
        {
            Header = "点对点联机",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.SwapHorizontalBold,
            },
            PageContent = new P2PConnectViewModel(),
        },
        new SukiSideMenuItem
        {
            Header = "设置",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.Settings,
            },
            PageContent = new SettingsViewModel(),
        },
        new SukiSideMenuItem
        {
            Header = "关于",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.Info,
            },
            PageContent = new AboutViewModel(),
        }
    };

    public ObservableCollection<SukiSideMenuItem> MainPages
    {
        get => _mainPages;
    }

    private SukiSideMenuItem? _activePage;
    public SukiSideMenuItem? ActivePage
    {
        get => _activePage;
        set
        {
            _activePage = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        ActivePage = MainPages.FirstOrDefault();
    }
}
