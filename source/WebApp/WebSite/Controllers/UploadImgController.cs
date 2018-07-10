namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    public class UploadImgController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// 图片存放根目录
        /// </summary>
        private readonly string rootPath = "/UploadFile/Image/";

        public UploadImgController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 单图上传
        /// </summary>
        /// <returns>图片路径</returns>
        [HttpPost]
        public string Upload()
        {
            JObject jo = new JObject();
            jo["result"] = false;
            jo["msg"] = "上传图片失败";

            if (this.Request.Form.Files.Count > 0)
            {
                var file = this.Request.Form.Files[0];

                // 文件后缀
                var fileExtension = Path.GetExtension(file.FileName);

                // 判断后缀是否是图片
                const string fileFilt = ".gif|.jpg|.jpeg|.png|.bmp";
                if (fileExtension == null)
                {
                    jo["result"] = false;
                    jo["msg"] = "上传的文件没有后缀";
                    return jo.ToString();
                }

                if (fileFilt.IndexOf(fileExtension.ToLower(), StringComparison.Ordinal) <= -1)
                {
                    jo["result"] = false;
                    jo["msg"] = "上传的文件不是图片";
                    return jo.ToString();
                }

                if (file.Length > 41943040)
                {
                    jo["result"] = false;
                    jo["msg"] = "文件大小不能超过40M";
                    return jo.ToString();
                }

                string file_RootPath = this.hostingEnvironment.ContentRootPath + this.rootPath + DateTime.Now.ToString("yyyyMMdd") + "/";

                if (!Directory.Exists(file_RootPath))
                {
                    Directory.CreateDirectory(file_RootPath);
                }

                string newfilename = Guid.NewGuid() + "_" + file.FileName;
                string returnPath = this.rootPath + DateTime.Now.ToString("yyyyMMdd") + "/" + newfilename;
                string filepath_last = file_RootPath + newfilename;

                using (FileStream fs = System.IO.File.Create(filepath_last))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }

                jo["result"] = true;
                jo["msg"] = returnPath;
            }

            return jo.ToString();
        }
    }
}