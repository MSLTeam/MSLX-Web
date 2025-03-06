using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MSLX.ViewModels
{
    public partial class ServerViewModel : ViewModelBase
    {
        // 添加无参数构造函数，否则axaml会报错
        public ServerViewModel() : this(0) { }

        public ServerViewModel(int id)
        {
            ID = id;
            Debug.WriteLine(ID);
            Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory + "Server");
        }

        public int ID { get; set; }
        public Process ServerProcess { get; set; } = new Process();
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
                    ServerProcess.StandardInput.WriteLine("stop");
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
            Task.Run(async () =>
            {
                ServerProcess.StartInfo= new ProcessStartInfo()
                {
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "Server",
                    FileName = "java",
                    Arguments = "-jar paper-1.8.8.jar",
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
                ServerProcess.StandardInput.WriteLine(CmdSendToServer);
                CmdSendToServer = string.Empty;
            }
            catch
            {
                return;
            }
        }
    }
}
