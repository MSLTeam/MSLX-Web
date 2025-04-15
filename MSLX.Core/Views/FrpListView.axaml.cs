using Avalonia.Controls;
using Avalonia.Interactivity;
using MSLX.Core.ViewModels;

namespace MSLX.Core.Views
{
    public partial class FrpListView : UserControl
    {
        public FrpListView()
        {
            InitializeComponent();
        }

        private void ListBox_DoubleTapped(object sender, RoutedEventArgs e)
        {
            var viewModel = (FrpListViewModel)DataContext;
            viewModel.OpenFrpConfig();
        }
    }
}