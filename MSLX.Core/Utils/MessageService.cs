using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MSLX.Core.ViewModels;
using SukiUI.Toasts;

namespace MSLX.Core.Utils
{

    public class MessageService
    {
        public static void ShowToast(string title,string msg, NotificationType type)
        {
            MainViewModel.ToastManager
                .CreateToast()
                .OfType(NotificationType.Information).WithTitle(title)
                .WithContent(msg)
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Dismiss().ByClicking().OfType(type)
                .Queue();
        }
    }
}