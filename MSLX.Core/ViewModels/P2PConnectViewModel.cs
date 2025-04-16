using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using MSLX.Core.Utils;
using Newtonsoft.Json.Linq;
using Tommy;

namespace MSLX.Core.ViewModels;

public partial class P2PConnectViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isHostExpand = true;

    [ObservableProperty] private string _consoleLogs = "";

    [ObservableProperty] private string _btnCreate = "创建房间";
    
    [ObservableProperty] private string _btnCreateVisitor = "加入房间";

    [ObservableProperty] private string _hostName;
    
    [ObservableProperty] private string _hostPassword;
    
    [ObservableProperty] private string _hostPort;
    
    [ObservableProperty] private string _visitorName;
    
    [ObservableProperty] private string _visitorPassword;
    
    [ObservableProperty] private string _visitorPort;

    private Process process;
    
    public P2PConnectViewModel()
    {
        HostName = StringHelper.GetRandomNumber(10000, 999999).ToString();
        HostPassword = StringHelper.GenerateRandomString(6, "MSLX");
        HostPort = "25565";
        VisitorPort = "25565";
        try
        {
            if (File.Exists(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc", "p2p.toml")))
            {
                string configs = File.ReadAllText(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc", "p2p.toml"));
                if (configs.Contains("xtcp"))
                {
                    var reader = new StringReader(configs);
                    TomlTable frpcToml = TOML.Parse(reader);
                    if (configs.Contains("visitors"))
                    {
                        // 配置是成员
                        VisitorPort = frpcToml["visitors"][0]["bindPort"].ToString();
                        VisitorName = frpcToml["visitors"][0]["serverName"].ToString();
                        VisitorPassword = frpcToml["visitors"][0]["secretKey"].ToString();
                        IsHostExpand = false;
                    }
                    else
                    {
                        // 配置是房主
                        HostName = frpcToml["proxies"][0]["name"].ToString();
                        HostPassword = frpcToml["proxies"][0]["secretKey"].ToString();
                        HostPort = frpcToml["proxies"][0]["localPort"].ToString();
                    }
                }
                else
                {
                    File.Delete(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc", "p2p.toml"));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private async void LaunchHost()
    {
        if (BtnCreate == "关闭房间")
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
                process.Dispose();
                ConsoleLogs = "已关闭房间！";
                BtnCreate = "创建房间";
                BtnCreateVisitor = "加入房间";
            }
            return;
        }
        try
        {
            string server = await GetBridgeServer();
            if (server == "")
            {
                return;
            }
            string configs = $"serverAddr = \"{server.Split(":")[0]}\"\nserverPort = {server.Split(":")[1]}\n\n[[proxies]]\nname = \"{HostName}\"\ntype = \"xtcp\"\nlocalIP = \"127.0.0.1\"\nlocalPort = {HostPort}\nsecretKey = \"{HostPassword}\"";
            if (!Directory.Exists(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc")))
            {
                Directory.CreateDirectory(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc"));
            }
            File.WriteAllText(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc", "p2p.toml"), configs);
            RunFrpc();
        }catch (Exception e)
        {
            ConsoleLogs = $"写入配置文件失败！{e.Message}";
            return;
        }
    }
    
    [RelayCommand]
    private async void LaunchVisitor()
    {
        if (BtnCreateVisitor == "退出房间")
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
                process.Dispose();
                ConsoleLogs = "已退出房间！";
                BtnCreate = "创建房间";
                BtnCreateVisitor = "加入房间";
            }
            return;
        }
        try
        {
            string server = await GetBridgeServer();
            if (server == "")
            {
                return;
            }
            string configs = $"serverAddr = \"{server.Split(":")[0]}\"\nserverPort = {server.Split(":")[1]}\n\n[[visitors]]\nname = \"msl_p2p_visitor\"\ntype = \"xtcp\"\nserverName = \"{VisitorName}\"\nsecretKey = \"{VisitorPassword}\"\nbindAddr = \"127.0.0.1\"\nbindPort = {VisitorPort}\n";
            if (!Directory.Exists(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc")))
            {
                Directory.CreateDirectory(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc"));
            }
            File.WriteAllText(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc", "p2p.toml"), configs);
            RunFrpc();
        }catch (Exception e)
        {
            ConsoleLogs = $"写入配置文件失败！{e.Message}";
            return;
        }
    }

    private async Task<string> GetBridgeServer()
    {
        try
        {
            HttpService.HttpResponse response = await MSLApi.GetAsync("/query/frp/MSLFrps", null);
            if (response.IsSuccessStatusCode)
            {
                JObject json = JObject.Parse(response.Content);
                if ((int)json["code"] == 200)
                {
                    return $"{(string)json["data"]["免费"]["免费节点请前往MSL-Frp(NEW)"]["server_addr"]}:{(string)json["data"]["免费"]["免费节点请前往MSL-Frp(NEW)"]["server_port"]}";
                }
                ConsoleLogs = $"获取搭桥服务器失败！{json["msg"]}";
                return "";
            }
            ConsoleLogs = $"获取搭桥服务器失败！{response.StatusCode}";
            return "";
        }catch (Exception e)
        {
            ConsoleLogs = $"获取搭桥服务器失败！{e.Message}";
            return "";
        }
    }

    private async Task RunFrpc()
    {
        if (!File.Exists(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc", "p2p.toml")))
        {
            ConsoleLogs = "配置文件不存在 联机服务启动失败！";
            return;
        }
        await CheckAndDownloadFrpc();
        if (!File.Exists(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile",
                $"frpc{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}")))
        {
            ConsoleLogs = "Frpc不存在 联机服务启动失败！";
            return;
        }

        ConsoleLogs = "联机服务启动中...";
        // 创建一个新的 Process 对象
        process = new Process();
        // 设置启动信息
        process.StartInfo.FileName = Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile", $"frpc{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}");
        process.StartInfo.Arguments = "-c p2p.toml";
        process.StartInfo.WorkingDirectory = Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc");
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.OutputDataReceived += (_, e) => { ConsoleLogs += $"\n{e.Data?.Replace("\u001b[1;34m", "").Replace("\u001b[0m", "").Replace("\u001b[1;33m","")??""}";};
        process.ErrorDataReceived += (_, e) => { ConsoleLogs += $"\n{e.Data?.Replace("\u001b[1;34m", "").Replace("\u001b[0m", "").Replace("\u001b[1;33m","")??""}"; };
        process.Exited += (_,_) =>
        {
            BtnCreate = "创建房间";
            BtnCreateVisitor = "加入房间";
            ConsoleLogs += "Frpc进程已退出！";
        };
        // 启动进程
        process.EnableRaisingEvents = true;
        process.Start();
        BtnCreate = "关闭房间";
        BtnCreateVisitor = "退出房间";
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

    }
    
    private async Task CheckAndDownloadFrpc()
    {
        string _frpcFileName = "frpc";
        // 检查Frpc程序是否存在
        if (!File.Exists(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile",
                $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}")))
        {
            // 开始下载Frpc

            try
            {
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/download", null);
                if (response.IsSuccessStatusCode)
                {
                    JObject downloadInfo = JObject.Parse(response.Content);
                    if (downloadInfo["code"]?.Value<int>() == 200)
                    {
                        string downloadUrl =
                            downloadInfo["data"]["cli"][0]["download"][PlatFormHelper.GetOs()]?[
                                PlatFormHelper.GetOsArch()]?["url"].Value<string>() ?? "";
                        if (downloadUrl == "")
                        {
                            throw new Exception("不存在支持此架构系统的Frpc客户端！");
                        }

                        MessageService.ShowToast("正在下载Frpc", "正在下载Frpc客户端···", NotificationType.Information);
                        // 下载文件
                        await DownloadFile(downloadUrl,
                            Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile",
                                $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".zip" : ".tar.gz")}"));

                        // 校验sha256
                        if (!FileHelper.CheckFileSha256(Path.Combine(ConfigService.GetAppDataPath(), "Configs",
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
                            FileHelper.ExtractZip(Path.Combine(ConfigService.GetAppDataPath(), "Configs",
                                    "FrpcExecutableFile",
                                    $"{_frpcFileName}.zip"),
                                Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile"));
                        }
                        else
                        {
                            FileHelper.ExtractTarGz(Path.Combine(ConfigService.GetAppDataPath(), "Configs",
                                    "FrpcExecutableFile",
                                    $"{_frpcFileName}.tar.gz"),
                                Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile"));
                        }

                        ConsoleLogs = "解压成功！";
                        File.Delete(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile",
                            $"{_frpcFileName}{(PlatFormHelper.GetOs() == "Windows" ? ".zip" : ".tar.gz")}"));
                    }
                }
                else
                {
                    throw new Exception("下载Frpc客户端失败");
                }
            }
            catch (Exception ex)
            {
                MessageService.ShowToast("下载Frpc客户端失败", ex.Message, NotificationType.Error);
            }
        }
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
}