using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using YarnNinja.App.WinApp.Views;
using YarnNinja.Common;
using YarnNinja.Common.Utils;

namespace YarnNinja.App.WinApp
{
    public sealed partial class Shell : Window
    {
        private readonly List<YarnApplication> yarnApps = new();
        readonly BackgroundWorker bgWorker = new();
        DoWorkEventHandler handler;
        private int cuurentContainerCount = 0;

        public Shell()
        {
            Title = "Yarn Ninja";

            InitializeComponent();
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;

            (Application.Current as App).EnsureSettings();
            ApplyTheme();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = (Application.Current as App).Settings;
            settings.IsLightTheme = !settings.IsLightTheme;
            (Application.Current as App).SaveSettings();
            ApplyTheme();
        }

        private void YarnFile_Click(object sender, RoutedEventArgs e)
        {
            SelectYarnFromFile();
        }

        private async Task SelectYarnFromFile()
        {
            var openFileDialog = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            openFileDialog.FileTypeFilter.Add(".log");

            WinRT.Interop.InitializeWithWindow.Initialize(openFileDialog, App.WindowHandle);

            var file = await openFileDialog.PickSingleFileAsync();
            if (file != null)
                OpenYarnAppFile(file.Path);

        }




        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                mainStatusBar.Message = $"Yarn App is loading: 0%, Containers: ({cuurentContainerCount})";
                mainStatusBar.IsOpen = true;
                mainStatusBar.Severity = InfoBarSeverity.Informational;
                mainProgressBar.IsActive = true;
                mainProgressBar.Visibility = Visibility.Visible;
            }
            else if (e.ProgressPercentage == 100)
            {
                bgWorker.DoWork -= handler;
                mainProgressBar.IsActive = false;
                mainProgressBar.Visibility = Visibility.Collapsed;
                mainStatusBar.IsOpen = true;

                mainStatusBar.Severity = InfoBarSeverity.Success;

                mainStatusBar.Message = $"Yarn App is loading: 100%, Containers: ({cuurentContainerCount})";


                NavigationViewItem navItem = new()
                {
                    Content = this.yarnApps[^1].Header.Id,
                    Tag = "YarnNinja.App.WinApp.Views.YarnAppPage"
                };
                navItem.Tapped += (sender, e) =>
                {
                    ToolTipService.SetToolTip(sender as NavigationViewItem, navItem.Content);
                };


                navItem.Icon = new BitmapIcon()
                {
                    UriSource = new Uri("ms-appx:///Assets/App.png"),
                    ShowAsMonochrome = false
                };

                AddMenuItem(NavigationView, (navItem));

                SetCurrentNavigationViewItem(navItem, this.yarnApps[^1]);
            }
            else
            {
                mainStatusBar.Severity = InfoBarSeverity.Informational;
                mainStatusBar.Message = $"Yarn App is loading: {e.ProgressPercentage}%,, Containers: ({cuurentContainerCount})";
                mainStatusBar.IsOpen = true;
            }
        }

        internal void CloseYarnAppContainer(YarnAppContainerPage yarnAppContainerPage)
        {
            var yarnAppContainerName = yarnAppContainerPage.YarnAppContainer.ShortId;

            var menuItems = GetNavigationViewItems();
            var menuItem = menuItems.Where(p => p.Content.ToString() == yarnAppContainerName).FirstOrDefault();


            var parent = GetParentMenuItem(yarnAppContainerName);
            parent.MenuItems.Remove(menuItem);
            menuItem = null;

            var yarnApp = this.yarnApps.Where(p => p.Header.Id == parent.Content.ToString()).FirstOrDefault();

            SetCurrentNavigationViewItem(parent, yarnApp);
        }

        internal void AddContainer(YarnApplication yarnApp, YarnApplicationContainer container)
        {

            var parent = GetNavigationViewItems().Where(p => p.Content.ToString() == yarnApp.Header.Id).FirstOrDefault();

            // Check if contianer already open then switch only
            NavigationViewItem navItem = parent.MenuItems.Select(i => (NavigationViewItem)i).Where(p => p.Content.ToString() == container.ShortId).FirstOrDefault();

            if (navItem == null)
            {
                navItem = new()
                {
                    Content = container.ShortId,

                    Tag = "YarnNinja.App.WinApp.Views.YarnAppContainerPage"
                };

                navItem.Tapped += (sender, e) =>
                {
                    ToolTipService.SetToolTip(sender as NavigationViewItem, container.Id);
                };

                navItem.Icon = new BitmapIcon()
                {
                    UriSource = new Uri("ms-appx:///Assets/Container.png"),
                    ShowAsMonochrome = false
                };

                AddMenuItem(parent, navItem);
            }

            SetCurrentNavigationViewItem(navItem, container);
        }

        private async Task OpenYarnAppLogFileAsync(string path)
        {
           
            var file = new YarnLogFileReader();
            file.OpenFile(path);

            this.bgWorker.ReportProgress((int)file.ProgressPrecent);
            var yarnApp = new YarnApplication(file);
            this.yarnApps.Add(yarnApp);
            file.ProgressEventHandler += File_ProgressEventHandler;
            yarnApp.ContainerAddedEvent += YarnApp_ContainerAddedEvent;

            await yarnApp.ParseContainersAsync();

            if (yarnApps.Where(p => p.Header.Id == yarnApp.Header.Id).Count() > 1)
            {
                this.yarnApps.Remove(yarnApp); 
            }
        }

        private void YarnApp_ContainerAddedEvent(object sender, ContainerAddedEventArgs e)
        {
            cuurentContainerCount = e.ContainersCount;
            this.bgWorker.ReportProgress(e.Progress, e.ContainersCount);

        }

        private void File_ProgressEventHandler(object sender, EventArgs e)
        {
            this.bgWorker.ReportProgress((int)(sender as YarnLogFileReader).ProgressPrecent);
        }


        private void ApplyTheme()
        {
            var settings = (Application.Current as App).Settings;
            Root.RequestedTheme = settings.IsLightTheme ? ElementTheme.Light : ElementTheme.Dark;
        }

        public void OpenYarnAppFile(string path)
        {
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            handler = (sender, e) =>
            {
                bgWorker.ReportProgress(0);
                OpenYarnAppLogFileAsync(path);
            };

            bgWorker.DoWork += handler;
            bgWorker.RunWorkerAsync();
        }

        public void CloseYarnApp(YarnAppPage yarnAppPage)
        {
            var yarnAppName = yarnAppPage.YarnApp.Header.Id;
            var yarnApp = this.yarnApps.Where(p => p.Header.Id == yarnAppName).FirstOrDefault();
            this.yarnApps.Remove(yarnApp);
            yarnApp = null;


            var menuItems = GetNavigationViewItems();
            var menuItem = menuItems.Where(p => p.Content.ToString() == yarnAppName).FirstOrDefault();
            NavigationView.MenuItems.Remove(menuItem);
            menuItem = null;
            if (this.yarnApps.Count > 0)
            {
                var select = menuItems.Where(p => p.Content.ToString() == this.yarnApps[^1].Header.Id).FirstOrDefault();

                SetCurrentNavigationViewItem(select, this.yarnApps[^1]);
            }
            else
            {
                var select = menuItems.Where(p => p.Content.ToString() == "About").FirstOrDefault();
                SetCurrentNavigationViewItem(select, null);
            }
        }
    }
}
