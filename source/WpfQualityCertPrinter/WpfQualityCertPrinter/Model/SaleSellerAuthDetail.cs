namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SaleSellerAuthDetail
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 授权主表键值
        /// </summary>
        public int AuthId { get; set; }

        /// <summary>
        /// Gets or sets 产品表键值
        /// </summary>
        public int Productid { get; set; }

        /// <summary>
        /// Gets or sets 产品品名（冗余）
        /// </summary>
        public int? Classid { get; set; }

        /// <summary>
        /// Gets or sets 产品材质（冗余）
        /// </summary>
        public int? Materialid { get; set; }

        /// <summary>
        /// Gets or sets 产品规格（冗余）
        /// </summary>
        public int? Specid { get; set; }

        /// <summary>
        /// Gets or sets 产品批号（冗余）
        /// </summary>
        public string Batcode { get; set; }
    }
}
