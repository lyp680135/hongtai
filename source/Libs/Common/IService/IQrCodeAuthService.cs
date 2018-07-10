namespace Common.IService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Util;

    /// <summary>
    /// 车间二维码数量授权服务 - 刘钟
    /// </summary>
    public interface IQrCodeAuthService
    {
        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="list_add">要添加的数据集合</param>
        /// <returns>影响行数</returns>
        int AddRange(List<DataLibrary.PdQRCodeAuth> list_add);

        /// <summary>
        /// 获取上个月的所有授权设置数据
        /// </summary>
        /// <returns>返回元组数据，第一个为上月的授权数据集合，第二个为PdQRCodeAuth表最大的ID</returns>
        Tuple<List<DataLibrary.PdQRCodeAuth>, int> GetDataForPrevMonth();

        /// <summary>
        /// 查询当月是否设置授权数据
        /// </summary>
        /// <returns>有为true,无为false</returns>
        bool IsExistsDataForThisMonty();

        /// <summary>
        /// 根据车间与规格ID获取当月授权数
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <returns>当前授权数</returns>
        int GetAuthNumber(int workShopId, int specId);

        /// <summary>
        /// 根据规格ID获取当月剩余可用数
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <returns>当月剩余可用数</returns>
        int GetAvailableNumber(int workShopId, int specId);

        /// <summary>
        /// 根据规格ID获取当前车间本月已使用数
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <returns>本月已使用数</returns>
        int GetUseNumber(int workShopId, int specId);

        /// <summary>
        /// 新增减少本月车间二维码授权使用数(如果本月不存在授权纪录，则自动新增)
        /// </summary>
        /// <param name="workShopId">车间ID</param>
        /// <param name="specId">规格ID</param>
        /// <param name="editNumber">需要更改的授权数值</param>
        /// <param name="adder">操作人ID</param>
        /// <returns>更改后的PdQRCodeAuth实体，操作失败返回NULL</returns>
        DataLibrary.PdQRCodeAuth SetAvailableNumber(int workShopId, int specId, int editNumber, int adder);
    }
}
