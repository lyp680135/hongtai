using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfCardPrinter.ModelAccess
{
    class BaseProductClassAccess : BaseAccess<BaseProductClass>
    {
        public BaseProductClassAccess() : base()
        {

        }

        public BaseProductClassAccess(BaseProductClass model)
            :base(model)
        {

        }

        public List<BaseProductClass> GetList()
        {
            List<BaseProductClass> list = null;

            try
            {
                using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM baseproductclass ORDER BY name ASC", _connection))
                {
                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            list = new List<BaseProductClass>();

                            while (dr.Read())
                            {
                                var productclass = new BaseProductClass();
                                productclass.Id = dr.GetInt32("id");
                                productclass.Name = dr.GetString("name");
                                productclass.Gbname = dr.GetString("gbname");
                                productclass.Note = (!Convert.IsDBNull(dr["note"])) ? dr.GetString("note") : null;
                                productclass.Measurement = (!Convert.IsDBNull(dr["measurement"])) ? dr.GetInt32("measurement") : new int?();
                                productclass.Createtime = (!Convert.IsDBNull(dr["createtime"])) ? dr.GetInt32("createtime") : new int?();

                                list.Add(productclass);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }

            return list;
        }
    }
}
