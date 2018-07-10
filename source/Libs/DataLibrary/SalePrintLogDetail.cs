namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 同一张质保书上的产品信息明细（按炉号、牌号、规格、质量等级分组）
    /// </summary>
    [Table("saleprintlogdetail")]
    public class SalePrintLogDetail
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 打印主表键值
        /// </summary>
        [Required]
        public int PrintId { get; set; }

        /// <summary>
        /// Gets or sets 授权主表键值
        /// </summary>
        [Required]
        public int Authid { get; set; }

        /// <summary>
        /// Gets or sets 打印产品分组件数
        /// </summary>
        [Required]
        public int Number { get; set; }
    }
}
