using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfQualityCertPrinter.Model;
using MySql.Data.MySqlClient;
namespace WpfQualityCertPrinter.ModelAccess
{
    public class GJSelectModelAccess : BaseAccess<GJSelectModel>
    {
        public List<GJSelectModel> GetList(string condition, Dictionary<string, object> param)
        {
            string sqlStr = @" SELECT * FROM (SELECT a.printno,a.status,a.createtime,a.consignor,d.Name,c.Lpn FROM saleprintlog as a
                                            inner join saleprintlogdetail as b on a.id=b.printid
                                            inner join salesellerauth as c on c.Id=b.authid
                                            inner join saleseller as d on d.Id=c.Sellerid) as t
                                            WHERE 1=1 and  status=1";
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sqlStr += condition;
            }
            List<MySqlParameter> sp = new List<MySqlParameter>();
            if (param.Count > 0)
            {
                foreach (var item in param)
                {
                    sp.Add(new MySqlParameter(item.Key, item.Value));
                }
            }
            return XYNetCloud.Utils.MySqlHelper.GetInstanct(myConnectionString).ExecuteList<GJSelectModel>(sqlStr, sp, System.Data.CommandType.Text);
        }
    }
}
