using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MSLX.Core.Controls;

public partial class LogViewerControl : UserControl
{
    public LogViewerControl()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is LogViewerControlViewModel vm)
        {
            vm.LogEntries.CollectionChanged += (s, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        ScrollViewer.ScrollToEnd();
                    }, DispatcherPriority.Render);
                }
            };
        }
    }
}