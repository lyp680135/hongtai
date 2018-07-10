namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("salesellerauthdetail")]
    public partial class SaleSellerAuthDetail
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 授权主表键值
        /// </summary>
        [Required]
        public int AuthId { get; set; }

        /// <summary>
        /// Gets or sets 产品表键值
        /// </summary>
        [Required]
        public int Productid { get; set; }

        /// <summary>
        /// Gets or sets 产品品名（冗余）
        /// </summary>
        [Required]
        public int? Classid { get; set; }

        /// <summary>
        /// Gets or sets 产品材质（冗余）
        /// </summary>
        [Required]
        public int? Materialid { get; set; }

        /// <summary>
        /// Gets or sets 产品规格（冗余）
        /// </summary>
        [Required]
        public int? Specid { get; set; }

        /// <summary>
        /// Gets or sets 产品批号（冗余）
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Batcode { get; set; }
    }
}
