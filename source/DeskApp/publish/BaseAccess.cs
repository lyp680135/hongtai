using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCardPrinter.Utils;

namespace WpfCardPrinter.ModelAccess
{
    public class BaseAccess<T> : IDisposable
    {
        public T _model;
        public MySqlConnection _connection;

        public string myConnectionString = "Server=db.xiaoyutt.com; Port=3306; Uid=warranty_hongtai; Pwd=HongTaiPWDty20910ScbsS86Xiaoyu_1; Database=warranty_hongtai";

        public BaseAccess()
        {
            _connection = new MySqlConnection(myConnectionString);
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        public BaseAccess(T model)
        {
            _model = model;

            _connection = new MySqlConnection(myConnectionString);
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        public BaseAccess(MySqlConnection conn)
        {
            _connection = conn;
        }

        public MySqlConnection GetConnection()
        {
            return _connection;
        }

        ~BaseAccess()
        {
            Dispose();
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
