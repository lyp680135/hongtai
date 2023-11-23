namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 闽源独有，质量证明书，手动输入版
    /// </summary>
    [Table("saleprintlognew")]
    public partial class SalePrintlogNew
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 材质ID
        /// </summary>
        public int? MaterialId { get; set; }

        /// <summary>
        /// Gets or sets 收货单位
        /// </summary>
        [MaxLength(255)]
        public string Consignor { get; set; }

        /// <summary>
        /// Gets or sets 车牌
        /// </summary>
        [MaxLength(100)]
        public string Lpn { get; set; }

        /// <summary>
        /// Gets or sets 质保书打印编号
        /// </summary>
        [MaxLength(50)]
        public string Printno { get; set; }

        /// <summary>
        /// Gets or sets 质保书印章角度
        /// </summary>
        public int? Signetangle { get; set; }

        public int? Createtime { get; set; }

        /// <summary>
        /// Gets or sets 状态：0-预览未下载  1-已下载 10-已撤销
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets 质量证明书随机校验码
        /// </summary>
        public string Checkcode { get; set; }

        /// <summary>
        /// Gets or sets 简单流程时保存签发人
        /// </summary>
        public int? Adder { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        [MaxLength(50)]
        public string Purchaseno { get; set; }
    }
}
