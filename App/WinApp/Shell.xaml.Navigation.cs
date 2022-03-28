using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using YarnNinja.App.WinApp.Services;
using YarnNinja.App.WinApp.Views;

namespace YarnNinja.App.WinApp
{
    public sealed partial class Shell : INavigation
    {
        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            //SetCurrentNavigationViewItem(GetNavigationViewItems(typeof(YarnAppPage)).First());
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var yarnApp = this.yarnApps.Where(p => p.Header.Id == sender.Header).FirstOrDefault();

            SetCurrentNavigationViewItem(args.SelectedItemContainer as NavigationViewItem, yarnApp);
        }

        public List<NavigationViewItem> GetNavigationViewItems()
        {
            List<NavigationViewItem> result = new();
            var items = NavigationView.MenuItems.Select(i => (NavigationViewItem)i).ToList();
            items.AddRange(NavigationView.FooterMenuItems.Select(i => (NavigationViewItem)i));
            result.AddRange(items);

            foreach (NavigationViewItem mainItem in items)
            {
                result.AddRange(mainItem.MenuItems.Select(i => (NavigationViewItem)i));
            }

            return result;
        }

        public List<NavigationViewItem> GetNavigationViewItems(Type type)
        {
            return GetNavigationViewItems().Where(i => i.Tag.ToString() == type.FullName).ToList();
        }

        public List<NavigationViewItem> GetNavigationViewItems(Type type, string title)
        {
            return GetNavigationViewItems(type).Where(ni => ni.Content.ToString() == title).ToList();
        }

        public void SetCurrentNavigationViewItem(NavigationViewItem item, object obj)
        {

            if (item == null)
            {
                return;
            }

            if (item.Tag == null)
            {
                return;
            }

            if (NavigationView.SelectedItem is not null && (NavigationView.SelectedItem as NavigationViewItem).Content == item.Content)
            {
                return;
            }



            ContentFrame.Navigate(Type.GetType(item.Tag.ToString()), obj);
            NavigationView.Header = item.Content;
            NavigationView.SelectedItem = item;
        }

        public NavigationViewItem GetCurrentNavigationViewItem()
        {
            return NavigationView.SelectedItem as NavigationViewItem;
        }

        public void SetCurrentPage(Type type, object obj)
        {
            ContentFrame.Navigate(type, obj);
        }


    }
}
