using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp
{
    public sealed partial class Shell : Window
    {
        private YarnApplication yarnApp;
        BackgroundWorker bgWorker = new BackgroundWorker();
        DoWorkEventHandler handler;

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

            OpenYarnAppFile(file.Path);

        }

        public void OpenYarnAppFile(string path)
        {
            bgWorker.WorkerReportsProgress = true;
            handler = (sender, e) =>
            {
                bgWorker.ReportProgress(0);
                OpenYarnAppLogFile(path);
                bgWorker.ReportProgress(100);
            };

            bgWorker.DoWork += handler;
            bgWorker.RunWorkerAsync();
        }


        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                mainStatusBar.Message = "Yarn App is loading";
                mainStatusBar.IsOpen = true;
                mainProgressBar.IsActive = true;
                mainProgressBar.Visibility = Visibility.Visible;
            }
            else if (e.ProgressPercentage == 100)
            {
                mainProgressBar.IsActive = false;
                mainProgressBar.Visibility = Visibility.Collapsed;

                mainStatusBar.Message = "Yarn App Loaded";
                mainStatusBar.IsOpen = true;
                bgWorker.DoWork -= handler;
            }
        }

        private void OpenYarnAppLogFile(string path)
        {
            var file = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
            var logText = FileIO.ReadTextAsync(file).GetAwaiter().GetResult();
            this.yarnApp = new Common.YarnApplication(logText);
        }



        private void OpenYarnAppLogFileAsync(StorageFile file)
        {


        }


        private async Task RefreshYarnAppInfo()
        {

        }


        private void ApplyTheme()
        {
            var settings = (Application.Current as App).Settings;
            Root.RequestedTheme = settings.IsLightTheme ? ElementTheme.Light : ElementTheme.Dark;
        }

    }
}
