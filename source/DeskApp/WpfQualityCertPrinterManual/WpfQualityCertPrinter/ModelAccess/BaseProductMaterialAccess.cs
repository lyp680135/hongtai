using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WpfQualityCertPrinter.ViewModel;

namespace WpfQualityCertPrinter.ModelAccess
{
    public class BaseProductMaterialAccess : BaseAccess<BaseProductMaterial>
    {
        public BaseProductMaterialAccess()
            : base()
        {

        }

        public BaseProductMaterialAccess(BaseProductMaterial model)
            : base(model)
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

        public List<MaterialList> GetShowList()
        {
            List<MaterialList> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT a.id,a.classid,a.name,a.materialiscancel,b.name as classname,b.measurement FROM baseproductmaterial as a left join baseproductclass as b on a.Classid = b.Id", _connection))
            {

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        list = new List<MaterialList>();
                        //逐行读取数据输出
                        while (dr.Read())
                        {
                            var obj = new MaterialList();
                            obj.Materialid = dr.GetInt32("id");
                            obj.MaterialIsCancel = (EnumList.MaterialIsCancel)dr.GetInt32("materialiscancel");
                            if (obj.MaterialIsCancel == EnumList.MaterialIsCancel.作废)
                            {
                                obj.Materialname = dr.GetString("name") + "(老)";
                            }
                            else
                            {
                                obj.Materialname = dr.GetString("name");
                            }
                            obj.Classname = dr.GetString("classname");
                            obj.ShowName = obj.Classname + " - " + obj.Materialname;
                            obj.Measurement = (EnumList.MeteringMode)dr.GetInt32("measurement");
                            list.Add(obj);
                        }
                    }
                }
            }

            return list;
        }
    }
}
