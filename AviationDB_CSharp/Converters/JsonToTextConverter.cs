using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace AviationDB_CSharp.Converters
{
    class JsonToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string json || string.IsNullOrWhiteSpace(json))
                return string.Empty;

            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("en").GetString() ?? string.Empty;
            }
            catch
            {
                return value?.ToString() ?? string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
