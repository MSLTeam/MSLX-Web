using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    [ObservableProperty] private bool _isHostEnabled = true;
    [ObservableProperty] private bool _isVisitorEnabled = true;

    [ObservableProperty] private string _consoleLogs = string.Empty;

    [ObservableProperty] private string _btnCreate = "创建房间";

    [ObservableProperty] private string _btnCreateVisitor = "加入房间";

    [ObservableProperty] private string _hostName;

    [ObservableProperty] private string _hostPassword;

    [ObservableProperty] private int _hostPort = 25565;

    [ObservableProperty] private string _visitorName = string.Empty;

    [ObservableProperty] private string _visitorPassword = string.Empty;

    [ObservableProperty] private int _visitorPort = 25565;

    private Process? FrpcProcess { get; set; }

    public P2PConnectViewModel()
    {
        HostName = StringHelper.GetRandomNumber(10000, 999999).ToString();
        HostPassword = StringHelper.GenerateRandomString(6, "MSLX");
        HostPort = 25565;
        VisitorPort = 25565;
    }

    [RelayCommand]
    private void Loaded()
    {
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
                        VisitorName = frpcToml["visitors"]?[0]?["serverName"]?.ToString() ?? string.Empty; ;
                        VisitorPassword = frpcToml["visitors"]?[0]?["secretKey"]?.ToString() ?? string.Empty;
                        VisitorPort = frpcToml["visitors"][0]["bindPort"];
                        IsHostExpand = false;
                    }
                    else
                    {
                        // 配置是房主
                        HostName = frpcToml["proxies"]?[0]?["name"]?.ToString() ?? string.Empty;
                        HostPassword = frpcToml["proxies"]?[0]?["secretKey"]?.ToString() ?? string.Empty;
                        HostPort = frpcToml["proxies"][0]["localPort"];
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
    private async Task LaunchHost()
    {
        if (BtnCreate == "关闭房间")
        {
            if (FrpcProcess != null && !FrpcProcess.HasExited)
            {
                FrpcProcess.Kill();
                //FrpcProcess.Dispose();
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
            await RunFrpc();
        }
        catch (Exception e)
        {
            ConsoleLogs = $"写入配置文件失败！{e.Message}";
            return;
        }
    }

    [RelayCommand]
    private async Task LaunchVisitor()
    {
        if (BtnCreateVisitor == "退出房间")
        {
            if (FrpcProcess != null && !FrpcProcess.HasExited)
            {
                FrpcProcess.Kill();
                //FrpcProcess.Dispose();
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
            await RunFrpc();
        }
        catch (Exception e)
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
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                JObject json = JObject.Parse(response.Content);
                if (json["code"]?.Value<int>() == 200)
                {
                    JToken? data = json["data"];
                    if (data == null)
                    {
                        ConsoleLogs = "获取桥接服务器失败！";
                        return "";
                    }
                    // 第一个属性
                    var firstCategory = ((JObject)data).Properties().First().Name;
                    JToken? firstItem = data[firstCategory];
                    if (firstItem == null)
                    {
                        ConsoleLogs = "获取桥接服务器失败！";
                        return "";
                    }
                    // 第一个节点
                    var firstNode = ((JObject)firstItem).Properties().First().Name;
                    JToken? serverAddr = data[firstCategory]?[firstNode]?["server_addr"];
                    JToken? serverPort = data[firstCategory]?[firstNode]?["server_port"];
                    if (serverAddr == null || serverPort == null)
                    {
                        ConsoleLogs = "获取桥接服务器失败！";
                        return "";
                    }
                    return $"{serverAddr.ToString()}:{serverPort.ToString()}";
                }
                ConsoleLogs = $"获取桥接服务器失败！{json["msg"]}";
                return "";
            }
            ConsoleLogs = $"获取桥接服务器失败！{response.StatusCode}";
            return "";
        }
        catch (Exception e)
        {
            ConsoleLogs = $"获取桥接服务器失败！{e.Message}";
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

        if (PlatFormHelper.GetOs() != "Windows")
        {
            FileHelper.SetFileExecutable(Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile",
                $"frpc"));
        }

        ConsoleLogs = "联机服务启动中...";
        try
        {
            // 创建一个新的 Process 对象
            FrpcProcess = new Process();
            // 设置启动信息
            FrpcProcess.StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(ConfigService.GetAppDataPath(), "Configs", "FrpcExecutableFile", $"frpc{(PlatFormHelper.GetOs() == "Windows" ? ".exe" : "")}"),
                Arguments = "-c p2p.toml",
                WorkingDirectory = Path.Combine(ConfigService.GetAppDataPath(), "Configs", "Frpc"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            FrpcProcess.OutputDataReceived += FrpcProcessLogRecieved;
            FrpcProcess.ErrorDataReceived += FrpcProcessLogRecieved;
            FrpcProcess.Exited += (_, _) =>
            {
                if (IsHostExpand)
                {
                    IsVisitorEnabled = true;
                    BtnCreate = "创建房间";
                    ConsoleLogs += "房间已关闭！";
                }
                else
                {
                    IsHostEnabled = true;
                    BtnCreateVisitor = "加入房间";
                    ConsoleLogs += "已退出房间！";
                }
                FrpcProcess.OutputDataReceived -= FrpcProcessLogRecieved;
                FrpcProcess.ErrorDataReceived -= FrpcProcessLogRecieved;
                FrpcProcess.Dispose();
            };
            // 启动进程
            FrpcProcess.EnableRaisingEvents = true;
            FrpcProcess.Start();
            FrpcProcess.BeginErrorReadLine();
            FrpcProcess.BeginOutputReadLine();
            
            if (IsHostExpand)
            {
                BtnCreate = "关闭房间";
                IsVisitorEnabled = false;
            }
            else
            {
                BtnCreateVisitor = "退出房间";
                IsHostEnabled = false;
            }
                
            //await Task.Run(FrpcProcess.WaitForExit);
            //FrpcProcess.Dispose();
        }
        catch
        {
            ConsoleLogs = "联机服务启动失败！";
            return;
        }

    }

    private void FrpcProcessLogRecieved(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) return;
        ConsoleLogs += $"{e.Data?.Replace("\u001b[1;34m", "").Replace("\u001b[0m", "").Replace("\u001b[1;33m", "") ?? ""}\n";
        if (e.Data?.Contains("start proxy success") ?? false)
        {
            ConsoleLogs += "联机服务启动成功！\n";
        }
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
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    JObject downloadInfo = JObject.Parse(response.Content);
                    if (downloadInfo["code"]?.Value<int>() == 200)
                    {
                        string downloadUrl =
                            downloadInfo["data"]?["cli"]?[0]?["download"]?[PlatFormHelper.GetOs()]?[
                                PlatFormHelper.GetOsArch()]?["url"]?.Value<string>() ?? "";
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
                                downloadInfo["data"]?["cli"]?[0]?["download"]?[PlatFormHelper.GetOs()]?[
                                    PlatFormHelper.GetOsArch()]?["sha256"]?.Value<string>() ?? string.Empty))
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
                    MessageService.ShowToast("下载Frpc客户端失败", (response?.ResponseException as Exception)?.Message ?? string.Empty, NotificationType.Error);
                    //throw new Exception("下载Frpc客户端失败");
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