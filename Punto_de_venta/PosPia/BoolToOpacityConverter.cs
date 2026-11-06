using Microsoft.UI.Xaml.Data;
using System;

namespace PosPia
{
 
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
         
            if (value is bool && (bool)value)
            {
                return 1.0; 
            }

            
            return 0.5; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
         
            throw new NotImplementedException();
        }
    }
}
