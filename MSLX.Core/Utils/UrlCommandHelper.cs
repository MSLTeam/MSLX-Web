using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;

namespace MSLX.Core.Utils
{
    public class UrlCommandHelper
    {
        private static readonly UrlCommandHelper _instance = new UrlCommandHelper();
        public static UrlCommandHelper Default => _instance;
        public IRelayCommand OpenUrlCommand { get; } = new RelayCommand<string>(OpenUrl);

        private static void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"打开链接失败: {ex.Message}");
            }
        }
    }
}
