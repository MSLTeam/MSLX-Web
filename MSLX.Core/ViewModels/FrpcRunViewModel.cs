using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using MSLX.Core.Utils;
using Newtonsoft.Json.Linq;
using Tommy;

namespace MSLX.Core.ViewModels
{
    public partial class FrpcRunViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _consoleLogs = "";

        [ObservableProperty] 
        private string _launchBtnText = "启动内网映射";
        
        [ObservableProperty]
        private string _frpcName = "";
        
        [ObservableProperty]
        private string _frpcIp = "";
        
        [ObservableProperty]
        private string _frpcDomain = "";
        
        public int FrpcId;
        private string FrpcService = "";
        private string FrpcConfigType = "toml";
        
        private Process process;
        
        public FrpcRunViewModel(int id)
        {
            FrpcId = id;
            FrpcService = (string)ConfigService.FrpList.GetFrpConfig(FrpcId)["Service"];
            FrpcName = (string)ConfigService.FrpList.GetFrpConfig(FrpcId)["Name"];
            FrpcConfigType = (string)ConfigService.FrpList.GetFrpConfig(FrpcId)["ConfigType"];
            // 处理不同格式的配置文件
            switch (FrpcConfigType)
            {
                case "toml":
                    string _frpcConfig = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Configs", "Frpc", FrpcId.ToString(),"frpc.toml"));
                    var reader = new StringReader(_frpcConfig);
                    TomlTable frpcToml = TOML.Parse(reader);
                    if (FrpcService == "MSLFrp")
                    {
                        FrpcDomain = $"{(string)frpcToml["metadatas"]["mslFrpRemoteDomain"]}:{(string)frpcToml["proxies"][0]["remotePort"]}";
                        FrpcIp = $"{(string)frpcToml["serverAddr"]}:{(string)frpcToml["proxies"][0]["remotePort"]}";
                    }
                    else
                    {
                        FrpcDomain = $"{(string)frpcToml["serverAddr"]}:{(string)frpcToml["proxies"][0]["remotePort"]}";
                        FrpcIp = $"{(string)frpcToml["serverAddr"]}:{(string)frpcToml["proxies"][0]["remotePort"]}";
                    }
                    break;
                case "cmd":
                    FrpcDomain = FrpcIp = "请启动隧道以查看连接地址！";
                    break;
            }

        }


