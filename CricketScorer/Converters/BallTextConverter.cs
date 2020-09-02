using System;
using System.Globalization;
using System.Windows.Data;

namespace CricketScorer.Converters
{
    public class BallTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Ball b)) return null;

            var result = $"Ball #{b.BallIndex}";
            if (b.BatsmanOut == null)
            {
                result += $" {b.RunsScored.RunCount} {b.RunsScored.RunType}";
            }
            else
            {
                result += $" Wicket {b.BatsmanOut.OutType}";
                if (b.RunsScored != null)
                {
                    result += $" and {b.RunsScored.RunCount} {b.RunsScored.RunType}";
                }
            }

            if (b.RunsScored != null)
            {
                result += $" {b.RunsScored.RunsShort}";
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}