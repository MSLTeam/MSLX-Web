using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Utils;
using MSLX.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels
{
    public partial class ServerViewModel : ViewModelBase
    {
        private static MCServerModel.ServerInfo? ServerInfo { get; set; }

        // 添加无参数构造函数，否则axaml会报错
        public ServerViewModel() : this(0) { }

        public ServerViewModel(int id)
        {
            ID = id;
            ServerInfo = Utility.ConfigService.GetServer(ID);
        }

        public int ID { get; set; }
        public Process? ServerProcess { get; set; }
        public string SendCmdBtn { get; } = "发送";

        [ObservableProperty]
        private string _controlServerBtn = "开服";

        [ObservableProperty]
        private string _cmdSendToServer = string.Empty;

        [ObservableProperty]
        private string _serverLogs = "Logs";



        [RelayCommand]
        private void RunServer()
        {
            if (ControlServerBtn == "关服")
            {
                try
                {
                    ServerProcess?.StandardInput.WriteLine("stop");
                    return;
                }
                catch
                {
                    return;
                }
            }
            ServerLogs = string.Empty;
            Debug.WriteLine(ID);
            ControlServerBtn = "关服";
            if (ServerInfo == null) return;
            Task.Run(async () =>
            {
                ServerProcess = new Process();
                ServerProcess.StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = ServerInfo.Base,
                    FileName = ServerInfo.Java,
                    Arguments = $"-jar {ServerInfo.Core}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ServerProcess.OutputDataReceived += LogReceivedHandler;
                ServerProcess.ErrorDataReceived += LogReceivedHandler;
                ServerProcess.Start();
                ServerProcess.BeginOutputReadLine();
                ServerProcess.BeginErrorReadLine();
                await ServerProcess.WaitForExitAsync();
                ServerProcess.CancelOutputRead();
                ServerProcess.CancelErrorRead();
                ServerProcess.OutputDataReceived -= LogReceivedHandler;
                ServerProcess.ErrorDataReceived -= LogReceivedHandler;
                ControlServerBtn = "开服";
            });
        }

        private void LogReceivedHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                ServerLogs += e.Data + "\n";
            }
        }

        [RelayCommand]
        private void SendCmdToServer()
        {
            try
            {
                ServerProcess?.StandardInput.WriteLine(CmdSendToServer);
                CmdSendToServer = string.Empty;
            }
            catch
            {
                return;
            }
        }
    }
}
