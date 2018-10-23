using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using WpfCardPrinterManual.Model;

namespace WpfCardPrinterManual.ModelAccess.SqliteAccess
{
    public class PdProductAccess : BaseSqliteAccess<PdProduct>
    {
        /// <summary>
        /// 初始建表
        /// </summary>
        public PdProductAccess()
            : base()
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
                                + "[GBStandard] VARCHAR(50),"
                                + "[Batcode] VARCHAR(50),"
                                + "[ClassName] VARCHAR(50),"
                                + "[MaterialName] VARCHAR(50),"
                                + "[SpecName] VARCHAR(50),"
                                + "[Bundlecode] VARCHAR(50),"
                                + "[Length] INTEGER,"
                                + "[Meterweight] DOUBLE,"
                                + "[Piececount] INTEGER,"
                                + "[Weight] DOUBLE,"
                                + "[Randomcode] VARCHAR(50),"
                                + "[Createtime] INTEGER"
                                + ")";
                cmd.ExecuteNonQuery();
            }

            cn.Close();
        }
        public PdProductAccess(PdProduct model)
            : base(model)
        {

        }

        public void InsertProduct(PdProduct pdProduct)
        {
            var sqlStr = string.Format(@"insert into product(GBStandard,Batcode,ClassName,MaterialName,SpecName,Bundlecode,Length,Meterweight,Piececount,Weight,Randomcode,Createtime) 
                                        values(
                                            @GBStandard,@Batcode,@ClassName,@MaterialName,@SpecName,@Bundlecode,@Length,@Meterweight,@Piececount,@Weight,@Randomcode,@Createtime
                                        )");
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                cmd.Parameters.Add("@GBStandard", System.Data.DbType.String).Value = pdProduct.GBStandard;
                cmd.Parameters.Add("@Batcode", System.Data.DbType.String).Value = pdProduct.Batcode;
                cmd.Parameters.Add("@ClassName", System.Data.DbType.String).Value = pdProduct.ClassName;
                cmd.Parameters.Add("@MaterialName", System.Data.DbType.String).Value = pdProduct.MaterialName;
                cmd.Parameters.Add("@SpecName", System.Data.DbType.String).Value = pdProduct.SpecName;
                cmd.Parameters.Add("@Bundlecode", System.Data.DbType.String).Value = pdProduct.Bundlecode;
                cmd.Parameters.Add("@Length", System.Data.DbType.Double).Value = pdProduct.Length;
                cmd.Parameters.Add("@Meterweight", System.Data.DbType.Double).Value = pdProduct.Meterweight;
                cmd.Parameters.Add("@Piececount", System.Data.DbType.Int16).Value = pdProduct.Piececount;
                cmd.Parameters.Add("@Weight", System.Data.DbType.Double).Value = pdProduct.Weight;
                cmd.Parameters.Add("@Randomcode", System.Data.DbType.String).Value = pdProduct.Randomcode;
                cmd.Parameters.Add("@Createtime", System.Data.DbType.Int64).Value = pdProduct.Createtime;
                cmd.ExecuteNonQuery();
            }           
        }
    }
}
