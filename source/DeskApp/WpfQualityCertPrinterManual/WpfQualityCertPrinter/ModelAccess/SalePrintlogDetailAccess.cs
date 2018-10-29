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
    class SalePrintLogDetaillAccess : BaseAccess<SalePrintLogDetail>
    {
        public SalePrintLogDetaillAccess() : base()
        {

        }

        public SalePrintLogDetaillAccess(SalePrintLogDetail model)
            :base(model)
        {

        }

        public SalePrintLogDetaillAccess(MySqlConnection conn)
            : base(conn)
        {

        }

        /// <summary>
        /// 获取质量证明书的详细产品明细清单
        /// </summary>
        /// <param name="printid">打印记录主表标识id</param>
        /// <returns></returns>
        public List<SalePrintLogDetail> GetListByPrintid(int printid)
        {
            List<SalePrintLogDetail> list = null;

            try
            {
                string sql = "SELECT * FROM saleprintlogdetail WHERE printid=@printid ORDER BY ID DESC"; 

                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    mysqlcom.Parameters.Add("@printid", MySqlDbType.Int32); ;
                    mysqlcom.Parameters["@printid"].Value = printid;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            list = new List<SalePrintLogDetail>();

                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var log = new SalePrintLogDetail();
                                log.Id = dr.GetInt32("id");
                                log.Authid = dr.GetInt32("authid");
                                log.Number = dr.GetInt32("number");
                                log.PrintId = dr.GetInt32("printid");
                                log.Printnumber = dr.GetInt32("printnumber");
								
                                list.Add(log);
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
                    string sql = "DELETE FROM saleprintlogdetail WHERE id IN (" + idsstr + ")";
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
