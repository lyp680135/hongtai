using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Util
{
    /// <summary>
    /// 系统扩展 - 公共
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 安全获取值，当值为null时，不会抛出异常
        /// </summary>
        /// <param name="value">可空值</param>
        public static T SafeValue<T>(this T? value) where T : struct
        {
            return value ?? default(T);
        }

        /// <summary>
        /// 转换为用分隔符连接的字符串
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="list">集合</param>
        /// <param name="quotes">引号，默认不带引号，范例：单引号 "'"</param>
        /// <param name="separator">分隔符，默认使用逗号分隔</param>
        public static string Join<T>(this IEnumerable<T> list, string quotes = "", string separator = ",")
        {
            return Util.Helpers.String.Join(list, quotes, separator);
        }

        /// <summary>
        /// 获取完整路径
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(this HttpRequest request, List<KeyValuePair<string,string>> AddQuery = null)
        {
            var AbsUri = new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
            
            if (AddQuery != null)
            {
                foreach (var str in AddQuery)
                {
                    Regex urlRegex = new Regex(@"(^|\?|&)" + str.Key + "=([^&]*)(&|$)");
                    Match m = urlRegex.Match(AbsUri);

                    if (m.Success)
                    {
                        AbsUri = AbsUri.Replace(str.Key + "=" + m.Groups[2].Value, str.Key + "=" + str.Value);
                    }
                    else
                    {
                        if (AbsUri.IndexOf("?") > -1)
                        {
                            AbsUri = AbsUri + "&" + str.Key + "=" + str.Value;
                        }
                        else
                        {
                            AbsUri = AbsUri + "?" + str.Key + "=" + str.Value;
                        }
                    }
                }
            }

            return AbsUri;


        }

        /// <summary>
        /// 去除html标签
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveHtml(this string str)
        {
            string text1 = "<.*?>";
            Regex regex1 = new Regex(text1);
            str = regex1.Replace(str, "");
            str = str.Replace("&nbsp;", " ");
            return str;
        }

        /// <summary>
        /// 剪切字符串
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="intLen"></param>
        /// <returns></returns>
        public static string CutString(this string strInput, int intLen)
        {
            strInput = strInput.Trim();
            byte[] buffer1 = Encoding.Default.GetBytes(strInput);
            if (buffer1.Length > intLen)
            {
                string text1 = "";
                for (int num1 = 0; num1 < strInput.Length; num1++)
                {
                    byte[] buffer2 = Encoding.Default.GetBytes(text1);
                    if (buffer2.Length >= (intLen - 4))
                    {
                        break;
                    }
                    text1 = text1 + strInput.Substring(num1, 1);
                }
                return (text1 + "...");
            }
            return strInput;
        }

        /// <summary>
        /// 文本框内容输出成html
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToHtmlText(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            StringBuilder builder1 = new StringBuilder();
            builder1.Append(str);
            builder1.Replace("&", "&amp;");
            builder1.Replace("<", "&lt;");
            builder1.Replace(">", "&gt;");
            builder1.Replace("\"", "&quot;");
            builder1.Replace("\r", "<br>");
            builder1.Replace(" ", "&nbsp;");
            return builder1.ToString();
        }

        /// <summary>
        /// 文本框内容输出成html
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToHtml(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            StringBuilder builder1 = new StringBuilder();
            builder1.Append(str);
            builder1.Replace("&amp", "&");
            builder1.Replace("&lt;", "<");
            builder1.Replace("&gt;", ">");
            builder1.Replace("&quot;", "\"");
            builder1.Replace("<br>", "\r");
            builder1.Replace("&nbsp;", " ");
            return builder1.ToString();
        }

    }
}
