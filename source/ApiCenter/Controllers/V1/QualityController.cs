namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SixLabors.ImageSharp;
    using Util;

    public class QualityController : BaseController
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private DataLibrary.DataContext db;
        private IUserService userService;
        private ICertService certService;

        public QualityController(DataLibrary.DataContext dataContext, IUserService user, ICertService cert, IHostingEnvironment hostingEnvironment)
        {
            this.db = dataContext;
            this.userService = user;
            this.certService = cert;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 根据传递的质保书编号返回生成的质保书图片
        /// </summary>
        /// <param name="p">质保书编号</param>
        /// <param name="iswater">是否加水印</param>
        /// <returns>返回质保书图片</returns>
        [HttpGet]
        public IActionResult Post(string p, int iswater)
        {
            string path = string.Empty;
            path = this.hostingEnvironment.ContentRootPath + "/qualitypics/";
            var cert = this.db.SalePrintlog.Where(s => s.Printno == p).FirstOrDefault();
            if (cert != null && cert.Status != (int)DataLibrary.EnumList.SalePrintlogStatus.已下载)
            {
                CommonResult result = this.certService.GenerateCert(p, path, iswater > 0 ? true : false);
                if (result.Status == (int)CommonResultStatus.Success)
                {
                    var data = (JObject)result.Data;
                    if (data != null)
                    {
                        if (iswater == 1)
                        {
                            path = path + "/" + cert.Id + cert.Checkcode + ".jpg";
                        }
                        else
                        {
                            path = path + "/" + cert.Id + cert.Checkcode + "p.jpg";
                        }
                    }
                }
                else
                {
                    path = path + "/" + cert.Id + cert.Checkcode + "p.jpg";
                }
            }
            else
            {
                path = path + "/" + cert.Id + cert.Checkcode + ".jpg";
            }

            if (System.IO.File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);

                var resp = this.File(buffer, "image/jpeg");
                return resp;
            }

            return null;
        }

        ///// <summary>
        ///// 直接返回二进制图片
        ///// </summary>
        ///// <param name="value">value</param>
        ///// <returns>result</returns>
        //// POST: api/Quality
        // [HttpPost]
        // [AllowAnonymous]
        // public IActionResult Post([FromForm]string value)
        // {
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        return null;
        //    }

        // JObject jo = (JObject)JsonConvert.DeserializeObject(value.Base64ToString());
        //    if (jo == null)
        //    {
        //        return null;
        //    }

        // if (jo["printno"] == null)
        //    {
        //        return null;
        //    }

        // if (jo["html"] == null)
        //    {
        //        return null;
        //    }

        // if (jo["filedomain"] == null)
        //    {
        //        return null;
        //    }

        // if (jo["width"] == null)
        //    {
        //        return null;
        //    }

        // if (jo["height"] == null)
        //    {
        //        return null;
        //    }

        // if (jo["rotate"] == null)
        //    {
        //        return null;
        //    }

        // if (jo["qrcodeurl"] == null)
        //    {
        //        return null;
        //    }

        // var cert = this.db.SalePrintlog.Where(s => s.Printno == jo["printno"].ToString()).FirstOrDefault();
        //    if (cert == null)
        //    {
        //        return null;
        //    }

        // HttpClient client = new HttpClient();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        //    client.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");

        // var values = new List<KeyValuePair<string, string>>();

        // string html = jo["html"].ToString();
        //    string qrcodeurl = jo["qrcodeurl"].ToString();

        // // 如果是预览图，则将序号后4位用****代替，并且扫码参数后多加一个参数
        //    if (jo["iswater"].ToString() != "1")
        //    {
        //        var pno = jo["printno"].ToString();
        //        string cryptpno = Regex.Replace(pno, @"(\d{4})(\d{4})", "$1****");
        //        html = html.Replace(pno, cryptpno);

        // qrcodeurl = qrcodeurl + "&t=1";
        //    }

        // values.Add(new KeyValuePair<string, string>("html", html));
        //    values.Add(new KeyValuePair<string, string>("filedomain", jo["filedomain"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("scale", jo["scale"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("rotate", jo["rotate"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("width", jo["width"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("height", jo["height"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("qrcodeurl", qrcodeurl));
        //    values.Add(new KeyValuePair<string, string>("iswater", jo["iswater"].ToString()));

        // var content = new FormUrlEncodedContent(values);
        //    content.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
        //    content.Headers.Add("Timeout", "10000");
        //    content.Headers.Add("KeepAlive", "true");

        // var response = client.PostAsync(jo["filedomain"].ToString() + "/qrcode/getqualityimg.ashx", content);
        //    response.Wait();
        //    var responsedata = response.Result.Content.ReadAsStreamAsync();
        //    responsedata.Wait();

        // string path = string.Empty;
        //    using (var img = Image.Load(responsedata.Result))
        //    {
        //        var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
        //        encoder.Quality = 90;

        // if (jo["iswater"].ToString() == "1")
        //        {
        //            path = this.hostingEnvironment.ContentRootPath + "/qualitypics/" + cert.Id + cert.Checkcode + ".jpg";
        //            img.Save(path, encoder);

        // cert.Status = 1;
        //        }
        //        else
        //        {
        //            path = this.hostingEnvironment.ContentRootPath + "/qualitypics/" + cert.Id + cert.Checkcode + "p.jpg";
        //            img.Save(path, encoder);

        // cert.Status = 0;
        //        }

        // this.db.SalePrintlog.Update(cert);
        //        this.db.SaveChanges();
        //    }

        // FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        //    byte[] buffer = new byte[fs.Length];
        //    fs.Read(buffer, 0, (int)fs.Length);

        // var resp = this.File(buffer, "image/jpeg");
        //    return resp;
        // }

        ///// <summary>
        ///// 返回路径__预留，不要删除！！！
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //// POST: api/Quality
        // [HttpPost]
        // [AllowAnonymous]
        // public ResponseModel Post([FromForm]string value)
        // {
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", string.Empty);
        //    }

        // JObject jo = (JObject)JsonConvert.DeserializeObject(value.Base64ToString());
        //    if (jo == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", string.Empty);
        //    }
        //    if (jo["printno"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "质保书号不能为空！");
        //    }
        //    if (jo["html"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "模板不能为空！");
        //    }
        //    if (jo["filedomain"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "二维码生成域名不能为空！");
        //    }
        //    if (jo["width"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "生成尺寸宽度不能为空！");
        //    }
        //    if (jo["height"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "生成尺寸高度不能为空！");
        //    }
        //    if (jo["rotate"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "盖章角度不能为空！");
        //    }
        //    if (jo["qrcodeurl"] == null)
        //    {
        //        return new ResponseModel(ApiResponseStatus.Failed, "缺少参数！", "二维码地址不能为空！");
        //    }

        // HttpClient client = new HttpClient();

        // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        //    client.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");

        // var values = new List<KeyValuePair<string, string>>();
        //    values.Add(new KeyValuePair<string, string>("html", jo["html"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("filedomain", jo["filedomain"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("scale", jo["scale"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("rotate", jo["rotate"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("width", jo["width"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("height", jo["height"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("qrcodeurl", jo["qrcodeurl"].ToString()));
        //    values.Add(new KeyValuePair<string, string>("iswater", jo["iswater"].ToString()));

        // var content = new FormUrlEncodedContent(values);
        //    content.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
        //    content.Headers.Add("Timeout", "10000");
        //    content.Headers.Add("KeepAlive", "true");

        // var response = client.PostAsync(jo["filedomain"].ToString() + "/qrcode/getqualityimg.ashx", content);
        //    response.Wait();
        //    var responsedata = response.Result.Content.ReadAsStreamAsync();
        //    responsedata.Wait();

        // string path = "";
        //    using (var img = Image.Load(responsedata.Result))
        //    {
        //        var encoder = new ImageSharp.Formats.JpegEncoder();
        //        encoder.Quality = 90;

        // if (jo["iswater"].ToString() == "1")
        //        {
        //            path ="qualitypics\\" + jo["printno"].ToString() + ".jpg";
        //        }
        //        else
        //        {
        //            path ="qualitypics\\" + jo["printno"].ToString() + "p.jpg";
        //        }

        // img.Save(_hostingEnvironment.ContentRootPath + "\\" + path, encoder);
        //    }

        // return new ResponseModel(ApiResponseStatus.Success, string.Empty, path);
        // }
    }
}
