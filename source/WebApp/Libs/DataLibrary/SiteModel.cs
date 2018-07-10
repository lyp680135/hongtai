namespace DataLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using static DataLibrary.EnumList;

    /// <summary>
    /// 模型管理
    /// </summary>
    [Table("sitemodel")]
    public class SiteModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets list<int>
        /// </summary>
        public string BaseManageId { get; set; }

        /// <summary>
        /// Gets or sets 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets 创建时间
        /// </summary>
        public int? CreateTime { get; set; }

        /// <summary>
        /// Gets or sets 模型录入审核状态
        /// </summary>
        public CheckStatus_ManageModel CheckStatus_ManageModel { get; set; }

        /// <summary>
        /// Gets or sets 创建人
        /// </summary>
        public int? Uid { get; set; }
    }
}
