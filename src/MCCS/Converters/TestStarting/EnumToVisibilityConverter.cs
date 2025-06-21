using MCCS.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MCCS.Converters.TestStarting
{
    public sealed class EnumToVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2 || parameter == null)
                return Visibility.Collapsed;
            if (values[1].ToString() == "Combine" && parameter.ToString() == "ComboBox") return Visibility.Visible;
            if (values[1].ToString() == "Combine" && values[0] is ControlCombineInfo combineInfo && combineInfo.CombineChannelId == "None" && parameter.ToString() == "TextBox")
                return Visibility.Visible;
            return Visibility.Collapsed;
        } 

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
