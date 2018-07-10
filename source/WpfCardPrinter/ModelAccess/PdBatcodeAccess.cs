using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;

namespace WpfCardPrinter.ModelAccess
{
    class PdBatcodeAccess : BaseAccess<PdBatcode>
    {
        public PdBatcodeAccess() : base()
        {

        }

        public PdBatcodeAccess(PdBatcode model)
            :base(model)
        {

        }

        public long Insert(PdBatcode pdcode)
        {
            long newid = -1;
            try
            {
                using (MySqlCommand mysqlcom = new MySqlCommand("INSERT INTO pdbatcode (batcode,adder,createtime,status,workshopid) VALUES (@batcode,@adder,unix_timestamp(now()),@status,@workshopid)", _connection))
                {
                    mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar);
                    mysqlcom.Parameters["@batcode"].Value = pdcode.Batcode;
                    mysqlcom.Parameters.Add("@adder", MySqlDbType.VarChar);
                    mysqlcom.Parameters["@adder"].Value = pdcode.Adder;
                    mysqlcom.Parameters.Add("@status", MySqlDbType.Int32);
                    mysqlcom.Parameters["@status"].Value = pdcode.Status;
                    mysqlcom.Parameters.Add("@workshopid", MySqlDbType.Int32);
                    mysqlcom.Parameters["@workshopid"].Value = pdcode.Workshopid;

                    if (mysqlcom.ExecuteNonQuery() > 0)
                    {
                        newid = mysqlcom.LastInsertedId;
                    }
                }
            }
            catch (Exception e)
            {

            }

            return newid;
        }

