using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfQualityCertPrinter.ModelAccess
{
    class MngAdminAccess : BaseAccess<MngAdmin>
    {
        public MngAdminAccess() : base()
        {

        }

        public MngAdminAccess(MngAdmin model)
            :base(model)
        {

        }

        public MngAdmin Single(string username, string password)
        {
            MngAdmin admin = null;

            try
            {
                string sql = "SELECT * FROM mngadmin WHERE (username=@username && password=@password) "
                                + " OR (realname=@username && password=@password) AND InJob=1 ORDER BY id ASC LIMIT 1";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {
                    mysqlcom.Parameters.Add("@username", MySqlDbType.VarChar);
                    mysqlcom.Parameters["@username"].Value = username;
                    mysqlcom.Parameters.Add("@password", MySqlDbType.VarChar);
                    mysqlcom.Parameters["@password"].Value = password;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                admin = new MngAdmin();
                                admin.Id = dr.GetInt32("id");
                                admin.RealName = (!Convert.IsDBNull(dr["realname"])) ? dr.GetString("realname") : null;
                                admin.UserName = dr.GetString("username");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return admin;
        }
    }
}
