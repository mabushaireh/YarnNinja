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
using Windows.Foundation;
using Windows.Foundation.Collections;
using YarnNinja.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace YarnNinja.App.WinApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ContainerPage : Page
    {
        YarnApplicationContainer item;
        string logType = "none";
        string[] logTypes = { "test", "test" };
        public ContainerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            item = e.Parameter as YarnApplicationContainer;
            var logtypes = this.item.Logs.Select(p => p.YarnLogType).Distinct().OrderBy( p => p)
                .ToList();

            foreach (var logtype in logtypes)
            {
                var menuItem = new MenuFlyoutItem()
                {
                    Text = logtype.Trim(),
                };

                menuItem.Click += MenuItem_Click;

                menuLogTypes.Items.Add(menuItem);
            }




        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            logtypesDrop.Content = item.Text;

        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void ToggleErrorState()
        {
        }
        private void ToggleWarnState()
        {
        }
        private void ToggleInfoState()
        {
        }


    }
}