        public PdBatcode SingleByBatcode(string batcode)
        {
            PdBatcode pdbatcode = null;

            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE batcode=@batcode ORDER BY ID DESC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar); ;
                mysqlcom.Parameters["@batcode"].Value = batcode;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdbatcode = new PdBatcode();
                            pdbatcode.Id = int.Parse(dr["id"].ToString());
                            pdbatcode.Batcode = dr["batcode"].ToString();
                            pdbatcode.Createtime = int.Parse(dr["createtime"].ToString());
                            pdbatcode.Status = int.Parse(dr["status"].ToString());
                            pdbatcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                            pdbatcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                            pdbatcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                        }
                    }
                }
            }

            return pdbatcode; 
        }

        public PdBatcode SingleByPrefixCode(string prefixCode)
        {
            PdBatcode pdcode = null;
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE batcode like @prefix ORDER BY ID DESC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@prefix", MySqlDbType.VarChar);
                mysqlcom.Parameters["@prefix"].Value = prefixCode;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            pdcode = new PdBatcode();
                            pdcode.Id = dr.GetInt32("id");
                            pdcode.Batcode = dr.GetString("batcode");
                            pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                            pdcode.Createtime = dr.GetInt32("createtime");
                            pdcode.Status = dr.GetInt32("status");
                            pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                            pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                            pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                        }
                    }
                }
            }

            return pdcode;
        }

        public PdBatcode SinglePrevById(int id, string shopcode)
        {
            PdBatcode pdcode = null;

            //判断下一个批号数据库中有没有
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE id<@currid AND LEFT(batcode,1)=@code ORDER BY ID DESC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@currid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@currid"].Value = id;
                mysqlcom.Parameters.Add("@code", MySqlDbType.VarChar);
                mysqlcom.Parameters["@code"].Value = shopcode;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdcode = new PdBatcode();
                            pdcode.Id = dr.GetInt32("id");
                            pdcode.Batcode = dr.GetString("batcode");
                            pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                            pdcode.Createtime = dr.GetInt32("createtime");
                            pdcode.Status = dr.GetInt32("status");
                            pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                            pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                            pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                        }
                    }
                }
            }

            return pdcode;
        }

        public PdBatcode SingleNextById(int id, string shopcode)
        {
            PdBatcode pdcode = null;

            //判断下一个批号数据库中有没有
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE id>@currid AND LEFT(batcode,1)=@code ORDER BY ID ASC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@currid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@currid"].Value = id;
                mysqlcom.Parameters.Add("@code", MySqlDbType.VarChar);
                mysqlcom.Parameters["@code"].Value = shopcode;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdcode = new PdBatcode();
                            pdcode.Id = dr.GetInt32("id");
                            pdcode.Batcode = dr.GetString("batcode");
                            pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                            pdcode.Createtime = dr.GetInt32("createtime");
                            pdcode.Status = dr.GetInt32("status");
                            pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                            pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                            pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                        }
                    }
                }
            }

            return pdcode;
        }


        public PdBatcode SingleLast(string shopcode)
        {
            PdBatcode pdcode = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM pdbatcode WHERE LEFT(batcode,1)=@code ORDER BY id DESC LIMIT 1", _connection))
                {
                    command.Parameters.Add("@code", MySqlDbType.VarChar);
                    command.Parameters["@code"].Value = shopcode;

                    using (MySqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                pdcode = new PdBatcode();
                                pdcode.Id = dr.GetInt32("id");
                                pdcode.Batcode = dr.GetString("batcode");
                                pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                                pdcode.Createtime = Convert.IsDBNull(dr["createtime"]) ? new int?() : dr.GetInt32("createtime");
                                pdcode.Status = dr.GetInt32("status");
                                pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                                pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                                pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return pdcode;
        }

        public PdBatcode SingleLastById(int shopid)
        {
            PdBatcode pdcode = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM pdbatcode WHERE workshopid=@id ORDER BY id DESC LIMIT 1", _connection))
                {
                    command.Parameters.Add("@id", MySqlDbType.VarChar);
                    command.Parameters["@id"].Value = shopid;

                    using (MySqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                pdcode = new PdBatcode();
                                pdcode.Id = dr.GetInt32("id");
                                pdcode.Batcode = dr.GetString("batcode");
                                pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                                pdcode.Createtime = Convert.IsDBNull(dr["createtime"]) ? new int?() : dr.GetInt32("createtime");
                                pdcode.Status = dr.GetInt32("status");
                                pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                                pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                                pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return pdcode;
        }


        public PdBatcode SinglePrevByIdAndShopId(int id, int shopid)
        {
            PdBatcode pdcode = null;

            //判断下一个批号数据库中有没有
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE id<@currid AND workshopid=@shopid ORDER BY ID DESC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@currid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@currid"].Value = id;
                mysqlcom.Parameters.Add("@shopid", MySqlDbType.Int32);
                mysqlcom.Parameters["@shopid"].Value = shopid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdcode = new PdBatcode();
                            pdcode.Id = dr.GetInt32("id");
                            pdcode.Batcode = dr.GetString("batcode");
                            pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                            pdcode.Createtime = dr.GetInt32("createtime");
                            pdcode.Status = dr.GetInt32("status");
                            pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                            pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                            pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                        }
                    }
                }
            }

            return pdcode;
        }


        public PdBatcode SingleNextByIdAndShopId(int id, int shopid)
        {
            PdBatcode pdcode = null;

            //判断下一个批号数据库中有没有
            using (MySqlCommand mysqlcom = new MySqlCommand("SELECT * FROM pdbatcode WHERE id>@currid AND workshopid=@shopid ORDER BY ID ASC LIMIT 1", _connection))
            {
                mysqlcom.Parameters.Add("@currid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@currid"].Value = id;
                mysqlcom.Parameters.Add("@shopid", MySqlDbType.Int32);
                mysqlcom.Parameters["@shopid"].Value = shopid;

                using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                {
                    //如果有数据就输出
                    if (dr.HasRows)
                    {
                        //逐行读取数据输出
                        if (dr.Read())
                        {
                            pdcode = new PdBatcode();
                            pdcode.Id = dr.GetInt32("id");
                            pdcode.Batcode = dr.GetString("batcode");
                            pdcode.Adder = Convert.IsDBNull(dr["adder"]) ? null : dr.GetString("adder");
                            pdcode.Createtime = dr.GetInt32("createtime");
                            pdcode.Status = dr.GetInt32("status");
                            pdcode.Productrate = Convert.IsDBNull(dr["productrate"]) ? new double?() : dr.GetDouble("productrate");
                            pdcode.Billetpieceweight = Convert.IsDBNull(dr["billetpieceweight"]) ? new double?() : dr.GetDouble("billetpieceweight");
                            pdcode.Billetnumber = Convert.IsDBNull(dr["billetnumber"]) ? new int?() : dr.GetInt32("billetnumber");
                        }
                    }
                }
            }

            return pdcode;
        }

        public int UnLockStatus(string batcode)
        {
            int ret = -1;
            //解锁当前炉号
            using (MySqlCommand mysqlcom = new MySqlCommand("UPDATE pdbatcode SET status=1 WHERE batcode=@batcode", _connection))
            {
                mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar); ;
                mysqlcom.Parameters["@batcode"].Value = batcode;

                ret = mysqlcom.ExecuteNonQuery();
            }

            return ret;
        }

        /// <summary>
        /// 确定批号的实际成材率是否合理
        /// </summary>
        /// <param name="batcode"></param>
        /// <returns></returns>
        public bool CheckBatcodeRate(string batcode)
        {
            //先获取到批号相关的计划信息
            var batcodeModel = this.SingleByBatcode(batcode);
            if (batcodeModel.Productrate > 0)
            {
                double planweight = batcodeModel.Billetpieceweight.Value * batcodeModel.Billetnumber.Value;
                double totalrealweight = 0;

                using (PdProductAccess access = new PdProductAccess())
                {
                    var list = access.GetListByBatcode(batcode);

                    //累加总重量
                    for(int i=0;i<list.Count;i++)
                    {
                        totalrealweight += list[i].Weight.Value;
                    }
                }

                if (totalrealweight >= planweight * batcodeModel.Productrate)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
