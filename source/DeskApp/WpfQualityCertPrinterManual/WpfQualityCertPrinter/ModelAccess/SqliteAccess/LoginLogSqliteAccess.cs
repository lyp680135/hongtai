﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using WpfQualityCertPrinter.Model;

namespace WpfQualityCertPrinter.ModelAccess.SqliteAccess
{
    public class LoginLogSqliteAccess : BaseSqliteAccess<LoginLog>
    {
        /// <summary>
        /// 初始建表
        /// </summary>
        public LoginLogSqliteAccess()
            : base()
        {
            SQLiteConnection cn = new SQLiteConnection("data source=" + sqlite_path);
            cn.SetPassword(sqlite_password);
            cn.Open();

            if (cn.State == System.Data.ConnectionState.Open)
            {
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = cn;
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS LoginLog ("
                                + "[Id] INTEGER PRIMARY KEY AUTOINCREMENT,"
                                + "[UserName] VARCHAR(255),"
                                + "[LoginTime] INTEGER,"
                                + "[RealName] VARCHAR(255),"
                                + "[Address] Address(255) "
                                + ")";
                cmd.ExecuteNonQuery();
            }

            cn.Close();
        }

        public LoginLogSqliteAccess(LoginLog model)
            : base(model)
        {

        }
        /// <summary>
        /// 插入登录记录
        /// </summary>
        /// <param name="loginlog"></param>
        /// <returns></returns>
        public int Insert(LoginLog loginlog)
        {
            int result = 0;
            string sqlStr = string.Format("insert into LoginLog(UserName,LoginTime,RealName,Address) values(@UserName,@LoginTime,@RealName,@Address)");
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                cmd.Parameters.Add("@UserName", DbType.String).Value = loginlog.UserName;
                cmd.Parameters.Add("@LoginTime", DbType.Int64).Value = loginlog.LoginTime;
                cmd.Parameters.Add("@RealName", DbType.String).Value = loginlog.RealName;
                cmd.Parameters.Add("@Address", DbType.String).Value = loginlog.Address;
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }
        /// <summary>
        /// 修改时间
        /// </summary>
        public void Update(LoginLog loginlog)
        {
            string sqlStr = string.Format(@"update loginlog set LoginTime=@LoginTime where UserName=@UserName ");
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                cmd.Parameters.Add("@UserName", DbType.String).Value = loginlog.UserName;
                cmd.Parameters.Add("@LoginTime", DbType.Int64).Value = loginlog.LoginTime;
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 返回单个对象
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public LoginLog LoginLogInfo(string username)
        {
            var loginlogInfo = new LoginLog();
            string sqlStr = @"select * from LoginLog where UserName=@UserName ";
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                cmd.Parameters.Add("@UserName", DbType.String).Value = username;
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        if (dr.Read())
                        {
                            loginlogInfo.Id = Convert.ToInt16(dr["Id"]);
                            loginlogInfo.UserName = dr["UserName"].ToString();
                            loginlogInfo.LoginTime = Convert.ToInt64(dr["LoginTime"]);
                            loginlogInfo.RealName = dr["RealName"].ToString();
                            loginlogInfo.Address = dr["Address"].ToString();
                        }
                    }
                }
            }
            return loginlogInfo;
        }
        /// <summary>
        /// 返回集合
        /// </summary>
        /// <returns></returns>
        public List<LoginLog> LoginLogList()
        {
            var loginlist = new List<LoginLog>();
            string sqlStr = @"select * from LoginLog order by LoginTime desc ";
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            var loginlogInfo = new LoginLog();
                            loginlogInfo.Id = Convert.ToInt16(dr["Id"]);
                            loginlogInfo.UserName = dr["UserName"].ToString();
                            loginlogInfo.LoginTime = Convert.ToInt64(dr["LoginTime"]);
                            loginlogInfo.RealName = dr["RealName"].ToString();
                            loginlogInfo.Address = dr["Address"].ToString();
                            loginlist.Add(loginlogInfo);
                        }
                    }
                }
            }
            return loginlist;
        }
    }
}
