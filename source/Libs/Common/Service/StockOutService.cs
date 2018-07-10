namespace Common.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Common.IService;
    using DataLibrary;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static DataLibrary.EnumList;

    public class StockOutService : IStockOutService
    {
        private DataLibrary.DataContext db;

        public StockOutService(DataLibrary.DataContext dataContext)
        {
            this.db = dataContext;
        }

        public CommonResult Stockout(string batcode, string lpn, int sellerid, int lengthtype, int number, int userid)
        {
            var list = new List<DataLibrary.PdProduct>();

            // 定尺出库
            if (lengthtype == (int)DataLibrary.EnumList.ProductQualityLevel.定尺)
            {
                list = (from p in
                            from pp in this.db.PdProduct
                            where pp.Batcode == batcode && pp.Lengthtype == lengthtype
                            select new
                            {
                                productlist = pp,
                                isstock = (from pd in this.db.PdStockOut where pd.Productid == pp.Id select pd).Count()
                            }
                        where p.isstock <= 0
                        select p.productlist).Take(number).ToList();
            }
            else if (lengthtype == (int)DataLibrary.EnumList.ProductQualityLevel.标准)
            {
                // 盘螺出库
                list = (from p in
                            from pp in this.db.PdProduct
                            where pp.Batcode == batcode && pp.Lengthtype == lengthtype
                            select new
                            {
                                productlist = pp,
                                isstock = (from pd in this.db.PdStockOut where pd.Productid == pp.Id select pd).Count()
                            }
                        where p.isstock <= 0
                        select p.productlist).Take(number).ToList();
            }
            else
            {
                // 非尺出库
                var intList = new List<int>() { -1, 1 };
                list = (from p in
                            from pp in this.db.PdProduct
                            where pp.Batcode == batcode && intList.Contains(Convert.ToInt32(pp.Lengthtype))
                            select new
                            {
                                productlist = pp,
                                isstock = (from pd in this.db.PdStockOut where pd.Productid == pp.Id select pd).Count()
                            }
                            where p.isstock <= 0
                            select p.productlist).Take(number).ToList();
            }

            List<SaleSellerAuth> authlist = new List<SaleSellerAuth>();

            int result = 0;
            if (list.Count > 0)
            {
                using (var tran = this.db.Database.BeginTransaction())
                {
                    foreach (var product in list)
                    {
                        var stockout = new DataLibrary.PdStockOut();
                        stockout.Lpn = lpn;
                        stockout.Sellerid = sellerid;
                        stockout.Productid = product.Id;
                        stockout.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                        stockout.Adder = userid;

                        var isout = this.db.PdStockOut.Where(s => s.Productid == product.Id).FirstOrDefault();

                        if (isout == null)
                        {
                            this.db.PdStockOut.Add(stockout);

                            if (this.db.SaveChanges() > 0)
                            {
                                result++;

                                // 0.定尺1.非尺
                                var lenType = (lengthtype == (int)ProductQualityLevel.定尺) ? 0 : 1;
                                if (lengthtype == (int)DataLibrary.EnumList.ProductQualityLevel.标准)
                                {
                                    lenType = (int)DataLibrary.EnumList.ProductQualityLevel.标准;
                                }

                                var auth = this.db.SaleSellerAuth.Where(s => s.Batcode == product.Batcode
                                        && s.Classid == product.Classid
                                        && s.Materialid == product.Materialid
                                        && s.Lpn == lpn
                                        && s.Sellerid == sellerid
                                        && s.Lengthtype == lenType).FirstOrDefault();
                                if (auth == null)
                                {
                                    auth = new DataLibrary.SaleSellerAuth();
                                    auth.Classid = product.Classid;
                                    auth.Materialid = product.Materialid;
                                    auth.Specid = product.Specid;
                                    auth.Lpn = lpn;
                                    auth.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                                    auth.Number = 1;
                                    auth.Sellerid = sellerid;
                                    auth.Batcode = product.Batcode;
                                    auth.Lengthtype = lenType;

                                    this.db.SaleSellerAuth.Add(auth);
                                }
                                else
                                {
                                    auth.Number = auth.Number + 1;

                                    this.db.SaleSellerAuth.Update(auth);
                                }

                                if (this.db.SaveChanges() > 0)
                                {
                                    bool finddup = false;
                                    for (var j = 0; j < authlist.Count; j++)
                                    {
                                        if (authlist[j].Id == auth.Id)
                                        {
                                            finddup = true;

                                            // 找到则更新
                                            authlist[j].Number++;
                                            break;
                                        }
                                    }

                                    // 没找到就追加
                                    if (!finddup)
                                    {
                                        authlist.Add(new SaleSellerAuth()
                                        {
                                            Id = auth.Id,
                                            Number = 1,
                                        });
                                    }

                                    // 添加授权详情表
                                    var authdetail = this.db.SaleSellerAuthDetail.Where(s => s.AuthId == auth.Id
                                                        && s.Productid == product.Id).FirstOrDefault();
                                    if (authdetail == null)
                                    {
                                        var detail = new SaleSellerAuthDetail();
                                        detail.AuthId = auth.Id;
                                        detail.Batcode = product.Batcode;
                                        detail.Classid = product.Classid;
                                        detail.Materialid = product.Materialid;
                                        detail.Specid = product.Specid;
                                        detail.Productid = product.Id;

                                        this.db.SaleSellerAuthDetail.Add(detail);

                                        if (this.db.SaveChanges() <= 0)
                                        {
                                            tran.Rollback();

                                            return new CommonResult(CommonResultStatus.Failed, "出库失败", "授权操作失败");
                                        }
                                    }
                                }
                                else
                                {
                                    tran.Rollback();

                                    return new CommonResult(CommonResultStatus.Failed, "出库失败", "出库操作失败");
                                }
                            }
                            else
                            {
                                tran.Rollback();

                                return new CommonResult(CommonResultStatus.Failed, "出库失败", "更新授权详情表失败");
                            }
                        }
                    }

                    tran.Commit();
                }
            }

            // var retobj = new JObject();
            // retobj["result"] = result;
            // retobj["authlist"] = JsonConvert.SerializeObject(authlist);
            return new CommonResult(CommonResultStatus.Success, "出库成功", null, authlist);
        }

        int IStockOutService.Stockout(string batcode, string lpn, int sellerid, int startbundle, int endbundle, int lengthtype, int userid)
        {
            throw new NotImplementedException();
        }
    }
}
