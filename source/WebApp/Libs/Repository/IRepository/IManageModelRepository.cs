using System;
using System.Collections.Generic;
using System.Text;

namespace Repository
{
  public  interface IManageModelRepository
    {
        /// <summary>
        /// 检测表名是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        bool IsExist(string tableName);
        /// <summary>
        /// 根据表名建表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        string CreateTableSql(string tableName);
    }
}
