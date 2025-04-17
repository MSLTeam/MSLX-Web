using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MSLX.Core.Utils
{
    public class UrlCommandHelper
    {
        // 使用单例模式
        private static readonly UrlCommandHelper _instance = new UrlCommandHelper();
        public static UrlCommandHelper Default => _instance;

        public ICommand OpenUrlCommand { get; } = new RelayCommand<string>(OpenUrl);

        private static void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                // 使用默认浏览器打开链接
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // 可以在这里添加日志记录
                Debug.WriteLine($"打开链接失败: {ex.Message}");
            }
        }
    }
}
