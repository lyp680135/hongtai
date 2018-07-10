using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;
using MySql.Data.MySqlClient;
namespace WpfCardPrinter.ModelAccess
{
    class PdWorkshopTeamLogAccess :BaseAccess<PdWorkshopTeamLog>
    {
        public PdWorkshopTeamLogAccess() : base()
        {

        }

        public PdWorkshopTeamLogAccess(PdWorkshopTeamLog model)
            :base(model)
        {

        }

        public PdWorkshopTeamLog SingleLastByWorkshopid(int workshopid)
        {
            PdWorkshopTeamLog log = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM pdworkshopteamlog WHERE workshopid=@workshopid ORDER BY id DESC LIMIT 1", _connection))
                {
                    command.Parameters.Add("@workshopid", MySqlDbType.VarChar);
                    command.Parameters["@workshopid"].Value = workshopid;

                    using (MySqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                log = new PdWorkshopTeamLog();
                                log.Id = dr.GetInt32("id");
                                log.WorkshopId = dr.GetInt32("workshopid");
                                log.Batcode = dr.GetString("batcode");
                                log.TeamId = dr.GetInt32("teamid");
                                log.CreateTime = dr.GetInt32("createtime");
                                log.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return log;
        }

        public long Insert(PdWorkshopTeamLog log)
        {
            using (MySqlCommand command = new MySqlCommand("INSERT INTO pdworkshopteamlog (id,workshopid,teamid,batcode,createtime,adder) "
                +" VALUES (null,@workshopid,@teamid,@batcode,@createtime,@adder)", _connection))
            {
                command.Parameters.Add("@workshopid", MySqlDbType.VarChar); ;
                command.Parameters["@workshopid"].Value = log.WorkshopId;
                command.Parameters.Add("@teamid", MySqlDbType.Int32); ;
                command.Parameters["@teamid"].Value = log.TeamId;
                command.Parameters.Add("@batcode", MySqlDbType.VarChar);
                command.Parameters["@batcode"].Value = log.Batcode;
                command.Parameters.Add("@createtime", MySqlDbType.Int32);
                command.Parameters["@createtime"].Value = log.CreateTime;
                command.Parameters.Add("@adder", MySqlDbType.VarChar);
                command.Parameters["@adder"].Value = log.Adder;

                command.ExecuteNonQuery();
                if (command.LastInsertedId > 0)
                {
                    return command.LastInsertedId;
                }
            }

            return -1;
        }

        public int CountCurrTeamProducted(PdWorkshopTeam team,string batcode)
        {
            int count = 0;
            try
            {
                var lastlog = SingleLastByWorkshopid(team.WorkshopId);
                if (lastlog != null)
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT COUNT(0) FROM pdproduct WHERE batcode=@batcode AND workshift=@workshift AND createtime>@createtime", _connection))
                    {
                        command.Parameters.Add("@batcode", MySqlDbType.VarChar);
                        command.Parameters["@batcode"].Value = batcode;
                        command.Parameters.Add("@workshift", MySqlDbType.VarChar);
                        command.Parameters["@workshift"].Value = lastlog.TeamId;
                        command.Parameters.Add("@createtime", MySqlDbType.VarChar);
                        command.Parameters["@createtime"].Value = lastlog.CreateTime;

                        using (MySqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                if (dr.Read())
                                {
                                    count = dr.GetInt32(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return count;
        }
    }
}
