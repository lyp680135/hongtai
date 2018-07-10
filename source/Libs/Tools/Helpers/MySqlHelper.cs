using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Reflection;

namespace Util.Helpers
{
    public class MySqlHelper
    {
        public static string connectionStr = string.Empty;
        private static MySqlHelper sqlHelperInstance;
        //public static string connection = System.Configuration.ConfigurationManager.ConnectionStrings["KTPushDB"].ToString();
        // public static void SetConnection(string con) { connection = con; }
        public static MySqlHelper GetInstanct(string connStr)
        {
            if (sqlHelperInstance == null)
                sqlHelperInstance = new MySqlHelper(connStr);
            return sqlHelperInstance;
        }
        /// <summary>
        /// 无参的构造函数
        /// </summary>
        public MySqlHelper()
        {

        }
        /// <summary>
        /// 构造函数初始化数据库连接
        /// </summary>
        /// <param name="connStr"></param>
        public MySqlHelper(string connStr)
        {
            connectionStr = connStr;
        }
        /// <summary>
        /// 返回单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public T ExecuteEntity<T>(string cmdText, CommandType commandType, params MySqlParameter[] args)
        {
            T obj = default(T);
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                {
                    cmd.CommandType = commandType;
                    if (args != null)
                    {
                        cmd.Parameters.AddRange(args);
                    }
                    con.Open();
                    using (MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            obj = ExecuteReader<T>(dr);
                            break;
                        }
                    }
                    con.Close();
                }
            }
            return obj;
        }
        /// <summary>
        /// 返回字典
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Dictionary<string, object> ExecuteEntityToDic(string cmdText, CommandType commandType, params MySqlParameter[] args)
        {
            Dictionary<string, object> keyValuePairs = null;
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                {
                    cmd.CommandType = commandType;
                    if (args != null)
                    {
                        cmd.Parameters.AddRange(args);
                    }
                    con.Open();
                    using (MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            keyValuePairs = ExecuteReaderToDic(dr);
                            break;
                        }
                    }
                    con.Close();
                }
            }
            return keyValuePairs;
        }
        /// <summary>
        /// 返回集合字典
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> ExecuteEntityToDicList(string cmdText, CommandType commandType, params MySqlParameter[] args)
        {
            List<Dictionary<string, object>> keyValuePairs = new List<Dictionary<string, object>>();
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                {
                    cmd.CommandType = commandType;
                    if (args != null)
                    {
                        cmd.Parameters.AddRange(args);
                    }
                    con.Open();
                    using (MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            keyValuePairs.Add(ExecuteReaderToDic(dr));                        
                        }
                    }
                    con.Close();
                }
            }
            return keyValuePairs;
        }
        private Dictionary<string, object> ExecuteReaderToDic(MySqlDataReader dr)
        {
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
            int fieldCount = dr.FieldCount;//get column count
            for (int i = 0; i < fieldCount; i++)
            {
                string fieldName = dr.GetName(i);//get column           
                object value = dr.GetValue(i);
                if (value != null && value != DBNull.Value)
                {
                    keyValuePairs.Add(fieldName, value);
                }
            }
            return keyValuePairs;
        }
        /// <summary>
        /// MySqlDataReader
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="dr">数据读取器</param>
        /// <returns></returns>
        private T ExecuteReader<T>(MySqlDataReader dr)
        {
            T obj = default(T);
            obj = Activator.CreateInstance<T>();//T obj = new T();//instance
            Type type = typeof(T);//get T class type by T's Name
            PropertyInfo[] propertyInfos = type.GetProperties();//get current Type's all properties
            int fieldCount = dr.FieldCount;//get column count
            for (int i = 0; i < fieldCount; i++)
            {
                string fieldName = dr.GetName(i);//get column
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {//per property infoname
                    string properyName = propertyInfo.Name;//get property name
                    if (string.Compare(fieldName, properyName, true) == 0)
                    {//column's name == propery's name
                        object value = dr.GetValue(i);//get column's value
                        if (value != null && value != DBNull.Value)
                        {
                            propertyInfo.SetValue(obj, value, null);//set property's value
                        }
                        break;
                    }
                }
            }
            return obj;
        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader ( 注意：使用后一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string strSQL, params MySqlParameter[] parameters)
        {
            MySqlConnection connection = new MySqlConnection(connectionStr);
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            cmd.CommandTimeout = 600;
            try
            {
                connection.Open();
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 返回受影响的行数
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, CommandType commandType, params MySqlParameter[] args)
        {
            int result = -1;
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                {
                    if (args != null)
                    {
                        cmd.Parameters.AddRange(args);
                    }
                    cmd.CommandType = commandType;
                    con.Open();
                    result = cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return result;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="commandType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object ExecuteEntity(string cmdText, CommandType commandType, params MySqlParameter[] args)
        {
            object obj = null;

            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                {
                    cmd.CommandType = commandType;
                    if (args != null)
                        cmd.Parameters.AddRange(args);
                    con.Open();
                    using (MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            if (dr.FieldCount > 0)
                                obj = dr.GetValue(0);
                            break;
                        }
                    }
                    con.Close();
                }
            }
            return obj;
        }
        /// <summary>
        /// 返回泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText"></param>
        /// <param name="args"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public List<T> ExecuteList<T>(string cmdText, List<MySqlParameter> args, CommandType commandType = CommandType.StoredProcedure)
        {
            List<T> list = new List<T>();
            using (MySqlConnection con = new MySqlConnection(connectionStr))
            {
                using (MySqlCommand cmd = new MySqlCommand(cmdText, con))
                {
                    cmd.CommandType = commandType;
                    if (args != null)
                    {
                        cmd.Parameters.AddRange(args.ToArray());
                    }
                    con.Open();

                    using (MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        do
                        {
                            while (dr.Read())
                            {
                                //dr.GetInt32(0);
                                //dr.GetString(1);
                                T obj = ExecuteReader<T>(dr);
                                list.Add(obj);
                            }
                        }
                        while (dr.NextResult());
                    }
                    con.Close();
                }
            }
            return list;
        }
    }
}
