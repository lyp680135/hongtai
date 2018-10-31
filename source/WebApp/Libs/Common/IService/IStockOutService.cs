namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Common.Service;
    using Newtonsoft.Json.Linq;

    public interface IStockOutService
    {
        /// <summary>
        /// 生成出库记录
        /// </summary>
        /// <param name="batcode">产品轧制批号</param>
        /// <param name="lpn">出库车号</param>
        /// <param name="sellerid">出库的售达方</param>
        /// <param name="startbundle">开始捆号</param>
        /// <param name="endbundle">结束捆号</param>
        /// <param name="lengthtype">定尺非尺</param>
        /// <param name="userid">出库员</param>
        /// <returns>出库结果</returns>
        int Stockout(string batcode, string lpn, int sellerid, int startbundle, int endbundle, int lengthtype, int userid);

        /// <summary>
        /// 生成出库记录
        /// </summary>
        /// <param name="batcode">产品轧制批号</param>
        /// <param name="lpn">出库车号</param>
        /// <param name="sellerid">出库的售达方</param>
        /// <param name="specid">规格</param>
        /// <param name="lengthtype">定尺非尺</param>
        /// <param name="number">数量</param>
        /// <returns>出库结果</returns>
        CommonResult StockoutWpf(string batcode, string lpn, int sellerid, int specid, int lengthtype, int number, int userid);
    }
}
