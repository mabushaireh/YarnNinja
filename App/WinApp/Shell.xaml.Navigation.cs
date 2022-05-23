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
            
            Object obj;

            if (args.SelectedItemContainer is null) return;

            if (!(args.SelectedItemContainer as NavigationViewItem).Content.ToString().StartsWith("application"))
            {
                NavigationViewItem parentMenuItem = GetParentMenuItem((args.SelectedItemContainer as NavigationViewItem).Content.ToString());

                var yarnAppId = parentMenuItem.Content.ToString();

                var yarnApp = this.yarnApps.Where(p => p.Header.Id == yarnAppId).FirstOrDefault();
                obj = yarnApp.Containers.Where(p => p.Id.EndsWith((args.SelectedItemContainer as NavigationViewItem).Content.ToString())).FirstOrDefault();
            }
            else
            {
                obj = this.yarnApps.Where(p => p.Header.Id == (args.SelectedItemContainer as NavigationViewItem).Content.ToString()).FirstOrDefault();
            }

            SetCurrentNavigationViewItem(args.SelectedItemContainer as NavigationViewItem, obj);
            (args.SelectedItemContainer as NavigationViewItem).IsExpanded = true;

        }

        private NavigationViewItem GetParentMenuItem(string menuName)
        {

            List<NavigationViewItem> result = new();
            var items = NavigationView.MenuItems.Select(i => (NavigationViewItem)i).ToList();
            foreach (var item in items)
            {
                var child = item.MenuItems.Where(p => (p as NavigationViewItem).Content.ToString() == menuName).FirstOrDefault();
                if (child != null) return item;
            }
            throw new Exception("Failed to get Parent menu item!");
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
