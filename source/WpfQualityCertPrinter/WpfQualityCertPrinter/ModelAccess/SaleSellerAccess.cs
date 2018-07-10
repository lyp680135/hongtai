using DataLibrary;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfQualityCertPrinter.Common;
using XYNetCloud.Utils;

namespace WpfQualityCertPrinter.ModelAccess
{
    class SaleSellerAccess : BaseAccess<SaleSeller>
    {
        public SaleSellerAccess() : base()
        {

        }

        public SaleSellerAccess(SaleSeller model)
            :base(model)
        {

        }

        public SaleSeller SingleById(int? id = null)
        {
            SaleSeller seller = null;

            string sql  = "SELECT * FROM saleseller WHERE id=@id ORDER BY ID DESC LIMIT 1";

            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {
                if (id > 0)
                {
                    mysqlcom.Parameters.Add("@id", MySqlDbType.Int32); ;
                    mysqlcom.Parameters["@id"].Value = id;
                }

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            seller = new SaleSeller();
                            seller.Id = int.Parse(dr["Id"].ToString());
                            seller.Name = (!Convert.IsDBNull(dr["Name"])) ? dr["Name"].ToString() : null;
                            seller.Mobile = (!Convert.IsDBNull(dr["Mobile"])) ? dr["Mobile"].ToString() : null;
                            seller.Createtime = (!Convert.IsDBNull(dr["Createtime"])) ? int.Parse(dr["Createtime"].ToString()) : new int?(); 
                            seller.Parent = (!Convert.IsDBNull(dr["Parent"])) ? int.Parse(dr["Parent"].ToString()) : new int?();
                        }
                    }
                }
            }

            return seller;
        }

        public List<SaleSeller> GetTopSellerList()
        {
            List<SaleSeller> sellerlist = null;

            try
            {
                string sql = "SELECT * FROM saleseller WHERE parent IS NULL";

                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            sellerlist = new List<SaleSeller>();
                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var seller = new SaleSeller();
                                seller.Id = int.Parse(dr["Id"].ToString());
                                seller.Name = (!Convert.IsDBNull(dr["Name"])) ? dr["Name"].ToString() : null;
                                seller.Mobile = (!Convert.IsDBNull(dr["Mobile"])) ? dr["Mobile"].ToString() : null;
                                seller.Createtime = (!Convert.IsDBNull(dr["Createtime"])) ? int.Parse(dr["Createtime"].ToString()) : new int?();
                                seller.Parent = (!Convert.IsDBNull(dr["Parent"])) ? int.Parse(dr["Parent"].ToString()) : new int?();

                                sellerlist.Add(seller);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return sellerlist;
        }
    }
}
