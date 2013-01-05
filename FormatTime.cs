using System;
using System.Globalization;
using System.Windows.Data;

namespace Imgu
{
    public class FormatTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var time = value is DateTime ? (DateTime) value : new DateTime();
            if (time.Date == DateTime.Parse("0001-01-01 00:00:00"))
                return "Inget datum";
            return time.Day + "-" + time.ToString("MMMM").UppercaseFirst() + "-" + time.Year;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
