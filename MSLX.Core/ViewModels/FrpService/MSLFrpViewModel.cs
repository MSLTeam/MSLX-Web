using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Utils;
using Newtonsoft.Json.Linq;
using SukiUI.Toasts;
using static MSLX.Core.Models.FrpService.MSLFrpModel;

namespace MSLX.Core.ViewModels.FrpService
{
    public partial class MSLFrpViewModel : ViewModelBase
    {
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

        [RelayCommand]
        private async Task Loaded()
        {
            if (ConfigService.Config.ReadConfigKey("MSLUserToken") != null)
            {
                token = ConfigService.Config.ReadConfigKey("MSLUserToken")?.ToString();
                await GetFrpInfo();
            }
            
        }
        
        [RelayCommand]
        private async Task LoginByPassword()
        {
            if (!String.IsNullOrEmpty(Password) && !String.IsNullOrEmpty(Account))
            {
                try
                {
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
                    }
                    else
                    {
                        Console.WriteLine((string)json["msg"]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            else
            {
               Console.WriteLine("请填写账户密码。");
            }
        }
        
        private async Task GetFrpInfo()
        {
            try
            {
                // 获取MSL Frp用户信息
                HttpService.HttpResponse response = await MSLUser.GetAsync("/frp/userInfo",null, new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {token}"
                });
                JObject json = JObject.Parse(response.Content);
                if ((int)json["code"] == 200)
                {
                    ShowMainPage = true;
                    Console.WriteLine("获取MSL用户信息成功！");
                    Username = (string)json["data"]["name"];
                    UserGroup = (int)json["data"]["user_group"]==6?"超级管理员":(int)json["data"]["user_group"]==1?"高级会员":(int)json["data"]["user_group"]==2?"超级会员":"普通用户";
                    UserMaxTunnels = (string)json["data"]["maxTunnelCount"];
                    UserOutdated = (long)json["data"]["outdated"] == 3749682420?"长期有效":StringHelper.SecondsToDateTime((long)json["data"]["outdated"]).ToString();
                    
                    // 获取隧道信息
                    await GetNodes(); 
                    await GetTunnels();
                }
                else
                {
                    Console.WriteLine((string)json["msg"]);
                    return;
                }

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
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
                                Name = (string)tunnel["name"],
                                Status = (int)tunnel["status"] == 0 ? "隧道未启动" : "隧道已在线",
                                LocalPort = (string)tunnel["local_port"],
                                RemotePort = (string)tunnel["remote_port"],
                                Node = nodeName
                            });
                            Console.WriteLine($"隧道名称：{tunnel["name"]}，隧道状态：{tunnel["status"]}，本地端口：{tunnel["local_port"]}，远程端口：{tunnel["remote_port"]}，节点：{tunnel["node_id"]}");
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public MSLFrpViewModel()
        {
            
        }
    }
}