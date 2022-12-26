using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YarnNinja.App.WinApp.Services;
using YarnNinja.App.WinApp.Views;

namespace YarnNinja.App.WinApp
{
    public sealed partial class Shell : INavigation
    {
        private void AppsBrowser_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

            Object obj;

            if (args.SelectedItemContainer is null) return;
            if ((args.SelectedItemContainer as NavigationViewItem).Content.ToString().Equals("About")) return;

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
        }

        private NavigationViewItem GetParentMenuItem(string menuName)
        {

            List<NavigationViewItem> result = new();
            var items = AppsBrowser.MenuItems.Select(i => (NavigationViewItem)i).ToList();
            foreach (var item in items)
            {
                var child = item.MenuItems.Where(p => (p as NavigationViewItem).Content.ToString() == menuName).FirstOrDefault();
                if (child != null) return item;
            }
            throw new Exception("Failed to get Parent menu item!");
        }

        private List<NavigationViewItem> GetNavigationViewItems()
        {
            List<NavigationViewItem> result = new();
            var items = AppsBrowser.MenuItems.Select(p => (NavigationViewItem)p).ToList();
            result.AddRange(AppsBrowser.FooterMenuItems.Select(p => (NavigationViewItem)p).ToList());
            result.AddRange(items);

            foreach (NavigationViewItem mainItem in items)
            {
                result.AddRange(mainItem.MenuItems.Select(i => (NavigationViewItem)i));
            }

            return result;
        }

        private List<NavigationViewItem> GetNavigationViewItems(Type type)
        {
            return GetNavigationViewItems().Where(i => i.Tag.ToString() == type.FullName).ToList();
        }

        private void SetCurrentNavigationViewItem(NavigationViewItem item, object obj)
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
            AppsBrowser.Header = item.Content;
            //NavigationView.SelectedItem = item;
        }

        private NavigationViewItem GetCurrentNavigationViewItem()
        {
            return AppsBrowser.SelectedItem as NavigationViewItem;
        }

        private void SetCurrentPage(Type type, object obj)
        {
            ContentFrame.Navigate(type, obj);
        }

        private List<NavigationViewItem> GetNavigationViewItems(Type type, string title)
        {
            return GetNavigationViewItems(type).Where(ni => ni.Content.ToString() == title).ToList();
        }

        public void AddMenuItem(string parent, string child)
        {



            if (!string.IsNullOrEmpty(parent))
            {

                NavigationViewItem navItem = new()
                {
                    Content = child,

                    Tag = "YarnNinja.App.WinApp.Views.YarnAppContainerPage"
                };

                //navItem.Tapped += (sender, e) =>
                //{
                //    //ToolTipService.SetToolTip(sender as NavigationViewItem, child);
                //};

                //navItem.Icon = new BitmapIcon()
                //{
                //    UriSource = new Uri("ms-appx:///Assets/Container.png"),
                //    ShowAsMonochrome = false
                //};
                var parentMenuItem = GetNavigationViewItems().Where(p => p.Content.ToString() == parent).FirstOrDefault();

                // Check if contianer already open then switch only
                parentMenuItem.MenuItems.Add(navItem);
                parentMenuItem.IsExpanded = true;
                var container = this.yarnApps.Where(p => p.Header.Id.Equals(parent)).FirstOrDefault().Containers.Where(p => p.ShortId.Equals(child)).FirstOrDefault();
                SetCurrentNavigationViewItem(navItem, container);

            }
            else
            {
                NavigationViewItem navItem = new()
                {
                    Content = child,
                    Tag = "YarnNinja.App.WinApp.Views.YarnAppPage"
                };
                navItem.Tapped += (sender, e) =>
                {
                    //ToolTipService.SetToolTip(sender as NavigationViewItem, navItem.Content);
                };


                navItem.Icon = new BitmapIcon()
                {
                    UriSource = new Uri("ms-appx:///Assets/App.png"),
                    ShowAsMonochrome = false
                };
                ;

                AppsBrowser.MenuItems.Add(navItem);
                SetCurrentNavigationViewItem(navItem, this.yarnApps[^1]);
            }


            return;


        }

        public void RemoveMenuItem(string parent, string child)
        {
            if (string.IsNullOrEmpty(parent))
            {
                var menuItems = GetNavigationViewItems();
                var menuItem = menuItems.Where(p => p.Content.ToString() == child).FirstOrDefault();
                AppsBrowser.MenuItems.Remove(menuItem);
                menuItem = null;



                var select = GetNavigationViewItems().Last();

                if (this.yarnApps.Count > 0)
                    SetCurrentNavigationViewItem(select, this.yarnApps[^1]);
                else
                    SetCurrentNavigationViewItem(select, null);

            }
            else
            {
                var menuItems = GetNavigationViewItems();
                var parentMenuItem = menuItems.Where(p => p.Content.ToString() == parent).FirstOrDefault();
                var childMenuItem = parentMenuItem.MenuItems.Select(p => (NavigationViewItem)p).Where(p => p.Content == child).FirstOrDefault();

                parentMenuItem.MenuItems.Remove(childMenuItem);
                childMenuItem = null;
                SetCurrentNavigationViewItem(parentMenuItem, this.yarnApps.Where(p => p.Header.Id == parent).FirstOrDefault());


            }
        }
    }
}