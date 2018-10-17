using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfCardPrinter.Model;

namespace WpfCardPrinter.ModelAccess
{
    public class PdWorkshopTeamAdminRelationAccess : BaseAccess<PdWorkshopTeamAdminRelation>
    {
        public PdWorkshopTeamAdminRelationAccess() : base()
        {

        }

        public PdWorkshopTeamAdminRelationAccess(PdWorkshopTeamAdminRelation model)
            : base(model)
        {

        }

        public PdWorkshopTeamAdminRelation Single(int adminId)
        {
            PdWorkshopTeamAdminRelation pdWorkshopTeamAdminRelation = null;
            try
            {
                string sql = "SELECT * FROM pdworkshopteamadminrelation WHERE AdminId=@AdminId";
                using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
                {

                    mysqlcom.Parameters.Add("@AdminId", MySqlDbType.VarChar).Value = adminId;                 
                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            pdWorkshopTeamAdminRelation = new PdWorkshopTeamAdminRelation();

                            if (dr.Read())
                            {
                                pdWorkshopTeamAdminRelation.Id = dr.GetInt32("id");
                                pdWorkshopTeamAdminRelation.AdminId = dr.GetInt32("AdminId");
                                pdWorkshopTeamAdminRelation.WorkShopTeamId = dr.GetInt32("WorkShopTeamId");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return pdWorkshopTeamAdminRelation;
        }
    }
}
