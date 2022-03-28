using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using YarnNinja.App.WinApp.Services;
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

        

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        private void ListViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType is PointerDeviceType.Mouse or PointerDeviceType.Pen)
            {
                VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
            }
        }

        private void ListViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }

        private async Task CloseYarnApp()
        {

            ((Application.Current as App).Navigation as Shell).CloseYarnApp(this);
        }

        private async Task OpenEditDialog()
        {
            EditDialog.Title = "Edit Character";
            EditDialog.PrimaryButtonText = "Update";
            //EditDialog.PrimaryButtonCommand = UpdateCommand;
            //var clone = ViewModel.Current.Clone();
            //clone.Name = ViewModel.Current.Name;
            //EditDialog.DataContext = clone;
            await EditDialog.ShowAsync();
        }

        private void Update()
        {
            //*ViewModel.UpdateItem(EditDialog.DataContext as Character, ViewModel.Current);
        }

        private void Insert()
        {
            // Does not work when filter is active:
            // ViewModel.Items.Add(EditDialog.DataContext as Character);

            /** var character = ViewModel.AddItem(EditDialog.DataContext as Character);
            if (ViewModel.Items.Contains(character))
            {
                ViewModel.Current = character;
            }**/
        }
    }
}
