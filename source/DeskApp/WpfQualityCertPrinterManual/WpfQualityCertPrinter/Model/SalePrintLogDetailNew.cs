namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// 同一张质保书上的产品信息明细（按炉号、牌号、规格、质量等级分组）
    /// </summary>
    public class SalePrintLogDetailNew
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 打印主表键值
        /// </summary>
        [Required]
        public int PrintId { get; set; }

        /// <summary>
        /// Gets or sets 炉批号
        /// </summary>
        [MaxLength(255)]
        public string BatCode { get; set; }

        /// <summary>
        /// Gets or sets 规格
        /// </summary>
        [MaxLength(50)]
        public string Spec { get; set; }

        /// <summary>
        /// Gets or sets 长度
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets 单重
        /// </summary>
        public float? SingleWeight { get; set; }

        /// <summary>
        /// Gets or sets 实际打印数量
        /// </summary>
        public int? Printnumber { get; set; }
    }
}
