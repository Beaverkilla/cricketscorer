using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CricketScorer.Converters
{
    class OversToSymbolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values?[0] is List<Ball>)) return null;

            var ballsInOver = (List<Ball>) values[0];

            var enumerable = ballsInOver.Select(b => b.GetBallSymbol()).ToList();
            return string.Join("", enumerable);

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
