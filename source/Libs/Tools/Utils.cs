using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Web;
using System.Security.Cryptography;

namespace Util
{
    public class Utils
    {

        #region 字符串处理

        /// <summary>
        /// 剪切字符串
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="intLen"></param>
        /// <returns></returns>
        public static string CutString(string strInput, int intLen)
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
        public static string ToHtmlText(string str)
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
        /// 去除html标签
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveHtml(string str)
        {
            string text1 = "<.*?>";
            Regex regex1 = new Regex(text1);
            str = regex1.Replace(str, "");
            str = str.Replace("&nbsp;", " ");
            return str;
        }

        /// <summary>
        /// 返回URL编码的值
        /// </summary>
        /// <param name="str">传入参数</param>
        /// <returns></returns>
        public static string UrlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            return System.Web.HttpUtility.UrlEncode(str);
        }


        public static string Md5(string value, int code)
        {
            byte[] bytes;
            using (var md5 = MD5.Create())
            {
                bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            var result = new StringBuilder();
            foreach (byte t in bytes)
            {
                result.Append(t.ToString("X2"));
            }

            if (code == 16) //16位MD5加密（取32位加密的9~25字符）  
            {
                return result.ToString().ToLower().Substring(8, 16);
            }
            if (code == 32) //32位加密  
            {
                return result.ToString().ToLower();
            }
            return "000000000000000";
        }


        public static bool IsNumber(string checkStr)
        {
            if (string.IsNullOrEmpty(checkStr)) { return false; }
            return Regex.IsMatch(checkStr, @"^[-]{0,1}\d+$");

        }



        /// <summary>
        /// 请求字符串类型
        /// </summary>
        /// <param name="requestStr">请求的Request值</param>
        /// <returns>返回字符串，null则返回""</returns>
        public static string RequestString(string requestStr)
        {
            if (string.IsNullOrEmpty(requestStr))
            {
                return "";
            }
            else
            {
                return requestStr;
            }
        }

        /// <summary>
        /// 获取request 数字类型值
        /// </summary>
        /// <param name="requestStr">获取request值</param>
        /// <returns>返回数字类型,空则返回0</returns>
        public static int RequestInt(string requestStr)
        {
            if (Utils.IsNumber(requestStr))
            {
                return Int32.Parse(requestStr);
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region 文件处理
         



        /// <summary>
        /// 获得一个17位时间随机数
        /// </summary>
        /// <returns>返回随机数</returns>
        public static string GetDataRandom()
        {

            return System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }


        public static string GetFileSize(float filesize)
        {
            float filesizeFloat = filesize / 1024;
            if (filesizeFloat < 1024)
            {
                return Math.Ceiling(filesizeFloat) + "K";
            }
            else
            {
                filesizeFloat = filesizeFloat / 1024;
                return Math.Round(filesizeFloat, 2) + "M";
            }

        }

         
        #endregion


        #region 网络文件获取
        /// <summary>
        /// 下载网络文件
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <param name="savePath">保存路径</param>
        /// <returns></returns>
        public static bool DownLoadFile(string url, string savePath)
        {
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(url, savePath);
                return true;
            }
            catch
            {
                return false;
            }

        }



        /// <summary>
        /// 下载网络文件(传递前导页，破简单的反盗链)
        /// </summary>
        /// <param name="url">网络文件地址</param>
        /// <param name="savePath">保存文件的路径</param>
        /// <param name="referer">需要传递的前导页</param>
        /// <returns>下载文件的大小(K)</returns>
        public static float DownLoadFile(string url, string savePath, string referer)
        {
            try
            {
                long ThisLength = 0;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.01; Windows NT 5.0)";
                myHttpWebRequest.Referer = referer;
                myHttpWebRequest.Timeout = 10 * 1000;
                myHttpWebRequest.Method = "GET";


                HttpWebResponse res = myHttpWebRequest.GetResponse() as HttpWebResponse;
                System.IO.Stream stream = res.GetResponseStream();


                byte[] b = new byte[1024];
                int nReadSize = 0;
                nReadSize = stream.Read(b, 0, 1024);

                System.IO.FileStream fs = System.IO.File.Create(savePath);
                try
                {

                    while (nReadSize > 0)
                    {
                        ThisLength += nReadSize;
                        fs.Write(b, 0, nReadSize);
                        nReadSize = stream.Read(b, 0, 1024);
                    }
                }
                catch
                {
                    ThisLength = 0;
                }
                finally
                {
                    fs.Close();
                }
                res.Close();
                stream.Close();
                myHttpWebRequest.Abort();
                if (ThisLength < 1024 && ThisLength > 0)
                {
                    return 1;
                }
                return (float)(ThisLength / 1024);
            }
            catch
            {
                return 0;
            }
        }



        #endregion



        #region URL参数辅助功能
        /// <summary> 
        /// URL中去除指定参数 
        /// </summary> 
        /// <param name="url">地址</param> 
        /// <param name="param">参数</param> 
        /// <returns></returns> 
        public static string buildurl(string url, string param)
        {
            string url1 = url;
            if (url.IndexOf(param) > 0)
            {
                if (url.IndexOf("&", url.IndexOf(param) + param.Length) > 0)
                {
                    url1 = url.Substring(0, url.IndexOf(param) - 1) + url.Substring(url.IndexOf("&", url.IndexOf(param) + param.Length) + 1);
                }
                else
                {
                    url1 = url.Substring(0, url.IndexOf(param) - 1);
                }
                return url1;
            }
            else
            {
                return url1;
            }
        }


        /// <summary> 
        /// 给URL中添加参数与值 ， 如果 当前参数，存在，则替换该址 
        /// </summary> 
        /// <param name="url">地址</param> 
        /// <param name="param">参数名</param> 
        /// <param name="paramValue">参数值</param> 
        /// <returns></returns> 
        public static string buildurl_AddParam(string url, string param, string paramValue)
        {
            string url1 = url;
            Regex urlRegex = new Regex(@"(^|\?|&)" + param + "=([^&]*)(&|$)");
            Match m = urlRegex.Match(url1.ToLower());
            string courseId = string.Empty;
            if (m.Success)
            {
                url1 = url1.Replace(param + "=" + m.Groups[2].Value, param + "=" + paramValue);
            }
            else
            {
                if (url1.IndexOf("?") > -1)
                {
                    url1 = url1 + "&" + param + "=" + paramValue;
                }
                else
                {
                    url1 = url1 + "?" + param + "=" + paramValue;
                }
            }
            return url1;
        }
        #endregion



        //#region "获取页面url"
        ///// <summary> 
        ///// 获取当前访问页面地址参数 
        ///// </summary> 
        //public static string GetScriptNameQueryString
        //{
        //    get
        //    {
        //        return HttpContext.Current.Request.ServerVariables["QUERY_STRING"].ToString();
        //    }
        //}
        ///// <summary> 
        ///// 获取当前访问页面地址 
        ///// </summary> 
        //public static string GetScriptName
        //{
        //    get
        //    {
        //        return HttpContext.Current.Request.ServerVariables["SCRIPT_NAME"].ToString();
        //    }
        //}
        ///// <summary> 
        ///// 获取当前访问页面Url 
        ///// </summary> 
        //public static string GetScriptUrl
        //{
        //    get
        //    {
        //        return GetScriptNameQueryString == "" ? GetScriptName : string.Format("{0}?{1}", GetScriptName, GetScriptNameQueryString);
        //    }
        //}
        ///// <summary> 
        ///// 获取当前访问页面 参数 
        ///// </summary> 
        //public static string GetScriptNameQuery
        //{
        //    get
        //    {
        //        return HttpContext.Current.Request.Url.Query;
        //    }
        //}
        //#endregion

    }
}
