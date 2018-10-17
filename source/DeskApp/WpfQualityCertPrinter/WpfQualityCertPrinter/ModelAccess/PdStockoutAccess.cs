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
    class PdStockoutAccess : BaseAccess<PdStockout>
    {
        public PdStockoutAccess() : base()
        {

        }

        public PdStockoutAccess(PdStockout model)
            :base(model)
        {

        }

        public PdStockoutAccess(MySqlConnection conn)
            : base(conn)
        {

        }

        public int DeleteByProductIds(List<int> productids, int sellerid)
        {
            int result = 0;

            try
            {
                string idsstr = string.Join(",", productids);

                if (!string.IsNullOrEmpty(idsstr))
                {
                    string sql = "DELETE FROM pdstockout WHERE sellerid=@sellerid AND productid IN (" + idsstr + ")";
                    using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                    {
                        mysqlcom.Parameters.Add("@sellerid", sellerid);

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
