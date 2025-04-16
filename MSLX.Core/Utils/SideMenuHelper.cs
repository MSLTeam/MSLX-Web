using MSLX.Core.ViewModels;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSLX.Core.Utils
{
    public class MainViewSideMenu
    {
        public static void NavigateTo<T>() where T : ViewModelBase
        {
            if (App.MainView == null) return;

            var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent is T);
            if (page != null)
                App.MainView.ActivePage = page;
        }

        public static void NavigateTo(ViewModelBase viewModel)
        {
            if (App.MainView == null) return;

            var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent.GetType() == viewModel.GetType());
            if (page != null)
                App.MainView.ActivePage = page;
        }

        public static void NavigateTo(SukiSideMenuItem sukiSideMenuItem, bool addToSideMenu = true, int insert = -1)
        {
            if (App.MainView == null) return;

            if (addToSideMenu)
            {
                if (insert != -1)
                    App.MainView.MainPages.Insert(insert, sukiSideMenuItem);
                else
                    App.MainView.MainPages.Add(sukiSideMenuItem);
            }
            App.MainView.ActivePage = sukiSideMenuItem;
        }

        public static void NavigateRemove<T>() where T : ViewModelBase
        {
            if (App.MainView == null) return;

            var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent is T);
            if (page != null)
            {
                if (App.MainView.ActivePage == page)
                    App.MainView.ActivePage = App.MainView.MainPages.FirstOrDefault();
                App.MainView.MainPages.Remove(page);
            }
        }

        public static void NavigateRemove(ViewModelBase viewModel)
        {
            if (App.MainView == null) return;

            var page = App.MainView.MainPages.FirstOrDefault(p => p.PageContent.GetType() == viewModel.GetType());
            if (page != null)
            {
                if (App.MainView.ActivePage == page)
                    App.MainView.ActivePage = App.MainView.MainPages.FirstOrDefault();
                App.MainView.MainPages.Remove(page);
            }
        }

        public static int GetActivePageIndex()
        {
            if (App.MainView == null) return -1;
            var page = App.MainView.MainPages.FirstOrDefault(p => p == App.MainView.ActivePage);
            if (page != null)
                return App.MainView.MainPages.IndexOf(page);
            return -1;
        }
    }
}
