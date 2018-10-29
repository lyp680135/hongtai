using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WpfQualityCertPrinter.ModelAccess
{
    public class BaseSpecificationsAccess : BaseAccess<BaseSpecifications>
    {
        public BaseSpecificationsAccess()
            : base()
        {

        }

        public BaseSpecificationsAccess(BaseSpecifications model)
            : base(model)
        {

        }

        public List<BaseSpecifications> GetList()
        {
            List<BaseSpecifications> list = null;
            //获取材质
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM basespecifications", _connection))
            {

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        list = new List<BaseSpecifications>();
                        //逐行读取数据输出
                        while (dr.Read())
                        {
                            var obj = new BaseSpecifications();
                            obj.Id = dr.GetInt32("id");
                            obj.Callname = dr.GetString("Callname");
                            obj.Specname = dr.GetString("Specname");
                            obj.Classid = dr.GetInt32("Classid");
                            obj.FullSpecname = obj.Callname + " X " + obj.Referlength;
                            obj.Materialid = dr.GetInt32("Materialid");
                            obj.Referlength = dr.GetDouble("Referlength");
                            obj.Refermeterweight = dr.GetDouble("Refermeterweight");
                            obj.Referpiececount = dr.GetInt32("Referpiececount");
                            obj.Referpieceweight = dr.GetDouble("Referpieceweight");
                            list.Add(obj);
                        }
                    }
                }
            }

            return list;
        }
    }
}
