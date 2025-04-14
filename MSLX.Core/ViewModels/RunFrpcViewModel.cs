using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Utils;
using Newtonsoft.Json.Linq;
using Tommy;

namespace MSLX.Core.ViewModels
{
    public partial class RunFrpcViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _frpcName = "";
        
        [ObservableProperty]
        private string _frpcIp = "";
        
        [ObservableProperty]
        private string _frpcDomain = "";
        
        private int FrpcId = 944604;
        private string FrpcService = "";
        
        public RunFrpcViewModel()
        {
            FrpcService = (string)ConfigService.FrpList.GetFrpConfig(FrpcId)["Service"];
            FrpcName = (string)ConfigService.FrpList.GetFrpConfig(FrpcId)["Name"];
            string _frpcConfig = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Configs", "Frpc", FrpcId.ToString(),"frpc.toml"));
            using var reader = new System.IO.StringReader(_frpcConfig);
            TomlTable frpcToml = TOML.Parse(reader);
            FrpcDomain = $"{(string)frpcToml["metadatas"]["mslFrpRemoteDomain"]}:{(string)frpcToml["proxies"][0]["remotePort"]}";
            FrpcIp = $"{(string)frpcToml["serverAddr"]}:{(string)frpcToml["proxies"][0]["remotePort"]}";
        }

        [RelayCommand]
        private async Task LaunchFrpc()
        {
            string _frpcFileName = "frpc";
            switch (FrpcService)
            {
                case "MSLFrp":
                    _frpcFileName = "frpc";
                    break;
            }
            
            // 检查Frpc程序是否存在
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "Configs", "Frpc", $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}")))
            {
                // 开始下载Frpc
                switch (FrpcService)
                {
                    case "MSLFrp":
                        try
                        {
                            HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/download", null);
                            if (response.IsSuccessStatusCode)
                            {
                                JObject downloadInfo = JObject.Parse(response.Content);
                                if (downloadInfo["code"]?.Value<int>() == 200)
                                {
                                    string downloadUrl = downloadInfo["data"]["cli"][0]["download"][PlatFormHelper.GetOs()]?[PlatFormHelper.GetOs()]?["url"].Value<string>() ?? "";
                                    if (downloadUrl == "")
                                    {
                                        throw new Exception("不存在支持此架构系统的Frpc客户端！");
                                    }
                                    MessageService.ShowToast("正在下载MSL Frpc客户端", downloadUrl, NotificationType.Information);
                                }
                            }
                            else
                            {
                               throw new Exception("下载MSL Frpc客户端失败");
                            }
                        }catch(Exception ex)
                        {
                            MessageService.ShowToast("下载Frpc客户端失败", ex.Message, NotificationType.Error);
                        }
                        break;
                }
            }
        }
    }
    
}