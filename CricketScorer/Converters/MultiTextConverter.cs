using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CricketScorer.Converters
{
    class MultiTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var valuesAsStrings = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                valuesAsStrings[i] = values[i].ToString();
            }

            return string.Join(" ", valuesAsStrings);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                // ReSharper disable once CoVariantArrayConversion
                return s.Split(",").ToArray();
            }
            else
            {
                return new object[0];
            }
        }
    }
}
