using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfQualityCertPrinter.ModelAccess
{
    class MngSettingAccess : BaseAccess<MngSetting>
    {
        public MngSettingAccess() : base()
        {

        }

        public MngSettingAccess(MngSetting model)
            :base(model)
        {

        }

        public MngSetting Single()
        {
            MngSetting setting = null;

            try
            {
                //获取基本配置
                using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM mngsetting ORDER BY id ASC LIMIT 1", _connection))
                {
                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                setting = new MngSetting();
                                setting.BatCode = dr["BatCode"].ToString();
                                setting.Name = dr["Name"].ToString();
                                setting.NameEn = dr["NameEn"].ToString();
                                setting.Client = dr["client"].ToString();
                                setting.ClientEn = dr["clienten"].ToString();
                                setting.Domain = (!Convert.IsDBNull(dr["Domain"])) ? dr.GetString("Domain") : null;
                            }
                            setting.SystemVersion = (EnumList.SystemVersion)Convert.ToInt32(dr["SystemVersion"]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return setting;
        }
    }
}
