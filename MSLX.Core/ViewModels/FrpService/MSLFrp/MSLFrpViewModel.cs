using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        // tabcontrol
        [ObservableProperty]
        private int _tabIndex;
        
        // 存储的隧道
        [ObservableProperty]
        private ObservableCollection<Tunnel> _tunnels;
        
        // 存储的节点
        [ObservableProperty]
        private ObservableCollection<Node> _nodes;
        
        // 用户信息字段
        [ObservableProperty]
        private string _username = "loading";
        
        [ObservableProperty]
        private string _userGroup = "普通用户";
        
        [ObservableProperty]
        private string _userMaxTunnels = "3";
        
        [ObservableProperty]
        private string _userOutdated = "loading";

        // 选中的隧道
        [ObservableProperty]
        private Tunnel _selectedTunnel;
        
        // 选中的隧道
        [ObservableProperty]
        private Node _selectedNode;
        
        // 是否显示主界面
        [ObservableProperty]
        private bool _showMainPage = false;
        
        // 登录相关
        [ObservableProperty]
        private string _account;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private bool _isSaveLoginStatus;
        private string token;
        
        // 节点ID到名称映射字典
        private Dictionary<int, string> _nodeMap = new Dictionary<int, string>();
        
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
        private string _createBindDomain;
        

        [RelayCommand]
        private async Task Inited()
        {
            CreateRemotePort = StringHelper.GetRandomNumber(10240, 60000).ToString();
            CreateName = StringHelper.GenerateRandomString(6, "MSLX_");
            if (ConfigService.Config.ReadConfigKey("MSLUserToken") != null)
            {
                token = ConfigService.Config.ReadConfigKey("MSLUserToken")?.ToString();
                await GetFrpInfo(true);
            }
            
        }
        
        [RelayCommand]
        private async Task LoginByPassword()
        {
            if (!String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Account))
            {
                try
                {
                    // 添加登录中弹窗
                    var dialog = MainViewModel.DialogManager.CreateDialog()
                        .WithTitle("登录中")
                        .WithContent(new TextBlock { Text = "正在登录，请稍候..." });
                    dialog.TryShow();

                    HttpService.HttpResponse response = await MSLUser.PostAsync("/user/login", HttpService.PostContentType.Json, new
                    {
                        email = Account,
                        password = Password
                    });
                    JObject json = JObject.Parse(response.Content);
                    if ((int)json["code"] == 200)
                    {
                        Console.WriteLine("登录成功！");
                        token = (string)json["data"]["token"];
                        if (IsSaveLoginStatus)
                        {
                            ConfigService.Config.WriteConfigKey("MSLUserToken", token);
                        }
                        ShowMainPage = true;
                        await GetFrpInfo();
                    }
                    else
                    {
                        Console.WriteLine((string)json["msg"]);
                    }

                    // 关闭登录中弹窗
                    MainViewModel.DialogManager.DismissDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // 关闭登录中弹窗
                    MainViewModel.DialogManager.DismissDialog();
                }
            }
            else
            {
               Console.WriteLine("请填写账户密码。");
            }
        }
        
        private async Task GetFrpInfo(bool showDialog = false)
        {
            try
            {
                if (showDialog)
                {
                    // 添加自动登录中弹窗
                    var dialog = MainViewModel.DialogManager.CreateDialog()
                        .WithTitle("登录中")
                        .WithContent(new TextBlock { Text = "正在自动登录，请稍候..." });
                    dialog.TryShow();
                }
                
                // 获取MSL Frp用户信息
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/userInfo",null, new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {token}"
                });
                JObject json = JObject.Parse(response.Content);
                if ((int)json["code"] == 200)
                {
                    MessageService.ShowToast("登录成功！","成功登录到MSL Frp服务", NotificationType.Success);
                        
                    ShowMainPage = true;
                    Console.WriteLine("获取MSL用户信息成功！");
                    Username = (string)json["data"]["name"];
                    UserGroup = (int)json["data"]["user_group"]==6?"超级管理员":(int)json["data"]["user_group"]==1?"高级会员":(int)json["data"]["user_group"]==2?"超级会员":"普通用户";
                    UserMaxTunnels = (string)json["data"]["maxTunnelCount"];
                    UserOutdated = (long)json["data"]["outdated"] == 3749682420?"长期有效":StringHelper.SecondsToDateTime((long)json["data"]["outdated"]).ToString();
                    
                    // 获取隧道信息
                    await GetNodes(); 
                    await GetTunnels();
                    MainViewModel.DialogManager.DismissDialog();
                }
                else
                {
                    Console.WriteLine((string)json["msg"]);
                    MainViewModel.DialogManager.DismissDialog();
                    return;
                }

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MainViewModel.DialogManager.DismissDialog();
                //ShowMainPage = false;
            }
        }
        
        // 新增获取节点列表的方法
        private async Task GetNodes()
        {
            try
            {
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/nodeList", null, new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {token}"
                });
                JObject json = JObject.Parse(response.Content);
                if ((int)json["code"] == 200)
                {
                    _nodeMap.Clear();
                    foreach (var node in json["data"])
                    {
                        int id = (int)node["id"];
                        string name = (string)node["node"];
                        _nodeMap[id] = name;
                        
                        Nodes = new ObservableCollection<Node>();
                        foreach (var nodeItem in json["data"])
                        {
                            int nodeId = (int)nodeItem["id"];
                            string nodeName = (string)nodeItem["node"];
                            _nodeMap[nodeId] = nodeName; // id和name的字典映射
                        
                            // 填充到节点列表数据
                            Nodes.Add(new Node()
                            {
                                Id = (int)nodeItem["id"],
                                AllowUserGroup = (int)nodeItem["allow_user_group"],
                                Type = (int)nodeItem["allow_user_group"]==0?"免费":(int)nodeItem["allow_user_group"]==1?"高级":"超级",
                                Bandwidth = (int)nodeItem["bandwidth"],
                                HttpSupport = (int)nodeItem["http_support"] == 1,
                                UdpSupport = (int)nodeItem["udp_support"] == 1,
                                KcpSupport = (int)nodeItem["kcp_support"] == 1,
                                MaxOpenPort = (int)nodeItem["max_open_port"],
                                MinOpenPort = (int)nodeItem["min_open_port"],
                                NeedRealName = (int)nodeItem["need_real_name"] == 1,
                                Name = nodeName,
                                Status = (int)nodeItem["status"] == 1 ? "在线" : "离线",
                                Remarks = (string)nodeItem["remarks"]
                            });
                        }

                        if (Nodes.Count > 0)
                        {
                            SelectedNode = Nodes[0];
                        }
                    }
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
                    ["Authorization"] = $"Bearer {token}"
                });
                JObject json = JObject.Parse(response.Content);
                if ((int)json["code"] == 200)
                {
                    Console.WriteLine("获取隧道列表成功！");
                        Tunnels = new ObservableCollection<Tunnel>();
                        foreach (var tunnel in json["data"])
                        {
                            int nodeId = (int)tunnel["node_id"];
                            string nodeName = _nodeMap.ContainsKey(nodeId) ? _nodeMap[nodeId] : "未知节点";
                            Tunnels.Add(new Tunnel()
                            {
                                Id = (int)tunnel["id"],
                                Name = (string)tunnel["name"],
                                Remarks = (string)tunnel["remarks"],
                                Status = (int)tunnel["status"] == 0 ? "隧道未启动" : "隧道已在线",
                                LocalPort = (string)tunnel["local_port"],
                                RemotePort = (string)tunnel["remote_port"],
                                Node = nodeName
                            });
                            Console.WriteLine($"隧道名称：{tunnel["name"]}，隧道状态：{tunnel["status"]}，本地端口：{tunnel["local_port"]}，远程端口：{tunnel["remote_port"]}，节点：{tunnel["node_id"]}");
                        }

                        if (Tunnels.Count > 0)
                        {
                            SelectedTunnel = Tunnels[0];
                        }
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
                    HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/getTunnelConfig",  new Dictionary<string, string>()
                    {
                        ["id"] = SelectedTunnel.Id.ToString()
                    }, new Dictionary<string, string>()
                    {
                        ["Authorization"] = $"Bearer {token}"
                    });
                    JObject json = JObject.Parse(response.Content);
                    if ((int)json["code"] == 200)
                    {
                        ConfigService.FrpList.CreateFrpConfig(
                            $"{SelectedTunnel.Name} | {SelectedTunnel.Node}", "MSLFrp","toml", json["data"].ToString());
                        MessageService.ShowToast("隧道配置成功", "MSLFrp隧道配置成功！", NotificationType.Success);
                        MainViewModel.NavigateTo<FrpListViewModel>();
                        MainViewModel.NavigateRemove<FrpTunnelViewModel>();
                    }
                    else
                    {
                        MessageService.ShowToast("隧道配置失败", json["msg"].ToString(), NotificationType.Error);
                    }
                }catch (Exception ex)
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
                        ["Authorization"] = $"Bearer {token}"
                    });
                    JObject json = JObject.Parse(response.Content);
                    if (json["code"]?.Value<int>() == 200)
                    {
                        MessageService.ShowToast("删除隧道", "隧道删除成功！", NotificationType.Success);
                        await GetTunnels();
                    }
                    else
                    {
                        MessageService.ShowToast("删除隧道失败", json["msg"]?.Value<string>() ?? "未知错误", NotificationType.Error);
                    }
                }catch (Exception ex)
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
                            ["remarks"] = $"Create By MSLX {Assembly.GetExecutingAssembly().GetName().Version.ToString()}",
                            ["use_kcp"] = "false"
                        }, new Dictionary<string, string>()
                        {
                            ["Authorization"] = $"Bearer {token}"
                        }
                    );
                    JObject json = JObject.Parse(response.Content);
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
                }catch (Exception ex)
                {
                    MessageService.ShowToast("创建隧道失败", ex.Message, NotificationType.Error);
                }
            }
        }

        public MSLFrpViewModel()
        {
            
        }
    }
}