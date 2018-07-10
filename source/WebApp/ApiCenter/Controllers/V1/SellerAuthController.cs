namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using DataLibrary;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class SellerAuthController : BaseController
    {
        private DataLibrary.DataContext db;
        private IUserService userService;

        public SellerAuthController(DataLibrary.DataContext dataContext, IUserService user)
        {
            this.db = dataContext;
            this.userService = user;
        }

        // GET: api/test
        [HttpGet]
        public ResponseModel Get(int? pageSize, int? pageIndex, string condition)
        {
            if (!pageSize.HasValue)
            {
                pageSize = 1;
            }

            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var user = this.userService.SaleSellerUser;

            var query = from c in this.db.SaleSellerAuth
                        join c1 in this.db.BaseProductClass on c.Classid equals c1.Id into temp1
                        from t1 in temp1.DefaultIfEmpty()
                        join c2 in this.db.BaseProductMaterial.DefaultIfEmpty() on c.Materialid equals c2.Id into temp2
                        from t2 in temp2.DefaultIfEmpty()
                        join c3 in this.db.BaseSpecifications.DefaultIfEmpty() on c.Specid equals c3.Id into temp3
                        from t3 in temp3.DefaultIfEmpty()
                        join c4 in this.db.SaleSeller.DefaultIfEmpty() on c.Sellerid equals c4.Id into temp4
                        from t4 in temp4.DefaultIfEmpty()
                        select new
                        {
                            c.Classid,
                            t1.Gbname,
                            t1.Name,
                            c.Materialid,
                            MaterialName = t2.Name,
                            c.Specid,
                            c.Lengthtype,
                            t3.Specname,
                            t3.Referpieceweight,
                            c.Id,
                            c.Sellerid,
                            SellerName = t4.Name,
                            c.Parentseller,
                            c.Lpn,
                            c.Batcode,
                            c.Number,
                            SelectedNumber = 0,
                            AuthNumber = this.db.SaleSellerAuth.Where(x => x.Parentseller == user.Id
                                && x.Classid == c.Classid && x.Materialid == c.Materialid && x.Specid == c.Specid
                                && x.Batcode == c.Batcode)
                                            .GroupBy(x => new { x.Classid, x.Materialid, x.Specid, x.Parentseller, x.Batcode })
                                            .Select(g => new
                                            {
                                                AuthNumber = g.Sum(x => x.Number)
                                            }).Sum(x => x.AuthNumber),
                            RealCreatetime = c.Createtime,
                            Createtime = c.Createtime.HasValue ? Util.Extensions.GetDateTimeFromUnixTime((long)c.Createtime).ToString("yyyy年MM月dd日") : string.Empty,
                        };

            // 当前经销商 可开列表
            query = query.Where(x => x.Sellerid == user.Id).OrderByDescending(x => x.RealCreatetime);

            var data = query.Skip(pageSize.Value * (pageIndex.Value - 1)).Take(pageSize.Value).OrderByDescending(o => o.Id).ToList();
            if (!string.IsNullOrWhiteSpace(condition))
            {
                data = data.Where(w => w.Batcode.ToUpper().Contains(condition.ToUpper()) || w.MaterialName.ToUpper().Contains(condition.ToUpper()) || w.Lpn.ToUpper().Contains(condition.ToUpper())).ToList();
            }
            if (data.Count() > 0)
            {
                List<object> list = new List<object>();
                JArray jarr = new JArray();
                string firstlpn = string.Empty;
                string firsttime = string.Empty;
                int index = 1;
                foreach (var item in data)
                {
                    var obj = new
                    {
                        item.Classid,
                        item.Gbname,
                        item.Name,
                        item.Materialid,
                        item.MaterialName,
                        item.Specid,
                        item.Specname,
                        item.Referpieceweight,
                        item.Id,
                        item.Sellerid,
                        item.SellerName,
                        item.Parentseller,
                        item.Lpn,
                        item.Batcode,
                        item.Number,
                        item.SelectedNumber,
                        item.AuthNumber,
                        item.Createtime,
                    };

                    // 如果从第二行开始，与上一条车牌号不同时，即新当成一个分组
                    if ((firstlpn != item.Lpn || firsttime != item.Createtime) && index > 1)
                    {
                        list.Add(new
                        {
                            Lpn = firstlpn,
                            Date = firsttime,
                            list = jarr
                        });
                        jarr = new JArray();
                    }

                    // 组装按车牌号和时间分组下的物资列表
                    jarr.Add(JToken.FromObject(obj));
                    firstlpn = item.Lpn;
                    firsttime = item.Createtime;

                    // 当到达最后一行数据时，应把最后一行的数据直接加入到列表
                    if (data.Count() == index)
                    {
                        list.Add(new
                        {
                            Lpn = firstlpn,
                            Date = firsttime,
                            list = jarr
                        });
                    }

                    index++;
                }

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(list));
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "记录为空", "数据为空");
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // POST: api/test
        [HttpPost]
        public ResponseModel Post(string mobile, string requestData)
        {
            if (string.IsNullOrEmpty(mobile))
            {
                return new ResponseModel(ApiResponseStatus.Failed, "授权失败", "被授权人手机号不能为空");
            }

            if (string.IsNullOrEmpty(requestData))
            {
                return new ResponseModel(ApiResponseStatus.Failed, "授权失败", "被授权物资信息不能为空");
            }

            // 如果输入的当前手机号在经销商中不存在，则添加
            var saleSeller = this.db.SaleSeller.FirstOrDefault(x => x.Mobile.Contains(mobile));
            if (saleSeller == null)
            {
                saleSeller = new SaleSeller
                {
                    Mobile = mobile,
                    Createtime = (int)Util.Extensions.GetCurrentUnixTime(),
                    Name = string.Empty,
                    Parent = this.userService.SaleSellerUser != null ? this.userService.SaleSellerUser.Id : 0
                };

                this.db.SaleSeller.Add(saleSeller);
                this.db.SaveChanges();
                saleSeller = this.db.SaleSeller.FirstOrDefault(x => x.Id == saleSeller.Id);
            }

            if (saleSeller == null)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "授权失败", "被授权人不存在");
            }

            if (saleSeller.Id == this.userService.SaleSellerUser.Id)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "授权失败", "无法授权给本人");
            }

            try
            {
                var user = this.userService.SaleSellerUser;
                JArray arr = (JArray)JsonConvert.DeserializeObject(requestData);
                foreach (JToken jt in arr)
                {
                    int.TryParse(jt["Id"].ToString(), out int id);
                    int.TryParse(jt["Classid"].ToString(), out int classid);
                    int.TryParse(jt["Materialid"].ToString(), out int materialid);
                    int.TryParse(jt["Specid"].ToString(), out int specid);
                    int.TryParse(jt["SelectedNumber"].ToString(), out int number);
                    string batcode = jt["Batcode"].ToString();
                    string lpn = jt["Lpn"].ToString();

                    DataLibrary.SaleSellerAuth sellerAuth = this.db.SaleSellerAuth.FirstOrDefault(x => x.Sellerid == saleSeller.Id && x.Classid.Value == classid && x.Materialid.Value == materialid && x.Specid.Value == specid);
                    if (sellerAuth == null)
                    {
                        SaleSellerAuth entity = new SaleSellerAuth
                        {
                            Classid = classid,
                            Materialid = materialid,
                            Specid = specid,
                            Sellerid = saleSeller.Id,
                            Number = number,
                            Parentseller = user.Id,
                            Batcode = batcode,
                            Lpn = lpn,
                            Createtime = (int)Util.Extensions.GetCurrentUnixTime()
                        };
                        this.db.SaleSellerAuth.Add(entity);
                    }
                    else
                    {
                        sellerAuth.Number = sellerAuth.Number + number;
                        this.db.SaleSellerAuth.Update(sellerAuth);
                    }
                }

                if (this.db.SaveChanges() > 0)
                {
                    return new ResponseModel(ApiResponseStatus.Success, "授权成功", "success");
                }

                return new ResponseModel(ApiResponseStatus.Failed, "授权失败", "授权失败！");
            }
            catch (Exception ex)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "授权失败", ex.Message);
            }
        }

        // PUT: api/test/5
        [HttpPut]
        public ResponseModel Put(string requestData)
        {
            try
            {
                var user = this.userService.SaleSellerUser;
                JObject jo = (JObject)JsonConvert.DeserializeObject(requestData);
                if (jo == null)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "撤消授权失败", "缺少参数！");
                }

                if (jo["authids"] == null)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "撤消授权失败", "没有选择的撤消项！");
                }

                string[] authids = jo["authids"].ToString().Split(',');
                if (authids.Length == 0)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "撤消授权失败", "没有选择的撤消项！");
                }

                // 当前要被撤消的项ID
                int[] ids = Array.ConvertAll<string, int>(authids, s => int.Parse(s));

                // 授权的资源列表
                var query = from c in this.db.SaleSellerAuth where c.Parentseller == user.Id && ids.Contains(c.Id) select c;
                if (query.Count() == 0)
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "撤消授权失败", "没有找到需要撤消的项！");
                }

                // 递归撤消
                this.CancelAuth(query);
                this.db.SaleSellerAuth.RemoveRange(query);

                if (this.db.SaveChanges() > 0)
                {
                    return new ResponseModel(ApiResponseStatus.Success, "撤消授权成功", "success");
                }
                else
                {
                    return new ResponseModel(ApiResponseStatus.Failed, "撤消授权失败", "撤消授权失败！");
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel(ApiResponseStatus.Failed, "撤消授权失败", ex.Message);
            }
        }

        private void CancelAuth(IQueryable<SaleSellerAuth> query)
        {
            foreach (var item in query)
            {
                // 查找当前资源经销商ID
                var query_node = this.db.SaleSellerAuth.Where(x => x.Parentseller == item.Sellerid && x.Classid == item.Classid && x.Materialid == item.Materialid && x.Specid == item.Specid);
                if (query_node.Count() > 0)
                {
                    this.CancelAuth(query_node);
                    this.db.SaleSellerAuth.RemoveRange(query_node);
                }
            }
        }
    }
}
