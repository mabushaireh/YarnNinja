using System;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.Views
{
    internal class ApplicationMasterConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public YarnApplicationContainer AppMaster{ set; get; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString().Equals(AppMaster.Id)) {
                return $" *** {value.ToString()}";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
