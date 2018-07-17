// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WarrantyApiCenter.Controllers.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.IService;
    using DataLibrary;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Util;
    using WarrantyApiCenter.Models;
    using static DataLibrary.EnumList;

    [Produces("application/json")]
    public class WarrantyPrintController : BaseController
    {
        private DataLibrary.DataContext db;
        private IUserService userService;
        private Util.Helpers.MySqlHelper mySqlHelper;

        public WarrantyPrintController(DataContext dataContext, IUserService userService, Util.Helpers.MySqlHelper mySqlHelper)
        {
            this.db = dataContext;
            this.userService = userService;
            this.mySqlHelper = mySqlHelper;
        }

        [HttpGet]
        public ResponseModel Get(int? pageSize, int? pageIndex, string condition = "")
        {
            if (!pageSize.HasValue)
            {
                pageSize = 20;
            }

            if (!pageIndex.HasValue)
            {
                pageIndex = 1;
            }

            var sqlStr = string.Format(
                @"select a.printno,a.createtime,a.consignor,SUM(b.number) as number,a.id as id from saleprintlog  a
                            inner JOIN saleprintlogdetail b on a.id=b.printid
                            inner JOIN salesellerauth c on b.authid=c.Id 
                            WHERE c.Sellerid={0} and a.status={1} ", this.userService.SaleSellerUser.Id,
                            (int)SalePrintlogStatus.已下载);
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sqlStr += $" and (printno like '%{Util.Helpers.String.UrnHtml(condition)}%' or Consignor like '%{Util.Helpers.String.UrnHtml(condition)}%') ";
            }

            sqlStr += $" GROUP BY a.printno,b.number,a.Createtime,a.consignor, a.id order by a.id desc ";
            var dicList = this.mySqlHelper.ExecuteList<DBModel>(sqlStr, null, System.Data.CommandType.Text);

            List<ResModel> resList = new List<ResModel>();
            if (dicList.Count > 0)
            {

                resList = dicList.GroupBy(o => o.PrintNo).ToList().Select(s => new ResModel
                {
                    PrintNo = s.FirstOrDefault().PrintNo,
                    Createtime = s.FirstOrDefault().Createtime.ToLong().GetDateTimeFromUnixTime().ToString("yyyy/MM/dd"),
                    Consignor = s.FirstOrDefault().Consignor,
                    Number = s.Sum(m => m.Number)
                }).ToList();
            }
            return new ResponseModel(ApiResponseStatus.Success, string.Empty, JsonConvert.SerializeObject(resList));
        }

        public class ResModel
        {
            public string PrintNo { get; set; }

            public string Createtime { get; set; }

            public string Consignor { get; set; }

            public decimal Number { get; set; }
        }

        public class DBModel
        {
            public string PrintNo { get; set; }

            public int Createtime { get; set; }

            public string Consignor { get; set; }

            public decimal Number { get; set; }
        }
    }
}
