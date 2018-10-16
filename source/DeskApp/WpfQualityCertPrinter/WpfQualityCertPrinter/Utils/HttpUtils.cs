using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Media.Imaging;

namespace XYNetCloud.Utils
{
    class HttpUtils
    {
        public int MAX_BUFF_LENGTH = 4096;
        private string request_source = "WPF";
        private string request_token = null;

        private static HttpUtils instance = null;

        public static HttpUtils GetInstance()
        {
            if (instance == null)
            {
                instance = new HttpUtils();
                instance.request_token = null;
            }
            else
            {
                instance.request_token = null;
            }

            return instance;
        }

        public static HttpUtils GetInstance(string token)
        {
            if (instance == null)
            {
                instance = new HttpUtils(token);
            }
            else
            {
                instance.SetToken(token);
            }

            return instance;
        }

        public HttpUtils()
        {

        }

        public HttpUtils(string token)
        {
            this.request_token = token;
        }

        public void SetToken(string token)
        {
            this.request_token = token;
        }

        public string Get(string url, List<Tuple<string, string>> paramlist)
        {
            HttpWebRequest request = null;

            StringBuilder postDataBuilder = new StringBuilder();
            string paramstr_format = "{0}={1}";
            int index = 0;
            foreach (var param in paramlist)
            {
                index++;
                postDataBuilder.Append(string.Format(paramstr_format, param.Item1, param.Item2));

                if (index < paramlist.Count)
                {
                    postDataBuilder.Append("&");
                }
            }

            request = WebRequest.Create(url + "?" + postDataBuilder.ToString()) as HttpWebRequest;
            request.Method = "GET";

            request.CookieContainer = new CookieContainer();

            request.Accept = "*/*";
            request.Headers["Accept-Language"] = "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3";
            request.Headers["Accept-Charset"] = "utf-8;q=0.7,*;q=0.3";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0";
            request.KeepAlive = false;
            request.ContentType = "application/json; charset=UTF-8";

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse, Encoding.UTF8);
            Char[] readBuff = new Char[MAX_BUFF_LENGTH];
            int count = streamRead.Read(readBuff, 0, MAX_BUFF_LENGTH);

            StringBuilder content = new StringBuilder();
            while (count > 0)
            {
                content.Append(readBuff, 0, count);
                count = streamRead.Read(readBuff, 0, MAX_BUFF_LENGTH);
            }

            streamRead.Close();

            return content.ToString();
        }

        public string Post(string url, List<Tuple<string,string>> paramlist, bool usecrypt = false)
        {
            HttpWebRequest request = null;

            request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version10;

            request.CookieContainer = new CookieContainer();

            request.AllowAutoRedirect = true;
            request.Accept = "*/*";
            request.Headers["Accept-Language"] = "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3";
            request.Headers["Accept-Charset"] = "utf-8;q=0.7,*;q=0.3";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0";
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            StringBuilder postDataBuilder = new StringBuilder();
            string paramstr_format = "{0}={1}";

            if (usecrypt)
            {
                JArray arr = new JArray();
                foreach (var param in paramlist)
                {
                    arr.Add(new JObject(){
                        {param.Item1,param.Item2},
                        {param.Item1,param.Item2},
                    });
                }

                postDataBuilder.Append(string.Format(paramstr_format, "Source", request_source));
                postDataBuilder.Append(string.Format(paramstr_format, "Token", request_token));
                postDataBuilder.Append(string.Format(paramstr_format, "RequestData", JsonConvert.SerializeObject(arr)));
            }
            else
            {
                int index = 0;
                foreach (var param in paramlist)
                {
                    index++;
                    postDataBuilder.Append(string.Format(paramstr_format, param.Item1, param.Item2));

                    if (index < paramlist.Count)
                    {
                        postDataBuilder.Append("&");
                    }
                }
            }

            byte[] data = Encoding.UTF8.GetBytes(postDataBuilder.ToString());
            request.ContentLength = data.Length;

            try
            {
                using (Stream newStream = request.GetRequestStream())
                {
                    newStream.Write(data, 0, data.Length);
                }

                request.Timeout = 60 * 1000;

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse, Encoding.UTF8);

                Char[] readBuff = new Char[MAX_BUFF_LENGTH];
                int count = streamRead.Read(readBuff, 0, MAX_BUFF_LENGTH);

                StringBuilder content = new StringBuilder();
                while (count > 0)
                {
                    content.Append(readBuff, 0, count);
                    count = streamRead.Read(readBuff, 0, MAX_BUFF_LENGTH);
                }

                streamRead.Close();

                return content.ToString();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("HTTP POST请求生成质保书出错：" + e.Message);

                throw e;
            }

            return null;
        }

        public static Bitmap GetHttpBitmap(string url)
        {
            Bitmap bitMap = null;

            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                WebResponse response = request.GetResponse();
                System.Drawing.Image img = System.Drawing.Image.FromStream(response.GetResponseStream());
                bitMap = new Bitmap(img);

            }
            catch (Exception e)
            {
                LogHelper.WriteLog("获取远程图片出错:"+e.Message+",path="+url);
            }

            return bitMap;
        }

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
