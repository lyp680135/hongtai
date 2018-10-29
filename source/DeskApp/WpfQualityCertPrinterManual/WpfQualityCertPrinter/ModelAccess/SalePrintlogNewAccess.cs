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
    class SalePrintlogNewAccess : BaseAccess<SalePrintlogNew>
    {
        public SalePrintlogNewAccess()
            : base()
        {

        }

        public SalePrintlogNewAccess(SalePrintlogNew model)
            : base(model)
        {

        }

        public SalePrintlogNewAccess(MySqlConnection conn)
            : base(conn)
        {

        }

        public SalePrintlogNew SingleById(int? id = null)
        {
            SalePrintlogNew log = null;

            string sql = "SELECT * FROM saleprintlognew WHERE status=1 ORDER BY ID DESC LIMIT 1";
            if (id > 0)
            {
                sql = "SELECT * FROM saleprintlognew WHERE id=@id AND status=1 ORDER BY ID DESC LIMIT 1";
            }

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
                            log = new SalePrintlogNew();
                            log.Id = int.Parse(dr["Id"].ToString());
                            log.Consignor = (!Convert.IsDBNull(dr["Consignor"])) ?  dr["Consignor"].ToString() : null;
                            log.Printno = dr["Printno"].ToString();
                            log.Status = int.Parse(dr["Status"].ToString());
                            log.Checkcode = (!Convert.IsDBNull(dr["Checkcode"])) ? dr["Checkcode"].ToString() : null;
                        }
                    }
                }
            }

            return log;
        }

        public SalePrintlogNew SingleAllById(int id)
        {
            SalePrintlogNew log = null;

            string sql = "SELECT * FROM saleprintlognew WHERE id=@id ORDER BY ID DESC LIMIT 1";
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
                            log = new SalePrintlogNew();
                            log.Id = int.Parse(dr["Id"].ToString());
                            log.Consignor = (!Convert.IsDBNull(dr["Consignor"])) ? dr["Consignor"].ToString() : null;
                            log.Printno = dr["Printno"].ToString();
                            log.Status = (!Convert.IsDBNull(dr["Status"])) ? int.Parse(dr["Status"].ToString()) : new int?();
                            log.Checkcode = (!Convert.IsDBNull(dr["Checkcode"])) ? dr["Checkcode"].ToString() : null;
                        }
                    }
                }
            }

            return log;
        }

        public SalePrintlogNew SingleByPrintno(string printno)
        {
            SalePrintlogNew log = null;

            if (!string.IsNullOrEmpty(printno))
            {
                string sql = "SELECT * FROM saleprintlognew WHERE printno=@printno AND status=1 ORDER BY ID DESC LIMIT 1";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {

                    mysqlcom.Parameters.Add("@printno", MySqlDbType.VarChar); ;
                    mysqlcom.Parameters["@printno"].Value = printno;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            //逐行读取数据输出
                            if (dr.Read())
                            {
                                log = new SalePrintlogNew();
                                log.Id = int.Parse(dr["Id"].ToString());
                                log.Consignor = (!Convert.IsDBNull(dr["Consignor"])) ? dr["Consignor"].ToString() : null;
                                log.Printno = dr["Printno"].ToString();
                                log.Status = int.Parse(dr["Status"].ToString());
                                log.Checkcode = (!Convert.IsDBNull(dr["Checkcode"])) ? dr["Checkcode"].ToString() : null;
                            }
                        }
                    }
                }
            }

            return log;
        }

        public List<CommonItem> GetRecentConsignor()
        {
            List<CommonItem> list = null;
            string sql = "SELECT consignor FROM saleprintlognew WHERE (consignor is not null AND consignor <> '') group by consignor ORDER BY CONVERT( consignor USING gbk ) COLLATE gbk_chinese_ci ASC";

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
                            item.Name = dr.GetString("consignor");

                            list.Add(item);
                        }
                    }
                }
            }

            return list;
        }

        public List<CommonItem> GetRecentLpns()
        {
            List<CommonItem> list = null;
            string sql = "SELECT lpn FROM saleprintlognew where lpn is not null group by lpn ORDER BY CONVERT( lpn USING gbk ) COLLATE gbk_chinese_ci ASC";

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

    }
}
