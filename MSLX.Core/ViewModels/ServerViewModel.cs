using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSLX.Core.Controls;
using MSLX.Core.Models;
using MSLX.Core.Utils;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MSLX.Core.ViewModels
{
    public partial class ServerViewModel : ViewModelBase
    {
        private static MCServerModel.ServerInfo? ServerInfo { get; set; }

        // Design time data
        public ServerViewModel()
        {
            ID = 0;
            ServerInfo = null;
        }

        public ServerViewModel(int id)
        {
            ID = id;
            ServerInfo = ConfigService.ServerList.GetServer(ID);
            LogViewer.AddLog("Logs", Colors.Blue);
        }

        public int ID { get; set; }
        public Process? ServerProcess { get; set; }
        public string SendCmdBtn { get; } = "发送";

        [ObservableProperty]
        private string _controlServerBtn = "开服";

        [ObservableProperty]
        private bool _serverEnable = false;

        [ObservableProperty]
        private string _cmdSendToServer = string.Empty;

        /*
        [ObservableProperty]
        private string _serverLogs = "Logs";
        */
        
        [ObservableProperty]
        private LogViewerControlViewModel _logViewer =new LogViewerControlViewModel();

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
            //ServerLogs = string.Empty;
            LogViewer.LogEntries.Clear();
            // Debug.WriteLine(ID);

            var thisserverInList = MainViewModel.ServerListView.ServerList.FirstOrDefault(s => s.ID == ID);

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
                try
                {
                    ServerProcess.Start();
                    ServerEnable = true;
                    ControlServerBtn = "关服";
                    if (thisserverInList != null) thisserverInList.Status = true;
                }
                catch
                {
                    return;
                }
                ServerProcess.BeginOutputReadLine();
                ServerProcess.BeginErrorReadLine();
                await ServerProcess.WaitForExitAsync();
                ServerProcess.CancelOutputRead();
                ServerProcess.CancelErrorRead();
                ServerProcess.OutputDataReceived -= LogReceivedHandler;
                ServerProcess.ErrorDataReceived -= LogReceivedHandler;

                ServerEnable = false;
                ControlServerBtn = "开服";
                if (thisserverInList != null) thisserverInList.Status = false;
            });
        }

        private void LogReceivedHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                //ServerLogs += e.Data + "\n";
                string logText = e.Data;
                if (logText.Contains("INFO"))
                    LogViewer.AddLog(logText, Colors.Green);
                else if (logText.Contains("ERROR"))
                    LogViewer.AddLog(logText, Colors.Red);
                else if (logText.Contains("WARN"))
                    LogViewer.AddLog(logText, Colors.Orange);
                else
                    LogViewer.AddLog(logText, Colors.Blue);
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

        [RelayCommand]
        private void TBSendCmdToServer(KeyEventArgs args)
        {
            if (args.Key != Key.Enter)
                return;

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
