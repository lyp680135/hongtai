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
    class SalePrintlogAccess : BaseAccess<SalePrintlog>
    {
        public SalePrintlogAccess()
            : base()
        {

        }

        public SalePrintlogAccess(SalePrintlog model)
            : base(model)
        {

        }

        public SalePrintlog SingleById(int? id = null)
        {
            SalePrintlog log = null;

            string sql = "SELECT * FROM saleprintlog WHERE status=1 ORDER BY ID DESC LIMIT 1";
            if (id > 0)
            {
                sql = "SELECT * FROM saleprintlog WHERE id=@id AND status=1 ORDER BY ID DESC LIMIT 1";
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
                            log = new SalePrintlog();
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

        public SalePrintlog SingleAllById(int id)
        {
            SalePrintlog log = null;

            string sql = "SELECT * FROM saleprintlog WHERE id=@id ORDER BY ID DESC LIMIT 1";
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
                            log = new SalePrintlog();
                            log.Id = int.Parse(dr["Id"].ToString());
                            log.Consignor = (!Convert.IsDBNull(dr["Consignor"])) ? dr["Consignor"].ToString() : null;
                            log.Printno = dr["Printno"].ToString();
                            log.Status = int.Parse(dr["Status"].ToString());
                            log.Checkcode = (!Convert.IsDBNull(dr["Checkcode"])) ? dr["Checkcode"].ToString() : null;
                        }
                    }
                }
            }

            return log;
        }

        public SalePrintlog SingleByPrintno(string printno)
        {
            SalePrintlog log = null;

            if (!string.IsNullOrEmpty(printno))
            {
                string sql = "SELECT * FROM saleprintlog WHERE printno=@printno AND status=1 ORDER BY ID DESC LIMIT 1";
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
                                log = new SalePrintlog();
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

        public SaleSeller GetPrintlogSeller(int printid)
        {
            SaleSeller seller = null;

            var log = this.SingleAllById(printid);
            if (log != null)
            {
                using (SalePrintLogDetaillAccess access = new SalePrintLogDetaillAccess())
                {
                    var details = access.GetListByPrintid(log.Id);
                    if (details != null)
                    {
                        //获取第一个售达方（一张质保书只开给一个售达方）
                        if (details.Count > 0)
                        {
                            using (SaleSellerAuthAccess saaccess = new SaleSellerAuthAccess())
                            {
                                var auth = saaccess.SingleById(details[0].Authid);
                                if (auth != null)
                                {
                                    seller = new SaleSeller();
                                    seller.Id = auth.Sellerid;
                                    seller.Name = auth.Sellername;
                                }
                            }
                        }
                    }
                }
            }

            return seller;
        }

        /// <summary>
        /// 获取只预览过没有打印的质量证明书
        /// </summary>
        /// <returns></returns>
        public List<SalePrintlog> GetListRecent()
        {
            List<SalePrintlog> list = null;

            try
            {
                string sql = "SELECT c.*,d.Name as sellername FROM ("
                                + " SELECT aa.*,bb.sellerid,bb.lpn FROM "
                                + " (SELECT a.*,b.authid as realauthid FROM saleprintlog as a,saleprintlogdetail as b"
                                + " WHERE (status=0 OR status is null) AND a.id=b.printid AND b.id=(SELECT id FROM saleprintlogdetail as b WHERE b.printid=a.id ORDER BY id DESC LIMIT 1)"
                                + " ORDER BY a.id DESC LIMIT 50) as aa,salesellerauth as bb WHERE aa.realauthid=bb.id) as c,"
                                + " saleseller as d WHERE c.sellerid=d.id ORDER BY c.id DESC";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            list = new List<SalePrintlog>();

                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var log = new SalePrintlog();
                                log.Id = dr.GetInt32("id");
                                log.Consignor = (!Convert.IsDBNull(dr["consignor"])) ? dr.GetString("consignor") : null;
                                log.Printno = dr.GetString("printno");
                                log.Createtime = dr.GetInt32("createtime");
                                log.Signetangle = dr.GetInt32("signetangle");
                                log.Checkcode = dr.GetString("checkcode");

                                log.Sellerid = dr.GetInt32("sellerid");
                                log.Sellername = (!Convert.IsDBNull(dr["sellername"])) ? dr.GetString("sellername") : null;
                                log.Lpn = (!Convert.IsDBNull(dr["lpn"])) ? dr.GetString("lpn") : null;

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


        public List<CommonItem> GetRecentConsignor()
        {
            List<CommonItem> list = null;
            string sql = "SELECT consignor FROM saleprintlog WHERE (consignor is not null AND consignor <> '') group by consignor ORDER BY CONVERT( consignor USING gbk ) COLLATE gbk_chinese_ci ASC";

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
    }
}
