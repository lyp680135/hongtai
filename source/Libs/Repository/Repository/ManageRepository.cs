using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
    public class ManageRepository : IManageModelRepository
    {

        public ManageRepository()
        {

        }
        public string CreateTableSql(string tableName)
        {
            throw new NotImplementedException();
        }

        public bool IsExist(string tableName)
        {
            throw new NotImplementedException();
        }
    }
}