        [RelayCommand]
        private async Task LaunchFrpc()
        {
            if (LaunchBtnText == "关闭内网映射")
            {
                ConsoleLogs = "正在关闭内网映射···";
                LaunchBtnText = "启动内网映射";
                process.Kill();
                return;
            }
            
            string _frpcFileName = "frpc";
            switch (FrpcService)
            {
                case "MSLFrp":
                    _frpcFileName = "frpc";
                    break;
            }
            
            // 检查Frpc程序是否存在
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile", $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}")))
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
                                    string downloadUrl = downloadInfo["data"]["cli"][0]["download"][PlatFormHelper.GetOs()]?[PlatFormHelper.GetOsArch()]?["url"].Value<string>() ?? "";
                                    if (downloadUrl == "")
                                    {
                                        throw new Exception("不存在支持此架构系统的Frpc客户端！");
                                    }
                                    MessageService.ShowToast("正在下载Frpc", "正在下载MSL Frpc客户端···", NotificationType.Information);
                                    // 下载文件
                                    await DownloadFile(downloadUrl,
                                        Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile",
                                            $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".zip" : ".tar.gz")}"));
                                    
                                    // 校验sha256
                                    if (!FileHelper.CheckFileSha256(Path.Combine(AppContext.BaseDirectory, "Configs",
                                                "FrpcExecutableFile",
                                                $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".zip" : ".tar.gz")}"),
                                            downloadInfo["data"]["cli"][0]["download"][PlatFormHelper.GetOs()]?[
                                                PlatFormHelper.GetOsArch()]?["sha256"].Value<string>()))
                                    {
                                        throw new Exception("Frpc客户端下载失败，请重试！");
                                    }
                                    
                                    // 解压
                                    if (PlatFormHelper.GetOs() == "Windows")
                                    {
                                        FileHelper.ExtractZip(Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile",
                                            $"{_frpcFileName}.zip"),
                                            Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile"));
                                    }
                                    else
                                    {
                                        FileHelper.ExtractTarGz(Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile",
                                            $"{_frpcFileName}.tar.gz"),
                                            Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile"));
                                    }

                                    ConsoleLogs = "解压成功！";
                                    File.Delete(Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile",
                                        $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".zip" : ".tar.gz")}"));
                                    
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
            // 处理启动参数
            string args;
            if (FrpcConfigType == "cmd")
            {
                args = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Configs", "Frpc",
                    FrpcId.ToString(), "frpc.cmd"));
            }
            else
            {
                args = $"-c frpc.{FrpcConfigType}";
            }
            
            // 进入启动流程
            if (PlatFormHelper.GetOs() != "Windows")
            {
                // 设置可执行权限
                FileHelper.SetFileExecutable(Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile",_frpcFileName));
            }

            ConsoleLogs = "开始启动Frpc进程···";
            // 创建一个新的 Process 对象
            process = new Process();
            // 设置启动信息
            process.StartInfo.FileName = Path.Combine(AppContext.BaseDirectory, "Configs", "FrpcExecutableFile", $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}");
            process.StartInfo.Arguments = args;
            process.StartInfo.WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "Configs", "Frpc",
                FrpcId.ToString());
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.OutputDataReceived += (_, e) => { ConsoleLogs += $"\n{e.Data?.Replace("\u001b[1;34m", "").Replace("\u001b[0m", "").Replace("\u001b[1;33m","")??""}";};
            process.ErrorDataReceived += (_, e) => { ConsoleLogs += $"\n{e.Data?.Replace("\u001b[1;34m", "").Replace("\u001b[0m", "").Replace("\u001b[1;33m","")??""}"; };
            process.Exited += (_,_) =>
            {
                LaunchBtnText = "启动内网映射";
                ConsoleLogs += "Frpc进程已退出！";
            };
            // 启动进程
            process.EnableRaisingEvents = true;
            process.Start();
            LaunchBtnText = "关闭内网映射";
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            
        }

        [RelayCommand]
        private void CopyDomain()
        {
            StringHelper.CopyToClipboard(FrpcDomain);
            MessageService.ShowToast("复制成功", "复制连接地址到剪切板成功！", NotificationType.Success);
        }
        
        [RelayCommand]
        private void CopyIp()
        {
            StringHelper.CopyToClipboard(FrpcIp);
            MessageService.ShowToast("复制成功", "复制备用连接地址到剪切板成功！", NotificationType.Success);
        }

        private async Task DownloadFile(string url, string filename)
        {
            var downloadOpt = new DownloadConfiguration()
            {
                ChunkCount = 1 // Number of file parts, default is 1
                //ParallelDownload = true // Download parts in parallel (default is false)
            };
            var downloader = new DownloadService(downloadOpt);
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            downloader.DownloadFileCompleted += OnDownloadFileCompleted;

            await downloader.DownloadFileTaskAsync(url, filename);
        }
        private void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            ConsoleLogs = "下载成功！";
        }

        private void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
        {
            
                if (e.BytesPerSecondSpeed > 1024 * 1024)
                    ConsoleLogs = "正在下载Frpc客户端···\n文件大小: " + Math.Round((double)e.ReceivedBytesSize / 1024 / 1024, 2) +
                                              " MB / " + Math.Round((double)e.TotalBytesToReceive / 1024 / 1024, 2) +
                                              " MB \n" +
                                              "速度: " + Math.Round(e.BytesPerSecondSpeed / 1024 / 1024, 2) + " MB/s";
                else
                    ConsoleLogs = "正在下载Frpc客户端···\n文件大小: " + Math.Round((double)e.ReceivedBytesSize / 1024 / 1024, 2) +
                                              " MB / " + Math.Round((double)e.TotalBytesToReceive / 1024 / 1024, 2) +
                                              " MB \n" +
                                              "速度: " + Math.Round(e.BytesPerSecondSpeed / 1024, 2) + " kb/s";
        }

        [RelayCommand]
        public void BackToList()
        {
            MainViewModel.NavigateTo<FrpListViewModel>();
        }
        
    }
    
}