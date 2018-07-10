using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WpfCardPrinter.ModelAccess;

namespace WpfCardPrinter.ModelAccess
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

        public BaseProductMaterial Single(int id)
        {
            BaseProductMaterial material = null;

            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT a.*,b.Name as classname,b.gbname,b.deliverytype,b.measurement FROM baseproductmaterial as a,baseproductclass as b WHERE a.id=@id AND a.classid=b.id", _connection))
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
                            material = new BaseProductMaterial();
                            material.Id = dr.GetInt32("id");
                            material.Classid = dr.GetInt32("classid");
                            material.Name = dr.GetString("name");
                            material.Classname = dr.GetString("classname");
                            material.GbClassname = dr.GetString("gbname");
                            material.Deliverytype = (!Convert.IsDBNull(dr["deliverytype"])) ? dr.GetInt32("deliverytype") : new int?();
                            material.Measurement = (!Convert.IsDBNull(dr["measurement"])) ? dr.GetInt32("measurement") : new int?();
                            material.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;

                            using (BaseProductMaterialAccess taccess = new BaseProductMaterialAccess())
                            {
                                var doc = taccess.GetMaterialGbdocument(material.Id);
                                if (doc != null)
                                {
                                    material.Gbdocument = doc.Name;
                                }
                            }

                        }
                    }
                }
            }

            return material;
        }

        public List<BaseProductMaterial> GetList()
        {
            List<BaseProductMaterial> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT a.*,b.Name as classname,b.gbname,b.deliverytype,b.measurement FROM baseproductmaterial as a,baseproductclass as b WHERE a.classid=b.id ORDER BY a.classid", _connection))
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
                            var material = new BaseProductMaterial();
                            material.Id = dr.GetInt32("id");
                            material.Classid = dr.GetInt32("classid");
                            material.Name = dr.GetString("name");
                            material.Classname = dr.GetString("classname");
                            material.GbClassname = dr.GetString("gbname");
                            material.Deliverytype = (!Convert.IsDBNull(dr["deliverytype"])) ? dr.GetInt32("deliverytype") : new int?();
                            material.Measurement = (!Convert.IsDBNull(dr["measurement"])) ? dr.GetInt32("measurement") : new int?();
                            material.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;

                            using (BaseProductMaterialAccess taccess = new BaseProductMaterialAccess())
                            {
                                var doc = taccess.GetMaterialGbdocument(material.Id);
                                if (doc != null)
                                {
                                    material.Gbdocument = doc.Name;
                                }
                            }

                            list.Add(material);
                        }
                    }
                }
            }

            return list;
        }

        public List<BaseProductMaterial> GetListByClass(int classid)
        {
            List<BaseProductMaterial> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT a.*,b.Name as classname,b.gbname,b.deliverytype,b.measurement FROM baseproductmaterial as a,baseproductclass as b WHERE a.classid=b.id AND a.classid=@classid", _connection))
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
                            var material = new BaseProductMaterial();
                            material.Id = dr.GetInt32("id");
                            material.Classid = dr.GetInt32("classid");
                            material.Name = dr.GetString("name");
                            material.Classname = dr.GetString("classname");
                            material.GbClassname = dr.GetString("gbname");
                            material.Deliverytype = (!Convert.IsDBNull(dr["deliverytype"])) ? dr.GetInt32("deliverytype") : new int?();
                            material.Measurement = (!Convert.IsDBNull(dr["measurement"])) ? dr.GetInt32("measurement") : new int?();
                            material.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;

                            using (BaseProductMaterialAccess taccess = new BaseProductMaterialAccess())
                            {
                                var doc = taccess.GetMaterialGbdocument(material.Id);
                                if (doc != null)
                                {
                                    material.Gbdocument = doc.Name;
                                }
                            }

                            list.Add(material);
                        }
                    }
                }
            }

            return list;
        }

        private BaseGbProduction GetMaterialGbdocument(int materialid)
        {
            BaseGbProduction doc = null;

            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("select a.Materialid,b.* from basequalitystandard as a,basegbproduction as b"
                +" where Materialid = @materialid AND a.Standardid=b.id GROUP BY Materialid,Standardid", _connection))
            {
                mysqlcom.Parameters.Add("@materialid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@materialid"].Value = materialid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            doc = new BaseGbProduction();
                            doc.Id = dr.GetInt32("id");
                            doc.Name = dr.GetString("name");
                            doc.Createtime = (!Convert.IsDBNull(dr["createtime"])) ? dr.GetInt32("createtime") : new int?();
                            doc.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;

                        }
                    }
                }
            }

            return doc;
        }
    }
}
