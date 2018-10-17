namespace WarrantyManage.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using static DataLibrary.EnumList;

    [Authorize]
    public class ProductMaterialController : Controller
    {
        private DataLibrary.DataContext db;
        private Common.IService.ISettingService settingService;

        public ProductMaterialController(DataLibrary.DataContext db, Common.IService.ISettingService settingService)
        {
            this.db = db;
            this.settingService = settingService;
        }

        [HttpPost]
        public string Create(string name, int classid, string note, int standardstrength, string templatename)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    var checkClass = this.db.BaseProductMaterial.FirstOrDefault(c => c.Name == name && c.Classid == classid);
                    if (checkClass != null)
                    {
                        return "-1";
                    }
                    else
                    {
                        var wsclass = new DataLibrary.BaseProductMaterial()
                        {
                            Name = name,
                            Classid = classid,
                            Templatename = templatename,
                            Note = note,
                            Standardstrength = standardstrength
                        };

                        this.db.BaseProductMaterial.Add(wsclass);
                        if (this.db.SaveChanges() > 0)
                        {
                            tran.Commit();
                            var apiurl = string.Empty;
#if DEBUG
                            apiurl = "http://localhost:41178/";
#else
            apiurl=this.settingService.MngSetting.Domain_WebApi;
#endif
                            var responData = Util.Helpers.HttpHelper.HttpPost(
                                $"{apiurl}api/v1/WarrantyTemplate/?materialid=" + wsclass.Id, null, System.Text.Encoding.UTF8);
                            var webApi_ResponseModel = JsonConvert.DeserializeObject<WebApiResponseModel>(responData);
                            if (webApi_ResponseModel.Status == ApiResponseStatus.Failed)
                            {
                                return "0";
                            }
                        }

                        return wsclass.Id.ToString();
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    var msg = ex.Message;
                    return "0";
                }
            }
        }

        [HttpGet]
        public ObjectResult Edit(int id)
        {
            var rs = this.db.BaseProductMaterial.FirstOrDefault(p => p.Id == id);
            if (rs != null)
            {
                return new ObjectResult(rs);
            }

            return null;
        }

        [HttpPost]
        public int Edit(int hiddId, string name, int classid, string note, int standardstrength, string templatename)
        {
            using (var tran = this.db.Database.BeginTransaction())
            {
                try
                {
                    var rs = this.db.BaseProductMaterial.FirstOrDefault(p => p.Id == hiddId);

                    if (rs == null)
                    {
                        return -1;
                    }

                    if (this.db.BaseProductMaterial.FirstOrDefault(p => p.Id != hiddId && p.Classid == classid && p.Name == name) != null)
                    {
                        return -1;
                    }

                    rs.Name = name;
                    rs.Classid = classid;
                    rs.Templatename = templatename;
                    rs.Note = note;
                    rs.Standardstrength = standardstrength;
                    this.db.BaseProductMaterial.Update(rs);
                    if (this.db.SaveChanges() > 0)
                    {
                        tran.Commit();
                        var apiurl = string.Empty;
#if DEBUG
                        apiurl = "http://localhost:41178/";
#else
            apiurl=this.settingService.MngSetting.Domain_WebApi;
#endif
                        var responData = Util.Helpers.HttpHelper.HttpPost(
                            $"{apiurl}api/v1/WarrantyTemplate/?materialid=" + hiddId, null, System.Text.Encoding.UTF8);
                        var webApi_ResponseModel = JsonConvert.DeserializeObject<WebApiResponseModel>(responData);
                        if (webApi_ResponseModel.Status == ApiResponseStatus.Failed)
                        {
                            return 0;
                        }
                    }

                    return 1;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    return 0;
                }
            }
        }

        [HttpPost]
        public int Delete(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return 0;
            }

            ids = ids.TrimEnd(',');
            string[] idarr = ids.Split(',');
            var tempFlag = true;
            if (idarr.Length > 0)
            {
                List<int> idlist = new List<int>();
                for (int i = 0; i < idarr.Length; i++)
                {
                    int.TryParse(idarr[i], out int id);
                    var temp = this.db.BaseSpecifications.FirstOrDefault(c => c.Materialid == id);
                    var temp2 = this.db.PdProduct.FirstOrDefault(c => c.Materialid == id);
                    if (temp != null || temp2 != null)
                    {
                        tempFlag = false;
                        break;
                    }
                    else
                    {
                        if (id > 0)
                        {
                            var bq = this.db.BaseQualityStandard.FirstOrDefault(p => p.Materialid == id);
                            if (bq != null)
                            {
                                this.db.BaseQualityStandard.Remove(bq);
                            }

                            idlist.Add(id);
                        }
                    }
                }

                if (tempFlag)
                {
                    this.db.SaveChanges();

                    var list = this.db.BaseProductMaterial.Where(p => idlist.Contains(p.Id)).ToList();
                    var sql = this.db.BaseProductMaterial.AsQueryable();

                    this.db.BaseProductMaterial.RemoveRange(list);

                    return this.db.SaveChanges();
                }
            }

            return 0;
        }
    }
}