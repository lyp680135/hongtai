using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLibrary;
using System.Windows;

namespace WpfCardPrinter.ModelAccess
{
    class PdProductAccess :BaseAccess<PdProduct>
    {
        public PdProductAccess() : base()
        {

        }

        public PdProductAccess(PdProduct model)
            :base(model)
        {

        }

        public long Insert(PdProduct product)
        {
            long newid = -1;

            //防止异常数据保存
            if (product.Classid == null)
            {
                MessageBox.Show("产品品名没有设置，不能保存", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return newid;
            }
            if (product.Materialid == null)
            {
                MessageBox.Show("产品材质没有设置，不能保存", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return newid;
            }
            if (product.WorkShift == null)
            {
                MessageBox.Show("班组信息异常，不能保存", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return newid;
            }
            if (product.Specid == null)
            {
                MessageBox.Show("产品规格没有设置，不能保存", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return newid;
            }

            if (string.IsNullOrEmpty(product.Batcode) 
                || string.IsNullOrEmpty(product.Bundlecode) 
                || product.Classid <= 0
                || product.Materialid <= 0
                || product.Specid <= 0
                || product.WorkShift <= 0)
            {
                return newid;
            }

            string sql = "INSERT INTO pdproduct (batcode,classid,materialid,specid,lengthtype,"
                        + "length,bundlecode,piececount,meterweight,weight,createtime,adder,workshift,randomcode)"
                        + " VALUES (@batcode,@classid,@materialid,@specid,"
                        + "@lengthtype,@length,@bundlecode,@piececount,"
                        + "@meterweight,@weight,unix_timestamp(now()),@adder,@workshift,@randomcode"
                        + ")";

            if (product.Createtime > 0)
            {
                sql = "INSERT INTO pdproduct (batcode,classid,materialid,specid,lengthtype,"
                        + "length,bundlecode,piececount,meterweight,weight,createtime,adder,workshift,randomcode)"
                        + " VALUES (@batcode,@classid,@materialid,@specid,"
                        + "@lengthtype,@length,@bundlecode,@piececount,"
                        + "@meterweight,@weight,@createtime,@adder,@workshift,@randomcode"
                        + ")";
            }

            using (MySqlCommand mysqlcom = new MySqlCommand(sql, _connection))
            {
                mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar);
                mysqlcom.Parameters["@batcode"].Value = product.Batcode;
                mysqlcom.Parameters.Add("@classid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@classid"].Value = product.Classid;
                mysqlcom.Parameters.Add("@materialid", MySqlDbType.Int32);
                mysqlcom.Parameters["@materialid"].Value = product.Materialid;
                mysqlcom.Parameters.Add("@specid", MySqlDbType.Int32);
                mysqlcom.Parameters["@specid"].Value = product.Specid;
                mysqlcom.Parameters.Add("@lengthtype", MySqlDbType.Int32);
                mysqlcom.Parameters["@lengthtype"].Value = product.Lengthtype;
                mysqlcom.Parameters.Add("@length", MySqlDbType.Double);
                mysqlcom.Parameters["@length"].Value = product.Length;
                mysqlcom.Parameters.Add("@bundlecode", MySqlDbType.VarChar);
                mysqlcom.Parameters["@bundlecode"].Value = product.Bundlecode;
                mysqlcom.Parameters.Add("@piececount", MySqlDbType.Int32);
                mysqlcom.Parameters["@piececount"].Value = product.Piececount;
                mysqlcom.Parameters.Add("@meterweight", MySqlDbType.Double);
                mysqlcom.Parameters["@meterweight"].Value = product.Meterweight;
                mysqlcom.Parameters.Add("@weight", MySqlDbType.Double);
                mysqlcom.Parameters["@weight"].Value = product.Weight;
                mysqlcom.Parameters.Add("@adder", MySqlDbType.Int32);
                mysqlcom.Parameters["@adder"].Value = product.Adder;
                mysqlcom.Parameters.Add("@workshift", MySqlDbType.Int32);
                mysqlcom.Parameters["@workshift"].Value = product.WorkShift;
                mysqlcom.Parameters.Add("@randomcode", MySqlDbType.VarChar);
                mysqlcom.Parameters["@randomcode"].Value = product.Randomcode;

                if (product.Createtime > 0)
                {
                    mysqlcom.Parameters.Add("@createtime", MySqlDbType.Int32);
                    mysqlcom.Parameters["@createtime"].Value = product.Createtime;
                }

                mysqlcom.ExecuteNonQuery();

                product.Id = (int)mysqlcom.LastInsertedId;

                newid = product.Id;
            }

            return newid;
        }

        public int Update(PdProduct product)
        {
            using (MySqlCommand mysqlcom = new MySqlCommand("UPDATE pdproduct SET classid=@classid,materialid=@materialid,"
                        + "specid=@specid,lengthtype=@lengthtype,length=@length,piececount=@piececount,"
                        + "meterweight=@meterweight,weight=@weight,adder=@adder,workshift=@workshift"
                        + " WHERE id=@id", _connection))
            {
                mysqlcom.Parameters.Add("@classid", MySqlDbType.Int32); ;
                mysqlcom.Parameters["@classid"].Value = product.Classid;
                mysqlcom.Parameters.Add("@materialid", MySqlDbType.Int32);
                mysqlcom.Parameters["@materialid"].Value = product.Materialid;
                mysqlcom.Parameters.Add("@specid", MySqlDbType.Int32);
                mysqlcom.Parameters["@specid"].Value = product.Specid;
                mysqlcom.Parameters.Add("@lengthtype", MySqlDbType.Int32);
                mysqlcom.Parameters["@lengthtype"].Value = product.Lengthtype;
                mysqlcom.Parameters.Add("@length", MySqlDbType.Double);
                mysqlcom.Parameters["@length"].Value = product.Length;
                mysqlcom.Parameters.Add("@piececount", MySqlDbType.Int32);
                mysqlcom.Parameters["@piececount"].Value = product.Piececount;
                mysqlcom.Parameters.Add("@meterweight", MySqlDbType.Double);
                mysqlcom.Parameters["@meterweight"].Value = product.Meterweight;
                mysqlcom.Parameters.Add("@weight", MySqlDbType.Double);
                mysqlcom.Parameters["@weight"].Value = product.Weight;
                mysqlcom.Parameters.Add("@adder", MySqlDbType.Int32);
                mysqlcom.Parameters["@adder"].Value = product.Adder;
                mysqlcom.Parameters.Add("@workshift", MySqlDbType.Int32);
                mysqlcom.Parameters["@workshift"].Value = product.WorkShift;

                mysqlcom.Parameters.Add("@id", MySqlDbType.Int32);
                mysqlcom.Parameters["@id"].Value = product.Id;

                return mysqlcom.ExecuteNonQuery();
            }
        }

        public List<PdProduct> GetListByBatcode(string batcode)
        {
            List<PdProduct> productlist = null;

            try
            {
                using (MySqlCommand mysqlcom = new MySqlCommand("SELECT C.*,D.specname FROM (SELECT A.*,B.TeamName FROM ("
                        + "SELECT * FROM pdproduct WHERE batcode=@batcode"
                        + ") AS A LEFT JOIN pdworkshopteam AS B ON A.workshift = B.id) AS C LEFT JOIN basespecifications AS D"
                        + " ON C.specid=D.id ORDER BY bundlecode ASC", _connection))
                {
                    mysqlcom.Parameters.Add("@batcode", MySqlDbType.VarChar); ;
                    mysqlcom.Parameters["@batcode"].Value = batcode;

                    using (MySqlDataReader dr = mysqlcom.ExecuteReader())
                    {
                        //如果有数据就输出
                        if (dr.HasRows)
                        {
                            productlist = new List<PdProduct>();

                            //逐行读取数据输出
                            while (dr.Read())
                            {
                                var obj = new PdProduct();
                                obj.Id = dr.GetInt32("id");
                                obj.Batcode = dr.GetString("batcode");
                                obj.Classid = dr.GetInt32("classid");
                                obj.Materialid = (!Convert.IsDBNull(dr["materialid"])) ? dr.GetInt32("materialid") : new int?();
                                obj.Specid = (!Convert.IsDBNull(dr["specid"])) ? dr.GetInt32("specid") : new int?();
                                obj.Lengthtype = dr.GetInt32("lengthtype");
                                obj.Length = (!Convert.IsDBNull(dr["length"])) ? dr.GetDouble("length") : new double?();
                                obj.Bundlecode = dr.GetString("bundlecode");
                                obj.Piececount = (!Convert.IsDBNull(dr["piececount"])) ? dr.GetInt32("piececount") : new int?();
                                obj.Weight = (!Convert.IsDBNull(dr["weight"])) ? dr.GetDouble("weight") : new double?();
                                obj.Meterweight = (!Convert.IsDBNull(dr["meterweight"])) ? dr.GetDouble("meterweight") : new double?();
                                obj.Createtime = dr.GetInt32("createtime");
                                obj.WorkShift = dr.GetInt32("workshift");
                                obj.Shiftname = dr.GetString("teamname");
                                obj.Randomcode = (!Convert.IsDBNull(dr["randomcode"])) ? dr.GetString("randomcode") : null;

                                int code = 0;
                                if (obj.Lengthtype == (int)EnumList.ProductQualityLevel.定尺)
                                {
                                    int.TryParse(obj.Bundlecode, out code);
                                }
                                else
                                {
                                    int.TryParse(obj.Bundlecode.Replace("F", ""), out code);
                                    code = -(5 - code);
                                }
                                obj.BundlecodeValue = code;

                                obj.Specname = (!Convert.IsDBNull(dr["specname"])) ? dr.GetString("specname") : null;

                                productlist.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return productlist;
        }

        public PdProduct SingleByBatcodeAndBundle(string batcode, string bundlecode)
        {
            PdProduct product = null;

            using (MySqlCommand command = new MySqlCommand("SELECT A.*,B.TeamName FROM ("
                + "SELECT * FROM pdproduct WHERE batcode=@batcode AND bundlecode=@bundlecode"
                + ") AS A LEFT JOIN pdworkshopteam AS B ON A.workshift = B.id", _connection))
            {
                command.Parameters.Add("@batcode", MySqlDbType.VarChar);
                command.Parameters["@batcode"].Value = batcode;
                command.Parameters.Add("@bundlecode", MySqlDbType.VarChar);
                command.Parameters["@bundlecode"].Value = bundlecode;

                using (MySqlDataReader dr = command.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            product = new PdProduct();
                            product.Id = dr.GetInt32("id");
                            product.Batcode = dr.GetString("batcode");
                            product.Classid = dr.GetInt32("classid");
                            product.Materialid = (!Convert.IsDBNull(dr["materialid"])) ? dr.GetInt32("materialid") : new int?();
                            product.Specid = (!Convert.IsDBNull(dr["specid"])) ? dr.GetInt32("specid") : new int?();
                            product.Lengthtype = dr.GetInt32("lengthtype");
                            product.Length = (!Convert.IsDBNull(dr["length"])) ? dr.GetDouble("length") : new double?();
                            product.Bundlecode = dr.GetString("bundlecode");
                            product.Piececount = (!Convert.IsDBNull(dr["piececount"])) ? dr.GetInt32("piececount") : new int?();
                            product.Weight = (!Convert.IsDBNull(dr["weight"])) ? dr.GetDouble("weight") : new double?();
                            product.Meterweight = (!Convert.IsDBNull(dr["meterweight"])) ? dr.GetDouble("meterweight") : new double?();
                            product.WorkShift = dr.GetInt32("workshift");
                            product.Shiftname = dr.GetString("teamname");
                            product.Createtime = dr.GetInt32("createtime");
                        }
                    }
                }
            }

            return product;
        }

        public PdProduct SingleLastProductByWorkshopid(int workshopid, string batcode)
        {
            PdProduct product = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT A.*,B.TeamName FROM ("
                    + "SELECT * FROM pdproduct WHERE batcode=@batcode AND workshift in (SELECT id FROM pdworkshopteam WHERE workshopid=@workshopid) ORDER BY id DESC LIMIT 1)"
                    + " AS A LEFT JOIN pdworkshopteam AS B ON A.workshift = B.id", _connection))
                {
                    command.Parameters.Add("@batcode", MySqlDbType.VarChar);
                    command.Parameters["@batcode"].Value = batcode;
                    command.Parameters.Add("@workshopid", MySqlDbType.Int32);
                    command.Parameters["@workshopid"].Value = workshopid;

                    using (MySqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                product = new PdProduct();
                                product.Id = dr.GetInt32("id");
                                product.Batcode = dr.GetString("batcode");
                                product.Classid = dr.GetInt32("classid");
                                product.Materialid = (!Convert.IsDBNull(dr["materialid"])) ? dr.GetInt32("materialid") : new int?();
                                product.Specid = (!Convert.IsDBNull(dr["specid"])) ? dr.GetInt32("specid") : new int?();
                                product.Lengthtype = dr.GetInt32("lengthtype");
                                product.Length = (!Convert.IsDBNull(dr["length"])) ? dr.GetDouble("length") : new double?();
                                product.Bundlecode = dr.GetString("bundlecode");
                                product.Piececount = (!Convert.IsDBNull(dr["piececount"])) ? dr.GetInt32("piececount") : new int?();
                                product.Weight = (!Convert.IsDBNull(dr["weight"])) ? dr.GetDouble("weight") : new double?();
                                product.Meterweight = (!Convert.IsDBNull(dr["meterweight"])) ? dr.GetDouble("meterweight") : new double?();
                                product.WorkShift = dr.GetInt32("workshift");
                                product.Shiftname = dr.GetString("teamname");
                                product.Createtime = dr.GetInt32("createtime");
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }

            return product;
        }

        /// <summary>
        /// 获取当前这个车间生产的最后一件产品
        /// </summary>
        /// <param name="workshopid"></param>
        /// <returns></returns>
        public PdProduct SingleLastProductByWorkshopid(int workshopid)
        {
            PdProduct product = null;

            try
            {
                using (MySqlCommand command = new MySqlCommand("SELECT A.*,B.TeamName FROM ("
                    + "SELECT * FROM pdproduct WHERE workshift in (SELECT id FROM pdworkshopteam WHERE workshopid=@workshopid) ORDER BY id DESC LIMIT 1)"
                    + " AS A LEFT JOIN pdworkshopteam AS B ON A.workshift = B.id", _connection))
                {
                    command.Parameters.Add("@workshopid", MySqlDbType.Int32);
                    command.Parameters["@workshopid"].Value = workshopid;

                    using (MySqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            if (dr.Read())
                            {
                                product = new PdProduct();
                                product.Id = dr.GetInt32("id");
                                product.Batcode = dr.GetString("batcode");
                                product.Classid = dr.GetInt32("classid");
                                product.Materialid = (!Convert.IsDBNull(dr["materialid"])) ? dr.GetInt32("materialid") : new int?();
                                product.Specid = (!Convert.IsDBNull(dr["specid"])) ? dr.GetInt32("specid") : new int?();
                                product.Lengthtype = dr.GetInt32("lengthtype");
                                product.Length = (!Convert.IsDBNull(dr["length"])) ? dr.GetDouble("length") : new double?();
                                product.Bundlecode = dr.GetString("bundlecode");
                                product.Piececount = (!Convert.IsDBNull(dr["piececount"])) ? dr.GetInt32("piececount") : new int?();
                                product.Weight = (!Convert.IsDBNull(dr["weight"])) ? dr.GetDouble("weight") : new double?();
                                product.Meterweight = (!Convert.IsDBNull(dr["meterweight"])) ? dr.GetDouble("meterweight") : new double?();
                                product.WorkShift = dr.GetInt32("workshift");
                                product.Shiftname = dr.GetString("teamname");
                                product.Createtime = dr.GetInt32("createtime");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return product;
        }

        public List<PdProduct> Query(string batcode, DateTime? createdate)
        {
            List<PdProduct> productlist = null;

            int time = 0, nexttime = 0;
            StringBuilder sb=new StringBuilder();


            if (string.IsNullOrEmpty(batcode) && createdate == null)
            {
                return null;
            }

            sb.Append("SELECT A.*,B.TeamName FROM (SELECT * FROM pdproduct WHERE 1=1");

            if(!string.IsNullOrEmpty(batcode))
            {
                sb.Append(" AND batcode=@batcode");
            }

            if (createdate != null)
            {
                var nextdate = createdate.Value.AddDays(1);

                time = (int)Utils.TimeUtils.GetUnixTimeFromDateTime(createdate.Value);
                nexttime = (int)Utils.TimeUtils.GetUnixTimeFromDateTime(nextdate);

                sb.Append(" AND createtime>@time AND createtime<@nexttime");
            }

            sb.Append(") AS A LEFT JOIN pdworkshopteam AS B ON A.workshift = B.id ORDER BY batcode ASC,bundlecode ASC");

            using (MySqlCommand command = new MySqlCommand(sb.ToString(), _connection))
            {
                if (!string.IsNullOrEmpty(batcode))
                {
                    command.Parameters.Add("@batcode", MySqlDbType.VarChar);
                    command.Parameters["@batcode"].Value = batcode;
                }

                if (createdate != null)
                {
                    command.Parameters.Add("@time", MySqlDbType.Int32);
                    command.Parameters["@time"].Value = time;
                    command.Parameters.Add("@nexttime", MySqlDbType.Int32);
                    command.Parameters["@nexttime"].Value = nexttime;
                }

                using (MySqlDataReader dr = command.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        productlist = new List<PdProduct>();

                        while (dr.Read())
                        {
                            var product = new PdProduct();
                            product.Id = dr.GetInt32("id");
                            product.Batcode = dr.GetString("batcode");
                            product.Classid = dr.GetInt32("classid");
                            product.Materialid = (!Convert.IsDBNull(dr["materialid"])) ? dr.GetInt32("materialid") : new int?();
                            product.Specid = (!Convert.IsDBNull(dr["specid"])) ? dr.GetInt32("specid") : new int?();
                            product.Lengthtype = dr.GetInt32("lengthtype");
                            product.Length = (!Convert.IsDBNull(dr["length"])) ? dr.GetDouble("length") : new double?();
                            product.Bundlecode = dr.GetString("bundlecode");
                            product.Piececount = (!Convert.IsDBNull(dr["piececount"])) ? dr.GetInt32("piececount") : new int?();
                            product.Weight = (!Convert.IsDBNull(dr["weight"])) ? dr.GetDouble("weight") : new double?();
                            product.Meterweight = (!Convert.IsDBNull(dr["meterweight"])) ? dr.GetDouble("meterweight") : new double?();
                            product.WorkShift = dr.GetInt32("workshift");
                            product.Shiftname = dr.GetString("teamname");
                            product.Createtime = dr.GetInt32("createtime");

                            productlist.Add(product);
                        }
                    }
                }
            }

            return productlist;
        }

        public void DeleteProduct(string batcode)
        {
            string sqlStr = @" delete from pdproduct where batcode=@batcode ";
            using (MySqlCommand sqlcmd = new MySqlCommand(sqlStr, _connection))
            {
                sqlcmd.Parameters.Add("@batcode", MySqlDbType.VarChar).Value = batcode;
                sqlcmd.ExecuteNonQuery();
            }
        }

        public void DeleteProductById(int Id)
        {
            string sqlStr = @" delete from pdproduct where Id=@Id ";
            using (MySqlCommand sqlcmd = new MySqlCommand(sqlStr, _connection))
            {
                sqlcmd.Parameters.Add("@Id", MySqlDbType.Int32).Value = Id;
                sqlcmd.ExecuteNonQuery();
            }
        }
    }
}
