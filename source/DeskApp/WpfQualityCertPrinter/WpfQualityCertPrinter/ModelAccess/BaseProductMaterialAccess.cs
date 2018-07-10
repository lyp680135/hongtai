using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfQualityCertPrinter.ModelAccess
{
    public class BaseProductMaterialAccess : BaseAccess<BaseProductMaterial>
    {
        public BaseProductMaterialAccess() : base()
        {

        }

        public BaseProductMaterialAccess(BaseProductMaterial model)
            :base(model)
        {

        }

        public List<BaseProductMaterial> GetListByClass(int classid)
        {
            List<BaseProductMaterial> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM baseproductmaterial WHERE classid=@classid", _connection))
            {
                mysqlcom.Parameters.Add("@classid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@classid"].Value = classid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        list = new List<BaseProductMaterial>();
                        //逐行读取数据输出
                        while (dr.Read())
                        {
                            var obj = new BaseProductMaterial();
                            obj.Id = dr.GetInt32("id");
                            obj.Classid = dr.GetInt32("classid");
                            obj.Name = dr.GetString("name");
                            obj.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;

                            list.Add(obj);
                        }
                    }
                }
            }

            return list;
        }

        public List<BaseProductMaterial> GetList()
        {
            List<BaseProductMaterial> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM baseproductmaterial", _connection))
            {

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        list = new List<BaseProductMaterial>();
                        //逐行读取数据输出
                        while (dr.Read())
                        {
                            var obj = new BaseProductMaterial();
                            obj.Id = dr.GetInt32("id");
                            obj.Classid = dr.GetInt32("classid");
                            obj.Name = dr.GetString("name");
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
