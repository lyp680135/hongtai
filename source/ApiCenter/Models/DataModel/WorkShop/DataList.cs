namespace WarrantyApiCenter.Models.DataModel.WorkShop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// WorkShopData的规格数据列表
    /// </summary>
    public class DataList
    {
        /// <summary>
        /// Gets or sets 产品Id
        /// </summary>
        public int ClassId { get; set; }

        /// <summary>
        /// Gets or sets 产品名称
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets 材质ID
        /// </summary>
        public int MaterialId { get; set; }

        /// <summary>
        /// Gets or sets 材质名称
        /// </summary>
        public string MaterialName { get; set; }

        public List<DataList_Spec> Spec { get; set; }
    }
}
