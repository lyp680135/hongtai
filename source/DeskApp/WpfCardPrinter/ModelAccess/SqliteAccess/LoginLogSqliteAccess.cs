using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WpfCardPrinter.Model;

namespace WpfCardPrinter.ModelAccess.SqliteAccess
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
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.Connection = cn;
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS loginlog ("
                                    + "[Id] INTEGER PRIMARY KEY AUTOINCREMENT,"
                                    + "[UserName] VARCHAR(255),"
                                    + "[LoginTime] INTEGER,"
                                    + "[RealName] VARCHAR(255),"
                                    + "[Address] VARCHAR(255), "
                                    + "[Code] VARCHAR(255) "
                                    + ")";
                    cmd.ExecuteNonQuery();

                    //for upgrade
                    cmd.CommandText = "ALTER TABLE loginlog ADD COLUMN "
                                   + "[Code] VARCHAR(255)";
                    cmd.ExecuteNonQuery();
                }
                catch
                {

                }
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
            string sqlStr = string.Format("insert into loginlog(UserName,LoginTime,RealName,Address,Code) values(@UserName,@LoginTime,@RealName,@Address,@Code)");
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                cmd.Parameters.Add("@UserName", DbType.String).Value = loginlog.UserName;
                cmd.Parameters.Add("@LoginTime", DbType.Int64).Value = loginlog.LoginTime;
                cmd.Parameters.Add("@RealName", DbType.String).Value = loginlog.RealName;
                cmd.Parameters.Add("@Address", DbType.String).Value = loginlog.Address;
                cmd.Parameters.Add("@Code", DbType.String).Value = loginlog.Code;
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }
        /// <summary>
        /// 修改时间
        /// </summary>
        public void Update(LoginLog loginlog)
        {
            string sqlStr = string.Format(@"update loginlog set LoginTime=@LoginTime,Code=@Code where UserName=@UserName ");
            using (SQLiteCommand cmd = new SQLiteCommand(sqlStr, _connection))
            {
                cmd.Parameters.Add("@UserName", DbType.String).Value = loginlog.UserName;
                cmd.Parameters.Add("@LoginTime", DbType.Int64).Value = loginlog.LoginTime;
                cmd.Parameters.Add("@Code", DbType.String).Value = loginlog.Code;
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
            string sqlStr = @"select * from loginlog where UserName=@UserName ";
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
                            loginlogInfo.Code = dr["Code"].ToString();
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
            string sqlStr = @"select * from loginlog order by LoginTime desc ";
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
                            loginlogInfo.Code = dr["Code"].ToString();
                            loginlist.Add(loginlogInfo);
                        }
                    }
                }
            }
            return loginlist;
        }
    }
}
