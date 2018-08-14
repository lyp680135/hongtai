namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class WarrantyTemplateController : BaseController
    {
        private DataLibrary.DataContext db;

        public WarrantyTemplateController(DataLibrary.DataContext db)
        {
            this.db = db;
        }

        [HttpGet]
        /// <summary>
        /// 获取所有模板文件
        /// </summary>
        /// <returns>返回文件</returns>
        public ResponseModel Get()
        {
            List<string> lt = new List<string>();
            string path = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"qualitypics\template\");
            DirectoryInfo folder = new DirectoryInfo(path);
            foreach (FileInfo file in folder.GetFiles())
            {
                if (file.Exists)
                {
                    if (!lt.Any(o => o == file.Name))
                    {
                        if (!file.Name.StartsWith("template"))
                        {
                            lt.Add(file.Name);
                        }
                    }
                }
            }

            return new ResponseModel(ApiResponseStatus.Success, "success", JsonConvert.SerializeObject(lt));
        }

        /// <summary>
        /// 根据选择的模板id动态生成templateid
        /// </summary>
        /// <param name="materialid">传递id</param>
        /// <returns>空</returns>
        [HttpPost]
        public ResponseModel Post(int materialid)
        {
            var lin = this.db.BaseProductMaterial.FirstOrDefault(c => c.Id == materialid);

            if (lin != null && !string.IsNullOrWhiteSpace(lin.Templatename))
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + string.Format(@"qualitypics\template\");
                string templatePath = path + lin.Templatename;
                if (System.IO.File.Exists(templatePath))
                {
                    System.IO.File.WriteAllText(path + $"template{materialid}.html", System.IO.File.ReadAllText(templatePath));
                    return new ResponseModel(ApiResponseStatus.Success, "创建成功！", string.Empty);
                }
                else
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "操作失败", "创建失败！");
                }
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "操作失败", "数据不存在！");
            }
        }
    }
}