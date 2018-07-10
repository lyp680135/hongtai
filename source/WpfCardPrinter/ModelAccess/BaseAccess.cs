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

#if DEBUG
        public string myConnectionString = System.Configuration.ConfigurationManager.AppSettings["DBConnectionString"];
#else
        public string myConnectionString = "Server=47.98.246.14; Port=3306; Uid=warranty_mingyuan; Pwd=warranty23MingYuan_DoIt_145Xiaoyu_6; Database=warranty_mingyuan";
#endif

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
