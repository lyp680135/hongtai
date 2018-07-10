using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfQualityCertPrinter.ModelAccess
{
    class PdWorkshopAccess :BaseAccess<PdWorkshop>
    {
        public PdWorkshopAccess() : base()
        {

        }

        public PdWorkshopAccess(PdWorkshop model)
            :base(model)
        {

        }

        public PdWorkshop Single(int shopid)
        {
            PdWorkshop shop = null;

            try
            {
                string sql="SELECT * FROM pdworkshop WHERE id=@id";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {

                    mysqlcom.Parameters.Add("@id", MySqlDbType.VarChar);
                    mysqlcom.Parameters["@id"].Value = shopid;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            shop = new PdWorkshop();

                            if (dr.Read())
                            {
                                shop.Id = dr.GetInt32("id");
                                shop.Name = dr.GetString("name");
                                shop.Code = dr.GetString("code");
                                shop.Address = (!Convert.IsDBNull(dr["address"])) ? dr.GetString("address") : null;
                                shop.Manager = (!Convert.IsDBNull(dr["manager"])) ? dr.GetString("manager") : null;
                                shop.Inputer = (!Convert.IsDBNull(dr["inputer"])) ? dr.GetString("inputer") : null;
                                shop.Outputer = (!Convert.IsDBNull(dr["outputer"])) ? dr.GetString("outputer") : null;
                                shop.QAInputer = (!Convert.IsDBNull(dr["qainputer"])) ? dr.GetString("qainputer") : null;
                                shop.CreateTime = dr.GetInt32("createtime");

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return shop;
        }

        /// <summary>
        /// 根据车间代码获取车间
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public PdWorkshop Single(string code)
        {
            PdWorkshop shop = null;

            try
            {
                string sql = "SELECT * FROM pdworkshop WHERE code=@code";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {

                    mysqlcom.Parameters.Add("@code", MySqlDbType.VarChar);
                    mysqlcom.Parameters["@code"].Value = code;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            shop = new PdWorkshop();

                            if (dr.Read())
                            {
                                shop.Id = dr.GetInt32("id");
                                shop.Name = dr.GetString("name");
                                shop.Code = dr.GetString("code");
                                shop.Address = (!Convert.IsDBNull(dr["address"])) ? dr.GetString("address") : null;
                                shop.Manager = (!Convert.IsDBNull(dr["manager"])) ? dr.GetString("manager") : null;
                                shop.Inputer = (!Convert.IsDBNull(dr["inputer"])) ? dr.GetString("inputer") : null;
                                shop.Outputer = (!Convert.IsDBNull(dr["outputer"])) ? dr.GetString("outputer") : null;
                                shop.QAInputer = (!Convert.IsDBNull(dr["qainputer"])) ? dr.GetString("qainputer") : null;
                                shop.CreateTime = dr.GetInt32("createtime");

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return shop;
        }

        public List<PdWorkshop> GetList()
        {
            List<PdWorkshop> list = null;

            try
            {
                using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdworkshop", _connection))
                {
                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            list = new List<PdWorkshop>();

                            while (dr.Read())
                            {
                                var shop = new PdWorkshop();
                                shop.Id = dr.GetInt32("id");
                                shop.Name = dr.GetString("name");
                                shop.Code = dr.GetString("code");
                                shop.Address = (!Convert.IsDBNull(dr["address"])) ? dr.GetString("address") : null;
                                shop.Manager = (!Convert.IsDBNull(dr["manager"])) ? dr.GetString("manager") : null;
                                shop.Inputer = (!Convert.IsDBNull(dr["inputer"])) ? dr.GetString("inputer") : null;
                                shop.Outputer = (!Convert.IsDBNull(dr["outputer"])) ? dr.GetString("outputer") : null;
                                shop.QAInputer = (!Convert.IsDBNull(dr["qainputer"])) ? dr.GetString("qainputer") : null;
                                shop.CreateTime = dr.GetInt32("createtime");

                                list.Add(shop);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return list;
        }
    }
}
