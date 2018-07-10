namespace WarrantyApiCenter.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using DataLibrary;

    public class StockoutModel
    {
        public int Stockout(DataLibrary.DataContext db, string batcode, string lpn, int sellerid, int startbundle, int endbundle, DataLibrary.EnumList.ProductQualityLevel deliveryType)
        {
            var list = new List<DataLibrary.PdProduct>();

            // 定尺出库
            if (deliveryType == DataLibrary.EnumList.ProductQualityLevel.定尺)
            {
                list = db.PdProduct.Where(p => p.Batcode == batcode && p.Lengthtype == (int)deliveryType
                              && int.Parse(p.Bundlecode) >= startbundle
                              && int.Parse(p.Bundlecode) <= endbundle).ToList();
            }
            else
            {
                // 非尺出库
                var intList = new List<int>() { -1, 1 };
                list = db.PdProduct.Where(p => p.Batcode == batcode && intList.Contains(System.Convert.ToInt16(p.Lengthtype))
                                 && int.Parse(p.Bundlecode.Replace("F", string.Empty)) >= startbundle
                                 && int.Parse(p.Bundlecode.Replace("F", string.Empty)) <= endbundle).ToList();
            }

            int result = 0;
            if (list.Count > 0)
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    foreach (var product in list)
                    {
                        var stockout = new DataLibrary.PdStockOut();
                        stockout.Lpn = lpn;
                        stockout.Sellerid = sellerid;
                        stockout.Productid = product.Id;
                        stockout.Createtime = (int)Util.Extensions.GetCurrentUnixTime();
                        stockout.Adder = 0;

                        var isout = db.PdStockOut.Where(s => s.Productid == product.Id).FirstOrDefault();

                        if (isout == null)
                        {
                            db.PdStockOut.Add(stockout);

                            if (db.SaveChanges() > 0)
                            {
                                result++;

                                // 0.定尺1.非尺
                                var lenType = deliveryType == DataLibrary.EnumList.ProductQualityLevel.定尺 ? 0 : 1;
                                var auth = db.SaleSellerAuth.Where(s => s.Batcode == product.Batcode
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
                                    auth.Lengthtype = deliveryType == DataLibrary.EnumList.ProductQualityLevel.定尺 ? 0 : 1;
                                    db.SaleSellerAuth.Add(auth);
                                }
                                else
                                {
                                    auth.Number = auth.Number + 1;

                                    db.SaleSellerAuth.Update(auth);
                                }

                                int addNum = db.SaveChanges();

                                if (addNum > 0)
                                {
                                    // 添加授权详情表
                                    var authdetail = db.SaleSellerAuthDetail.Where(s => s.AuthId == auth.Id
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

                                        db.SaleSellerAuthDetail.Add(detail);

                                        if (db.SaveChanges() < 1)
                                        {
                                            tran.Rollback();
                                            return 0;
                                        }
                                    }
                                }
                                else
                                {
                                    tran.Rollback();
                                    return 0;
                                }
                            }
                        }
                    }

                    tran.Commit();
                }
            }

            return result;
        }
    }
}
