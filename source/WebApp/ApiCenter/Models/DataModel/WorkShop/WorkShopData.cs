namespace WarrantyApiCenter.Models.DataModel.WorkShop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// 设置单个车间二维码数据时返回的Json数据实体类
    /// </summary>
    public class WorkShopData
    {
        /// <summary>
        /// Gets or sets 车间ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 车间名称
        /// </summary>
        public string Name { get; set; }

        public List<DataList> Data { get; set; }
    }
}
