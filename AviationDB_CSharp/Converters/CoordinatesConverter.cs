using System;
using System.Globalization;
using System.Windows.Data;

namespace AviationDB_CSharp.Converters
{
    public class CoordinatesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Правильный синтаксис 1: с явной проверкой типа
            if (value is double?)
            {
                var nullableValue = (double?)value;
                if (nullableValue.HasValue)
                {
                    return nullableValue.Value.ToString("F4");
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str, out double result))
            {
                return result;
            }
            return null;
        }
    }
}
