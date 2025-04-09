using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using MSLX.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.Controls
{
    public partial class LogViewerControlViewModel : ViewModelBase
    {
        public class LogEntry
        {
            public required string Text { get; set; }
            public required IBrush Color { get; set; }
        }

        [ObservableProperty]
        private ObservableCollection<LogEntry> _logEntries = new();

        private readonly object _bufferLock = new();
        private readonly List<(string Message, Color Color)> _logBuffer = new();
        private bool _isProcessingBuffer;

        public void AddLog(string message, Color color)
        {
            lock (_bufferLock)
            {
                _logBuffer.Add((message, color));
            }

            if (!_isProcessingBuffer)
            {
                _isProcessingBuffer = true;
                Dispatcher.UIThread.Post(ProcessBuffer, DispatcherPriority.Normal);
            }
        }

        private void ProcessBuffer()
        {
            List<(string Message, Color Color)> bufferCopy;
            lock (_bufferLock)
            {
                bufferCopy = new List<(string, Color)>(_logBuffer);
                _logBuffer.Clear();
            }

            // 在 UI 线程创建 UI 资源
            var newEntries = bufferCopy.Select(item => new LogEntry
            {
                Text = item.Message,
                Color = new SolidColorBrush(item.Color)
            }).ToList();

            foreach (var log in newEntries)
            {
                LogEntries.Add(log);
            }

            if (LogEntries.Count > 1000)
            {
                var removeCount = LogEntries.Count - 800;
                for (int i = 0; i < removeCount; i++)
                {
                    LogEntries.RemoveAt(0);
                }
            }

            _isProcessingBuffer = false;
        }
    }
}
