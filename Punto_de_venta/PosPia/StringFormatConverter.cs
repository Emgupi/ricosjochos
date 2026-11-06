using Microsoft.UI.Xaml.Data;
using System;

namespace PosPia
{
    public class StringFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value == null)
                return null;

            if (parameter == null)
                return value.ToString();

            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:" + parameter?.ToString() + "}", value);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
        {

            throw new NotImplementedException();
        }
    }
}
