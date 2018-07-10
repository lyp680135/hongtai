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
    class PdBatcodeAccess : BaseAccess<PdBatcode>
    {
        public PdBatcodeAccess()
            : base()
        {

        }

        public PdBatcodeAccess(PdBatcode model)
            : base(model)
        {

        }

        public PdBatcode SingleByBatcode(string batcode)
        {
            PdBatcode pdbatcode = null;

            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE batcode=@batcode ORDER BY ID DESC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar); ;
                mysqlcom.Parameters["@batcode"].Value = batcode;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdbatcode = new PdBatcode();
                            pdbatcode.Id = int.Parse(dr["id"].ToString());
                            pdbatcode.Batcode = dr["batcode"].ToString();
                            pdbatcode.Createtime = int.Parse(dr["createtime"].ToString());
                            pdbatcode.Status = int.Parse(dr["status"].ToString());
                        }
                    }
                }
            }

            return pdbatcode;
        }

        public List<PdBatcode> GetList(string where = null, int count = 10, string orderby = "Id DESC")
        {
            List<PdBatcode> batcodelist = null;

            try
            {
                string sql = "SELECT * FROM pdbatcode WHERE 1=1";
                if (!string.IsNullOrEmpty(where))
                {
                    sql += " " + where;
                }

                if (!string.IsNullOrEmpty(orderby))
                {
                    sql += " ORDER BY " + orderby;
                }

                if (count > 0)
                {
                    sql += " LIMIT " + count;
                }

                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            batcodelist = new List<PdBatcode>();

                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var obj = new PdBatcode();
                                obj.Id = dr.GetInt32("id");
                                obj.Batcode = dr.GetString("batcode");
                                obj.Status = (!Convert.IsDBNull(dr["status"])) ? dr.GetInt32("status") : new int?();
                                obj.Createtime = dr.GetInt32("createtime");
                                obj.Adder = (!Convert.IsDBNull(dr["adder"])) ? dr.GetString("adder") : null;

                                batcodelist.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return batcodelist;
        }

    }
}
