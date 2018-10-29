using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfQualityCertPrinter.ModelAccess
{
    public class BaseAccess<T> : IDisposable
    {
        public T _model;
        public MySqlConnection _connection;
#if DEBUG
        public string myConnectionString = "Server=114.215.102.245; Port=3306; Uid=warranty; Pwd=warranty123456; Database=qualitytest";
#else
        public string myConnectionString = "Server=db.xiaoyutt.com; Port=3306; Uid=warranty_hongtai; Pwd=; Database=warranty_hongtai";
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
            _connection.Close();
            _connection.Dispose();
        }
    }
}
