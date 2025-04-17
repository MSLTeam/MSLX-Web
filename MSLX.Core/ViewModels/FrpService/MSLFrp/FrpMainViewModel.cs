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
    public partial class FrpMainViewModel : ViewModelBase
    {
        public string UserToken { get; set; } = string.Empty;
        // tabcontrol
        [ObservableProperty]
        private int _tabIndex;

        // 存储的隧道
        [ObservableProperty]
        private ObservableCollection<Tunnel> _tunnels = new ObservableCollection<Tunnel>();

        // 存储的节点
        [ObservableProperty]
        private ObservableCollection<Node> _nodes = new ObservableCollection<Node>();

        // 节点ID到名称映射字典
        private Dictionary<int, string> _nodeMap = new Dictionary<int, string>();

        // 用户信息字段
        [ObservableProperty]
        private string _username = "loading";

        [ObservableProperty]
        private string _userGroup = "普通用户";

        [ObservableProperty]
        private string _userMaxTunnels = "3";

        [ObservableProperty]
        private string _userOutdated = "loading";

        public int SelectedTunnelIndex
        {
            get => _selectedTunnelIndex;
            set
            {
                if (SetProperty(ref _selectedTunnelIndex, value))
                {
                    OnPropertyChanged(nameof(SelectedTunnelIndex));
                }
                if (value != -1)
                {
                    SelectedTunnel = Tunnels[value];
                }
            }
        }
        private int _selectedTunnelIndex = -1;

        public int SelectedNodeIndex
        {
            get => _selectedNodeIndex;
            set
            {
                if (SetProperty(ref _selectedNodeIndex, value))
                {
                    OnPropertyChanged(nameof(SelectedNodeIndex));
                }
                if (value != -1)
                {
                    SelectedNode = Nodes[value];
                }
            }
        }
        private int _selectedNodeIndex = -1;

        [ObservableProperty]
        private Tunnel _selectedTunnel = new Tunnel
        {
            Id = 0,
            LocalPort = 0,
            RemotePort = 0,
            Name = string.Empty,
            Node = string.Empty,
            Remarks = string.Empty,
            Status = string.Empty
        };

        [ObservableProperty]
        private Node _selectedNode = new Node
        {
            Id = 0,
            AllowUserGroup = 0,
            Bandwidth = 0,
            HttpSupport = false,
            KcpSupport = false,
            UdpSupport = false,
            MaxOpenPort = 0,
            MinOpenPort = 0,
            Name = string.Empty,
            NeedRealName = false,
            Remarks = string.Empty,
            Status = string.Empty,
            Type = string.Empty
        };

        // 创建节点相关字段
        [ObservableProperty]
        private string _createName;

        [ObservableProperty]
        private int _createType = 0;

        [ObservableProperty]
        private string _createLocalIp = "127.0.0.1";

        [ObservableProperty]
        private string _createLocalPort = "25565";

        [ObservableProperty]
        private string _createRemotePort;

        [ObservableProperty]
        private string? _createBindDomain;

        public FrpMainViewModel(JObject json)
        {
            Username = json["data"]?["name"]?.Value<string>() ?? string.Empty;
            int userGroup = json["data"]?["user_group"]?.Value<int>() ?? 0;
            UserGroup = userGroup == 6 ? "超级管理员"
                : userGroup == 1 ? "高级会员"
                : userGroup == 2 ? "超级会员" : "普通用户";
            UserMaxTunnels = json["data"]?["maxTunnelCount"]?.Value<string>() ?? string.Empty;
            UserOutdated = json["data"]?["outdated"]?.Value<long>() == 3749682420 ? "长期有效"
                : StringHelper.SecondsToDateTime(json["data"]?["outdated"]?.Value<long>() ?? 0).ToString();

            CreateRemotePort = StringHelper.GetRandomNumber(10240, 60000).ToString();
            CreateName = StringHelper.GenerateRandomString(6, "MSLX_");
        }

        // Design Time
        public FrpMainViewModel()
        {
            // 设计时数据
            Username = "MSLX";
            UserGroup = "普通用户";
            UserMaxTunnels = "3";
            UserOutdated = "长期有效";

            CreateRemotePort = StringHelper.GetRandomNumber(10240, 60000).ToString();
            CreateName = StringHelper.GenerateRandomString(6, "MSLX_");
        }

        [RelayCommand]
        private async Task GetFrpInfo()
        {
            await GetNodes();
            await GetTunnels();
        }

        [RelayCommand]
        private async Task GetNodes()
        {
            try
            {
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/nodeList", null, new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {UserToken}"
                });
                if (response.StatusCode != 200 || response.Content == null)
                {
                    MessageService.ShowToast("错误", "获取节点列表失败", NotificationType.Error);
                    return;
                }
                JObject json = JObject.Parse(response.Content);
                if (json["code"]?.Value<int>() != 200)
                {
                    MessageService.ShowToast("获取隧道列表失败", json["msg"]?.Value<string>() ?? "Err", NotificationType.Error);
                    return;
                }
                _nodeMap.Clear();
                JToken? data = json["data"];
                if (data == null || !data.HasValues)
                {
                    MessageService.ShowToast("获取节点列表失败", "没有节点数据", NotificationType.Error);
                    return;
                }

                foreach (var node in data)
                {
                    int id = node["id"]?.Value<int>() ?? 0;
                    string name = node["node"]?.Value<string>() ?? string.Empty;
                    _nodeMap[id] = name;

                    JToken? nodeData = json["data"];
                    if (nodeData == null || !nodeData.HasValues)
                    {
                        MessageService.ShowToast("获取节点列表失败", "没有节点数据", NotificationType.Error);
                        return;
                    }
                    Nodes.Clear();
                    foreach (var nodeItem in nodeData)
                    {
                        int nodeId = nodeItem["id"]?.Value<int>() ?? 0;
                        string nodeName = nodeItem["node"]?.Value<string>() ?? string.Empty;
                        _nodeMap[nodeId] = nodeName; // id和name的字典映射

                        // 填充到节点列表数据
                        Nodes.Add(new Node()
                        {
                            Id = nodeItem["id"]?.Value<int>() ?? 0,
                            AllowUserGroup = nodeItem["allow_user_group"]?.Value<int>() ?? 0,
                            Type = (nodeItem["allow_user_group"]?.Value<int>() ?? 0) == 0 ? "免费" : (nodeItem["allow_user_group"]?.Value<int>() ?? 1) == 1 ? "高级" : "超级",
                            Bandwidth = nodeItem["bandwidth"]?.Value<int>() ?? 0,
                            HttpSupport = (nodeItem["http_support"]?.Value<int>() ?? 0) == 1,
                            UdpSupport = (nodeItem["udp_support"]?.Value<int>() ?? 0) == 1,
                            KcpSupport = (nodeItem["kcp_support"]?.Value<int>() ?? 0) == 1,
                            MaxOpenPort = nodeItem["max_open_port"]?.Value<int>() ?? 0,
                            MinOpenPort = nodeItem["min_open_port"]?.Value<int>() ?? 0,
                            NeedRealName = (nodeItem["need_real_name"]?.Value<int>() ?? 0) == 1,
                            Name = nodeName,
                            Status = (nodeItem["status"]?.Value<int>() ?? 0) == 1 ? "在线" : "离线",
                            Remarks = nodeItem["remarks"]?.Value<string>() ?? string.Empty
                        });
                    }
                }

                if (Nodes.Count > 0)
                {
                    SelectedNodeIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [RelayCommand]
        private async Task GetTunnels()
        {
            try
            {
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/getTunnelList", null, new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {UserToken}"
                });
                if (response.StatusCode != 200 || response.Content == null)
                {
                    MessageService.ShowToast("错误", "获取隧道列表失败", NotificationType.Error);
                    return;
                }
                JObject json = JObject.Parse(response.Content);
                if (json["code"]?.Value<int>() != 200)
                {
                    MessageService.ShowToast("获取隧道列表失败", json["msg"]?.Value<string>() ?? "Err", NotificationType.Error);
                    return;
                }

                JToken? data = json["data"];
                if (data == null || !data.HasValues)
                {
                    MessageService.ShowToast("获取隧道列表失败", "没有隧道数据", NotificationType.Error);
                    return;
                }

                Tunnels.Clear();
                foreach (var tunnel in data)
                {
                    int nodeId = tunnel["node_id"]?.Value<int>() ?? 0;
                    string nodeName = _nodeMap.ContainsKey(nodeId) ? _nodeMap[nodeId] : "未知节点";
                    Tunnels.Add(new Tunnel()
                    {
                        Id = tunnel["id"]?.Value<int>() ?? 0,
                        Name = tunnel["name"]?.Value<string>() ?? string.Empty,
                        Remarks = tunnel["remarks"]?.Value<string>() ?? string.Empty,
                        Status = (tunnel["status"]?.Value<int>() ?? 0) == 0 ? "隧道未启动" : "隧道已在线",
                        LocalPort = tunnel["local_port"]?.Value<int>() ?? 0,
                        RemotePort = tunnel["remote_port"]?.Value<int>() ?? 0,
                        Node = nodeName
                    });
                    Console.WriteLine($"隧道名称：{tunnel["name"]}，隧道状态：{tunnel["status"]}，本地端口：{tunnel["local_port"]}，远程端口：{tunnel["remote_port"]}，节点：{tunnel["node_id"]}");
                }

                if (Tunnels.Count > 0)
                {
                    SelectedTunnelIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [RelayCommand]
        private async Task SetTunnelConfig()
        {
            if (SelectedTunnel == null)
            {
                MessageService.ShowToast("错误", "请选择一条隧道！", NotificationType.Error);
            }
            else
            {
                try
                {
                    HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/getTunnelConfig", new Dictionary<string, string>()
                    {
                        ["id"] = SelectedTunnel.Id.ToString()
                    }, new Dictionary<string, string>()
                    {
                        ["Authorization"] = $"Bearer {UserToken}"
                    });
                    if (response.IsSuccessStatusCode == false || response.Content == null)
                    {
                        MessageService.ShowToast("隧道配置失败", "获取隧道配置失败", NotificationType.Error);
                        return;
                    }
                    JObject json = JObject.Parse(response.Content);
                    if (json["code"]?.Value<int>() != 200)
                    {
                        MessageService.ShowToast("隧道配置失败", json["msg"]?.Value<string>() ?? "Err", NotificationType.Error);
                        return;
                    }
                    JToken? data = json["data"];
                    if (data == null || !data.HasValues)
                    {
                        MessageService.ShowToast("隧道配置失败", "没有隧道配置数据", NotificationType.Error);
                        return;
                    }
                    ConfigService.FrpList.CreateFrpConfig(
                        $"{SelectedTunnel.Name} | {SelectedTunnel.Node}",
                        "MSLFrp", "toml",
                        data.Value<string>() ?? string.Empty);
                    MessageService.ShowToast("隧道配置成功", "MSLFrp隧道配置成功！", NotificationType.Success);
                    MainViewSideMenu.NavigateTo<FrpListViewModel>();
                    MainViewSideMenu.NavigateRemove<FrpProviderViewModel>();
                }
                catch (Exception ex)
                {
                    MessageService.ShowToast("隧道配置失败", ex.Message, NotificationType.Error);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteTunnel()
        {
            if (SelectedTunnel == null)
            {
                MessageService.ShowToast("错误", "请选择一条隧道！", NotificationType.Error);
            }
            else
            {
                try
                {
                    HttpService.HttpResponse response = await MSLUser.PostAsync("/frp/deleteTunnel", HttpService.PostContentType.Json, new Dictionary<string, string>()
                    {
                        ["id"] = SelectedTunnel.Id.ToString()
                    }, new Dictionary<string, string>()
                    {
                        ["Authorization"] = $"Bearer {UserToken}"
                    });
                    JObject json = JObject.Parse(response.Content ?? string.Empty);
                    if (json["code"]?.Value<int>() == 200)
                    {
                        MessageService.ShowToast("删除隧道", "隧道删除成功！", NotificationType.Success);
                        await GetTunnels();
                    }
                    else
                    {
                        MessageService.ShowToast("删除隧道失败", json["msg"]?.Value<string>() ?? "未知错误", NotificationType.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageService.ShowToast("删除隧道失败", ex.Message, NotificationType.Error);
                }
            }
        }

        [RelayCommand]
        private async Task CreateTunnel()
        {
            if (SelectedNode == null)
            {
                MessageService.ShowToast("错误", "请选择一个节点！", NotificationType.Error);
            }
            else
            {
                try
                {
                    string type = CreateType == 0 ? "tcp" : CreateType == 1 ? "udp" : CreateType == 2 ? "http" : "https";
                    HttpService.HttpResponse response = await MSLUser.PostAsync("/frp/addTunnel",
                        HttpService.PostContentType.Json, new Dictionary<string, string>()
                        {
                            ["name"] = CreateName,
                            ["local_ip"] = CreateLocalIp,
                            ["local_port"] = CreateLocalPort,
                            ["remote_port"] = CreateRemotePort,
                            ["id"] = SelectedNode.Id.ToString(),
                            ["type"] = type,
                            ["remarks"] = $"Create By MSLX {Assembly.GetExecutingAssembly().GetName().Version}",
                            ["use_kcp"] = "false"
                        }, new Dictionary<string, string>()
                        {
                            ["Authorization"] = $"Bearer {UserToken}"
                        }
                    );
                    JObject json = JObject.Parse(response?.Content ?? string.Empty);
                    if (json["code"]?.Value<int>() == 200)
                    {
                        MessageService.ShowToast("创建隧道", "隧道创建成功！", NotificationType.Success);
                        await GetTunnels();
                        TabIndex = 0;
                    }
                    else
                    {
                        MessageService.ShowToast("创建隧道失败", json["msg"]?.Value<string>() ?? "未知错误", NotificationType.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageService.ShowToast("创建隧道失败", ex.Message, NotificationType.Error);
                }
            }
        }
    }
}