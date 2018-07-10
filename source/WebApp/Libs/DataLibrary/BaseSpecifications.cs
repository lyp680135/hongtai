namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("basespecifications")]
    public partial class BaseSpecifications
    {
        [Key]
        public int Id { get; set; }

        public int? Classid { get; set; }

        public int? Materialid { get; set; }

        [MaxLength(50)]
        public string Callname { get; set; }

        [MaxLength(50)]
        public string Specname { get; set; }

        // 参考长度
        public double? Referlength { get; set; }

        // 参考米重
        public double? Refermeterweight { get; set; }

        public double? Referpieceweight { get; set; }

        public int? Referpiececount { get; set; }

        /// <summary>
        /// Gets or sets 冷弯计算系数
        /// </summary>
        public double? Coldratio { get; set; }

        /// <summary>
        /// Gets or sets 反弯计算系数
        /// </summary>
        public double? Reverseratio { get; set; }
    }
}
