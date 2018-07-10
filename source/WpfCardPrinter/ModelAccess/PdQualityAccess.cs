using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfCardPrinter.ModelAccess
{
    public class PdQualityAccess : BaseAccess<PdQuality>
    {
        public PdQualityAccess()
            : base()
        {

        }

        public PdQualityAccess(PdQuality model)
            : base(model)
        {

        }

        public List<PdQuality> GetList(string batcode, int materialid)
        {
            List<PdQuality> list = null;

            string sql = "SELECT * FROM pdquality WHERE batcode=@batcode AND materialid=@materialid AND createflag=0 ORDER BY id DESC";
            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {
                mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar); ;
                mysqlcom.Parameters["@batcode"].Value = batcode;
                mysqlcom.Parameters.Add("@materialid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@materialid"].Value = materialid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        list = new List<PdQuality>();
                        if (dr.Read())
                        {
                            var data = new PdQuality();
                            data.Id = dr.GetInt32("Id");
                            data.Batcode = dr.GetString("batcode");
                            data.MaterialId = dr.GetInt32("materialid");
                            data.CreateFlag = dr.GetInt32("createflag");
                            data.Createtime = dr.GetInt32("createtime");

                            list.Add(data);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取材质下的预置数据
        /// </summary>
        /// <param name="materialid"></param>
        /// <returns></returns>
        public List<PdQuality> GetList(int materialid)
        {
            List<PdQuality> list = null;

            string sql = "SELECT * FROM pdquality WHERE materialid=@materialid AND createflag=1 ORDER BY id DESC";
            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {
                mysqlcom.Parameters.Add("@materialid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@materialid"].Value = materialid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        list = new List<PdQuality>();
                        if (dr.Read())
                        {
                            var data = new PdQuality();
                            data.Id = dr.GetInt32("Id");
                            data.Batcode = dr.GetString("batcode");
                            data.MaterialId = dr.GetInt32("materialid");
                            data.CreateFlag = dr.GetInt32("createflag");
                            data.Createtime = dr.GetInt32("createtime");

                            list.Add(data);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取下一单条预置质量数据
        /// </summary>
        /// <param name="materialid"></param>
        /// <param name="currentid"></param>
        /// <returns></returns>
        public PdQuality FindNext(int materialid, int currentid)
        {
            var pdqulity = new PdQuality();
            string sqlStr = string.Empty;
            sqlStr += @" SELECT *
                                    FROM pdquality
                                    WHERE 1=1 AND CreateFlag=1 ";
            sqlStr += @" And  Id > @Id ";
            sqlStr += @" And MaterialId=@MaterialId  ";
            sqlStr += @" Order By Id ASC LIMIT 1 ";

            using (MySqlCommand mysqlcom = new MySqlCommand(sqlStr, _connection))
            {
                mysqlcom.Parameters.Add("@Id", MySqlDbType.Int32).Value = currentid;
                mysqlcom.Parameters.Add("@MaterialId", MySqlDbType.Int32).Value = materialid;
                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            pdqulity.Id = dr.GetInt32("Id");
                            pdqulity.Batcode = dr.GetString("batcode");
                            pdqulity.MaterialId = dr.GetInt32("materialid");
                            pdqulity.CreateFlag = dr.GetInt32("createflag");
                            pdqulity.Createtime = dr.GetInt32("createtime");
                        }
                    }
                }
            }
            return pdqulity;
        }
    }
}
