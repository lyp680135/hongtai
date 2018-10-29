using DataLibrary;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfQualityCertPrinter.Common;
using WpfQualityCertPrinter.ViewModel;
using XYNetCloud.Utils;

namespace WpfQualityCertPrinter.ModelAccess
{
    class SaleSellerAuthAccess : BaseAccess<SaleSellerAuth>
    {
        public SaleSellerAuthAccess() : base()
        {

        }

        public SaleSellerAuthAccess(SaleSellerAuth model)
            :base(model)
        {

        }

        public SaleSellerAuthAccess(MySqlConnection conn)
            : base(conn)
        {

        }

        public SaleSellerAuth SingleById(int id)
        {
            SaleSellerAuth auth = null;

            string sql  = "SELECT a.*,b.Name FROM salesellerauth as a,saleseller as b WHERE a.id=@id AND a.sellerid=b.id ORDER BY ID DESC LIMIT 1";

            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {
               
                mysqlcom.Parameters.Add("@id", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@id"].Value = id;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            auth = new SaleSellerAuth();
                            auth.Id = int.Parse(dr["Id"].ToString());
                            auth.Classid = int.Parse(dr["Classid"].ToString());
                            auth.Materialid = int.Parse(dr["Materialid"].ToString());
                            auth.Specid = int.Parse(dr["Specid"].ToString());
                            auth.Batcode = dr.GetString("Batcode");
                            auth.Number = int.Parse(dr["Number"].ToString());
                            auth.Lpn = dr.GetString("Lpn");
                            auth.Sellerid = int.Parse(dr["SellerId"].ToString());
                            auth.Parentseller = (!Convert.IsDBNull(dr["Parentseller"])) ? int.Parse(dr["Parentseller"].ToString()) : new int?();
                            auth.Lengthtype = (!Convert.IsDBNull(dr["Lengthtype"])) ? int.Parse(dr["Lengthtype"].ToString()) : new int?();
                            auth.Createtime = (!Convert.IsDBNull(dr["Createtime"])) ? int.Parse(dr["Createtime"].ToString()) : new int?();

                            auth.Sellername = (!Convert.IsDBNull(dr["Name"])) ? dr.GetString("Name") : null;
                        }
                    }
                }
            }

            return auth;
        }

        public SelectedProductInfo GetProductInfoById(int id)
        {
            SelectedProductInfo info = null;
            string sql = "SELECT a.*,b.Name as Classname,c.Name as MaterialName,d.Specname,d.Referlength FROM salesellerauth as a,baseproductclass as b,baseproductmaterial as c,basespecifications as d"
                            +" WHERE a.id=@id AND a.classid=b.id AND a.materialid=c.id AND a.specid=d.id ORDER BY ID DESC LIMIT 1";

            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {

                mysqlcom.Parameters.Add("@id", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@id"].Value = id;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            info = new SelectedProductInfo();
                            info.Batcode = dr.GetString("Batcode");
                            info.Number = int.Parse(dr["Number"].ToString());
                            info.Classid = dr.GetInt32("Classid");
                            info.Classname = dr.GetString("Classname");
                            info.Materialid = dr.GetInt32("Materialid");
                            info.Materialname = dr.GetString("Materialname");
                            info.Specid = dr.GetInt32("Specid");
                            info.Specfullname = dr.GetString("Specname") + "x" + dr.GetString("Referlength");
                            info.Length = dr.GetString("Referlength");
                            info.Lengthtype = dr.GetInt32("Lengthtype");

                            if (info.Lengthtype != (int)EnumList.ProductQualityLevel.定尺)
                            {
                                if (info.Lengthtype == (int)EnumList.ProductQualityLevel.短尺)
                                {
                                    info.Length = "S";
                                }
                                else
                                {
                                    info.Length = "L";
                                }

                                info.Lengthnote = "非尺";
                            }
                            else
                            {
                                info.Lengthnote = "定尺";
                            }
                        }
                    }
                }
            }

            return info;
        }

        public List<CommonItem> GetRecentLpns()
        {
            List<CommonItem> list = null;
            string sql = "SELECT lpn FROM salesellerauth group by lpn ORDER BY CONVERT( lpn USING gbk ) COLLATE gbk_chinese_ci ASC";

            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {
                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        list = new List<CommonItem>();
                       
                        int index = 1;
                        while (dr.Read())
                        {
                            var item = new CommonItem();
                            item.Id = index;
                            item.Name = dr.GetString("lpn");

                            list.Add(item);
                        }
                    }
                }
            }

            return list;
        }

        public int UpdateNums(SaleSellerAuth model)
        {
            int result = 0;

            try
            {
                string sql = "UPDATE salesellerauth SET number=@number WHERE id=@id";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    mysqlcom.Parameters.Add(new MySqlParameter("@number", model.Number));
                    mysqlcom.Parameters.Add(new MySqlParameter("@id", model.Id));

                    result = mysqlcom.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

            }

            return result;
        }

        public int Delete(int id)
        {
            int result = 0;

            try
            {
                string sql = "DELETE FROM salesellerauth WHERE id=@id";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    mysqlcom.Parameters.Add(new MySqlParameter("@id", id));

                    result = mysqlcom.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

            }

            return result;
        }
    }
}
