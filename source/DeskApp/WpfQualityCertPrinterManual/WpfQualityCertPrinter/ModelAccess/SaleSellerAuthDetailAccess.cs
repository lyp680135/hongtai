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
    class SaleSellerAuthDetailAccess : BaseAccess<SaleSellerAuthDetail>
    {
        public SaleSellerAuthDetailAccess() : base()
        {

        }

        public SaleSellerAuthDetailAccess(SaleSellerAuthDetail model)
            :base(model)
        {

        }

        public SaleSellerAuthDetailAccess(MySqlConnection conn)
            : base(conn)
        {

        }

        public List<SaleSellerAuthDetail> GetListByAuthid(int authid, int nums = 0)
        {
            List<SaleSellerAuthDetail> list = null;

            try
            {
                string sql = "SELECT * FROM salesellerauthdetail WHERE authid=@authid ORDER BY id DESC";
                if (nums > 0)
                {
                    sql += " LIMIT " + nums;
                }

                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    var param = new MySqlParameter("@authid", MySqlDbType.Int32);
                    param.Value = authid;

                    mysqlcom.Parameters.Add(param);

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            list = new List<SaleSellerAuthDetail>();

                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var detail = new SaleSellerAuthDetail();
                                detail.Id = dr.GetInt32("id");
                                detail.AuthId = dr.GetInt32("authid");
                                detail.Productid = dr.GetInt32("productid");
                                detail.Classid = dr.GetInt32("classid");
                                detail.Materialid = dr.GetInt32("materialid");
                                detail.Specid = dr.GetInt32("specid");
                                detail.Batcode = (!Convert.IsDBNull(dr["batcode"])) ? dr.GetString("batcode") : null;

                                list.Add(detail);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return list;
        }

        public int DeleteByIds(List<int> ids)
        {
            int result = 0;

            try
            {
                string idsstr = string.Join(",", ids);

                if (!string.IsNullOrEmpty(idsstr))
                {
                    string sql = "DELETE FROM salesellerauthdetail WHERE id IN (" + idsstr + ")";
                    using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                    {
                        result = mysqlcom.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {

            }

            return result;
        }
    }
}
