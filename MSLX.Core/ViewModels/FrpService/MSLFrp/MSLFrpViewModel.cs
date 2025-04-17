using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Utils;
using Newtonsoft.Json.Linq;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using static MSLX.Core.Models.FrpService.MSLFrpModel;

namespace MSLX.Core.ViewModels.FrpService.MSLFrp
{
    public partial class MSLFrpViewModel : ViewModelBase
    {
        public string UserToken { get; set; }
        
        [ObservableProperty]
        private object? _mainContent;

        public MSLFrpViewModel()
        {
            UserToken = string.Empty;
        }

        [RelayCommand]
        private async Task Inited()
        {

            var _token = ConfigService.Config.ReadConfigKey("MSLUserToken")?.ToString();
            if (!string.IsNullOrEmpty(_token))
            {
                UserToken = _token;
                await GetFrpInfoAsync();
            }
            else
            {
                MainContent = new LoginViewModel(this);
            }
        }

        public async Task GetFrpInfoAsync()
        {
            try
            {
                // 添加自动登录中弹窗
                var dialog = MainViewModel.DialogManager.CreateDialog()
                    .WithTitle("登录中")
                    .WithContent(new TextBlock { Text = "获取用户信息……" });
                dialog.TryShow();

                // 获取MSL Frp用户信息
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/userInfo", null, new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {UserToken}"
                });
                MainViewModel.DialogManager.DismissDialog();
                if (response.IsSuccessStatusCode == false || response.Content == null)
                {
                    MessageService.ShowToast("获取用户信息失败，请重新登录", (response.ResponseException as Exception)?.Message ?? string.Empty, NotificationType.Error);
                    //ConfigService.Config.WriteConfigKey("MSLUserToken", string.Empty);
                    MainContent = new LoginViewModel(this);
                    return;
                }
                JObject json = JObject.Parse(response.Content);
                if (json["code"]?.Value<int>() == 200)
                {
                    MessageService.ShowToast("登录成功！", "成功登录到MSL Frp服务", NotificationType.Success);

                    MainContent = new FrpMainViewModel(json)
                    {
                        UserToken = UserToken,
                    };
                }
                else
                {
                    MessageService.ShowToast("获取用户信息失败", json["msg"]?.ToString() ?? string.Empty, NotificationType.Error);
                    Debug.WriteLine(json["msg"]?.ToString() ?? string.Empty);
                    return;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MainViewModel.DialogManager.DismissDialog();
                MessageService.ShowToast("获取用户信息失败", ex.Message, NotificationType.Error);
                //ShowMainPage = false;
            }
            
            return;
        }
    }
}