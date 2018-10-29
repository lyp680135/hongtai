using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using System.Net;
using System.IO;

namespace XYNetCloud.Utils
{
    class QRCodeUtils
    {
        public static string SHORT_API_URL = System.Configuration.ConfigurationManager.AppSettings["ShorturlAPIUrl"];

        /// <summary>
        /// 获取短链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetShortUrl(string url)
        {
            ///直接用cnzui.com转发
            return url;

            HttpWebRequest request = null;

            request = WebRequest.Create(SHORT_API_URL+string.Format("?access_token={0}&url_long={1}","abc",url)) as HttpWebRequest;
            request.Method = "GET";

            request.CookieContainer = new CookieContainer();


            request.Accept = "*/*";
            request.Headers["Accept-Language"] = "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3";
            request.Headers["Accept-Charset"] = "utf-8;q=0.7,*;q=0.3";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0";
            request.KeepAlive = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0"; ;
            request.ContentType = "application/json; charset=UTF-8";

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

           
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse, Encoding.UTF8);
            Char[] readBuff = new Char[256];
            int count = streamRead.Read(readBuff, 0, 256);

            StringBuilder content = new StringBuilder();
            while (count > 0)
            {
                content.Append(readBuff, 0, count);
                count = streamRead.Read(readBuff, 0, 256);
            }

            streamRead.Close();

            return content.ToString();
        }

        /// <summary>
        /// 生成基础二维码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Bitmap Generate(string url, int width)
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Renderer = new BitmapRenderer { Background = Color.White, Foreground = Color.Black };
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            writer.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.L);

            writer.Options.Height = writer.Options.Width = width;
            writer.Options.Margin = 0;
            BitMatrix bm = writer.Encode(url);

            Bitmap img = writer.Write(bm);
            return img;
        }

        /// <summary>
        /// 生成有LOGO的二维码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="logopath"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static Bitmap Generate(string url, string logopath, int width)
        {
             Bitmap logo = null;
            try 
            {
                logo = new Bitmap(logopath);
            }
            catch
            {
                
            }

            MultiFormatWriter multiwriter = new MultiFormatWriter();
           
            Dictionary<EncodeHintType, object> hint = new Dictionary<EncodeHintType, object>();
            hint.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            hint.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.L);

            //生成二维码 
            BitMatrix bm = multiwriter.encode(url, BarcodeFormat.QR_CODE, width, width, hint);
            BarcodeWriter writer = new BarcodeWriter();
            writer.Renderer = new BitmapRenderer { Background = Color.White, Foreground = Color.Black };
            Bitmap img = writer.Write(bm);

            if (logo != null)
            {
                //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
                int[] rectangle = bm.getEnclosingRectangle();

                //计算插入图片的大小和位置
                int middleW = Math.Min((int)(rectangle[2] / 3.5), logo.Width);
                int middleH = Math.Min((int)(rectangle[3] / 3.5), logo.Height);
                int middleL = (img.Width - middleW) / 2 ;
                int middleT = (img.Height - middleH) / 2 ;

                //将img转换成bmp格式，否则后面无法创建Graphics对象
                Bitmap bmpimg = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmpimg))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    
                    g.DrawImage(img, 0, 0);
                }
                //将二维码插入图片
                Graphics myGraphic = Graphics.FromImage(bmpimg);
                //白底
                myGraphic.FillRectangle(Brushes.White, middleL - 3, middleT - 3, middleW+6, middleH+6);
                myGraphic.DrawImage(logo, middleL, middleT, middleW, middleH);

                return bmpimg;
            }

            return img;
        }
    }
}
