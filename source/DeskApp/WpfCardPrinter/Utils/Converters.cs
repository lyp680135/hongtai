using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WpfCardPrinter.Utils;

namespace WpfCardPrinter
{
    public partial class QualityLevelDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            return (DataLibrary.EnumList.ProductQualityLevel)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class DateTimeDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }

            int timestamp = 0;
            DateTime date = TimeUtils.GetUnixStartTime();
            int.TryParse(value.ToString(), out timestamp);

            if (timestamp > 0)
            {
                date = TimeUtils.GetDateTimeFromUnixTime(timestamp);
            }

            if (date == TimeUtils.GetUnixStartTime())
            {
                return "";
            }

            return date.ToString("yyyy-MM-dd");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
