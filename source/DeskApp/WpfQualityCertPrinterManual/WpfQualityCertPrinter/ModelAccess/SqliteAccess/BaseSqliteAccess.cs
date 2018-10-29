using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using XYNetCloud.Utils;

namespace WpfQualityCertPrinter.ModelAccess.SqliteAccess
{
    public class BaseSqliteAccess<T> : IDisposable
    {
        public T _model;
        public SQLiteConnection _connection;

        protected static string sqlite_path = @"database\offcaching.dat";
        protected static string sqlite_password = "xiaoyutech888";

        /// <summary>
        /// 创建数据库
        /// </summary>
        public static void CreateDB()
        {
            //判断文件是否已经存在，如果存在，则不用再次创建
            if (!File.Exists(sqlite_path))
            {
                if (!Directory.Exists("database"))
                {
                    Directory.CreateDirectory("database");
                }           
            }
        }

        public BaseSqliteAccess()
        {           
            try
            {
                CreateDB();
                _connection = new SQLiteConnection("data source=" + sqlite_path);
                _connection.SetPassword(sqlite_password);
                _connection.Open();
                
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        public BaseSqliteAccess(T model)
        {
            _model = model;

            SQLiteConnection cn = new SQLiteConnection("data source=" + sqlite_path);
            try
            {
                _connection.Open();
                _connection.SetPassword(sqlite_password);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }
        public void Dispose()
        {
            try
            {
                _connection.Close();
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }
    }
}
