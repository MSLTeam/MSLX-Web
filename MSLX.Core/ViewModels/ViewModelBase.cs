using CommunityToolkit.Mvvm.ComponentModel;
using MSLX.Core.Utils;

namespace MSLX.Core.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public static UrlCommandHelper UrlUtils => UrlCommandHelper.Default;
}
