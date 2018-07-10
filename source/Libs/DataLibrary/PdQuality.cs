namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using static DataLibrary.EnumList;

    [Table("pdquality")]
    public partial class PdQuality
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 炉批号
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string Batcode { get; set; }

        /// <summary>
        /// Gets or sets 产品质量数据- 化学
        /// </summary>
        public JsonObject<object> Qualityinfos { get; set; }

        /// <summary>
        /// Gets or sets 产品质量数据- 力学
        /// </summary>
        public JsonObject<List<object>> Qualityinfos_Dynamics { get; set; }

        /// <summary>
        /// Gets or sets 录入人
        /// </summary>
        public int EntryPerson { get; set; }

        /// <summary>
        /// Gets or sets 审核人
        /// </summary>
        public int? CheckPerson { get; set; }

        /// <summary>
        /// Gets or sets 创建时间
        /// </summary>
        public int Createtime { get; set; }

        /// <summary>
        /// Gets or sets 审核状态
        /// </summary>
        public CheckStatus_PdQuality CheckStatus { get; set; }

        /// <summary>
        /// Gets or sets 材质Id
        /// </summary>
        public int? MaterialId { get; set; }

        /// <summary>
        /// Gets or sets 创建标识,0:非预置数据,1:预置数据
        /// </summary>
        public int? CreateFlag { get; set; }

        /// <summary>
        /// Gets or sets 审核备注
        /// </summary>
        [MaxLength(255)]
        public string CheckMark { get; set; }
    }
}
