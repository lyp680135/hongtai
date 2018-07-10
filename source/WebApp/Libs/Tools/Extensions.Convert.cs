using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util {
    /// <summary>
    /// 系统扩展 - 类型转换
    /// </summary>
    public static partial class Extensions {
        /// <summary>
        /// 安全转换为字符串，去除两端空格，当值为null时返回""
        /// </summary>
        /// <param name="input">输入值</param>
        public static string SafeString( this object input ) {
            return input == null ? string.Empty : input.ToString().Trim();
        }

        /// <summary>
        /// 转换为bool
        /// </summary>
        /// <param name="obj">数据</param>
        public static bool ToBool( this object obj ) {
            return Util.Helpers.Convert.ToBool( obj );
        }

        /// <summary>
        /// 转换为可空bool
        /// </summary>
        /// <param name="obj">数据</param>
        public static bool? ToBoolOrNull( this object obj ) {
            return Util.Helpers.Convert.ToBoolOrNull( obj );
        }

        /// <summary>
        /// 转换为int
        /// </summary>
        /// <param name="obj">数据</param>
        public static int ToInt( this object obj ) {
            return Util.Helpers.Convert.ToInt( obj );
        }

        /// <summary>
        /// 转换为可空int
        /// </summary>
        /// <param name="obj">数据</param>
        public static int? ToIntOrNull( this object obj ) {
            return Util.Helpers.Convert.ToIntOrNull( obj );
        }

        /// <summary>
        /// 转换为long
        /// </summary>
        /// <param name="obj">数据</param>
        public static long ToLong( this object obj ) {
            return Util.Helpers.Convert.ToLong( obj );
        }

        /// <summary>
        /// 转换为可空long
        /// </summary>
        /// <param name="obj">数据</param>
        public static long? ToLongOrNull( this object obj ) {
            return Util.Helpers.Convert.ToLongOrNull( obj );
        }

        /// <summary>
        /// 转换为double
        /// </summary>
        /// <param name="obj">数据</param>
        public static double ToDouble( this object obj , int? digits = null)
        {
            return Util.Helpers.Convert.ToDouble( obj , digits);
        }

        /// <summary>
        /// 转换为可空double
        /// </summary>
        /// <param name="obj">数据</param>
        public static double? ToDoubleOrNull( this object obj ) {
            return Util.Helpers.Convert.ToDoubleOrNull( obj );
        }

        /// <summary>
        /// 转换为decimal
        /// </summary>
        /// <param name="obj">数据</param>
        public static decimal ToDecimal( this object obj ) {
            return Util.Helpers.Convert.ToDecimal( obj );
        }

        /// <summary>
        /// 转换为可空decimal
        /// </summary>
        /// <param name="obj">数据</param>
        public static decimal? ToDecimalOrNull( this object obj ) {
            return Util.Helpers.Convert.ToDecimalOrNull( obj );
        }

        /// <summary>
        /// 转换为日期
        /// </summary>
        /// <param name="obj">数据</param>
        public static DateTime ToDate( this object obj ) {
            return Util.Helpers.Convert.ToDate( obj );
        }

        /// <summary>
        /// 转换为可空日期
        /// </summary>
        /// <param name="obj">数据</param>
        public static DateTime? ToDateOrNull( this object obj ) {
            return Util.Helpers.Convert.ToDateOrNull( obj );
        }

        /// <summary>
        /// 转换为Guid
        /// </summary>
        /// <param name="obj">数据</param>
        public static Guid ToGuid( this object obj ) {
            return Util.Helpers.Convert.ToGuid( obj );
        }

        /// <summary>
        /// 转换为可空Guid
        /// </summary>
        /// <param name="obj">数据</param>
        public static Guid? ToGuidOrNull( this object obj ) {
            return Util.Helpers.Convert.ToGuidOrNull( obj );
        }
 
        /// <summary>
        /// 转换为Guid集合
        /// </summary>
        /// <param name="obj">字符串集合</param>
        public static List<Guid> ToGuidList( this IList<string> obj ) {
            if( obj == null )
                return new List<Guid>();
            return obj.Select( t => t.ToGuid() ).ToList();
        }

        public static string Base64ToString(this string base64, Encoding encoding = null)
        {
            base64 = base64.Replace(" ", "+").Replace("%2F", "/").Replace("%3D", "=");
            string result = string.Empty;
            byte[] c = System.Convert.FromBase64String(base64);
            if (encoding == null)
            {
                result = Encoding.UTF8.GetString(c);
            }
            else
            {
                result = encoding.GetString(c);
            }
            return result;
        }
    }
}
