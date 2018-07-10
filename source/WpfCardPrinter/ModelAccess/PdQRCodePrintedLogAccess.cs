using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfCardPrinter.ModelAccess
{
    class PdQRCodePrintedLogAccess : BaseAccess<PdQRCodePrintedLog>
    {
        public PdQRCodePrintedLogAccess() : base()
        {

        }

        public PdQRCodePrintedLogAccess(PdQRCodePrintedLog model)
            :base(model)
        {

        }

        public int GetCanPrintNumber(int workshopid, int specid)
        {
            int leavenum = 0;
            int authnumber = 0;

            //获取当月二维码授权打印数量
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdqrcodeauth WHERE workshopid=@workshopid AND specid=@specid AND date_format(FROM_UNIXTIME(AuthDate),'%Y%m01')=date_format(now(),'%Y%m01')", _connection))
            {
                mysqlcom.Parameters.Add("@workshopid", MySqlDbType.Int32);
                mysqlcom.Parameters["@workshopid"].Value = workshopid;
                mysqlcom.Parameters.Add("@specid", MySqlDbType.Int32);
                mysqlcom.Parameters["@specid"].Value = specid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            PdQRCodeAuth auth = new PdQRCodeAuth();
                            auth.Number = dr.GetInt32("number");

                            authnumber = auth.Number;
                        }
                    }
                }
            }

            if (authnumber > 0)
            {
                using (MySqlCommand mysqlcom = new MySqlCommand("SELECT COUNT(0) FROM pdqrcodeprintedlog WHERE workshopid=@workshopid AND specid=@specid AND date_format(FROM_UNIXTIME(createtime),'%Y%m01')=date_format(now(),'%Y%m01')", _connection))
                {
                    mysqlcom.Parameters.Add("@workshopid", MySqlDbType.Int32);
                    mysqlcom.Parameters["@workshopid"].Value = workshopid;
                    mysqlcom.Parameters.Add("@specid", MySqlDbType.Int32);
                    mysqlcom.Parameters["@specid"].Value = specid;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                leavenum = authnumber - dr.GetInt32(0);
                            }
                        }
                    }
                }
            }

            return leavenum;
        }

        public PdQRCodePrintedLog SingleByProductid(int productid)
        {
            PdQRCodePrintedLog log = null;

            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdqrcodeprintedlog WHERE productid=@productid LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@productid", MySqlDbType.Int32);
                mysqlcom.Parameters["@productid"].Value = productid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            log = new PdQRCodePrintedLog();
                            log.WorkshopId = dr.GetInt32("workshopid");
                            log.SpecId = dr.GetInt32("specid");
                            log.ProductId = dr.GetInt32("productid");
                            log.Number = dr.GetInt32("number");
                            log.Status = dr.GetInt32("status");
                            log.Adder = dr.GetInt32("adder");
                            log.Createtime = dr.GetInt32("createtime");
                        }
                    }
                }
            }

            return log;
        }

        public long Insert(PdQRCodePrintedLog log)
        {
            long newid = -1;
            using (MySqlCommand mysqlcom = new MySqlCommand("INSERT INTO pdqrcodeprintedlog (workshopid,specid,productid,number,status,adder,createtime)"
                +" VALUES (@workshopid,@specid,@productid,@number,@status,@adder,unix_timestamp(now()))", _connection))
            {
                mysqlcom.Parameters.Add("@workshopid", MySqlDbType.Int32);
                mysqlcom.Parameters["@workshopid"].Value = log.WorkshopId;
                mysqlcom.Parameters.Add("@specid", MySqlDbType.Int32);
                mysqlcom.Parameters["@specid"].Value = log.SpecId;
                mysqlcom.Parameters.Add("@productid", MySqlDbType.Int32);
                mysqlcom.Parameters["@productid"].Value = log.ProductId;
                mysqlcom.Parameters.Add("@number", MySqlDbType.Int32);
                mysqlcom.Parameters["@number"].Value = log.Number;
                mysqlcom.Parameters.Add("@status", MySqlDbType.Int32);
                mysqlcom.Parameters["@status"].Value = log.Status;
                mysqlcom.Parameters.Add("@adder", MySqlDbType.Int32);
                mysqlcom.Parameters["@adder"].Value = log.Adder;

                if (mysqlcom.ExecuteNonQuery() > 0)
                {
                    newid = mysqlcom.LastInsertedId;
                }
            }

            return newid;
        }


        public int Update(PdQRCodePrintedLog log)
        {
            int result = -1;
            using (MySqlCommand mysqlcom = new MySqlCommand("UPDATE pdqrcodeprintedlog SET number=@number WHERE id=@id", _connection))
            {
                mysqlcom.Parameters.Add("@number", MySqlDbType.Int32);
                mysqlcom.Parameters["@number"].Value = log.Number;
                mysqlcom.Parameters.Add("@id", MySqlDbType.Int32);
                mysqlcom.Parameters["@id"].Value = log.Id;

                result = mysqlcom.ExecuteNonQuery();
            }

            return result;
        }

    }
}
