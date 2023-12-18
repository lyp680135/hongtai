namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Common.Service;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// 质量证明书手动输入版 （闽源特有）
    /// </summary>
    public interface ICertNewService
    {
        /// <summary>
        /// 生成质保书记录
        /// </summary>
        /// <param name="list">能打在一张质保书里的产品分组列表</param>
        /// <param name="sellerid">售达方</param>
        /// <param name="lpn">车牌号</param>
        /// <param name="consignor">收货单位</param>
        /// <param name="userid">打印人员Id</param>
        /// <param name="outDate">出证日期</param>
        /// <param name="purchaseno">采购单号</param>
        /// <returns>质保书序号</returns>
        CommonResult AddCert(JArray list, int sellerid, string lpn, string consignor, int userid, DateTime outDate, string purchaseno="");

        /// <summary>
        /// 生成质保书记录
        /// </summary>
        /// <param name="list">能打在一张质保书里的产品分组列表</param>
        /// <param name="sellerid">售达方</param>
        /// <param name="lpn">车牌号</param>
        /// <param name="consignor">收货单位</param>
        /// <param name="userid">打印人员Id</param>
        /// <param name="outDate">出证日期</param>
        /// <param name="purchaseno">采购单号</param>
        /// <returns>质保书序号</returns>
        CommonResult AddCert2(JArray list, int sellerid, string lpn, string consignor, int userid, DateTime outDate, string purchaseno = "");


        /// <summary>
        /// 获取填入质保书中的数据
        /// </summary>
        /// <param name="printno">质保书序号</param>
        /// <returns>质保书数据</returns>
        CommonResult GetCertData(string printno);

        /// <summary>
        /// 生成质保书图片
        /// </summary>
        /// <param name="printno">质保书序号</param>
        /// <param name="savepath">质保书保存路径、模板目录默认在template下</param>
        /// <param name="iswater">是否加水印</param>
        /// <returns>生成的图片路径</returns>
        CommonResult GenerateCert(string printno, string savepath, bool iswater);

        /// <summary>
        /// 生成质保书图片，取真实质量数据
        /// </summary>
        /// <param name="printno">质保书序号</param>
        /// <param name="savepath">质保书保存路径、模板目录默认在template下</param>
        /// <param name="iswater">是否加水印</param>
        /// <returns>生成的图片路径</returns>
        CommonResult GenerateCert2(string printno, string savepath, bool iswater);
    }
}
