namespace WarrantyApiCenter.Models.DataModel.WorkShop
{
    /// <summary>
    /// DataList里面的规格数据列表
    /// </summary>
    public class DataList_Spec
    {
        /// <summary>
        /// Gets or sets 规格ID
        /// </summary>
        public int SpecId { get; set; }

        /// <summary>
        /// Gets or sets 规格名称
        /// </summary>
        public string SpecName { get; set; }

        /// <summary>
        /// Gets or sets 授权数
        /// </summary>
        public int Number_Auth { get; set; }

        /// <summary>
        /// Gets or sets 可用数
        /// </summary>
        public int Number_Available { get; set; }

        /// <summary>
        /// Gets or sets 增减数
        /// </summary>
        public int EditNumber { get; set; }
    }
}
