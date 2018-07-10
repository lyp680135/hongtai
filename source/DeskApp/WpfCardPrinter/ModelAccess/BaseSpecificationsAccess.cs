using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WpfCardPrinter.ModelAccess;

namespace WpfCardPrinter.ModelAccess
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

        public List<BaseSpecifications> GetListByClassAndMaterial(int classid, int materialid)
        {
            List<BaseSpecifications> list = null;

            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM basespecifications WHERE classid=@classid AND materialid=@materialid", _connection))
            {
                mysqlcom.Parameters.Add("@classid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@classid"].Value = classid;
                mysqlcom.Parameters.Add("@materialid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@materialid"].Value = materialid;

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
                            obj.Classid = dr.GetInt32("classid");
                            obj.Materialid = dr.GetInt32("materialid");
                            obj.Callname = dr.GetString("callname");
                            obj.Specname = dr.GetString("specname");
                            obj.Referlength = dr.GetDouble("referlength");
                            obj.Refermeterweight = dr.GetDouble("refermeterweight");
                            obj.Referpieceweight = dr.GetDouble("referpieceweight");
                            obj.Referpiececount = dr.GetInt32("referpiececount");

                            list.Add(obj);
                        }
                    }
                }
            }

            return list;
        }
        public List<BaseSpecifications> GetListById(int id)
        {
            List<BaseSpecifications> list = null;

            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM basespecifications WHERE id=@id ", _connection))
            {
                mysqlcom.Parameters.Add("@id", MySqlDbType.Int32).Value=id;               
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
                            obj.Classid = dr.GetInt32("classid");
                            obj.Materialid = dr.GetInt32("materialid");
                            obj.Callname = dr.GetString("callname");
                            obj.Specname = dr.GetString("specname");
                            obj.Referlength = dr.GetDouble("referlength");
                            obj.Refermeterweight = dr.GetDouble("refermeterweight");
                            obj.Referpieceweight = dr.GetDouble("referpieceweight");
                            obj.Referpiececount = dr.GetInt32("referpiececount");

                            list.Add(obj);
                        }
                    }
                }
            }

            return list;
        }
    }
}
