using System;
using System.Text;

namespace Util {
    /// <summary>
    /// 系统扩展 - 日期
    /// </summary>
    public static partial class Extensions {
        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <param name="removeSecond">是否移除秒</param>
        public static string ToDateTimeString( this DateTime dateTime, bool removeSecond = false ) {
            if( removeSecond )
                return dateTime.ToString( "yyyy-MM-dd HH:mm" );
            return dateTime.ToString( "yyyy-MM-dd HH:mm:ss" );
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <param name="removeSecond">是否移除秒</param>
        public static string ToDateTimeString( this DateTime? dateTime, bool removeSecond = false ) {
            if( dateTime == null )
                return string.Empty;
            return ToDateTimeString( dateTime.Value, removeSecond );
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy-MM-dd"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToDateString( this DateTime dateTime ) {
            return dateTime.ToString( "yyyy-MM-dd" );
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy-MM-dd"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToDateString( this DateTime? dateTime ) {
            if( dateTime == null )
                return string.Empty;
            return ToDateString( dateTime.Value );
        }

        /// <summary>
        /// 获取格式化字符串，不带年月日，格式："HH:mm:ss"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToTimeString( this DateTime dateTime ) {
            return dateTime.ToString( "HH:mm:ss" );
        }

        /// <summary>
        /// 获取格式化字符串，不带年月日，格式："HH:mm:ss"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToTimeString( this DateTime? dateTime ) {
            if( dateTime == null )
                return string.Empty;
            return ToTimeString( dateTime.Value );
        }

        /// <summary>
        /// 获取格式化字符串，带毫秒，格式："yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToMillisecondString( this DateTime dateTime ) {
            return dateTime.ToString( "yyyy-MM-dd HH:mm:ss.fff" );
        }

        /// <summary>
        /// 获取格式化字符串，带毫秒，格式："yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToMillisecondString( this DateTime? dateTime ) {
            if( dateTime == null )
                return string.Empty;
            return ToMillisecondString( dateTime.Value );
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy年MM月dd日"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToChineseDateString( this DateTime dateTime ) {
            return string.Format( "{0}年{1}月{2}日", dateTime.Year, dateTime.Month, dateTime.Day );
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy年MM月dd日"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToChineseDateString( this DateTime? dateTime ) {
            if( dateTime == null )
                return string.Empty;
            return ToChineseDateString( dateTime.SafeValue() );
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy年MM月dd日 HH时mm分"
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <param name="removeSecond">是否移除秒</param>
        public static string ToChineseDateTimeString( this DateTime dateTime, bool removeSecond = false ) {
            StringBuilder result = new StringBuilder();
            result.AppendFormat( "{0}年{1}月{2}日", dateTime.Year, dateTime.Month, dateTime.Day );
            result.AppendFormat( " {0}时{1}分", dateTime.Hour, dateTime.Minute );
            if( removeSecond == false )
                result.AppendFormat( "{0}秒", dateTime.Second );
            return result.ToString();
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy年MM月dd日 HH时mm分"
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <param name="removeSecond">是否移除秒</param>
        public static string ToChineseDateTimeString( this DateTime? dateTime, bool removeSecond = false ) {
            if( dateTime == null )
                return string.Empty;
            return ToChineseDateTimeString( dateTime.Value, removeSecond );
        }

        /// <summary>
        /// 获取描述
        /// </summary>
        /// <param name="span">时间间隔</param>
        public static string Description( this TimeSpan span ) {
            StringBuilder result = new StringBuilder();
            if( span.Days > 0 )
                result.AppendFormat( "{0}天", span.Days );
            if( span.Hours > 0 )
                result.AppendFormat( "{0}小时", span.Hours );
            if( span.Minutes > 0 )
                result.AppendFormat( "{0}分", span.Minutes );
            if( span.Seconds > 0 )
                result.AppendFormat( "{0}秒", span.Seconds );
            if( span.Milliseconds > 0 )
                result.AppendFormat( "{0}毫秒", span.Milliseconds );
            if ( result.Length > 0 )
                return result.ToString();
            return $"{span.TotalSeconds * 1000}毫秒";
        }
 
        public static DateTime GetUnixStartTime()
        {
#pragma warning disable CS0618 // 类型或成员已过时
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
#pragma warning restore CS0618 // 类型或成员已过时
        }

       /// <summary>
       /// 获取当前时间并转为long
       /// </summary>
       /// <returns></returns>
        public static long GetCurrentUnixTime()
        {
            System.DateTime startTime = GetUnixStartTime();
            long timeStamp = (long)(DateTime.Now - startTime).TotalSeconds;

            return timeStamp;
        }

        /// <summary>
        /// 根据传入的DateTime 转为long
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long GetUnixTimeFromDateTime(this DateTime date)
        {
            System.DateTime startTime = GetUnixStartTime();
            long timeStamp = (long)(date - startTime).TotalSeconds;

            return timeStamp;
        }

        /// <summary>
        /// 根据long转为 DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromUnixTime(this long timestamp)
        {
            System.DateTime startTime = GetUnixStartTime();
            DateTime date = startTime.AddSeconds(timestamp);

            return date;
        }
    }
}
