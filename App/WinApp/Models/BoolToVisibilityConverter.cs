using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls;

namespace YarnNinja.App.WinApp.Models
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        // Define the Convert method to convert a DateTime value to 
        // a month string.
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            // value is the data from the source object.
            bool thisflag = (bool)value;
            if (thisflag)
                return double.NaN;


            else
                return 0.0;

        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
