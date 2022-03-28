using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace YarnNinja.App.WinApp.Services
{
    public interface INavigation
    {
        NavigationViewItem GetCurrentNavigationViewItem();

        List<NavigationViewItem> GetNavigationViewItems();

        List<NavigationViewItem> GetNavigationViewItems(Type type);

        List<NavigationViewItem> GetNavigationViewItems(Type type, string title);

        void SetCurrentNavigationViewItem(NavigationViewItem item, object obj);

        void SetCurrentPage(Type type, object obj);
    }
}
