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

        public PdWorkshopTeamLog SingleLastByWorkshopidTeamid(int workshopid, int teamid, int batcodeid)
        {
            PdWorkshopTeamLog log = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM pdworkshopteamlog WHERE workshopid=@workshopid AND teamid=@teamid AND batcodeid<=@batcodeid ORDER BY id DESC LIMIT 1", _connection))
                {
                    command.Parameters.Add("@workshopid", MySqlDbType.Int32);
                    command.Parameters["@workshopid"].Value = workshopid;
                    command.Parameters.Add("@teamid", MySqlDbType.Int32);
                    command.Parameters["@teamid"].Value = teamid;
                    command.Parameters.Add("@batcodeid", MySqlDbType.Int32);
                    command.Parameters["@batcodeid"].Value = batcodeid;

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
                                log.BatcodeId = dr.GetInt32("batcodeid");
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

        public PdWorkshopTeamLog SingleLastByWorkshopid(int workshopid)
        {
            PdWorkshopTeamLog log = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM pdworkshopteamlog WHERE workshopid=@workshopid ORDER BY id DESC LIMIT 1", _connection))
                {
                    command.Parameters.Add("@workshopid", MySqlDbType.Int32);
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
                                log.BatcodeId = dr.GetInt32("batcodeid");
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
            using (MySqlCommand command = new MySqlCommand("INSERT INTO pdworkshopteamlog (id,workshopid,teamid,batcode,batcodeid,createtime,adder) "
                + " VALUES (null,@workshopid,@teamid,@batcode,@batcodeid,unix_timestamp(now()),@adder)", _connection))
            {
                command.Parameters.Add("@workshopid", MySqlDbType.VarChar); ;
                command.Parameters["@workshopid"].Value = log.WorkshopId;
                command.Parameters.Add("@teamid", MySqlDbType.Int32); ;
                command.Parameters["@teamid"].Value = log.TeamId;
                command.Parameters.Add("@batcode", MySqlDbType.VarChar);
                command.Parameters["@batcode"].Value = log.Batcode;
                command.Parameters.Add("@batcodeid", MySqlDbType.Int32); ;
                command.Parameters["@batcodeid"].Value = log.BatcodeId;
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

        public int CountCurrTeamProducted(PdWorkshopTeam team, int batcodeid, out double totalweight)
        {
            int count = 0;
            totalweight = 0;

            try
            {
                var lastlog = SingleLastByWorkshopidTeamid(team.WorkshopId,team.Id,batcodeid);
                if (lastlog != null)
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT COUNT(0) as nums,SUM(weight) as weight FROM pdproduct WHERE workshift=@workshift AND createtime>=@createtime", _connection))
                    {
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
                                    count = dr.GetInt32("nums");
                                    totalweight = Convert.IsDBNull(dr["weight"]) ? 0 : dr.GetInt32("weight");
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
