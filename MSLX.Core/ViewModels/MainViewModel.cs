using Material.Icons.Avalonia;
using Material.Icons;
using SukiUI.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MSLX.Core.ViewModels.FrpService;
using SukiUI.Dialogs;
using Markdown.Avalonia;

namespace MSLX.Core.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public static ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
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
            PageContent = new MSLFrpViewModel(),
        },
        new SukiSideMenuItem
        {
            Header = "点对点联机",
            Icon = new MaterialIcon()
            {
                Kind = MaterialIconKind.SwapHorizontalBold,
            },
            PageContent = new AboutViewModel(),
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

    public static void NavigateTo<T>() where T : ViewModelBase
    {
        if (App.MainView == null) return;

        var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent is T);
        if (page != null)
            App.MainView.ActivePage = page;
    }

    public static void NavigateTo(ViewModelBase viewModel)
    {
        if (App.MainView == null) return;

        var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent.GetType() == viewModel.GetType());
        if (page != null)
            App.MainView.ActivePage = page;
    }

    public static void NavigateTo(SukiSideMenuItem sukiSideMenuItem, bool addToSideMenu = false, int insert = -1)
    {
        if (App.MainView == null) return;

        if (addToSideMenu)
        {
            if (insert != -1)
                App.MainView.MainPages.Insert(insert, sukiSideMenuItem);
            else
                App.MainView.MainPages.Add(sukiSideMenuItem);
        }
        App.MainView.ActivePage = sukiSideMenuItem;
    }

    public static void NavigateRemove(ViewModelBase viewModel)
    {
        if (App.MainView == null) return;

        var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent.GetType() == viewModel.GetType());
        if (page != null)
        {
            if (App.MainView.ActivePage == page)
                App.MainView.ActivePage = App.MainView.MainPages.FirstOrDefault();
            App.MainView.MainPages.Remove(page);
        }
    }

    public MainViewModel()
    {
        ActivePage = MainPages.FirstOrDefault();
    }
}
