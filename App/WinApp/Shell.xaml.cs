using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using YarnNinja.App.WinApp.Views;
using YarnNinja.Common;
using YarnNinja.Common.Utils;

namespace YarnNinja.App.WinApp
{
    public sealed partial class Shell : Window
    {
        #region "Private Fields"
        private readonly List<YarnApplication> yarnApps = new();
        private readonly BackgroundWorker bgWorker = new();
        private DoWorkEventHandler handler;
        private int cuurentContainerCount = 0;
        #endregion


        public Shell()
        {
            Title = "Yarn Ninja";

            InitializeComponent();
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;

            (Application.Current as App).EnsureSettings();
            ApplyTheme();
        }


        #region "Event Handlers"
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

                //FIXME: find a better way to trigger 100% complete, better ti add menu item on yarn parse complete
                while (this.yarnApps[^1].Header is null)
                    Thread.Sleep(100);

                AddMenuItem(null, this.yarnApps[^1].Header.Id);

            }
            else
            {
                mainStatusBar.Severity = InfoBarSeverity.Informational;
                mainStatusBar.Message = $"Yarn App is loading: {e.ProgressPercentage}%,, Containers: ({cuurentContainerCount})";
                mainStatusBar.IsOpen = true;
            }
        }
        private void File_ProgressEventHandler(object sender, EventArgs e)
        {
            this.bgWorker.ReportProgress((int)(sender as YarnLogFileReader).ProgressPrecent);
        }
        #endregion


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

     
        internal void CloseYarnAppContainer(YarnAppContainerPage yarnAppContainerPage)
        {
            var yarnAppContainerName = yarnAppContainerPage.YarnAppContainer.ShortId;

            RemoveMenuItem(yarnAppContainerPage.YarnAppContainer.YarnApplication.Header.Id, yarnAppContainerPage.YarnAppContainer.ShortId);
        }

        internal void AddContainer(YarnApplication yarnApp, YarnApplicationContainer container)
        {

            var parent = GetNavigationViewItems().Where(p => p.Content.ToString() == yarnApp.Header.Id).FirstOrDefault();

            // Check if contianer already open then switch only
           
            AddMenuItem(yarnApp.Header.Id, container.ShortId);
    }

       

        private void YarnApp_ContainerAddedEvent(object sender, ContainerAddedEventArgs e)
        {
            cuurentContainerCount = e.ContainersCount;
            this.bgWorker.ReportProgress(e.Progress, e.ContainersCount);

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
            file.CloseFile();
        }

        public void CloseYarnApp(YarnAppPage yarnAppPage)
        {
            var yarnAppName = yarnAppPage.YarnApp.Header.Id;
            var yarnApp = this.yarnApps.Where(p => p.Header.Id == yarnAppName).FirstOrDefault();
            this.yarnApps.Remove(yarnApp);
            yarnApp = null;

            RemoveMenuItem("", yarnAppName);
        }
    }
}
