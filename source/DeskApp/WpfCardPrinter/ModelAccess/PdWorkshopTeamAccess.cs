using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfCardPrinter.ModelAccess
{
    class PdWorkshopTeamAccess :BaseAccess<PdWorkshopTeam>
    {
        public PdWorkshopTeamAccess() : base()
        {

        }

        public PdWorkshopTeamAccess(PdWorkshopTeam model)
            :base(model)
        {

        }

        public List<PdWorkshopTeam> GetListByWorkshop(string shopcode)
        {
            List<PdWorkshopTeam> list = null;

            try
            {
                using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdworkshopteam WHERE workshopid=(SELECT id FROM pdworkshop WHERE code=@code)", _connection))
                {
                	mysqlcom.Parameters.Add("@code", MySqlDbType.VarChar); 
                    mysqlcom.Parameters["@code"].Value = shopcode;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            list = new List<PdWorkshopTeam>();

                            while (dr.Read())
                            {
                                var team = new PdWorkshopTeam();
                                team.Id = dr.GetInt32("id");
                                team.WorkshopId = dr.GetInt32("workshopid");
                                team.TeamName = dr.GetString("teamname");
                                team.Leader = (!Convert.IsDBNull(dr["leader"])) ? dr.GetString("leader") : null;
                                team.Adder = (!Convert.IsDBNull(dr["adder"])) ? dr.GetString("adder") : null;
                                team.CreateTime = (!Convert.IsDBNull(dr["createtime"])) ? dr.GetInt32("createtime") : new int?();

                                list.Add(team);
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

        public bool CheckTeamChangeable(string batcode, PdWorkshopTeam team)
        {
            bool enable = false;

            if (string.IsNullOrEmpty(batcode))
            {
                return false;
            }

            using (PdBatcodeAccess access = new PdBatcodeAccess())
            {
                string code = batcode.Substring(0, 1);
                var pdcode = access.SingleLast(code);
                if (pdcode != null)
                {
                    if(pdcode.Batcode == batcode)
                    {
                        enable = true;
                    }
                }
            }

            return enable;
        }
    }
}
