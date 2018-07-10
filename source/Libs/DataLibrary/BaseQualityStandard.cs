namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("basequalitystandard")]
    public partial class BaseQualityStandard
    {
        [Key]
        public int Id { get; set; }

        public int? Classid { get; set; }

        public int? Materialid { get; set; }

        public int? Standardid { get; set; }

        /// <summary>
        /// Gets or sets 指标名
        /// </summary>
        [MaxLength(255)]
        public string TargetName { get; set; }

        /// <summary>
        /// Gets or sets 指标英文名
        /// </summary>
        [MaxLength(255)]
        public string TargetNameEn { get; set; }

        /// <summary>
        /// Gets or sets 指标下限
        /// </summary>
        public double? TargetMin { get; set; }

        /// <summary>
        /// Gets or sets 指标上限
        /// </summary>
        public double? TargetMax { get; set; }

        /// <summary>
        /// Gets or sets 指标说明
        /// </summary>
        [MaxLength(255)]
        public string TargetNote { get; set; }

        /// <summary>
        /// Gets or sets 状态0.未禁用1.已禁用
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Gets or sets 指标类型：0.单行 1.多行
        /// </summary>
        public int? TargetType { get; set; }

        /// <summary>
        ///  Gets or sets 指标类型：0.不必填 1.必填
        /// </summary>
        public int? TargetIsNull { get; set; }
    }
}
