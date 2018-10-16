
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfCardPrinter.Model;

namespace WpfCardPrinter.ModelAccess
{
    public class PdStockOutAccess : BaseAccess<PdStockOut>
    {
        public PdStockOutAccess() : base()
        {

        }

        public PdStockOutAccess(PdStockOut model)
            : base(model)
        {

        }

        public PdStockOut SingleByProductid(string Productids)
        {
            PdStockOut pdStockOut = new PdStockOut();
            string sqlStr = string.Format(@"select * from PdStockOut where Productid in ({0}) ", "@Productid");
            using (MySqlCommand sqlCmd = new MySqlCommand(sqlStr, _connection))
            {
                sqlCmd.Parameters.Add("@Productid", MySqlDbType.VarChar).Value = Productids;
                using (MySqlDataReader dr = sqlCmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdStockOut.Id = dr.GetInt16("Id");
                            pdStockOut.Lpn = dr.GetString("Lpn");
                            pdStockOut.Productid = dr.GetInt16("Productid");
                            pdStockOut.Sellerid = dr.GetInt16("Sellerid");
                            pdStockOut.Createtime = dr.GetInt32("Createtime");
                            pdStockOut.Adder = dr.GetInt16("Adder");
                        }
                    }
                }
            }
            return pdStockOut;
        }
    }
}
