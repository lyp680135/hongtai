using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using WpfCardPrinter.Utils;
using DataLibrary;
using System.Windows;
using System.Data;

namespace WpfCardPrinter.ModelAccess.SqliteAccess
{
   public class ProductSqliteAccess : BaseSqliteAccess<PdProduct>
    {     
       public ProductSqliteAccess() : base()
        {

        }

       public ProductSqliteAccess(PdProduct model)
            :base(model)
        {

        }
        /// <summary>
        /// 创建表格
        /// </summary>
        public static void CreateTable()
        {
            SQLiteConnection cn = new SQLiteConnection("data source=" + sqlite_path);
            cn.SetPassword(sqlite_password);
            cn.Open();
            
            if (cn.State == System.Data.ConnectionState.Open)
            {
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = cn;
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS product ("
                                + "[Id] INTEGER PRIMARY KEY AUTOINCREMENT,"
                                + "[Batcode] VARCHAR(50),"
                                + "[Classid] INTEGER,"
                                + "[Materialid] INTEGER,"
                                + "[Specid] INTEGER,"
                                + "[WorkShift] INTEGER,"
                                + "[Bundlecode] VARCHAR(50),"
                                + "[Lengthtype] INTEGER,"
                                + "[Length] INTEGER,"
                                + "[Meterweight] DOUBLE,"
                                + "[Piececount] INTEGER,"
                                + "[Weight] DOUBLE,"
                                + "[Randomcode] VARCHAR(50),"
                                + "[Createtime] INTEGER,"
                                + "[Adder] INTEGER,"
                                + "[Uploaded] INTEGER"
                                + ")";
                cmd.ExecuteNonQuery();
            }

            cn.Close();
        }

       

        ~ProductSqliteAccess()
        {
            Dispose();
        }

        /// <summary>
        /// 获取离线缓存的产品
        /// </summary>
        /// <returns></returns>
        public List<PdProduct> GetList()
        {
            List<PdProduct> list = null;

            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = "SELECT * FROM product WHERE [Uploaded]=0 ORDER BY [Createtime] ASC";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) 
                {
                    if (reader.HasRows)
                    {
                        list = new List<PdProduct>();

                        while (reader.Read())
                        {
                            var product = new PdProduct();
                            product.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                            product.Batcode = reader.GetString(reader.GetOrdinal("batcode"));
                            product.Classid = reader.GetInt32(reader.GetOrdinal("classid"));
                            product.Materialid = (!Convert.IsDBNull(reader["materialid"])) ? reader.GetInt32(reader.GetOrdinal("materialid")) : new int?();
                            product.Specid = (!Convert.IsDBNull(reader["specid"])) ? reader.GetInt32(reader.GetOrdinal("specid")) : new int?();
                            product.Lengthtype = reader.GetInt32(reader.GetOrdinal("lengthtype"));
                            product.Length = (!Convert.IsDBNull(reader["length"])) ? reader.GetDouble(reader.GetOrdinal("length")) : new double?();
                            product.Bundlecode = reader.GetString(reader.GetOrdinal("bundlecode"));
                            product.Piececount = (!Convert.IsDBNull(reader["piececount"])) ? reader.GetInt32(reader.GetOrdinal("piececount")) : new int?();
                            product.Weight = (!Convert.IsDBNull(reader["weight"])) ? reader.GetDouble(reader.GetOrdinal("weight")) : new double?();
                            product.Meterweight = (!Convert.IsDBNull(reader["meterweight"])) ? reader.GetDouble(reader.GetOrdinal("meterweight")) : new double?();
                            product.Createtime = reader.GetInt32(reader.GetOrdinal("createtime"));
                            product.WorkShift = reader.GetInt32(reader.GetOrdinal("workshift"));
                            product.Randomcode = (!Convert.IsDBNull(reader["randomcode"])) ? reader.GetString(reader.GetOrdinal("randomcode")) : null;
                            product.Adder = (!Convert.IsDBNull(reader["Adder"])) ? reader.GetInt32(reader.GetOrdinal("Adder")) : 0;

                            list.Add(product);
                        }
                    }
                }
            }

            return list;
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

            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO product (batcode,classid,materialid,specid,lengthtype,"
                        + "length,bundlecode,piececount,meterweight,weight,createtime,adder,workshift,randomcode,uploaded)"
                        + " VALUES (@batcode,@classid,@materialid,@specid,"
                        + "@lengthtype,@length,@bundlecode,@piececount,"
                        + "@meterweight,@weight,strftime('%s','now'),@adder,@workshift,@randomcode"
                        + ",0);select last_insert_rowid()", _connection))
            {
                cmd.Parameters.Add("@batcode", DbType.String, 50);
                cmd.Parameters["@batcode"].Value = product.Batcode;
                cmd.Parameters.Add("@classid", DbType.Int32); ;
                cmd.Parameters["@classid"].Value = product.Classid;
                cmd.Parameters.Add("@materialid", DbType.Int32);
                cmd.Parameters["@materialid"].Value = product.Materialid;
                cmd.Parameters.Add("@specid", DbType.Int32);
                cmd.Parameters["@specid"].Value = product.Specid;
                cmd.Parameters.Add("@lengthtype", DbType.Int32);
                cmd.Parameters["@lengthtype"].Value = product.Lengthtype;
                cmd.Parameters.Add("@length", DbType.Double);
                cmd.Parameters["@length"].Value = product.Length;
                cmd.Parameters.Add("@bundlecode", DbType.String, 50);
                cmd.Parameters["@bundlecode"].Value = product.Bundlecode;
                cmd.Parameters.Add("@piececount", DbType.Int32);
                cmd.Parameters["@piececount"].Value = product.Piececount;
                cmd.Parameters.Add("@meterweight", DbType.Double);
                cmd.Parameters["@meterweight"].Value = product.Meterweight;
                cmd.Parameters.Add("@weight", DbType.Double);
                cmd.Parameters["@weight"].Value = product.Weight;
                cmd.Parameters.Add("@adder", DbType.Int32);
                cmd.Parameters["@adder"].Value = product.Adder;
                cmd.Parameters.Add("@workshift", DbType.Int32);
                cmd.Parameters["@workshift"].Value = product.WorkShift;
                cmd.Parameters.Add("@randomcode", DbType.String, 50);
                cmd.Parameters["@randomcode"].Value = product.Randomcode;

                product.Id =  Convert.ToInt32(cmd.ExecuteScalar());

                newid = product.Id;
            }

            return newid;
        }

        public int UpdateUploaded(int id)
        {
            int ret = 0;

            if (id > 0)
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = _connection;
                    cmd.CommandText = "UPDATE product SET Uploaded=1 WHERE [Id]=@id";

                    cmd.Parameters.Add("@id", DbType.Int32); ;
                    cmd.Parameters["@id"].Value = id;

                    ret = cmd.ExecuteNonQuery();
                }
            }

            return ret;
        }
    }
}
