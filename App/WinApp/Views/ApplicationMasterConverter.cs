using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using CommunityToolkit.WinUI.UI.Converters;
using YarnNinja.Common;

namespace YarnNinja.App.WinApp.Views
{
    internal class ApplicationMasterConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public List<YarnApplicationContainer> Containers { set; get; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var appMasterId = this.Containers.OrderBy(p => p.Order).Select(p => p.Id).FirstOrDefault();

            if (value.ToString().Equals(appMasterId)) {
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
