using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace KnockerWPF.Infrastructure
{
    [ValueConversion(typeof(string), typeof(bool))]
    class UrlRxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type target)
        {
            Regex rx_route = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
            
            return rx_route.Match((string)value);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
