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
    public sealed partial class YarnAppContainerPage : Page
    {
        public YarnApplicationContainer YarnAppContainer { get; set; }

        public YarnAppContainerPage()
        {
            InitializeComponent();
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null) {
                this.YarnAppContainer = e.Parameter as YarnApplicationContainer;

                //(ViewModel as YarnAppPageViewModel).YarnApp = this.YarnAppContainer;
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

        public ICommand CloseCommand => new AsyncRelayCommand(CloseYarnAppContainer);



        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           
        }

        private void ListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            
        }

        private void ListViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }

        private async Task CloseYarnAppContainer()
        {
            ((Application.Current as App).Navigation as Shell).CloseYarnAppContainer(this);
        }



        private void ContainersDataGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
           
        }
    }
}
