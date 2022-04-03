using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;
using YarnNinja.App.WinApp.ViewModels;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.Views
{
    public sealed partial class YarnAppPage : Page
    {
        public YarnApplication YarnApp { get; set; }

        public YarnAppPage()
        {
            InitializeComponent();
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null) {
                this.YarnApp = e.Parameter as YarnApplication;

                (ViewModel as YarnAppPageViewModel).YarnApp = this.YarnApp;
            }
            ViewModel.IsActive = true;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

            base.OnNavigatingFrom(e);
        }

        public ICommand CloseCommand => new AsyncRelayCommand(CloseYarnApp);
        //public ICommand OpenContainersCommand => new AsyncRelayCommand(OpenEditDialog);



        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Current" && ViewModel.HasCurrent)
            {
                WorkersListView.ScrollIntoView(ViewModel.Current);
                ContainersDataGrid.ItemsSource = null;
                ContainersDataGrid.ItemsSource = ViewModel.Containers;


            }
        }

        private void ListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
            {
            }
        }

        private void ListViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }

        private async Task CloseYarnApp()
        {

            ((Application.Current as App).Navigation as Shell).CloseYarnApp(this);
        }



        private void ContainersDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is not DataGrid)
            {
                return;
            }

            var dgrid = sender as DataGrid;
           

            var selectedContainer = dgrid.SelectedItem as YarnApplicationContainer;
            ((Application.Current as App).Navigation as Shell).AddContainer(YarnApp, selectedContainer);
        }
    }
}
