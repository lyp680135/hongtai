namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.IService;
    using Common.Service;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class SellerOpenedAuthController : BaseController
    {
        private DataLibrary.DataContext db;
        private IUserService userService;

        public SellerOpenedAuthController(DataLibrary.DataContext dataContext, IUserService user)
        {
            this.db = dataContext;
            this.userService = user;
        }

        // GET: api/SellerOpened
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
                        join c1 in this.db.BaseProductClass on c.Classid equals c1.Id
                        join c2 in this.db.BaseProductMaterial on c.Materialid equals c2.Id
                        join c3 in this.db.BaseSpecifications on c.Specid equals c3.Id
                        join c4 in this.db.SaleSeller on c.Sellerid equals c4.Id
                        select new
                        {
                            c.Classid,
                            c1.Gbname,
                            c1.Name,
                            c.Materialid,
                            MaterialName = c2.Name,
                            c.Specid,
                            c3.Specname,
                            c3.Referpieceweight,
                            c.Id,
                            c.Sellerid,
                            c.Batcode,
                            SellerName = c4.Name,
                            c4.Mobile,
                            Sid = c4.Id,
                            c.Parentseller,
                            c.Number,
                            c.Createtime
                        };

            // 当前经销商 已开列表
            query = query.Where(x => x.Parentseller == user.Id);

            var data = query.Skip(pageSize.Value * (pageIndex.Value - 1)).Take(pageSize.Value).OrderByDescending(o => o.Id).ToList();

            if (!string.IsNullOrWhiteSpace(condition))
            {
                data = data.Where(w => w.Batcode.ToUpper().Contains(condition.ToUpper()) || w.MaterialName.ToUpper().Contains(condition.ToUpper())||w.Mobile.ToUpper().Contains(condition.ToUpper())).ToList();
            }

            if (data.Count() > 0)
            {
                List<object> list = new List<object>();
                foreach (var item in data.Select(s => s.Sid).Distinct())
                {
                    var obj = data.Where(w => w.Sid == item).ToList();
                    JArray jarr = new JArray();
                    foreach (var objitem in obj)
                    {
                        var o = new
                        {
                            objitem.Classid,
                            objitem.Gbname,
                            objitem.Name,
                            objitem.Materialid,
                            objitem.MaterialName,
                            objitem.Specid,
                            objitem.Specname,
                            objitem.Referpieceweight,
                            objitem.Id,
                            objitem.Batcode,
                            objitem.Sellerid,
                            objitem.SellerName,
                            objitem.Parentseller,
                            objitem.Number,
                            Createtime = objitem.Createtime.HasValue ? Util.Extensions.GetDateTimeFromUnixTime((long)objitem.Createtime).ToString("yyyy年MM月dd日") : string.Empty
                        };
                        jarr.Add(JToken.FromObject(o));
                    }

                    list.Add(new
                    {
                        sellerid = obj.FirstOrDefault().Sellerid,
                        sellername = obj.FirstOrDefault().SellerName,
                        mobile = obj.FirstOrDefault().Mobile,
                        list = jarr
                    });

                }

                return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(list));
            }
            else
            {
                return new ResponseModel(ApiResponseStatus.Failed, "记录为空", string.Empty);
            }
        }

        // POST: api/SellerOpened
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/SellerOpened/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
