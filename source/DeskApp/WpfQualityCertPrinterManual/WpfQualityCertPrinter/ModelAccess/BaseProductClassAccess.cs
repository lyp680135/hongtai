using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfQualityCertPrinter.ModelAccess
{
    public class BaseProductClassAccess : BaseAccess<BaseProductClass>
    {
        public BaseProductClassAccess()
            : base()
        {

        }

        public BaseProductClassAccess(BaseProductClass model)
            : base(model)
        {

        }

        public List<BaseProductClass> GetList()
        {
            List<BaseProductClass> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM baseproductclass", _connection))
            {

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        list = new List<BaseProductClass>();
                        //逐行读取数据输出
                        while (dr.Read())
                        {
                            var obj = new BaseProductClass();
                            obj.Id = dr.GetInt32("id");
                            obj.Gbname = dr.GetString("Gbname");
                            obj.Name = dr.GetString("name");
                            obj.Createtime = (!Convert.IsDBNull(dr["Createtime"])) ? dr.GetInt32("Createtime") : 0;
                            obj.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;

                            list.Add(obj);
                        }
                    }
                }
            }

            return list;
        }
    }
}
