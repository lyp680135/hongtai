using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYNetCloud.Utils
{
    class TimeUtils
    {
        public static DateTime GetUnixStartTime()
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
        }

        public static long GetCurrentUnixTime()
        {
            System.DateTime startTime = GetUnixStartTime();
            long timeStamp = (long)(DateTime.Now - startTime).TotalSeconds;

            return timeStamp;
        }

        public static long GetUnixTimeFromDateTime(DateTime date)
        {
            System.DateTime startTime = GetUnixStartTime();
            long timeStamp = (long)(date - startTime).TotalSeconds;

            return timeStamp;
        }

        public static DateTime GetDateTimeFromUnixTime(long timestamp)
        {
            System.DateTime startTime = GetUnixStartTime();
            DateTime date = startTime.AddSeconds(timestamp);

            return date;
        }
    }
}
