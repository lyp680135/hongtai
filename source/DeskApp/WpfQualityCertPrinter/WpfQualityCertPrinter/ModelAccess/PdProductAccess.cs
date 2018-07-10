using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;
using System.Windows;
using XYNetCloud.Utils;

namespace WpfQualityCertPrinter.ModelAccess
{
    class PdProductAccess :BaseAccess<PdProduct>
    {
        public PdProductAccess() : base()
        {

        }

        public PdProductAccess(PdProduct model)
            :base(model)
        {

        }

        /// <summary>
        /// 获取轧制批号中还没有出库的产品列表
        /// </summary>
        /// <param name="batcode"></param>
        /// <returns></returns>
        public List<PdProduct> GetListByBatcode(string batcode)
        {
            List<PdProduct> productlist = null;

            try
            {
                string sql="SELECT C.*,D.specname,D.callname,E.Name AS materialname FROM (SELECT A.*,B.TeamName FROM ("
                        + "SELECT x.*,y.productid FROM pdproduct as x LEFT JOIN pdstockout as y ON  x.id=y.productid WHERE x.batcode=@batcode AND y.productid is null"
                        + ") AS A LEFT JOIN pdworkshopteam AS B ON A.workshift = B.id) AS C, basespecifications AS D,baseproductmaterial AS E"
                        + " WHERE C.specid=D.id AND C.Materialid=E.Id ORDER BY bundlecode ASC";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar); ;
                    mysqlcom.Parameters["@batcode"].Value = batcode;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            productlist = new List<PdProduct>();

                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var obj = new PdProduct();
                                obj.Id = dr.GetInt32("id");
                                obj.Batcode = dr.GetString("batcode");
                                obj.Classid = dr.GetInt32("classid");
                                obj.Materialid = (!Convert.IsDBNull(dr["materialid"])) ? dr.GetInt32("materialid") : new int?();
                                obj.Materialname = dr.GetString("materialname");
                                obj.Specid = (!Convert.IsDBNull(dr["specid"])) ? dr.GetInt32("specid") : new int?();
                                obj.Lengthtype = dr.GetInt32("lengthtype");
                                obj.Length = (!Convert.IsDBNull(dr["length"])) ? dr.GetDouble("length") : new double?();
                                obj.Bundlecode = dr.GetString("bundlecode");
                                obj.Piececount = (!Convert.IsDBNull(dr["piececount"])) ? dr.GetInt32("piececount") : new int?();
                                obj.Weight = (!Convert.IsDBNull(dr["weight"])) ? dr.GetDouble("weight") : new double?();
                                obj.Meterweight = (!Convert.IsDBNull(dr["meterweight"])) ? dr.GetDouble("meterweight") : new double?();
                                obj.Createtime = dr.GetInt32("createtime");
                                obj.WorkShift = dr.GetInt32("workshift");
                                obj.Shiftname = dr.GetString("teamname");
                                obj.Randomcode = (!Convert.IsDBNull(dr["randomcode"])) ? dr.GetString("randomcode") : null;

                                int code = 0;
                                if (obj.Lengthtype == (int)EnumList.ProductQualityLevel.定尺)
                                {
                                    int.TryParse(obj.Bundlecode, out code);
                                }
                                else
                                {
                                    int.TryParse(obj.Bundlecode.Replace("F", ""), out code);
                                    code = -(5 - code);
                                }
                                obj.BundlecodeValue = code;

                                obj.Specname = (!Convert.IsDBNull(dr["specname"])) ? dr.GetString("specname") : null;
                                obj.Callspecname = (!Convert.IsDBNull(dr["callname"])) ? dr.GetString("callname") : null;

                                productlist.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return productlist;
        }

    }
}
