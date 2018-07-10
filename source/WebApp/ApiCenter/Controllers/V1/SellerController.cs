namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataLibrary;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 经销商
    /// </summary>
    [Produces("application/json")]
    public class SellerController : BaseController
    {
        private DataLibrary.DataContext db;

        public SellerController(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        [HttpGet]
        public ResponseModel Get()
        {
            var user = this.HttpContext.User;
            var sellerlist = this.db.SaleSeller.Where(p => p.Parent == null || p.Parent == 0).OrderBy(c => c.Name).ToList();

            if (sellerlist != null)
            {
                var list = new List<JObject>();
                var sellerbyword = new JArray();
                var prevword = string.Empty;

                var index = 0;
                foreach (var seller in sellerlist)
                {
                    string firstword = string.Empty;
                    var pinyin = Util.Helpers.String.PinYin(seller.Name);
                    if (string.IsNullOrEmpty(pinyin))
                    {
                        firstword = "#";
                    }
                    else
                    {
                        firstword = pinyin.Substring(0, 1).ToUpper();
                    }

                    var jobj = new JObject()
                   {
                        { "id", seller.Id },
                        { "name", seller.Name },
                        { "mobile", seller.Mobile },
                        { "createtime", seller.Createtime },
                    };

                    if (prevword == firstword)
                    {
                        sellerbyword.Add(jobj);
                    }
                    else
                    {
                        if (index == 0)
                        {
                        }
                        else
                        {
                            list.Add(new JObject()
                            {
                                { "name", prevword },
                                { "list", sellerbyword }
                            });

                            sellerbyword = new JArray();
                        }

                        sellerbyword.Add(jobj);
                    }

                    prevword = firstword;

                    index++;
                }

                if (sellerbyword.Count > 0)
                {
                    list.Add(new JObject()
                            {
                                { "name", prevword },
                                { "list", sellerbyword }
                            });
                }

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(list));
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "售达方记录为空", string.Empty);
            }
        }

        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            return this.Json("value");
        }

        [HttpPost]
        public ResponseModel Post([FromForm]string value)
        {
            var user = this.HttpContext.User;
            var requestmodel = new { id = string.Empty, name = string.Empty, mobiles = string.Empty };
            var data = JsonConvert.DeserializeAnonymousType(value.Base64ToString(), requestmodel);

            // 判断重复
            var dupsaler = this.db.SaleSeller.Where(p => p.Name == data.name).FirstOrDefault();
            if (dupsaler != null)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "售达方添加失败：名称有重名", string.Empty);
            }

            var seller = new SaleSeller();
            var mobiles = data.mobiles.Replace(' ', ',');
            mobiles = mobiles.Replace('，', ',');
            mobiles = mobiles.Replace("\r\n", ",");
            mobiles = mobiles.Replace('\n', ',');

            seller.Name = data.name;

            var mobiles_arr = mobiles.Split(',');
            var mobiles_for_update = new List<string>();

            // 排除输入的重复手机号
            foreach (var mobile in mobiles_arr)
            {
                var find = false;
                foreach (var m in mobiles_for_update)
                {
                    if (m == mobile)
                    {
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    mobiles_for_update.Add(mobile);
                }
            }

            // 排除数据库中重复的手机号
            var dupmobiles = new List<string>();
            var realmobiles = new List<string>();
            foreach (var mobile in mobiles_for_update)
            {
                var m = this.db.SaleSeller.Where(p => p.Mobile.Contains(mobile)).FirstOrDefault();
                if (m == null)
                {
                    realmobiles.Add(mobile);
                }
                else
                {
                    dupmobiles.Add(mobile);
                }
            }

            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < realmobiles.Count; i++)
            {
                sb.Append(realmobiles[i]);

                if (i < realmobiles.Count - 1)
                {
                    sb.Append(",");
                }
            }

            seller.Mobile = sb.ToString();

            if (seller.Mobile == string.Empty)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "该手机号已被使用", string.Empty);
            }

            this.db.SaleSeller.Add(seller);

            int id = this.db.SaveChanges();

            if (id > 0)
            {
                seller.Id = id;
                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(seller));
            }

            return new ResponseModel(ApiResponseStatus.Failed, "参数错误", string.Empty);
        }

        [HttpPut("{id}")]
        public ResponseModel Put(int id, [FromForm]string value)
        {
            var user = this.HttpContext.User;
            var requestmodel = new { id = string.Empty, name = string.Empty, mobiles = string.Empty };
            var data = JsonConvert.DeserializeAnonymousType(value.Base64ToString(), requestmodel);

            if (id > 0)
            {
                // 排除重复的售达方
                var dupsaler = this.db.SaleSeller.Where(p => p.Name == data.name && p.Id != id).FirstOrDefault();
                if (dupsaler != null)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "售达方更新失败：名称有重名", string.Empty);
                }

                var seller = this.db.SaleSeller.Where(p => p.Id == id).FirstOrDefault();

                if (seller != null)
                {
                    var mobiles = data.mobiles.Replace(' ', ',');
                    mobiles = mobiles.Replace('，', ',');
                    mobiles = mobiles.Replace("\r\n", ",");
                    mobiles = mobiles.Replace('\n', ',');

                    seller.Name = data.name;
                    var mobiles_arr = mobiles.Split(',');
                    var mobiles_for_update = new List<string>();

                    // 排除输入的重复手机号
                    foreach (var mobile in mobiles_arr)
                    {
                        var find = false;
                        foreach (var m in mobiles_for_update)
                        {
                            if (m == mobile)
                            {
                                find = true;
                                break;
                            }
                        }

                        if (!find)
                        {
                            mobiles_for_update.Add(mobile);
                        }
                    }

                    // 排除数据库中重复的手机号
                    var dupmobiles = new List<string>();
                    var realmobiles = new List<string>();
                    foreach (var mobile in mobiles_for_update)
                    {
                        var m = this.db.SaleSeller.Where(p => p.Mobile.Contains(mobile) && p.Id != id).FirstOrDefault();
                        if (m == null)
                        {
                            realmobiles.Add(mobile);
                        }
                        else
                        {
                            dupmobiles.Add(mobile);
                        }
                    }

                    StringBuilder sb = new StringBuilder();
                    for (var i = 0; i < realmobiles.Count; i++)
                    {
                        sb.Append(realmobiles[i]);

                        if (i < realmobiles.Count - 1)
                        {
                            sb.Append(",");
                        }
                    }

                    seller.Mobile = sb.ToString();
                    if (seller.Mobile == string.Empty)
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "该手机号已被使用", string.Empty);
                    }

                    this.db.SaleSeller.Update(seller);

                    if (this.db.SaveChanges() > 0)
                    {
                        string result = "更新成功";
                        if (dupmobiles.Count > 0)
                        {
                            result += "，有部分重复手机号被过滤";
                        }

                        return new ResponseModel(ApiResponseStatus.Success, string.Empty, result);
                    }
                    else
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "更新失败", string.Empty);
                    }
                }
                else
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "售达方记录不存在", string.Empty);
                }
            }

            return new ResponseModel(ApiResponseStatus.Failed, "参数错误", string.Empty);
        }

        [HttpDelete("{id}")]
        public ResponseModel Delete(int id)
        {
            if (id > 0)
            {
                // 判断该售达方下面有没有出库记录
                var stockout = this.db.PdStockOut.Where(s => s.Sellerid == id).Count();
                if (stockout > 0)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "该售达方下面有授权资源，不允许删除", string.Empty);
                }

                var seller = this.db.SaleSeller.Where(s => s.Id == id).FirstOrDefault();
                if (seller != null)
                {
                    this.db.SaleSeller.Remove(seller);

                    if (this.db.SaveChanges() > 0)
                    {
                        return new ResponseModel(ApiResponseStatus.Success, string.Empty, "1");
                    }
                    else
                    {
                        return new ResponseModel(ApiResponseStatus.Failed, "删除失败没有影响行", string.Empty);
                    }
                }
            }

            return new ResponseModel(ApiResponseStatus.Failed, "参数错误", string.Empty);
        }
    }
}