
using CommunityToolkit.Mvvm.ComponentModel;

namespace YarnNinja.App.WinApp.Models
{
    public partial class Settings : ObservableObject
    {
        [ObservableProperty]
        private bool isLightTheme;

        public Settings()
        {
            // Required for serialization.
        }
    }
}