using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WpfCardPrinter.Utils
{
    public class HttpUtils
    {
        public static string GetResponseText(string url, int timeout, string method = "GET", string cookie = "", string useragent = null, string encode = null)
        {
            string responseText = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Method = method;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36";
            request.Timeout = timeout * 1000;
            request.KeepAlive = false;
            request.Headers.Add("Cookie", cookie);          
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null)
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            //TODO: charset and encoding
                            Encoding code = Encoding.UTF8;
                            if (!string.IsNullOrWhiteSpace(encode))
                            {
                                code = Encoding.GetEncoding(encode);
                            }
                            StreamReader reader = new StreamReader(stream, code);
                            string responseString = reader.ReadToEnd();
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception(string.Format("返回了错误的消息，StatusCode=[{0}-{1}]，ResponseText=[{2}]", response.StatusCode, response.StatusDescription, responseString));
                            }
                            else
                            {
                                responseText = responseString;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                request.Abort();
                throw e;
            }
            return responseText;
        }
    }
}
