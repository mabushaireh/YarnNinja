using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using YarnNinja.Common;
using YarnNinja.Common.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace YarnNinja.App.WinApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class YarnAppPage : Page
    {
        public static YarnAppPage Current;

        public YarnAppPage()
        {
            this.InitializeComponent();
            Current = this;
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if ((e.Parameter is StorageFile) && ((StorageFile)e.Parameter) != null)
            {
                OpenYarnAppLogFile((StorageFile)e.Parameter);
            }
        }

        private YarnApplication yarnApp;
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem selectedItem)
            {
                string sortOption = selectedItem.Tag.ToString();
                switch (sortOption)
                {
                    case "open":
                        OpenYarnApp();
                        break;
                    case "close":
                        //SortByMatch();
                        break;
                    case "exit":
                        App.Current.Exit();
                        break;
                }
            }

        }

        private async void OpenYarnApp()
        {
            var openFileDialog = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };

            openFileDialog.FileTypeFilter.Add(".log");

            WinRT.Interop.InitializeWithWindow.Initialize(openFileDialog, App.WindowHandle);



            StorageFile file = await openFileDialog.PickSingleFileAsync();

            if (file != null)
            {
                brogressBar.IsActive = true;
                brogressBar.Visibility = Visibility.Visible;
                await OpenYarnAppLogFile(file);
                brogressBar.IsActive = false;
                brogressBar.Visibility = Visibility.Collapsed;
            }
        }

        public async Task OpenYarnAppLogFile(StorageFile file)
        {
            var logText = await FileIO.ReadTextAsync(file);
            this.yarnApp = new YarnApplication(logText);

            await RefreshYarnAppInfo();
        }

        private Task RefreshYarnAppInfo()
        {
            tbApplicationId.Text = yarnApp.Header.Id;
            tbApplicationType.Text = yarnApp.Header.Type.ToString();
            tbStart.Text = yarnApp.Header.Start.ToString("yyyy-MM-dd HH:mm:ss,fff");
            tbFinish.Text = yarnApp.Header.Finish.ToString("yyyy-MM-dd HH:mm:ss,fff");
            tbDuration.Text = yarnApp.Header.Duration.ToString(@"hh\:mm\:ss");
            tbNumOfContainers.Text = yarnApp.Containers.Count().ToString();
            tbStatus.Text = yarnApp.Header.Status.ToString();
            if (yarnApp.Header.Type == YarnApplicationType.Tez)
                tbDAGs_Tasks.Text = $"Submitted: {yarnApp.Header.SubmittedDags}, Successfull: {yarnApp.Header.SuccessfullDags}, Failed: {yarnApp.Header.FailedDags}, Killed: {yarnApp.Header.KilledDags}";
            else if (yarnApp.Header.Type == YarnApplicationType.MapReduce)
                tbDAGs_Tasks.Text = $"Completed Mappers: {yarnApp.Header.CompletedMappers}, Completed Reducers: {yarnApp.Header.CompletedReducers}";
            else
            { }

            tbUser.Text = yarnApp.Header.User;
            tbQueue.Text = yarnApp.Header.QueueName;

            //fill wokers
            var workers = yarnApp.WorkerNodes.OrderBy(t => t).ToList();
            workers.Insert(0, "ALL");
            listWorkers.ItemsSource = workers;
            var containers = yarnApp.Containers.OrderBy(p => p.Order).ToList();
            dgContainers.ItemsSource = containers;

            listWorkers.SelectionChanged += (sender, e) => {
                var item = (sender as ListView).SelectedValue;
                if (item != null)
                {
                    string applicatgionMasterId = "NA";

                    if (yarnApp.ApplicationMaster is not null)
                    {
                        applicatgionMasterId = yarnApp.ApplicationMaster.Id;
                    }
                    var containers = yarnApp.Containers.Where(p => p.WorkerNode.Equals(item.ToString()) || item.ToString().Equals("ALL")).OrderBy(p => p.Order).ToList();

                    dgContainers.ItemsSource = containers;
                }
            };


            return Task.CompletedTask;
        }

        private void dgContainers_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dgrid = sender as DataGrid;
            if (!(sender is DataGrid))
            {
                return;
            }

            var selectedContainer = dgrid.SelectedItem;
            this.Frame.Navigate(typeof(ContainerPage), selectedContainer);

        }
    }
}
