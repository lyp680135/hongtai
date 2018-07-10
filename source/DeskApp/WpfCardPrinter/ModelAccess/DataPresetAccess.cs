using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLibrary;
using MySql.Data.MySqlClient;
namespace WpfCardPrinter.ModelAccess
{
    public class DataPresetAccess : BaseAccess<PdQualityProductPreset>
    {
        public DataPresetAccess()
            : base()
        {

        }

        public DataPresetAccess(PdQualityProductPreset model)
            : base(model)
        {

        }

        /// <summary>
        /// 添加的方法
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public int InsertDataPreset(PdQualityProductPreset dp)
        {
            int result = 0;
            string sqlStr = string.Empty;
            sqlStr += " insert into pdqualityproductpreset(BatCode,Materialid,Qid,CreateTime) values(@BatCode,@Materialid,@Qid,@CreateTime) ";
            using (MySqlCommand mysqlcom = new MySqlCommand(sqlStr, _connection))
            {
                mysqlcom.Parameters.Add("@BatCode", MySqlDbType.VarChar).Value = dp.BatCode;
                mysqlcom.Parameters.Add("@Materialid", MySqlDbType.Int32).Value = dp.Materialid;
                mysqlcom.Parameters.Add("@Qid", MySqlDbType.Int32).Value = dp.Qid;
                mysqlcom.Parameters.Add("@CreateTime", MySqlDbType.Int64).Value = dp.CreateTime;
                result = mysqlcom.ExecuteNonQuery();
            }
            return result;
        }

        /// <summary>
        /// 获取预置关系表数据
        /// </summary>
        /// <param name="createtime"></param>
        /// <param name="mid"></param>
        /// <param name="batcode"></param>
        /// <returns></returns>
        public PdQualityProductPreset GetDpList(int mid = 0, string batcode = "")
        {
            PdQualityProductPreset resDbInfo = null;

            string sqlStr = string.Empty;
            sqlStr += @" select Id,CreateTime,Materialid,Qid,BatCode  from pdqualityproductpreset where 1=1  ";
            if (!string.IsNullOrEmpty(batcode))
            {
                sqlStr += "  and batcode=@batcode  ";
            }
            sqlStr += "  and Materialid=@mid  ";
            sqlStr += "   ORDER BY ID DESC  LIMIT 1   ";

            using (MySqlCommand mysqlcom = new MySqlCommand(sqlStr, _connection))
            {
                if (!string.IsNullOrEmpty(batcode))
                {
                    mysqlcom.Parameters.Add("@batcode", MySqlDbType.String).Value = batcode;
                }

                mysqlcom.Parameters.Add("@mid", MySqlDbType.Int32).Value = mid;
                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //只读一条
                        if (dr.Read())
                        {
                            resDbInfo = new PdQualityProductPreset();
                            resDbInfo.Id = Convert.ToInt32(dr["Id"]);
                            resDbInfo.CreateTime = dr.GetInt32("createtime");
                            resDbInfo.Materialid = dr.GetInt32("Materialid");
                            resDbInfo.Qid = dr.GetInt32("Qid");
                            resDbInfo.BatCode = dr.GetString("BatCode");
                        }
                    }
                }
            }
            return resDbInfo;
        }
    }
}
