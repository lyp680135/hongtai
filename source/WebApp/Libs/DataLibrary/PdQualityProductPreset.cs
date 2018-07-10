namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    /// <summary>
    /// 数据关系预置表
    /// </summary>
    [Table("pdqualityproductpreset")]
    public class PdQualityProductPreset
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets 炉批号
        /// </summary>
        public string Batcode { get; set; }

        /// <summary>
        /// Gets or sets  材质id
        /// </summary>
        public int? Materialid { get; set; }

        /// <summary>
        /// Gets or sets 预置质量数据id
        /// </summary>
        public int? Qid { get; set; }

        /// <summary>
        /// Gets or sets 创建时间
        /// </summary>
        public int? Createtime { get; set; }
    }
}
